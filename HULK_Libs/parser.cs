using System.Linq.Expressions;

namespace HULK_libs;

using static TokenType;
using static Lexer;

public class Parser {
	private readonly AST _ast = new();
	private readonly Token[] _tokens;

	/*
	 * Constructor
	 */

	private Parser(string src) {
		_tokens = Tokenize(src);
		while (At().Key != EOE) _ast.Body.Add(ParseStmt());
		_ast.Body.TrimExcess();
	}

	public static AST GetAST(string src) => new Parser(src)._ast;

	/*
	 * Methods
	 */

	private IStmt ParseStmt() =>
		At().Key switch {
			// STATEMENTS
			FunctionDeclarator => ParseFunDeclaration(),
			OpenVar or ColonConjunction => ParseVarDeclaration(),
			// EXPRESSIONS 
			_ => ParseExpr()
		};

	/*
	 * STATEMENTS
	 */

	private Statement ParseVarDeclaration() {
		Eat();
		Token name = Expect(Identifier, "¡" +
		                                "Invalid Var Name.");
		Expect(AssignOperator, "After the name goes the '='... man ... bro, please focus.");
		VarDeclaration declaration = new VarDeclaration(name.Value, ParseExpr());

		if (At().Key != ColonConjunction)
			Expect(CloseVar, "Need to close the var declaration with 'in'.");

		return declaration;
	}

	private Statement ParseFunDeclaration() {
		Eat();
		string funName = Expect(Identifier, "Need to Set a Function Identifier.").Value;
		VarName[] @params = ArgsGatherer(() => new VarName(
			                                 Expect(Identifier, "Invalid Syntax found declaring function args.").Value));
		Expect(LambdaOperator, "Use the lambda operator to declare a body");
		return new FunDeclaration(funName,@params,ParseStmt());
	}

	/*
	 * EXPRESSIONS
	 */

	private Expression ParseExpr() => ParseComparativeExpression();

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
			case OpenParen:
				Eat();
				Expression tempExpr = ParseExpr();
				Expect(CloseParen, "Your lose your parenthesis count => )");
				return tempExpr;
			case TokenType.Number:
				return new NumberLiteral(Eat().Value);
			case TokenType.Text:
				return new TextLiteral(Eat().Value);
			case Identifier: {
				Token temp = Eat();
				return At().Key == OpenParen
					       ? new FunCall(temp.Value, ArgsGatherer(ParseExpr))
					       : new VarName(temp.Value);
			}
			case BinaryValue:
				return new BooleanLiteral(Eat().Value);
			case TokenType.Null: {
				Eat();
				return new NullLiteral();
			}
			default: throw new Exception($"Unexpected Token Found!! => {At().Value}");
		}
	}

	// Aux Methods

	private T[] ArgsGatherer<T>(Func<T> getExpr) {
		Expect(OpenParen, "You need to declare your arguments in the () place,");
		if (At().Key is CloseParen) return new T[] { };
		List<T> args = new();
		while (true) {
			args.Add(getExpr());
			if (At().Key == ColonConjunction) {
				Eat();
				continue;
			}

			Expect(CloseParen, "Close your function argument declaration scope.");
			break;
		}

		return args.ToArray();
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