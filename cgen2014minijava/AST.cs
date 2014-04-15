﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace cgen2014minijava
{
    abstract public class ASTNode
    {
        public int position;
        public int line;
        public override bool Equals(Object o)
        {
            return false;
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
        public ProgramNode()
        {
            classes = new List<ClassNode>();
            mainClass = null;
        }
        public List<ClassNode> classes; //contains the main class
        public ClassNode mainClass;
    }
    public class ClassNode : ASTNode
    {
        public ClassNode()
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
        public UnboundClassType(String name)
        {
            this.name = name;
        }
    }
    public class MethodNode : ASTNode
    {
        public MethodNode()
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
        public UnboundMethodName(String name)
        {
            this.name = name;
        }
    }
    public class VariableNode : ASTNode
    {
        public VariableNode()
        {
            name = "";
            type = null;
        }
        public String name;
        public TypeNode type;
    }
    public class UnboundVariableName : VariableNode
    {
        public UnboundVariableName(String name)
        {
            this.name = name;
        }
    }
    abstract public class StatementNode : ASTNode
    {
    }
    public class BlockStatementNode : StatementNode
    {
        public BlockStatementNode()
        {
            locals = new List<VariableNode>();
            statements = new List<StatementNode>();
        }
        public List<VariableNode> locals;
        public List<StatementNode> statements;
    }
    public class WhileNode : StatementNode
    {
        public ExpressionNode condition;
        public StatementNode doThis;
    }
    public class IfNode : StatementNode
    {
        public ExpressionNode condition;
        public StatementNode thenNode;
        public StatementNode elseNode;
    }
    public class AssignmentNode : StatementNode
    {
        public LValue target;
        public ExpressionNode value;
    }
    public class ReturnNode : StatementNode
    {
        public ExpressionNode value;
    }
    public class AssertNode : StatementNode
    {
        public ExpressionNode value;
    }
    public class PrintNode : StatementNode
    {
        public ExpressionNode value;
    }
    abstract public class ExpressionNode : StatementNode
    {
        public TypeNode type;
    }
    public class ThisNode : ExpressionNode
    {
    }
    public class ObjectMethodReference : ExpressionNode
    {
        public ExpressionNode obj;
        public MethodNode method;
    }
    public class IntConstant : ExpressionNode
    {
        public IntConstant(int value)
        {
            this.value = value;
        }
        public int value;
    }
    public class BoolConstant : ExpressionNode
    {
        public BoolConstant(bool value)
        {
            this.value = value;
        }
        public bool value;
    }
    public class FunctionCall : ExpressionNode
    {
        public FunctionCall()
        {
            f = null;
            args = new List<ExpressionNode>();
        }
        public ObjectMethodReference f;
        public List<ExpressionNode> args;
    }
    public class UnaryOperatorCall : ExpressionNode
    {
        public Operator op;
        public ExpressionNode lhs;
    }
    public class BinaryOperatorCall : ExpressionNode
    {
        public Operator op;
        public ExpressionNode lhs;
        public ExpressionNode rhs;
    }
    public class NewNode : ExpressionNode
    {
    }
    public class NewSingular : NewNode
    {
        public TypeNode newType;
    }
    public class NewArray : NewNode
    {
        public TypeNode arrayType;
        public ExpressionNode length;
    }
    abstract public class LValue : ExpressionNode
    {
    }
    public class ObjectMemberReference : LValue
    {
        public ExpressionNode obj;
        public VariableNode member;
    }
    //not actually an lvalue or an object member reference, just put here to make parsing simpler. Semantics shall check that this won't be assigned
    public class ArrayLengthRead : ObjectMemberReference
    {
    }
    public class LocalOrMemberReference : LValue
    {
        public VariableNode var;
    }
    public class ArrayReference : LValue
    {
        public LValue array;
        public ExpressionNode index;
    }
    abstract public class TypeNode : ASTNode
    {
    }
    public class ArrayType : TypeNode
    {
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
        public BaseType(Type type)
        {
            this.type = type;
        }
        public Type type;
    }
    public class ClassType : TypeNode
    {
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
        public ClassType(ClassNode type)
        {
            this.type = type;
        }
        public ClassNode type;
    }
    public class SemanticError : Exception
    {
        public SemanticError(ASTNode node, String msg) : base("" + node.line + ":" + node.position + " Semantic error: " + msg)
        {
        }
    }

    public class ASTParser
    {
        public ASTParser()
        {
        }
        private Dictionary<String, ClassNode> classTable;
        private Stack<Dictionary<String, VariableNode>> variableTable;
        private ClassNode currentClass;
        private MethodNode currentMethod;
        public ProgramNode parse(SyntaxTree tree)
        {
            classTable = new Dictionary<String, ClassNode>();
            variableTable = new Stack<Dictionary<String, VariableNode>>();
            ProgramNode unbound = parseProgram(tree.root);
            bindClassNames(unbound); //bind method return types to classes
            bindNames(unbound); //bind all other names
            return unbound;
        }
        private ClassNode getClass(String name)
        {
            return classTable[name];
        }
        private VariableNode getVariable(String name)
        {
            for (int i = variableTable.Count-1; i >= 0; i--)
            {
                if (variableTable.ElementAt(i).Keys.Contains(name))
                {
                    return variableTable.ElementAt(i)[name];
                }
            }
            throw new Exception("this shouldn't happen: AST get variable name unknown var \"" + name + "\"");
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
            foreach (MethodNode m in node.methods)
            {
                bindClassNames(m);
            }
        }
        private void bindClassNames(MethodNode node)
        {
            if (node.type is ClassType)
            {
                node.type = new ClassType(getClass(((ClassType)node.type).type.name));
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
            if (node.inherits is UnboundClassType)
            {
                node.inherits = getClass(node.inherits.name);
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
                    ((ClassType)node).type = getClass(((ClassType)node).type.name);
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
                if (!((AssignmentNode)node).target.type.Equals(((AssignmentNode)node).value.type))
                {
                    throw new SemanticError(node, "Assignment must have compatible types (" + ((AssignmentNode)node).target.type + " vs " + ((AssignmentNode)node).value.type + ")");
                }
            }
            else if (node is ReturnNode)
            {
                bindNames(((ReturnNode)node).value);
                if (!((ReturnNode)node).value.type.Equals(currentMethod.type))
                {
                    throw new SemanticError(node, "Return type must be same as function's declared type");
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
                node.type = new BaseType(typeof(Int32));
            }
            else if (node is IntConstant)
            {
                node.type = new BaseType(typeof(Int32));
            }
            else if (node is BoolConstant)
            {
                node.type = new BaseType(typeof(Boolean));
            }
            else if (node is ThisNode)
            {
                node.type = new ClassType(currentClass);
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
                throw new SemanticError(node.obj, "Method call must be applied to a class");
            }
            ClassType objType = new ClassType(getClass(((ClassType)node.obj.type).type.name));
            node.obj.type = objType;
            MethodNode n = objType.type.methods.Find(m => m.name == node.method.name);
            if (n == null)
            {
                throw new SemanticError(node.method, "Method not found");
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
                throw new SemanticError(node, "Wrong number of arguments, expected " + node.f.method.arguments.Count);
            }
            node.type = node.f.type;
        }
        private void bindNames(UnaryOperatorCall node)
        {
            bindNames(node.lhs);
            if (node.lhs.type.Equals(new BaseType(typeof(Boolean))))
            {
                if (!node.op.Equals(new Operator("!")))
                {
                    throw new SemanticError(node, "Operator " + node.op.value + " can't be applied to booleans");
                }
                node.type = new BaseType(typeof(Boolean));
            }
            else if (node.lhs.type.Equals(new BaseType(typeof(Int32))))
            {
                if (!node.op.Equals(new Operator("-")) && !node.op.Equals(new Operator("+")))
                {
                    throw new SemanticError(node, "Operator " + node.op.value + " can't be applied to ints");
                }
                node.type = new BaseType(typeof(Int32));
            }
            else
            {
                throw new SemanticError(node, "Operator " + node.op.value + " can't be applied to type");
            }
        }
        private void bindNames(BinaryOperatorCall node)
        {
            bindNames(node.lhs);
            bindNames(node.rhs);
            TypeNode resultType = operatorResultType(node.lhs.type, node.rhs.type, node.op);
            if (resultType == null)
            {
                throw new SemanticError(node, "Operator " + node.op.value + " can't be applied to types");
            }
            node.type = operatorResultType(node.lhs.type, node.rhs.type, node.op);
        }
        private TypeNode operatorResultType(TypeNode lhs, TypeNode rhs, Operator op)
        {
            if (!lhs.Equals(rhs))
            {
                return null;
            }
            if (op.Equals(new Operator("==")))
            {
                return lhs;
            }
            if (!(lhs is BaseType))
            {
                return null;
            }
            if (op.Equals(new Operator("&&")))
            {
                if (lhs.Equals(new BaseType(typeof(Boolean))))
                {
                    return new BaseType(typeof(Boolean));
                }
                return null;
            }
            if (op.Equals(new Operator("||")))
            {
                if (lhs.Equals(new BaseType(typeof(Boolean))))
                {
                    return new BaseType(typeof(Boolean));
                }
                return null;
            }
            // op is one of < > + - * / %
            if (lhs.Equals(new BaseType(typeof(Int32))))
            {
                return new BaseType(typeof(Int32));
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
            node.type = new ArrayType();
            ((ArrayType)node.type).baseType = node.arrayType;
        }
        private void bindNames(ObjectMemberReference node)
        {
            bindNames(node.obj);
            if (!(node.obj.type is ClassType))
            {
                throw new SemanticError(node.obj, "Member access can only be done on classes");
            }
            node.member = getClass(((ClassType)node.obj.type).type.name).members.Find(v => v.name == node.member.name);
            if (node.member == null)
            {
                throw new SemanticError(node.member, "Member " + node.member.name + " not found in class " + ((ClassType)node.obj.type).type.name);
            }
            node.type = node.member.type;
        }
        private void bindNames(LocalOrMemberReference node)
        {
            node.var = getVariable(node.var.name);
            node.type = node.var.type;
        }
        private void bindNames(ArrayReference node)
        {
            bindNames(node.array);
            bindNames(node.index);
            if (!(node.array.type is ArrayType))
            {
                throw new SemanticError(node, "Array reference must be applied to an array");
            }
            if (!node.index.type.Equals(new BaseType(typeof(Int32))))
            {
                throw new SemanticError(node.index, "Array reference index must be an int");
            }
            node.type = ((ArrayType)node.array.type).baseType;
        }
        private ProgramNode parseProgram(SyntaxNode node)
        {
            if (!node.token.Equals(new NonTerminal("prog")))
            {
                throw new Exception("this shouldn't happen: AST parse program not a prog");
            }
            ProgramNode ret = new ProgramNode();
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
            ClassNode ret = new ClassNode();
            ret.name = ((Identifier)node.children[1].token).value;
            classTable.Add(ret.name, ret);
            MethodNode mainMethod = new MethodNode();
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
            ClassNode ret = new ClassNode();
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
            MethodNode ret = new MethodNode();
            ret.type = parseType(node.children[1]);
            ret.name = ((Identifier)node.children[2].token).value;
            parseFormals(ret, node.children[4]);
            ret.statements = parseStatements(node.children[7]);
            return ret;
        }
        private BlockStatementNode parseStatements(SyntaxNode node)
        {
            BlockStatementNode ret = new BlockStatementNode();
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
            VariableNode var = new VariableNode();
            var.type = parseType(node.children[0]);
            var.name = ((Identifier)node.children[1].token).value;
            to.locals.Add(var);
        }
        private LocalOrMemberReference parseLocalOrMemberVariableRef(SyntaxNode node)
        {
            LocalOrMemberReference ret = new LocalOrMemberReference();
            ret.var = new UnboundVariableName(((Identifier)node.token).value);
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
            throw new Exception("this shouldn't happen: AST parse lvalue unknown type " + node.token);
        }
        private ObjectMemberReference parseMemberVariableRef(SyntaxNode node)
        {
            if (node.children[1].token.Equals(new Keyword("length")))
            {
                ArrayLengthRead ret = new ArrayLengthRead();
                ret.obj = parseExpression(node.children[0]);
                return ret;
            }
            else
            {
                ObjectMemberReference ret = new ObjectMemberReference();
                ret.obj = parseExpression(node.children[0]);
                ret.member = new UnboundVariableName(((Identifier)node.children[1].token).value);
                return ret;
            }
        }
        private ArrayReference parseArrayReference(SyntaxNode node)
        {
            ArrayReference ret = new ArrayReference();
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
                return new ThisNode();
            }
            if (node.token is IntLiteral)
            {
                return new IntConstant(((IntLiteral)node.token).value);
            }
            if (node.token is BoolLiteral)
            {
                return new BoolConstant(((BoolLiteral)node.token).value);
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
            NewSingular ret = new NewSingular();
            ClassType t = new ClassType(new UnboundClassType(((Identifier)node.children[0].token).value));
            ret.newType = t;
            return ret;
        }
        private NewArray parseNewArray(SyntaxNode node)
        {
            NewArray ret = new NewArray();
            ret.length = parseExpression(node.children[1]);
            ret.arrayType = parseBaseType(node.children[0]);
            return ret;
        }
        private BinaryOperatorCall parseBinaryOperator(SyntaxNode node)
        {
            BinaryOperatorCall ret = new BinaryOperatorCall();
            ret.lhs = parseExpression(node.children[0]);
            ret.rhs = parseExpression(node.children[1]);
            ret.op = (Operator)node.token;
            return ret;
        }
        private UnaryOperatorCall parseUnaryOperator(SyntaxNode node)
        {
            UnaryOperatorCall ret = new UnaryOperatorCall();
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
            FunctionCall ret = new FunctionCall();
            ret.f = parseObjectMethodReference(node.children[0]);
            for (int i = 1; i < node.children.Count; i++)
            {
                ret.args.Add(parseExpression(node.children[i]));
            }
            return ret;
        }
        private ObjectMethodReference parseObjectMethodReference(SyntaxNode node)
        {
            ObjectMethodReference ret = new ObjectMethodReference();
            if (!node.token.Equals(new Keyword(".")))
            {
                if (node.token is Identifier || node.token.Equals(new Keyword("this")))
                {
                    ret.obj = new ThisNode();
                    ret.method = new UnboundMethodName(((Identifier)node.token).value);
                    return ret;
                }
                else
                {
                    throw new Exception("this shouldn't happen: AST parse method reference not . or identifier or this");
                }
            }
            ret.obj = parseExpression(node.children[0]);
            ret.method = new UnboundMethodName(((Identifier)node.children[1].token).value);
            return ret;
        }
        private AssertNode parseAssert(SyntaxNode node)
        {
            AssertNode ret = new AssertNode();
            ret.value = parseExpression(node.children[2]);
            return ret;
        }
        private WhileNode parseWhile(SyntaxNode node)
        {
            WhileNode ret = new WhileNode();
            ret.condition = parseExpression(node.children[2]);
            ret.doThis = parseStatement(node.children[4]);
            return ret;
        }
        private PrintNode parsePrint(SyntaxNode node)
        {
            PrintNode ret = new PrintNode();
            ret.value = parseExpression(node.children[2]);
            return ret;
        }
        private ReturnNode parseReturn(SyntaxNode node)
        {
            ReturnNode ret = new ReturnNode();
            ret.value = parseExpression(node.children[1]);
            return ret;
        }
        private AssignmentNode parseAssignment(SyntaxNode node)
        {
            AssignmentNode ret = new AssignmentNode();
            ret.target = parseLValue(node.children[0]);
            ret.value = parseExpression(node.children[1].children[1]);
            return ret;
        }
        private IfNode parseIf(SyntaxNode node)
        {
            IfNode ret = new IfNode();
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
            VariableNode newNode = new VariableNode();
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
            VariableNode newNode = new VariableNode();
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
            VariableNode ret = new VariableNode();
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
                ArrayType ret = new ArrayType();
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
                return new BaseType(typeof(Int32));
            }
            if (node.token.Equals(new Keyword("bool")))
            {
                return new BaseType(typeof(Boolean));
            }
            if (node.token.Equals(new Keyword("void")))
            {
                return new BaseType(null);
            }
            if (node.token is Identifier)
            {
                ClassType ret = new ClassType(new UnboundClassType(((Identifier)node.token).value));
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
            return new UnboundClassType(((Identifier)node.children[1].token).value);
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
