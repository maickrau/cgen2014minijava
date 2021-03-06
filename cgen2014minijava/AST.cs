﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace cgen2014minijava
{
    abstract public class ASTNode : Positionable
    {
        protected ASTNode(Positionable t)
        {
            position = t.position;
            line = t.line;
        }
        public int position { get; set; }
        public int line { get; set; }
        public override bool Equals(Object o)
        {
            return ReferenceEquals(this, o);
        }
        public override int GetHashCode()
        {
            return position.GetHashCode() * line.GetHashCode();
        }
        public void debugPrint(int depth)
        {
            for (int i = 0; i < depth; i++)
            {
                System.Console.Write(" ");
            }
            System.Console.WriteLine(this.GetType());
            foreach (FieldInfo f in this.GetType().GetFields())
            {
                object o = f.GetValue(this);
                if (o is ASTNode)
                {
                    ((ASTNode)o).debugPrint(depth + 1);
                }
                if (o is List<ASTNode>)
                {
                    foreach (ASTNode n in (List<ASTNode>)o)
                    {
                        ((ASTNode)n).debugPrint(depth + 1);
                    }
                }
                if (o is List<VariableNode>)
                {
                    foreach (ASTNode n in (List<VariableNode>)o)
                    {
                        ((ASTNode)n).debugPrint(depth + 1);
                    }
                }
                if (o is List<MethodNode>)
                {
                    foreach (ASTNode n in (List<MethodNode>)o)
                    {
                        ((ASTNode)n).debugPrint(depth + 1);
                    }
                }
                if (o is List<StatementNode>)
                {
                    foreach (ASTNode n in (List<StatementNode>)o)
                    {
                        ((ASTNode)n).debugPrint(depth + 1);
                    }
                }
                if (o is List<ExpressionNode>)
                {
                    foreach (ASTNode n in (List<ExpressionNode>)o)
                    {
                        ((ASTNode)n).debugPrint(depth + 1);
                    }
                }
            }
        }
    }
    public class ProgramNode : ASTNode
    {
        public ProgramNode(Positionable t) : base(t)
        {
            classes = new List<ClassNode>();
            mainClass = null;
        }
        public List<ClassNode> classes; //contains the main class
        public ClassNode mainClass;
    }
    public class ClassNode : ASTNode
    {
        public ClassNode(Positionable t) : base(t)
        {
            name = "";
            methods = new List<MethodNode>();
            members = new List<VariableNode>();
            inherits = null;
        }
        public String name;
        public List<MethodNode> methods;
        public List<VariableNode> members;
        public ClassNode inherits;
    }
    public class UnboundClassType : ClassNode
    {
        public UnboundClassType(String name, Positionable t) : base(t)
        {
            this.name = name;
        }
    }
    public class MethodNode : ASTNode
    {
        public MethodNode(Positionable t) : base(t)
        {
            name = "";
            arguments = new List<VariableNode>();
            statements = null;
        }
        public TypeNode type;
        public String name;
        public List<VariableNode> arguments;
        public BlockStatementNode statements;
    }
    public class UnboundMethodName : MethodNode
    {
        public UnboundMethodName(String name, Positionable t) : base(t)
        {
            this.name = name;
        }
    }
    public class VariableNode : ASTNode
    {
        public VariableNode(Positionable t) : base(t)
        {
            name = "";
            type = null;
        }
        public String name;
        public TypeNode type;
    }
    public class UnboundVariableName : VariableNode
    {
        public UnboundVariableName(String name, Positionable t) : base(t)
        {
            this.name = name;
        }
    }
    abstract public class StatementNode : ASTNode
    {
        protected StatementNode(Positionable t) : base(t) { }
    }
    public class BlockStatementNode : StatementNode
    {
        public BlockStatementNode(Positionable t) : base(t)
        {
            locals = new List<VariableNode>();
            statements = new List<StatementNode>();
        }
        public List<VariableNode> locals;
        public List<StatementNode> statements;
    }
    public class WhileNode : StatementNode
    {
        public WhileNode(Positionable t) : base(t) { }
        public ExpressionNode condition;
        public StatementNode doThis;
    }
    public class IfNode : StatementNode
    {
        public IfNode(Positionable t) : base(t) { }
        public ExpressionNode condition;
        public StatementNode thenNode;
        public StatementNode elseNode;
    }
    public class AssignmentNode : StatementNode
    {
        public AssignmentNode(Positionable t) : base(t) { }
        public LValue target;
        public ExpressionNode value;
    }
    public class ReturnNode : StatementNode
    {
        public ReturnNode(Positionable t) : base(t) { }
        public ExpressionNode value;
    }
    public class AssertNode : StatementNode
    {
        public AssertNode(Positionable t) : base(t) { }
        public ExpressionNode value;
    }
    public class PrintNode : StatementNode
    {
        public PrintNode(Positionable t) : base(t) { }
        public ExpressionNode value;
    }
    abstract public class ExpressionNode : StatementNode
    {
        public ExpressionNode(Positionable t) : base(t) { }
        public TypeNode type;
    }
    public class ThisNode : ExpressionNode
    {
        public ThisNode(Positionable t) : base(t) { }
    }
    public class ObjectMethodReference : ExpressionNode
    {
        public ObjectMethodReference(Positionable t) : base(t) { }
        public ExpressionNode obj;
        public MethodNode method;
    }
    public class IntConstant : ExpressionNode
    {
        public IntConstant(int value, Positionable t) : base(t)
        {
            this.value = value;
        }
        public int value;
    }
    public class BoolConstant : ExpressionNode
    {
        public BoolConstant(bool value, Positionable t) : base(t)
        {
            this.value = value;
        }
        public bool value;
    }
    public class FunctionCall : ExpressionNode
    {
        public FunctionCall(Positionable t) : base(t)
        {
            f = null;
            args = new List<ExpressionNode>();
        }
        public ObjectMethodReference f;
        public List<ExpressionNode> args;
    }
    public class UnaryOperatorCall : ExpressionNode
    {
        public UnaryOperatorCall(Positionable t) : base(t) { }
        public Operator op;
        public ExpressionNode lhs;
    }
    public class BinaryOperatorCall : ExpressionNode
    {
        public BinaryOperatorCall(Positionable t) : base(t) { }
        public Operator op;
        public ExpressionNode lhs;
        public ExpressionNode rhs;
    }
    public class NewNode : ExpressionNode
    {
        public NewNode(Positionable t) : base(t) { }
    }
    public class NewSingular : NewNode
    {
        public NewSingular(Positionable t) : base(t) { }
        public ClassType newType;
    }
    public class NewArray : NewNode
    {
        public NewArray(Positionable t) : base(t) { }
        public TypeNode arrayType;
        public ExpressionNode length;
    }
    abstract public class LValue : ExpressionNode
    {
        public LValue(Positionable t) : base(t) { }
    }
    public class ObjectMemberReference : LValue
    {
        public ObjectMemberReference(Positionable t) : base(t) { }
        public ExpressionNode obj;
        public VariableNode member;
    }
    //not actually an lvalue or an object member reference, just put here to make parsing simpler. Semantics shall check that this won't be assigned
    public class ArrayLengthRead : ObjectMemberReference
    {
        public ArrayLengthRead(Positionable t) : base(t) { }
    }
    public class LocalOrMemberReference : LValue
    {
        public LocalOrMemberReference(Positionable t) : base(t) { isMember = false; }
        public bool isMember;
        public VariableNode var;
    }
    public class ArrayReference : LValue
    {
        public ArrayReference(Positionable t) : base(t) { }
        public LValue array;
        public ExpressionNode index;
    }
    abstract public class TypeNode : ASTNode
    {
        public abstract String Type { get; }
        public TypeNode(Positionable t) : base(t) { }
        public TypeNode(ASTNode t) : base(t) { }
    }
    public class ArrayType : TypeNode
    {
        public override String Type { get { return baseType.Type + "[]";  } }
        public ArrayType(Positionable t) : base(t) { }
        public override bool Equals(object o)
        {
            if (!(o is ArrayType))
            {
                return false;
            }
            return baseType.Equals(((ArrayType)o).baseType);
        }
        public override string ToString()
        {
            return "ArrayType(" + baseType.ToString() + ")";
        }
        public TypeNode baseType;
    }
    public class BaseType : TypeNode
    {
        public override String Type { get { if (type != null) return type.ToString(); else return "void"; } }
        public BaseType(Type type, Positionable t) : base(t)
        {
            this.type = type;
        }
        public override bool Equals(object o)
        {
            if (!(o is BaseType))
            {
                return false;
            }
            return type == ((BaseType)o).type;
        }
        public override string ToString()
        {
            return "BaseType(" + type.ToString() + ")";
        }
        public Type type;
    }
    public class ClassType : TypeNode
    {
        public override String Type { get { return type.name; } }
        public ClassType(ClassNode type, Positionable t) : base(t)
        {
            this.type = type;
        }
        public override bool Equals(object o)
        {
            if (!(o is ClassType))
            {
                return false;
            }
            return type.name == ((ClassType)o).type.name;
        }
        public override string ToString()
        {
            return "ClassType(" + type.name + ")";
        }
        public ClassNode type;
    }
    public class SemanticError : Exception
    {
        public SemanticError(String msg) : base(msg)
        {
        }
    }
    public class ExpressionVisitor
    {
        public static void visit(ProgramNode node, Action<ExpressionNode> f, bool postOrder)
        {
            StatementVisitor.visit(node, statementVisitor(f, postOrder), postOrder);
        }
        private static void visit(ExpressionNode n, Action<ExpressionNode> f, bool postOrder)
        {
            if (!postOrder)
            {
                f.DynamicInvoke(n);
            }
            if (n is ObjectMethodReference)
            {
                visit(((ObjectMethodReference)n).obj, f, postOrder);
            }
            else if (n is FunctionCall)
            {
                visit(((FunctionCall)n).f, f, postOrder);
                foreach (ExpressionNode e in ((FunctionCall)n).args)
                {
                    visit(e, f, postOrder);
                }
            }
            else if (n is UnaryOperatorCall)
            {
                visit(((UnaryOperatorCall)n).lhs, f, postOrder);
            }
            else if (n is BinaryOperatorCall)
            {
                visit(((BinaryOperatorCall)n).lhs, f, postOrder);
                visit(((BinaryOperatorCall)n).rhs, f, postOrder);
            }
            else if (n is NewArray)
            {
                visit(((NewArray)n).length, f, postOrder);
            }
            else if (n is ObjectMemberReference)
            {
                visit(((ObjectMemberReference)n).obj, f, postOrder);
            }
            else if (n is ArrayReference)
            {
                visit(((ArrayReference)n).array, f, postOrder);
                visit(((ArrayReference)n).index, f, postOrder);
            }
            if (postOrder)
            {
                f.DynamicInvoke(n);
            }
        }
        private static Action<StatementNode> statementVisitor(Action<ExpressionNode> f, bool postOrder)
        {
            return delegate(StatementNode node)
            {
                if (node is WhileNode)
                {
                    visit(((WhileNode)node).condition, f, postOrder);
                }
                else if (node is IfNode)
                {
                    visit(((IfNode)node).condition, f, postOrder);
                }
                else if (node is AssignmentNode)
                {
                    visit(((AssignmentNode)node).target, f, postOrder);
                    visit(((AssignmentNode)node).value, f, postOrder);
                }
                else if (node is ReturnNode)
                {
                    visit(((ReturnNode)node).value, f, postOrder);
                }
                else if (node is AssertNode)
                {
                    visit(((AssertNode)node).value, f, postOrder);
                }
                else if (node is PrintNode)
                {
                    visit(((PrintNode)node).value, f, postOrder);
                }
                else if (node is ExpressionNode)
                {
                    visit((ExpressionNode)node, f, postOrder);
                }
            };
        }
    }
    public class StatementVisitor
    {
        public static void visit(ProgramNode node, Action<StatementNode> f, bool postOrder)
        {
            foreach (ClassNode c in node.classes)
            {
                foreach (MethodNode m in c.methods)
                {
                    visit(m.statements, f, postOrder);
                }
            }
        }
        private static void visit(StatementNode node, Action<StatementNode> f, bool postOrder)
        {
            if (!postOrder)
            {
                f.DynamicInvoke(node);
            }
            if (node is BlockStatementNode)
            {
                foreach (StatementNode s in ((BlockStatementNode)node).statements)
                {
                    visit(s, f, postOrder);
                }
            }
            else if (node is IfNode)
            {
                visit(((IfNode)node).thenNode, f, postOrder);
                if (((IfNode)node).elseNode != null)
                {
                    visit(((IfNode)node).elseNode, f, postOrder);
                }
            }
            else if (node is WhileNode)
            {
                visit(((WhileNode)node).doThis, f, postOrder);
            }
            if (postOrder)
            {
                f.DynamicInvoke(node);
            }
        }
    }

    public class ASTParser
    {
        public ASTParser()
        {
        }
        private Dictionary<String, ClassNode> classTable;
        private Stack<Dictionary<String, VariableNode>> variableTable;
        private HashSet<String> locals; //for validating local variable name uniqueness
        private ClassNode currentClass;
        private MethodNode currentMethod;
        private List<String> errors;
        private void addError(Positionable position, String message)
        {
            errors.Add("" + position.line + ":" + position.position + " Semantic error: " + message);
        }
        private void breakOnErrors()
        {
            if (errors.Count > 0)
            {
                throw new SemanticError(String.Join(Environment.NewLine, errors));
            }
        }
        public ProgramNode parse(SyntaxTree tree)
        {
            errors = new List<String>();
            classTable = new Dictionary<String, ClassNode>();
            variableTable = new Stack<Dictionary<String, VariableNode>>();
            ProgramNode unbound = parseProgram(tree.root);
            breakOnErrors();
            validateUniqueLocals(unbound); //local variable names must be unique (may shadow member variables)
            breakOnErrors();
            bindClassNames(unbound); //bind method return types to classes
            breakOnErrors();
            bindNames(unbound); //bind all other names
            breakOnErrors();
            validateInheritance(unbound); //sort the classes so derived classes are after base classes
            breakOnErrors();
            StatementVisitor.visit(unbound, delegate(StatementNode s) { if (s is ExpressionNode && !(s is FunctionCall)) addError(s, "Statement expression must be a function call"); }, false);
            breakOnErrors();
            ExpressionVisitor.visit(unbound, this.validateFunctionCall, false);
            breakOnErrors();
            return unbound;
        }
        private bool isSubclass(TypeNode sub, TypeNode super)
        {
            if (sub is BaseType && super is BaseType)
            {
                return ((BaseType)sub).type.Equals(((BaseType)super).type);
            }
            if (sub is ArrayType && super is ArrayType)
            {
                return ((ArrayType)sub).baseType.Equals(((ArrayType)super).baseType);
            }
            if (sub is ClassType && super is ClassType)
            {
                if (((ClassType)sub).type.Equals(((ClassType)super).type))
                {
                    return true;
                }
                if (((ClassType)sub).type.inherits != null)
                {
                    //a subclass may be assigned to a superclass
                    return isSubclass(new ClassType(((ClassType)sub).type.inherits, super), super);
                }
            }
            return false;
        }
        private void validateFunctionCall(ExpressionNode node)
        {
            if (node is FunctionCall)
            {
                FunctionCall expr = (FunctionCall)node;
                if (expr.args.Count != expr.f.method.arguments.Count)
                {
                    addError(node, "Function call has wrong number of arguments, expected " + expr.f.method.arguments.Count + ", received " + expr.args.Count);
                }
                for (int i = 0; i < expr.args.Count; i++)
                {
                    if (!isSubclass(expr.args[i].type, expr.f.method.arguments[i].type))
                    {
                        addError(expr.args[i], "Function call types are not compatible, expected " + expr.f.method.arguments[i].type + ", received " + expr.args[i].type);
                    }
                }
            }
        }
        private MethodNode findMethod(ClassNode node, String methodName, ASTNode calledFrom)
        {
            MethodNode found = node.methods.Find(x => x.name == methodName);
            if (found != null)
            {
                return found;
            }
            if (node.inherits != null)
            {
                return findMethod(node.inherits, methodName, calledFrom);
            }
            return null;
        }
        private void validateUniqueLocals(ProgramNode prog)
        {
            foreach (ClassNode c in prog.classes)
            {
                foreach (MethodNode m in c.methods)
                {
                    validateUniqueLocals(m);
                }
            }
        }
        private void validateUniqueLocals(MethodNode m)
        {
            locals = new HashSet<String>();
            foreach (VariableNode arg in m.arguments)
            {
                if (locals.Contains(arg.name))
                {
                    addError(arg, "Local variable already declared");
                }
                locals.Add(arg.name);
            }
            validateUniqueLocals(m.statements);
        }
        private void validateUniqueLocals(BlockStatementNode b)
        {
            foreach (VariableNode local in b.locals)
            {
                if (locals.Contains(local.name))
                {
                    addError(local, "Local variable already declared");
                }
                locals.Add(local.name);
            }
            foreach(StatementNode s in b.statements)
            {
                validateUniqueLocals(s);
            }
        }
        private void validateUniqueLocals(StatementNode s)
        {
            if (s == null)
            {
                return;
            }
            if (s is BlockStatementNode)
            {
                validateUniqueLocals((BlockStatementNode)s);
            }
            else if (s is IfNode)
            {
                validateUniqueLocals(((IfNode)s).thenNode);
                validateUniqueLocals(((IfNode)s).elseNode);
            }
            else if (s is WhileNode)
            {
                validateUniqueLocals(((WhileNode)s).doThis);
            }
        }
        private void validateInheritance(ProgramNode prog)
        {
            //http://en.wikipedia.org/wiki/Topological_sorting
            List<ClassNode> sorted = new List<ClassNode>();
            Queue<ClassNode> eligible = new Queue<ClassNode>(); //base class has been handled
            List<ClassNode> rest = new List<ClassNode>(); //base class hasn't been handled
            foreach (ClassNode c in prog.classes)
            {
                if (c.inherits == null)
                {
                    eligible.Enqueue(c);
                }
                else
                {
                    rest.Add(c);
                }
            }
            while (eligible.Count > 0)
            {
                ClassNode picked = eligible.Dequeue();
                sorted.Add(picked);
                var derived = rest.Where(n => n.inherits == picked);
                foreach (ClassNode n in derived)
                {
                    eligible.Enqueue(n);
                }
                rest.RemoveAll(n => derived.Contains(n));
            }
            if (rest.Count > 0)
            {
                addError(prog, "Inheritance cycle (" + String.Join(", ", rest.Select(n => n.name)) + ")");
            }
            prog.classes = sorted;
        }
        private ClassNode getClass(ASTNode caller, String name)
        {
            if (!classTable.Keys.Contains(name))
            {
                return null;
            }
            return classTable[name];
        }
        private VariableNode getVariable(String name)
        {
            for (int i = 0; i < variableTable.Count; i++)
            {
                if (variableTable.ElementAt(i).Keys.Contains(name))
                {
                    return variableTable.ElementAt(i)[name];
                }
            }
            return null;
        }
        private bool variableIsLocal(String name)
        {
            if (variableTable.Count < 2)
            {
                throw new Exception("This shouldn't happen: Variable localness check variable table stack size " + variableTable.Count);
            }
            for (int i = 0; i < variableTable.Count-1; i++)
            {
                if (variableTable.ElementAt(i).Keys.Contains(name))
                {
                    return true;
                }
            }
            if (variableTable.ElementAt(variableTable.Count-1).Keys.Contains(name))
            {
                return false;
            }
            throw new Exception("This shouldn't happen: Variable localness check variable not found");
        }
        private void bindClassNames(ProgramNode node)
        {
            foreach (ClassNode c in node.classes)
            {
                bindClassNames(c);
            }
        }
        private void bindClassNames(ClassNode node)
        {
            if (node.inherits != null)
            {
                ClassNode inherits = getClass(node.inherits, node.inherits.name);
                if (inherits == null)
                {
                    addError(node.inherits, "Class name not found: \"" + node.inherits.name + "\"");
                }
                node.inherits = inherits;
            }
            foreach (MethodNode m in node.methods)
            {
                bindClassNames(m);
            }
        }
        private void bindClassNames(MethodNode node)
        {
            if (node.type is ClassType)
            {
                ClassNode type = getClass(node, ((ClassType)node.type).type.name);
                if (type == null)
                {
                    addError(node.type, "Class not found");
                    return;
                }
                node.type = new ClassType(type, node);
            }
        }
        private void bindNames(ProgramNode node)
        {
            foreach (ClassNode c in node.classes)
            {
                bindNames(c);
            }
        }
        private void bindNames(ClassNode node)
        {
            currentClass = node;
            if (variableTable.Count != 0)
            {
                throw new Exception("This shouldn't happen: Variable table stack size is " + variableTable.Count + " when starting to bind names in class");
            }
            variableTable.Push(new Dictionary<String, VariableNode>());
            foreach (VariableNode v in node.members)
            {
                bindNames(v);
                variableTable.Peek().Add(v.name, v);
            }
            foreach (MethodNode m in node.methods)
            {
                bindNames(m);
            }
            variableTable.Pop();
        }
        private void bindNames(MethodNode node)
        {
            if (variableTable.Count != 1)
            {
                throw new Exception("This shouldn't happen: Variable table stack size is " + variableTable.Count + " when starting to bind names in method");
            }
            currentMethod = node;
            bindNames(node.type);
            variableTable.Push(new Dictionary<String, VariableNode>());
            foreach (VariableNode v in node.arguments)
            {
                bindNames(v);
                variableTable.Peek().Add(v.name, v);
            }
            bindNames(node.statements);
            variableTable.Pop();
        }
        private void bindNames(BlockStatementNode node)
        {
            if (variableTable.Count < 2)
            {
                throw new Exception("This shouldn't happen: Variable table stack size is " + variableTable.Count + " when starting to bind names in block statement");
            }
            variableTable.Push(new Dictionary<String, VariableNode>());
            foreach (VariableNode v in node.locals)
            {
                bindNames(v);
                variableTable.Peek().Add(v.name, v);
            }
            foreach (StatementNode m in node.statements)
            {
                bindNames(m);
            }
            variableTable.Pop();
        }
        private void bindNames(VariableNode node)
        {
            bindNames(node.type);
        }
        private void bindNames(TypeNode node)
        {
            if (node is ArrayType)
            {
                bindNames(((ArrayType)node).baseType);
            }
            if (node is ClassType)
            {
                if (((ClassType)node).type is UnboundClassType)
                {
                    ClassNode type = getClass(node, ((ClassType)node).type.name);
                    if (type == null)
                    {
                        addError(((ClassType)node).type, "Class not found");
                        return;
                    }
                    ((ClassType)node).type = type;
                }
            }
        }
        private void bindNames(StatementNode node)
        {
            if (node == null)
            {
                return;
            }
            if (node is BlockStatementNode)
            {
                bindNames((BlockStatementNode)node);
            }
            else if (node is WhileNode)
            {
                bindNames(((WhileNode)node).condition);
                bindNames(((WhileNode)node).doThis);
            }
            else if (node is IfNode)
            {
                bindNames(((IfNode)node).condition);
                bindNames(((IfNode)node).thenNode);
                bindNames(((IfNode)node).elseNode);
            }
            else if (node is AssignmentNode)
            {
                bindNames((ExpressionNode)((AssignmentNode)node).target);
                bindNames((ExpressionNode)((AssignmentNode)node).value);
                if (!isSubclass(((AssignmentNode)node).value.type, ((AssignmentNode)node).target.type))
                {
                    addError(node, "Assignment must have compatible types (" + ((AssignmentNode)node).target.type + " vs " + ((AssignmentNode)node).value.type + ")");
                }
            }
            else if (node is ReturnNode)
            {
                bindNames(((ReturnNode)node).value);
                if (!isSubclass(((ReturnNode)node).value.type, currentMethod.type))
                {
                    addError(node, "Return type must be compatible with the function's declared type");
                }
            }
            else if (node is AssertNode)
            {
                bindNames(((AssertNode)node).value);
            }
            else if (node is PrintNode)
            {
                bindNames(((PrintNode)node).value);
            }
            else if (node is ExpressionNode)
            {
                bindNames((ExpressionNode)node);
            }
        }
        private void bindNames(ExpressionNode node)
        {
            if (node is ArrayLengthRead)
            {
                node.type = new BaseType(typeof(Int32), node);
            }
            else if (node is IntConstant)
            {
                node.type = new BaseType(typeof(Int32), node);
            }
            else if (node is BoolConstant)
            {
                node.type = new BaseType(typeof(Boolean), node);
            }
            else if (node is ThisNode)
            {
                ClassNode type = getClass(node, currentClass.name);
                if (type == null)
                {
                    addError(currentClass, "Class not found");
                }
                node.type = new ClassType(type, node);
            }
            else if (node is ObjectMethodReference)
            {
                bindNames((ObjectMethodReference)node);
            }
            else if (node is FunctionCall)
            {
                bindNames((FunctionCall)node);
            }
            else if (node is UnaryOperatorCall)
            {
                bindNames((UnaryOperatorCall)node);
            }
            else if (node is BinaryOperatorCall)
            {
                bindNames((BinaryOperatorCall)node);
            }
            else if (node is NewSingular)
            {
                bindNames((NewSingular)node);
            }
            else if (node is NewArray)
            {
                bindNames((NewArray)node);
            }
            else if (node is ObjectMemberReference)
            {
                bindNames((ObjectMemberReference)node);
            }
            else if (node is ArrayLengthRead)
            {
                bindNames((ArrayLengthRead)node);
            }
            else if (node is LocalOrMemberReference)
            {
                bindNames((LocalOrMemberReference)node);
            }
            else if (node is ArrayReference)
            {
                bindNames((ArrayReference)node);
            }
        }
        private void bindNames(ObjectMethodReference node)
        {
            bindNames(node.obj);
            if (!(node.obj.type is ClassType))
            {
                addError(node.obj, "Method call must be applied to a class");
            }
            ClassNode type = getClass(node.obj, ((ClassType)node.obj.type).type.name);
            if (type == null)
            {
                addError(((ClassType)node.obj.type).type, "Class not found");
                return;
            }
            ClassType objType = new ClassType(type, node.obj);
            node.obj.type = objType;
            MethodNode n = findMethod(objType.type, node.method.name, node);
            if (n == null)
            {
                addError(node.method, "Method not found");
                return;
            }
            node.method = n;
            node.type = node.method.type;
        }
        private void bindNames(FunctionCall node)
        {
            bindNames(node.f);
            foreach (ExpressionNode e in node.args)
            {
                bindNames(e);
            }
            if (node.args.Count != node.f.method.arguments.Count)
            {
                addError(node, "Wrong number of arguments, expected " + node.f.method.arguments.Count);
            }
            node.type = node.f.type;
        }
        private void bindNames(UnaryOperatorCall node)
        {
            bindNames(node.lhs);
            if (node.lhs.type.Equals(new BaseType(typeof(Boolean), node)))
            {
                if (!node.op.Equals(new Operator("!")))
                {
                    addError(node, "Operator " + node.op.value + " can't be applied to booleans");
                }
                node.type = new BaseType(typeof(Boolean), node);
            }
            else if (node.lhs.type.Equals(new BaseType(typeof(Int32), node)))
            {
                if (!node.op.Equals(new Operator("-")) && !node.op.Equals(new Operator("+")))
                {
                    addError(node, "Operator " + node.op.value + " can't be applied to ints");
                }
                node.type = new BaseType(typeof(Int32), node);
            }
            else
            {
                addError(node, "Operator " + node.op.value + " can't be applied to type");
            }
        }
        private void bindNames(BinaryOperatorCall node)
        {
            bindNames(node.lhs);
            bindNames(node.rhs);
            TypeNode resultType = operatorResultType(node.lhs.type, node.rhs.type, node.op);
            if (resultType == null)
            {
                addError(node, "Operator " + node.op.value + " can't be applied to types");
            }
            node.type = operatorResultType(node.lhs.type, node.rhs.type, node.op);
        }
        private TypeNode operatorResultType(TypeNode lhs, TypeNode rhs, Operator op)
        {
            if (op.Equals(new Operator("==")))
            {
                if (lhs.Equals(rhs))
                {
                    return new BaseType(typeof(Boolean), op); ;
                }
                if (isSubclass(lhs, rhs))
                {
                    return new BaseType(typeof(Boolean), op); ;
                }
                if (isSubclass(rhs, lhs))
                {
                    return new BaseType(typeof(Boolean), op); ;
                }
                return null;
            }
            if (!lhs.Equals(rhs))
            {
                return null;
            }
            if (!(lhs is BaseType))
            {
                return null;
            }
            if (op.Equals(new Operator("&&")))
            {
                if (lhs.Equals(new BaseType(typeof(Boolean), op)))
                {
                    return new BaseType(typeof(Boolean), op);
                }
                return null;
            }
            if (op.Equals(new Operator("||")))
            {
                if (lhs.Equals(new BaseType(typeof(Boolean), op)))
                {
                    return new BaseType(typeof(Boolean), op);
                }
                return null;
            }
            // op is one of < > + - * / %
            if (lhs.Equals(new BaseType(typeof(Int32), op)))
            {
                return new BaseType(typeof(Int32), op);
            }
            return null;
        }
        private void bindNames(NewSingular node)
        {
            bindNames(node.newType);
            node.type = node.newType;
        }
        private void bindNames(NewArray node)
        {
            bindNames(node.arrayType);
            node.type = new ArrayType(node);
            ((ArrayType)node.type).baseType = node.arrayType;
        }
        private void bindNames(ObjectMemberReference node)
        {
            bindNames(node.obj);
            if (!(node.obj.type is ClassType))
            {
                addError(node.obj, "Member access can only be done on classes");
            }
            node.member = getClass(node.obj, ((ClassType)node.obj.type).type.name).members.Find(v => v.name == node.member.name);
            if (node.member == null)
            {
                addError(node.member, "Member " + node.member.name + " not found in class " + ((ClassType)node.obj.type).type.name);
            }
            node.type = node.member.type;
        }
        private void bindNames(LocalOrMemberReference node)
        {
            node.var = getVariable(node.var.name);
            if (node.var == null)
            {
                addError(node, "Variable not found");
                return;
            }
            node.isMember = !variableIsLocal(node.var.name);
            node.type = node.var.type;
        }
        private void bindNames(ArrayReference node)
        {
            bindNames(node.array);
            bindNames(node.index);
            if (!(node.array.type is ArrayType))
            {
                addError(node, "Array reference must be applied to an array");
            }
            if (!node.index.type.Equals(new BaseType(typeof(Int32), node.index)))
            {
                addError(node.index, "Array reference index must be an int");
            }
            node.type = ((ArrayType)node.array.type).baseType;
        }
        private ProgramNode parseProgram(SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("prog")))
            {
                throw new Exception("this shouldn't happen: AST parse program not a prog");
            }
            ProgramNode ret = new ProgramNode(node);
            ret.mainClass = parseMainClass(node.children[0]);
            ret.classes.Add(ret.mainClass);
            parseClassDecls(ret, node.children[1]);
            return ret;
        }
        private ClassNode parseMainClass(SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("main class")))
            {
                throw new Exception("this shouldn't happen: AST parse main class not main class");
            }
            ClassNode ret = new ClassNode(node);
            ret.name = ((Identifier)node.children[1].token).value;
            classTable.Add(ret.name, ret);
            MethodNode mainMethod = new MethodNode(node);
            mainMethod.type = new BaseType(null, mainMethod);
            mainMethod.name = "main";
            mainMethod.statements = parseStatements(node.children[7]);
            ret.methods.Add(mainMethod);
            return ret;
        }
        private ClassNode parseClassDecl(SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("class decl")))
            {
                throw new Exception("this shouldn't happen: AST parse class decl not a class decl");
            }
            ClassNode ret = new ClassNode(node);
            ret.name = ((Identifier)node.children[1].token).value;
            classTable.Add(ret.name, ret);
            ret.inherits = parseInheritance(ret, node.children[2]);
            parseDecls(ret, node.children[4]);
            return ret;
        }
        private void parseDecls(ClassNode to, SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("decls")))
            {
                throw new Exception("this shouldn't happen: AST parse decls not decls");
            }
            if (node.children.Count != 2)
            {
                return;
            }
            parseDecl(to, node.children[0]);
            parseDecls(to, node.children[1]);
        }
        private void parseDecl(ClassNode to, SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("decl")))
            {
                throw new Exception("this shouldn't happen: AST parse decl not decl");
            }
            if (node.children.Count != 1)
            {
                throw new Exception("this shouldn't happen: AST decl children != 1");
            }
            if (node.children[0].token.Equals(new NonTerminal("variable decl")))
            {
                to.members.Add(parseVariableDecl(node.children[0]));
            }
            else if (node.children[0].token.Equals(new NonTerminal("method decl")))
            {
                to.methods.Add(parseMethod(node.children[0]));
            }
            else
            {
                throw new Exception("this shouldn't happen: AST parse decl child not variable or method");
            }
        }
        private MethodNode parseMethod(SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("method decl")))
            {
                throw new Exception("this shouldn't happen: AST parse method not method decl");
            }
            if (node.children.Count != 9)
            {
                throw new Exception("this shouldn't happen: AST parse method children != 9");
            }
            MethodNode ret = new MethodNode(node);
            ret.type = parseType(node.children[1]);
            ret.name = ((Identifier)node.children[2].token).value;
            parseFormals(ret, node.children[4]);
            ret.statements = parseStatements(node.children[7]);
            return ret;
        }
        private BlockStatementNode parseStatements(SyntaxNode node)
        {
            BlockStatementNode ret = new BlockStatementNode(node.token);
            parseStatements(ret, node);
            return ret;
        }
        private void parseStatements(BlockStatementNode to, SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("statements")))
            {
                throw new Exception("this shouldn't happen: AST parse statements not statements");
            }
            if (node.children.Count != 2)
            {
                return;
            }
            parseStatement(to, node.children[0]);
            parseStatements(to, node.children[1]);
        }
        private void parseStatement(BlockStatementNode to, SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("statement")))
            {
                throw new Exception("this shouldn't happen: AST parse statement not statement");
            }
            if (node.children.Count == 0)
            {
                throw new Exception("this shouldn't happen: AST parse statement children == 0");
            }
            if (node.children[0].token.Equals(new NonTerminal("variable decl")))
            {
                parseLocalVariableDecl(to, node.children[0]);
            }
            else
            {
                to.statements.Add(parseStatement(node));
            }
        }
        private void parseLocalVariableDecl(BlockStatementNode to, SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("variable decl")))
            {
                throw new Exception("this shouldn't happen: AST parse local variable not variable decl");
            }
            VariableNode var = new VariableNode(node);
            var.type = parseType(node.children[0]);
            var.name = ((Identifier)node.children[1].token).value;
            to.locals.Add(var);
        }
        private LocalOrMemberReference parseLocalOrMemberVariableRef(SyntaxNode node)
        {
            LocalOrMemberReference ret = new LocalOrMemberReference(node);
            ret.var = new UnboundVariableName(((Identifier)node.token).value, node);
            return ret;
        }
        private LValue parseLValue(SyntaxNode node)
        {
            while (node.token.Equals(new NonTerminal("expr")))
            {
                node = node.children[0];
            }
            if (node.token is Identifier)
            {
                return parseLocalOrMemberVariableRef(node);
            }
            if (node.token.Equals(new Keyword("[")))
            {
                return parseArrayReference(node);
            }
            if (node.token.Equals(new Keyword(".")))
            {
                return parseMemberVariableRef(node);
            }
            addError(node, "The target of an assignment must be an lvalue");
            return null;
        }
        private ObjectMemberReference parseMemberVariableRef(SyntaxNode node)
        {
            if (node.children[1].token.Equals(new Keyword("length")))
            {
                ArrayLengthRead ret = new ArrayLengthRead(node);
                ret.obj = parseExpression(node.children[0]);
                return ret;
            }
            else
            {
                ObjectMemberReference ret = new ObjectMemberReference(node);
                ret.obj = parseExpression(node.children[0]);
                ret.member = new UnboundVariableName(((Identifier)node.children[1].token).value, node.children[1]);
                if (!(ret.obj is ThisNode))
                {
                    //this is silly, but the specs said all members are private
                    addError(node, "Variable " + ((Identifier)node.children[1].token).value + " is private");
                }
                return ret;
            }
        }
        private ArrayReference parseArrayReference(SyntaxNode node)
        {
            ArrayReference ret = new ArrayReference(node);
            ret.array = parseLValue(node.children[0]);
            ret.index = parseExpression(node.children[1]);
            return ret;
        }
        private ExpressionNode parseExpression(SyntaxNode node)
        {
            while (node.token.Equals(new NonTerminal("expr")))
            {
                node = node.children[0];
            }
            if (node.token is Operator)
            {
                if (node.children.Count == 2)
                {
                    return parseBinaryOperator(node);
                }
                else if (node.children.Count == 1)
                {
                    return parseUnaryOperator(node);
                }
                throw new Exception("this shouldn't happen: AST operator with " + node.children.Count + " children");
            }
            if (node.token.Equals(new Keyword("(")))
            {
                return parseMethodCall(node);
            }
            if (node.token.Equals(new Keyword(".")))
            {
                return parseMemberVariableRef(node);
            }
            if (node.token.Equals(new Keyword("[")))
            {
                return parseArrayReference(node);
            }
            if (node.token.Equals(new Keyword("this")))
            {
                return new ThisNode(node);
            }
            if (node.token is IntLiteral)
            {
                return new IntConstant(((IntLiteral)node.token).value, node);
            }
            if (node.token is BoolLiteral)
            {
                return new BoolConstant(((BoolLiteral)node.token).value, node);
            }
            if (node.token is Identifier)
            {
                return parseLocalOrMemberVariableRef(node);
            }
            if (node.token.Equals(new Keyword("new")))
            {
                return parseNew(node);
            }
            throw new Exception("this shouldn't happen: AST parse expression unknown type " + node.token);
        }
        private NewNode parseNew(SyntaxNode node)
        {
            if (node.children[0].token.Equals(new Keyword("[")))
            {
                return parseNewArray(node.children[0]);
            }
            else if (node.children[0].token.Equals(new Keyword("(")))
            {
                return parseNewSingular(node.children[0]);
            }
            throw new Exception("this shouldn't happen: AST parse new unknown thing " + node.token);
        }
        private NewSingular parseNewSingular(SyntaxNode node)
        {
            NewSingular ret = new NewSingular(node);
            ClassType t = new ClassType(new UnboundClassType(((Identifier)node.children[0].token).value, node.children[0]), node);
            ret.newType = t;
            return ret;
        }
        private NewArray parseNewArray(SyntaxNode node)
        {
            NewArray ret = new NewArray(node);
            ret.length = parseExpression(node.children[1]);
            ret.arrayType = parseBaseType(node.children[0]);
            return ret;
        }
        private BinaryOperatorCall parseBinaryOperator(SyntaxNode node)
        {
            BinaryOperatorCall ret = new BinaryOperatorCall(node);
            ret.lhs = parseExpression(node.children[0]);
            ret.rhs = parseExpression(node.children[1]);
            ret.op = (Operator)node.token;
            return ret;
        }
        private UnaryOperatorCall parseUnaryOperator(SyntaxNode node)
        {
            UnaryOperatorCall ret = new UnaryOperatorCall(node);
            ret.lhs = parseExpression(node.children[0]);
            ret.op = (Operator)node.token;
            return ret;
        }
        private FunctionCall parseMethodCall(SyntaxNode node)
        {
            if (!node.token.Equals(new Keyword("(")))
            {
                throw new Exception("this shouldn't happen: AST parse method call not method call, " + node.token);
            }
            if (node.children.Count == 0)
            {
                throw new Exception("this shouldn't happen: AST parse method call children == 0");
            }
            FunctionCall ret = new FunctionCall(node);
            ret.f = parseObjectMethodReference(node.children[0]);
            for (int i = 1; i < node.children.Count; i++)
            {
                ret.args.Add(parseExpression(node.children[i]));
            }
            return ret;
        }
        private ObjectMethodReference parseObjectMethodReference(SyntaxNode node)
        {
            ObjectMethodReference ret = new ObjectMethodReference(node);
            if (!node.token.Equals(new Keyword(".")))
            {
                if (node.token is Identifier)
                {
                    ret.obj = new ThisNode(node);
                    ret.method = new UnboundMethodName(((Identifier)node.token).value, node);
                    return ret;
                }
                else
                {
                    throw new Exception("this shouldn't happen: AST parse method reference not . or identifier or this");
                }
            }
            ret.obj = parseExpression(node.children[0]);
            ret.method = new UnboundMethodName(((Identifier)node.children[1].token).value, node.children[1]);
            return ret;
        }
        private AssertNode parseAssert(SyntaxNode node)
        {
            AssertNode ret = new AssertNode(node);
            ret.value = parseExpression(node.children[2]);
            return ret;
        }
        private WhileNode parseWhile(SyntaxNode node)
        {
            WhileNode ret = new WhileNode(node);
            ret.condition = parseExpression(node.children[2]);
            ret.doThis = parseStatement(node.children[4]);
            return ret;
        }
        private PrintNode parsePrint(SyntaxNode node)
        {
            PrintNode ret = new PrintNode(node);
            ret.value = parseExpression(node.children[2]);
            return ret;
        }
        private ReturnNode parseReturn(SyntaxNode node)
        {
            ReturnNode ret = new ReturnNode(node);
            ret.value = parseExpression(node.children[1]);
            return ret;
        }
        private AssignmentNode parseAssignment(SyntaxNode node)
        {
            AssignmentNode ret = new AssignmentNode(node);
            ret.target = parseLValue(node.children[0]);
            ret.value = parseExpression(node.children[1].children[1]);
            return ret;
        }
        private IfNode parseIf(SyntaxNode node)
        {
            IfNode ret = new IfNode(node);
            ret.condition = parseExpression(node.children[2]);
            ret.thenNode = parseStatement(node.children[4]);
            if (node.children.Count == 6)
            {
                ret.elseNode = parseStatement(node.children[5]);
            }
            return ret;
        }
        private StatementNode parseStatement(SyntaxNode node)
        {
            StatementNode ret = null;
            if (node.children[0].token.Equals(new Keyword("assert")))
            {
                ret = parseAssert(node);
            }
            else if (node.children[0].token.Equals(new Keyword("{")))
            {
                ret = parseStatements(node.children[1]);
            }
            else if (node.children[0].token.Equals(new NonTerminal("if statement")))
            {
                ret = parseIf(node.children[0]);
            }
            else if (node.children[0].token.Equals(new Keyword("while")))
            {
                ret = parseWhile(node);
            }
            else if (node.children[0].token.Equals(new Keyword("System.out.println")))
            {
                ret = parsePrint(node);
            }
            else if (node.children[0].token.Equals(new Keyword("return")))
            {
                ret = parseReturn(node);
            }
            else if (node.children[0].token.Equals(new NonTerminal("expr")))
            {
                if (node.children[1].children[0].token.Equals(new Keyword("=")))
                {
                    ret = parseAssignment(node);
                }
                else if (node.children[1].children[0].token.Equals(new Keyword(";")))
                {
                    ret = parseMethodCall(node.children[0].children[0]);
                }
            }
            if (ret == null)
            {
                throw new Exception("this shouldn't happen: AST parse statement unknown statement");
            }
            return ret;
        }
        private void parseFormals(MethodNode to, SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("formals")))
            {
                throw new Exception("this shouldn't happen: AST parse formals not formals, " + node.token);
            }
            if (node.children.Count != 2)
            {
                return;
            }
            VariableNode newNode = new VariableNode(node);
            newNode.type = parseType(node.children[0].children[0]);
            newNode.name = ((Identifier)node.children[0].children[1].token).value;
            to.arguments.Add(newNode);
            parseMoreFormals(to, node.children[1]);
        }
        private void parseMoreFormals(MethodNode to, SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("more formals")))
            {
                throw new Exception("this shouldn't happen: AST parse more formals not more formals, " + node.token);
            }
            if (node.children.Count != 3)
            {
                return;
            }
            VariableNode newNode = new VariableNode(node);
            newNode.type = parseType(node.children[1].children[0]);
            newNode.name = ((Identifier)node.children[1].children[1].token).value;
            to.arguments.Add(newNode);
            parseMoreFormals(to, node.children[2]);
        }
        private VariableNode parseVariableDecl(SyntaxNode node)
        {
            if (node.children.Count != 3)
            {
                throw new Exception("this shouldn't happen: AST parse variable decl children != 3");
            }
            VariableNode ret = new VariableNode(node);
            ret.type = parseType(node.children[0]);
            ret.name = ((Identifier)node.children[1].token).value;
            return ret;
        }
        private TypeNode parseType(SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("type")))
            {
                throw new Exception("this shouldn't happen: AST parse type not type " + node.token);
            }
            if (node.children.Count != 2)
            {
                throw new Exception("this shouldn't happen: AST parse type children != 2");
            }
            if (node.children[1].children.Count != 0)
            {
                ArrayType ret = new ArrayType(node);
                ret.baseType = parseBaseType(node.children[0]);
                return ret;
            }
            return parseBaseType(node.children[0]);
        }
        private TypeNode parseBaseType(SyntaxNode node)
        {
            if (node.token.Equals(new NonTerminal("base type")))
            {
                node = node.children[0];
            }
            if (node.token.Equals(new Keyword("int")))
            {
                return new BaseType(typeof(Int32), node);
            }
            if (node.token.Equals(new Keyword("bool")))
            {
                return new BaseType(typeof(Boolean), node);
            }
            if (node.token.Equals(new Keyword("void")))
            {
                return new BaseType(null, node);
            }
            if (node.token is Identifier)
            {
                ClassType ret = new ClassType(new UnboundClassType(((Identifier)node.token).value, node), node);
                return ret;
            }
            throw new Exception("this shouldn't happen: AST parse base type unknown type");
        }
        private ClassNode parseInheritance(ClassNode to, SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("maybe extends")))
            {
                throw new Exception("this shouldn't happen: AST parse inheritance not maybe extends");
            }
            if (node.children.Count != 2)
            {
                return null;
            }
            return new UnboundClassType(((Identifier)node.children[1].token).value, node.children[1]);
        }
        private void parseClassDecls(ProgramNode to, SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("class decls")))
            {
                return;
            }
            if (node.children.Count == 2)
            {
                to.classes.Add(parseClassDecl(node.children[0]));
                parseClassDecls(to, node.children[1]);
            }
        }
    }
}
