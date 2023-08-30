using System.Linq.Expressions;

namespace HULK_libs;

public enum ASTNode {
	Program,
	Identifier,
	Number,
	Text,
	Null,
	Boolean,
	BinaryExpression,
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

//

public class BinaryExpression : Expression {
	public Expression Right { get; }
	public Expression Left { get; }
	public string Op { get; }

	public BinaryExpression(Expression left, string op, Expression right) : base(ASTNode.BinaryExpression) {
		(Op, Left, Right) = (op, left, right);
	}
}

//

public class NullLiteral : Expression {
	public string Value = "null";
	public NullLiteral() : base(ASTNode.Null) { }
}

//

public class BooleanLiteral : Expression {
	public bool Value;

	public BooleanLiteral(string value) : base(ASTNode.Boolean) {
		this.Value = value switch {
			"true" => true,
			"false" => false,
			_ => throw new Exception($"{value} can't be be parse into a boolean.")
		};
	}
}

//

public class NumberLiteral : Expression {
	public readonly float Value;

	public NumberLiteral(string value) : base(ASTNode.Number) {
		if (!float.TryParse(value, out Value)) throw new Exception($"{value} isn't a valid number.");
	}
}

//

public class TextLiteral : Expression {
	public readonly string Value;

	public TextLiteral(string text) : base(ASTNode.Text) => Value = text;
}

//

public class Identifier : Expression {
	public string Symbol;

	public Identifier(string sym) : base(ASTNode.Identifier) => Symbol = sym;
}