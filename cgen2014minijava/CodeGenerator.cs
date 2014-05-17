using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace cgen2014minijava
{
    public class ClassBuilder
    {
        public ClassBuilder(CodeGenerator holder, TypeBuilder builder, ClassNode node)
        {
            inherits = null;
            classnode = node;
            typeholder = holder;
            this.builder = builder;
            vars = new Dictionary<VariableNode, FieldBuilder>();
            methods = new Dictionary<MethodNode, MethodBuilder>();
            constructor = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Any, new Type[]{});
            var a = constructor.GetILGenerator();
            a.Emit(System.Reflection.Emit.OpCodes.Ret);
        }
        public void bindMethods()
        {
            foreach (MethodNode n in classnode.methods)
            {
                methods.Add(n, builder.DefineMethod(n.name, System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Virtual, typeholder.getType(n.type.Type), n.arguments.Select(a => typeholder.getType(a.type.Type)).ToArray()));
            }
        }
        public void bindMainMethod()
        {
            MethodNode met = classnode.methods[0];
            methods.Add(met, builder.DefineMethod(met.name, System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static, typeholder.getType(met.type.Type), met.arguments.Select(a => typeholder.getType(a.type.Type)).ToArray()));
        }
        public ClassBuilder inherits;
        public ClassNode classnode;
        public ConstructorBuilder constructor;
        public CodeGenerator typeholder;
        public TypeBuilder builder;
        public FieldBuilder getField(VariableNode var)
        {
            if (!vars.Keys.Contains(var))
            {
                vars.Add(var, builder.DefineField(var.name, typeholder.getType(var.type.Type), System.Reflection.FieldAttributes.Private));
            }
            return vars[var];
        }
        public MethodBuilder getMethod(MethodNode met)
        {
            if (!methods.Keys.Contains(met))
            {
                if (inherits != null)
                {
                    return inherits.getMethod(met);
                }
                throw new Exception("This shouldn't happen: code generation method not found (class " + classnode.name + ", method " + met.name + ")");
            }
            return methods[met];
        }
        public void finish()
        {
            builder.CreateType();
        }
        private Dictionary<VariableNode, FieldBuilder> vars;
        private Dictionary<MethodNode, MethodBuilder> methods;
    }
    public class CodeGenerator
    {
        private Dictionary<ClassNode, ClassBuilder> classes;
        private Dictionary<String, Type> types;
        private ClassBuilder currentClass;
        private bool currentClassIsMain;
        private MethodBuilder currentMethod;
        private Dictionary<VariableNode, LocalBuilder> locals;
        private Dictionary<ObjectMethodReference, MethodBuilder> objectMethods;
        public MethodBuilder getMethod(ObjectMethodReference node)
        {
            return classes[((ClassType)node.obj.type).type].getMethod(node.method);
        }
        public Type getType(String typeName)
        {
            if (typeName.Last() == ']')
            {
                return typeof(Object);
            }
            return types[typeName];
        }
        public CodeGenerator()
        {
        }
        public AssemblyBuilder generateModule(ProgramNode prog, String name)
        {
            //don't bother doing proper stacks with locals or vars, assume AST checked everything and overwrite whenever needed
            classes = new Dictionary<ClassNode, ClassBuilder>();
            types = new Dictionary<String, Type>();
            locals = new Dictionary<VariableNode, LocalBuilder>();
            types.Add("void", typeof(void));
            types.Add(typeof(Int32).ToString(), typeof(Int32));
            types.Add(typeof(Boolean).ToString(), typeof(Int32)); //bools are ints
            AppDomain ad = AppDomain.CurrentDomain;
            AssemblyName am = new AssemblyName(System.IO.Path.GetFileNameWithoutExtension(name));
            AssemblyBuilder ab = ad.DefineDynamicAssembly(am, AssemblyBuilderAccess.Save);
            ModuleBuilder module = ab.DefineDynamicModule(name);
            //classes are assumed to be topologically sorted by their inheritance
            foreach (ClassNode n in prog.classes)
            {
                declareClass(module, n);
            }
            foreach (ClassNode n in prog.classes)
            {
                if (n != prog.mainClass)
                {
                    if (n.inherits != null)
                    {
                        classes[n].inherits = classes[n.inherits];
                    }
                    classes[n].bindMethods();
                }
                else
                {
                    classes[n].bindMainMethod();
                }
            }
            foreach (ClassNode n in prog.classes)
            {
                if (n != prog.mainClass)
                {
                    generateClass(module, n);
                }
                else
                {
                    generateMainClass(module, n);
                }
            }
            foreach (ClassBuilder b in classes.Values)
            {
                b.finish();
            }
            ab.SetEntryPoint(classes[prog.mainClass].builder.DeclaredMethods.First());
            return ab;
        }
        private void declareClass(ModuleBuilder mb, ClassNode c)
        {
            TypeBuilder newType;
            if (c.inherits != null)
            {
                newType = mb.DefineType(c.name, System.Reflection.TypeAttributes.Class, classes[c.inherits].builder);
            }
            else
            {
                newType = mb.DefineType(c.name, System.Reflection.TypeAttributes.Class);
            }
            classes[c] = new ClassBuilder(this, newType, c);
            types[c.name] = newType;
        }
        private void generateMainClass(ModuleBuilder mb, ClassNode c)
        {
            currentClassIsMain = true;
            currentClass = classes[c];
            TypeBuilder thisClass = currentClass.builder;
            if (c.methods.Count != 1)
            {
                throw new Exception("Main class methods.count != 1");
            }
            generateMethod(currentClass.getMethod(c.methods[0]), c.methods[0]);
        }
        private void generateClass(ModuleBuilder mb, ClassNode c)
        {
            currentClassIsMain = false;
            currentClass = classes[c];
            TypeBuilder thisClass = currentClass.builder;
            foreach (MethodNode m in c.methods)
            {
                generateMethod(currentClass.getMethod(m), m);
            }
        }
        private void generateMethod(MethodBuilder builder, MethodNode method)
        {
            currentMethod = builder;
            ILGenerator methodCode = builder.GetILGenerator();
            for (int i = 0; i < method.arguments.Count; i++)
            {
                ParameterBuilder arg = builder.DefineParameter(i + 1, System.Reflection.ParameterAttributes.In, method.arguments[i].name);
                //load arguments as locals
                locals[method.arguments[i]] = methodCode.DeclareLocal(getType(method.arguments[i].type.Type));
                methodCode.Emit(System.Reflection.Emit.OpCodes.Ldarg, i + 1);
                methodCode.Emit(System.Reflection.Emit.OpCodes.Stloc, locals[method.arguments[i]]);
            }
            generateStatementCode(methodCode, method.statements);
            methodCode.Emit(System.Reflection.Emit.OpCodes.Ret);
        }
        private void generateStatementCode(ILGenerator generator, BlockStatementNode node)
        {
            foreach (VariableNode v in node.locals)
            {
                locals[v] = generator.DeclareLocal(getType(v.type.Type));
            }
            foreach (StatementNode s in node.statements)
            {
                generateStatementCode(generator, s);
            }
        }
        private void generateStatementCode(ILGenerator generator, StatementNode node)
        {
            if (node is BlockStatementNode)
            {
                generateStatementCode(generator, (BlockStatementNode)node);
            }
            else if (node is WhileNode)
            {
                generateStatementCode(generator, (WhileNode)node);
            }
            else if (node is IfNode)
            {
                generateStatementCode(generator, (IfNode)node);
            }
            else if (node is AssignmentNode)
            {
                generateStatementCode(generator, (AssignmentNode)node);
            }
            else if (node is ReturnNode)
            {
                generateStatementCode(generator, (ReturnNode)node);
            }
            else if (node is AssertNode)
            {
                generateStatementCode(generator, (AssertNode)node);
            }
            else if (node is PrintNode)
            {
                generateStatementCode(generator, (PrintNode)node);
            }
            else if (node is FunctionCall)
            {
                generateExpressionCode(generator, (FunctionCall)node);
                //pop the return value
                if (((FunctionCall)node).type is BaseType)
                {
                    if (((BaseType)((FunctionCall)node).type).type == null)
                    {
                        //void function, don't need to pop
                    }
                    else
                    {
                        generator.Emit(System.Reflection.Emit.OpCodes.Pop);
                    }
                }
                else
                {
                    generator.Emit(System.Reflection.Emit.OpCodes.Pop);
                }
            }
        }
        private void generateExpressionCode(ILGenerator generator, ExpressionNode node)
        {
            if (node is ThisNode)
            {
                generateExpressionCode(generator, (ThisNode)node);
            }
            else if (node is IntConstant)
            {
                generateExpressionCode(generator, (IntConstant)node);
            }
            else if (node is BoolConstant)
            {
                generateExpressionCode(generator, (BoolConstant)node);
            }
            else if (node is FunctionCall)
            {
                generateExpressionCode(generator, (FunctionCall)node);
            }
            else if (node is UnaryOperatorCall)
            {
                generateExpressionCode(generator, (UnaryOperatorCall)node);
            }
            else if (node is BinaryOperatorCall)
            {
                generateExpressionCode(generator, (BinaryOperatorCall)node);
            }
            else if (node is NewSingular)
            {
                generateExpressionCode(generator, (NewSingular)node);
            }
            else if (node is NewArray)
            {
                generateExpressionCode(generator, (NewArray)node);
            }
            else if (node is ObjectMemberReference)
            {
                generateExpressionCode(generator, (ObjectMemberReference)node);
            }
            else if (node is ArrayLengthRead)
            {
                generateExpressionCode(generator, (ArrayLengthRead)node);
            }
            else if (node is LocalOrMemberReference)
            {
                generateExpressionCode(generator, (LocalOrMemberReference)node);
            }
            else if (node is ArrayReference)
            {
                generateExpressionCode(generator, (ArrayReference)node);
            }
            else
            {
                throw new Exception("Unhandled expression type " + node.ToString());
            }
        }
        private void generateExpressionCode(ILGenerator generator, BinaryOperatorCall node)
        {
            generateExpressionCode(generator, node.lhs);
            generateExpressionCode(generator, node.rhs);
            if (node.op.Equals(new Operator("+")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Add);
            }
            else if (node.op.Equals(new Operator("*")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Mul);
            }
            else if (node.op.Equals(new Operator("-")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Sub);
            }
            else if (node.op.Equals(new Operator("/")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Div);
            }
            else if (node.op.Equals(new Operator("%")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Rem);
            }
            else if (node.op.Equals(new Operator("==")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Ceq);
            }
            else if (node.op.Equals(new Operator("<")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Clt);
            }
            else if (node.op.Equals(new Operator(">")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Cgt);
            }
            else if (node.op.Equals(new Operator("||")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Or);
            }
            else if (node.op.Equals(new Operator("&&")))
            {
                generator.Emit(System.Reflection.Emit.OpCodes.And);
            }
            else
            {
                throw new Exception("Unknown binary operator \"" + node.op + "\"");
            }
        }
        private void generateExpressionCode(ILGenerator generator, ThisNode node)
        {
            if (currentClassIsMain)
            {
                throw new Exception("Reference to this in main class");
            }
            generator.Emit(System.Reflection.Emit.OpCodes.Ldarg, 0);
        }
        private void generateExpressionCode(ILGenerator generator, UnaryOperatorCall node)
        {
            if (node.op.Equals(new Operator("-")))
            {
                generateExpressionCode(generator, node.lhs);
                generator.Emit(System.Reflection.Emit.OpCodes.Neg);
            }
            else if (node.op.Equals(new Operator("!")))
            {
                //not a <=> 1-a because bools are ints with value 0 or 1
                generator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, 1);
                generateExpressionCode(generator, node.lhs);
                generator.Emit(System.Reflection.Emit.OpCodes.Sub);
            }
            else
            {
                throw new Exception("Unknown unary operator \"" + node.op + "\"");
            }
        }
        private void generateExpressionCode(ILGenerator generator, ArrayReference node)
        {
            generateExpressionCode(generator, node.array);
            generateExpressionCode(generator, node.index);
            generator.Emit(System.Reflection.Emit.OpCodes.Ldelem, getType(((ArrayType)node.array.type).baseType.Type));
        }

        private void generateExpressionCode(ILGenerator generator, NewSingular node)
        {
            generator.Emit(System.Reflection.Emit.OpCodes.Newobj, classes[node.newType.type].constructor);
        }
        private void generateExpressionCode(ILGenerator generator, ObjectMemberReference node)
        {
            generateExpressionCode(generator, node.obj);
            generator.Emit(System.Reflection.Emit.OpCodes.Ldfld, classes[((ClassType)node.obj.type).type].getField(node.member));
        }
        private void generateExpressionCode(ILGenerator generator, LocalOrMemberReference node)
        {
            if (((LocalOrMemberReference)node).isMember)
            {
                //a member variable
                ObjectMemberReference r = new ObjectMemberReference(node);
                r.member = ((LocalOrMemberReference)node).var;
                r.obj = new ThisNode(r);
                r.obj.type = new ClassType(currentClass.classnode, r.obj);
                generateExpressionCode(generator, r);
            }
            else
            {
                //local
                generator.Emit(System.Reflection.Emit.OpCodes.Ldloc, locals[node.var]);
            }
        }
        private void generateExpressionCode(ILGenerator generator, NewArray node)
        {
            generateExpressionCode(generator, node.length);
            generator.Emit(System.Reflection.Emit.OpCodes.Newarr, getType(node.arrayType.Type));
        }
        private void generateExpressionCode(ILGenerator generator, BoolConstant node)
        {
            generator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, node.value ? 1 : 0);
        }
        private void generateExpressionCode(ILGenerator generator, IntConstant node)
        {
            generator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, node.value);
        }
        private void generateExpressionCode(ILGenerator generator, ArrayLengthRead node)
        {
            generateExpressionCode(generator, node.obj);
            generator.Emit(System.Reflection.Emit.OpCodes.Ldlen);
        }
        private void generateExpressionCode(ILGenerator generator, FunctionCall node)
        {
            generateExpressionCode(generator, node.f.obj);
            foreach (ExpressionNode val in node.args)
            {
                generateExpressionCode(generator, val);
            }
            generator.Emit(System.Reflection.Emit.OpCodes.Callvirt, getMethod(node.f));
        }
        private void generateLocalAssignment(ILGenerator generator, LocalOrMemberReference node, ExpressionNode value)
        {
            generateExpressionCode(generator, value);
            generator.Emit(System.Reflection.Emit.OpCodes.Stloc, locals[node.var]);
        }
        private void generateMemberAssignment(ILGenerator generator, ObjectMemberReference node, ExpressionNode value)
        {
            generateExpressionCode(generator, node.obj);
            generateExpressionCode(generator, value);
            generator.Emit(System.Reflection.Emit.OpCodes.Stfld, classes[((ClassType)node.obj.type).type].getField(node.member));
        }
        private void generateArrayAssignment(ILGenerator generator, ArrayReference node, ExpressionNode value)
        {
            generateExpressionCode(generator, node.array);
            generateExpressionCode(generator, node.index);
            generateExpressionCode(generator, value);
            generator.Emit(System.Reflection.Emit.OpCodes.Stelem, types[value.type.Type]);
        }
        private void generateStatementCode(ILGenerator generator, AssignmentNode node)
        {
            if (node.target is ArrayReference)
            {
                generateArrayAssignment(generator, (ArrayReference)node.target, node.value);
            }
            if (node.target is LocalOrMemberReference)
            {
                if (((LocalOrMemberReference)node.target).isMember)
                {
                    ObjectMemberReference r = new ObjectMemberReference(node);
                    r.member = ((LocalOrMemberReference)node.target).var;
                    r.obj = new ThisNode(r);
                    r.obj.type = new ClassType(currentClass.classnode, r.obj);
                    generateMemberAssignment(generator, r, node.value);
                }
                else
                {
                    generateLocalAssignment(generator, (LocalOrMemberReference)node.target, node.value);
                }
            }
            if (node.target is ObjectMemberReference)
            {
                generateMemberAssignment(generator, (ObjectMemberReference)node.target, node.value);
            }
        }
        private void generateStatementCode(ILGenerator generator, ReturnNode node)
        {
            if (node.value == null)
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Ret);
            }
            else
            {
                generateExpressionCode(generator, node.value);
                generator.Emit(System.Reflection.Emit.OpCodes.Ret);
            }
        }
        private void generateStatementCode(ILGenerator generator, AssertNode node)
        {
            Label end = generator.DefineLabel();
            generateExpressionCode(generator, node.value);
            generator.Emit(System.Reflection.Emit.OpCodes.Brtrue, end);
            //todo: throw something useful
            generator.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, 0);
            generator.Emit(System.Reflection.Emit.OpCodes.Throw);
            generator.MarkLabel(end);
        }
        private void generateStatementCode(ILGenerator generator, PrintNode node)
        {
            generateExpressionCode(generator, node.value);
            generator.Emit(System.Reflection.Emit.OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(Int32) }));
        }
        private void generateStatementCode(ILGenerator generator, IfNode node)
        {
            Label elseL = generator.DefineLabel();
            Label end = generator.DefineLabel();
            generateExpressionCode(generator, node.condition);
            if (node.elseNode == null)
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Brfalse, end);
            }
            else
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Brfalse, elseL);
            }
            generateStatementCode(generator, node.thenNode);
            if (node.elseNode != null)
            {
                generator.Emit(System.Reflection.Emit.OpCodes.Br, end);
                generator.MarkLabel(elseL);
                generateStatementCode(generator, node.elseNode);
            }
            generator.MarkLabel(end);
        }
        private void generateStatementCode(ILGenerator generator, WhileNode node)
        {
            Label start = generator.DefineLabel();
            Label end = generator.DefineLabel();
            generator.MarkLabel(start);
            generateExpressionCode(generator, node.condition);
            generator.Emit(System.Reflection.Emit.OpCodes.Brfalse, end);
            generateStatementCode(generator, node.doThis);
            generator.Emit(System.Reflection.Emit.OpCodes.Br, start);
            generator.MarkLabel(end);
        }
    }
}
