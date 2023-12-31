namespace HULK_libs;

public enum ASTNode {
	Program,
	Var,
	VarDeclaration,
	FunCall,
	FunDeclaration,
	Number,
	Text,
	Null,
	Boolean,
	BinaryExpression,
	Condition,
	Math,
	Print
}

public enum StmtType {
	Statement,
	Expression
}

public abstract class Expression : AbstractStatement {
	protected Expression(ASTNode kind) : base(StmtType.Expression, kind) { }
}

public abstract class Statement : AbstractStatement {
	protected Statement(ASTNode kind) : base(StmtType.Statement, kind) { }
}

public abstract class AbstractStatement : IStmt {
	public StmtType Type { get; }
	public ASTNode Kind { get; }

	protected AbstractStatement(StmtType type, ASTNode kind) {
		(Type, Kind) = (type, kind);
	}
}

public interface IStmt {
	public StmtType Type { get; }
	public ASTNode Kind { get; }
}
/*
 *  Expressions Types
 */

public class AST : Statement {
	public readonly List<IStmt> Body = new();
	public AST() : base(ASTNode.Program) { }
}

// STATEMENTS

public class VarDeclaration : Statement {
	public readonly string? Sym;
	public readonly Expression Value;

	public VarDeclaration(string? sym, Expression value) : base(ASTNode.VarDeclaration) {
		(Sym, Value) = (sym, value);
	}
}

public class FunDeclaration : Statement {
	public readonly string? Sym;
	public readonly string[] Args;
	public readonly AST Body;

	public FunDeclaration(string? sym, string[] args, AST body) : base(ASTNode.FunDeclaration) {
		(Sym, Args, Body) = (sym, args, body);
	}
}

// EXPRESSIONS

public class BinaryExpression : Expression {
	public Expression Right { get; }
	public Expression Left { get; }
	public string? Op { get; }

	public BinaryExpression(Expression left, string? op, Expression right) : base(ASTNode.BinaryExpression) {
		(Op, Left, Right) = (op, left, right);
	}
}

//

public class NullLiteral : Expression {
	public NullLiteral() : base(ASTNode.Null) { }
}

//

public class BooleanLiteral : Expression {
	public readonly bool Value;

	public BooleanLiteral(string? value) : base(ASTNode.Boolean) {
		Value = value switch {
			"true" => true,
			"false" => false,
			_ => throw new Exception($"{value} can't be be parse into a boolean.")
		};
	}
}

//

public class NumberLiteral : Expression {
	public readonly float Value;

	public NumberLiteral(string? value) : base(ASTNode.Number) {
		if (!float.TryParse(value, out Value)) throw new Exception($"{value} isn't a valid number.");
	}
}

//

public class TextLiteral : Expression {
	public readonly string? Value;

	public TextLiteral(string? text) : base(ASTNode.Text) => Value = text;
}

//

public class VarName : Expression {
	public readonly string? Symbol;

	public VarName(string? sym) : base(ASTNode.Var) => Symbol = sym;
}

// 

public class FunCall : Expression {
	public readonly string? Symbol;
	public readonly Expression[] Args;

	public FunCall(string? symbol, Expression[] args) : base(ASTNode.FunCall) {
		(Symbol, Args) = (symbol, args);
	}
}

// 

public class Condition : Expression {
	public readonly Expression ConditionExpr;
	public readonly IStmt Positive;
	public readonly IStmt Negative;

	public Condition(Expression condition, IStmt positive, IStmt negative) : base(ASTNode.Condition) {
		(ConditionExpr, Positive, Negative) = (condition, positive, negative);
	}
}

/*
 * Built-in Functions
 */

public enum MathFunType {
	Log,
	Cos,
	Sin
}

public class MathFun : Expression {
	public MathFunType FunType;
	public MathFun(MathFunType type) : base(ASTNode.Math) => FunType = type;
}

public class PrintFun: Expression {
	public PrintFun() : base(ASTNode.Print){ }
}