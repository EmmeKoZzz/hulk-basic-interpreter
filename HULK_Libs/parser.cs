namespace HULK_libs;
using static HULK_libs.Lexer;

public class Parser {
	private readonly AST _ast = new();
	private readonly Token[] _tokens;

	/*
	 * Constructor
	 */

	private Parser(string src) {
		_tokens = Tokenize(src);
		while (At().Key != TokenType.EOE) _ast.Body.Add(ParseStmt());
		_ast.Body.TrimExcess();
	}

	public static AST GetAST(string src) => new Parser(src)._ast;

	/*
	 * Methods
	 */

	private IStmt ParseStmt() {
		return ParseExpr();
	}

	private Expression ParseExpr() {
		return ParseComparativeExpression();
	}

	/*
	 * Table of precedence
	 *  - ComparativeExpressions
	 *  - AdditiveExpressions
	 *  - MultiplicativeExpressions
	 *  - PowerExpressions
	 *  - ConcatenationExpressions
	 *  - PrimaryExpressions
	 */

	// Parse Binary Expression 
	private Expression ParseBinaryExpression(string[] ops, Func<Expression> parseAction) {
		Expression left = parseAction();
		while (ops.Contains(At().Value)) {
			string op = Eat().Value;
			Expression right = op is "^" ? ParseExpr() : parseAction();
			left = new BinaryExpression(left, op, right);
		}

		return left;
	}

	// Parsers
	private Expression ParseComparativeExpression() =>
		ParseBinaryExpression(new[] { "==", "<=", ">=", "<", ">" }, ParseAdditiveExpression);

	private Expression ParseAdditiveExpression() =>
		ParseBinaryExpression(new[] { "+", "-" }, ParseMultiplicativeExpression);

	private Expression ParseMultiplicativeExpression() =>
		ParseBinaryExpression(new[] { "*", "/", "%" }, ParsePowerExpression);

	private Expression ParsePowerExpression() => ParseBinaryExpression(new[] { "^" }, ParseConcatExpression);
    
	private Expression ParseConcatExpression() => ParseBinaryExpression(new[] { "@" }, ParsePrimaryExpression);

	private Expression ParsePrimaryExpression() {
		switch (At().Key) {
			case TokenType.OpenParen:
				Eat();
				Expression tempExpr = ParseExpr();
				Expect(TokenType.CloseParen, "Your lose your parenthesis count => )");
				return tempExpr;
			case TokenType.Number:
				return new NumberLiteral(Eat().Value);
			case TokenType.Text:
				return new TextLiteral(Eat().Value);
			case TokenType.Identifier:
				return new Identifier(Eat().Value);
			case TokenType.BinaryValue:
				return new BooleanLiteral(Eat().Value);
			case TokenType.Null: {
				Eat();
				return new NullLiteral();
			}
			default: throw new Exception("Unexpected Token Found!!");
		}
	}


	// Pointers 
	private int _at;
	private Token At() => _tokens[_at];
	private Token Eat() => _tokens[_at++];

	private Token Expect(TokenType type, string errMsg) {
		Token tempTk = Eat();
		if (tempTk.Key != type) throw new Exception(errMsg);
		return tempTk;
	}
}