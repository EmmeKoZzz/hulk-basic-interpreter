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

public interface IExpression {
	ASTNode Kind { get; }
}

/*
 *  Expressions Types
 */

public struct AST : IExpression {
	public ASTNode Kind => ASTNode.Program;
	public IExpression[] Body = Array.Empty<IExpression>();
	public AST() { }
}

//

public struct BinaryExpression : IExpression {
	public ASTNode Kind => ASTNode.BinaryExpression;
	public IExpression Right { get; }
	public IExpression Left { get; }
	public string Op { get; }

	public BinaryExpression(IExpression left, string op, IExpression right) {
		(Op, Left, Right) = (op, left, right);
	}
}

//

public struct NullLiteral : IExpression {
	public ASTNode Kind => ASTNode.Null;
	public string Value = "null";
	public NullLiteral() { }
}

//

public struct BooleanLiteral : IExpression {
	public ASTNode Kind => ASTNode.Boolean;
	public bool Value;

	public BooleanLiteral(string value) {
		this.Value = value switch {
			"true" => true,
			"false" => false,
			_ => throw new Exception($"{value} can't be be parse into a boolean.")
		};
	}
}

//

public struct Number : IExpression {
	public ASTNode Kind => ASTNode.Number;
	public readonly float Value = 0;

	public Number(string value) {
		if (!float.TryParse(value, out this.Value)) throw new Exception($"{value} isn't a valid number.");
	}
}

//

public struct Text : IExpression {
	public ASTNode Kind => ASTNode.Text;
	public readonly string Value;

	public Text(string text) => this.Value = text;
}

//

public struct Identifier : IExpression {
	public ASTNode Kind => ASTNode.Identifier;
	public string Symbol;

	public Identifier(string sym) => this.Symbol = sym;
}