namespace HULK_libs;

using static TokenType;
using static Lexer;

public class Parser {
	private readonly AST _ast = new();
	private readonly Token[] _tokens;
	private int _parenCount;

	/*
	 * Constructor
	 */

	private Parser(string src) {
		_tokens = Tokenize(src);
		while (At().Key != EOE) {
			_ast.Body.Add(ParseStmt());
			if (At().Key == OpenParen) HandleParenStmt(true);
			else if (At().Key == CloseParen) HandleParenStmt(false);
		}

		if (_parenCount != 0) throw new Exception();
		_ast.Body.TrimExcess();
	}

	public static AST GetAST(string src) => new Parser(src)._ast;

	/*
	 * Methods
	 */

	private IStmt ParseStmt() {
		return At().Key switch {
			// STATEMENTS
			FunctionDeclarator => ParseFunDeclaration(),
			OpenVar or ColonConjunction => ParseVarDeclaration(),
			_ => ParseExpr()
		};
	}

	/*
	 * STATEMENTS
	 */

	private void HandleParenStmt(bool add) {
		// if the next token is a Expression token then do nothing
		if (Peek() is Identifier or Number or Text or Null or PositiveCondition or BinaryValue) return;
		TokenType paren = add ? OpenParen : CloseParen;
		do {
			Eat();
			_parenCount += add ? 1 : -1;
		} while (At().Key == paren);
	}

	private Statement ParseVarDeclaration() {
		Eat();
		Token name = Expect(Identifier, "Invalid Var Name.");
		Expect(AssignOperator, "After the name goes the '='... man ... bro, please focus.");
		VarDeclaration declaration = new(name.Value, ParseExpr());

		if (At().Key != ColonConjunction)
			Expect(CloseVar, "Need to close the var declaration with 'in'.");

		return declaration;
	}

	private Statement ParseFunDeclaration() {
		Eat();
		string? funName = Expect(Identifier, "Need to Set a Function Identifier.").Value;
		string?[] @params = ArgsGatherer(
			() => new VarName(Expect(Identifier, "Invalid Syntax found declaring function args.").Value).Symbol);
		Expect(LambdaOperator, "Use the lambda operator to declare a body");

		AST funBody = new();
		IStmt stmt;
		do {
			stmt = ParseStmt();
			funBody.Body.Add(stmt);
		} while (stmt.Kind == ASTNode.VarDeclaration);

		return new FunDeclaration(funName, @params, funBody);
	}

	/*
	 * EXPRESSIONS
	 */

	private Expression ParseExpr() =>
		At().Key switch {
			// I assume that in H.U.L.K the Conditional Statement it's an expression
			PositiveCondition => ParseConditionalExpr(),
			_ => ParseComparativeExpression()
		};

	private Expression ParseConditionalExpr() {
		Eat();
		Expect(OpenParen, "You need to open the enclose the conditional expression between parenthesis, in fail ad '('");
		Expression conExpr = ParseExpr();
		Expect(CloseParen, "You need to open the enclose the conditional expression between parenthesis, in fail ad ')'");
		IStmt positiveExpr = ParseStmt();
		IStmt negativeExpr = Eat().Key == NegativeCondition ? ParseStmt() : new NullLiteral();
		return new Condition(conExpr, positiveExpr, negativeExpr);
	}

	/*
	 * Table of precedence
	 *  - ComparativeExpressions
	 *  - ConcatenationExpressions
	 *  - AdditiveExpressions
	 *  - MultiplicativeExpressions
	 *  - PowerExpressions
	 *  - PrimaryExpressions
	 */

	// Parse Binary Expression 
	private Expression ParseBinaryExpression(string[] ops, Func<Expression> parseAction) {
		Expression left = parseAction();
		while (ops.Contains(At().Value)) {
			string? op = Eat().Value;
			Expression right = op is "^" ? ParseExpr() : parseAction();
			left = new BinaryExpression(left, op, right);
		}

		return left;
	}

	// Parsers

	private Expression ParseComparativeExpression() =>
		ParseBinaryExpression(new[] { "==", "<=", ">=", "<", ">" }, ParseConcatExpression);

	private Expression ParseConcatExpression() => ParseBinaryExpression(new[] { "@" }, ParseAdditiveExpression);

	private Expression ParseAdditiveExpression() =>
		ParseBinaryExpression(new[] { "+", "-" }, ParseMultiplicativeExpression);

	private Expression ParseMultiplicativeExpression() =>
		ParseBinaryExpression(new[] { "*", "/", "%" }, ParsePowerExpression);

	private Expression ParsePowerExpression() => ParseBinaryExpression(new[] { "^" }, ParsePrimaryExpression);

	private Expression ParsePrimaryExpression() {
		switch (At().Key) {
			case OpenParen:
				return HandleOpenParenExpr();
			case Number:
				return new NumberLiteral(Eat().Value);
			case Text:
				return new TextLiteral(Eat().Value);
			case Identifier: {
				Token temp = Eat();
				return At().Key == OpenParen
					       ? new FunCall(temp.Value, ArgsGatherer(ParseExpr))
					       : new VarName(temp.Value);
			}
			case BinaryValue:
				return new BooleanLiteral(Eat().Value);
			case Null: {
				Eat();
				return new NullLiteral();
			}
			default: throw new Exception($"Unexpected Token Found!! => {At().Value}");
		}
	}

	// Aux Methods

	private Expression HandleOpenParenExpr() {
		Eat();
		Expression exprInside = ParseExpr();
		Expect(CloseParen, "Your lose your parenthesis count => )");
		return exprInside;
	}

	private T[] ArgsGatherer<T>(Func<T> getExpr) {
		Expect(OpenParen, "You need to declare your arguments in the () place,");
		if (At().Key is CloseParen) {
			Eat();
			return Array.Empty<T>();
		}

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
	private TokenType Peek() => _tokens[_at + 1].Key;

	private Token Expect(TokenType type, string errMsg) {
		Token tempTk = Eat();
		if (tempTk.Key != type) throw new Exception(errMsg);
		return tempTk;
	}
}