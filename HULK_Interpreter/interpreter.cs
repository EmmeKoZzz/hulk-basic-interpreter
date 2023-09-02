namespace HULK_Interpreter;

using HULK_libs;

public class Interpreter {
	private Scope _scope;

	public Interpreter(Scope scope) {
		_scope = scope;
	}
	/*
	 * Public Methods
	 */

	public IRuntimeValue Interprete(string src) {
		AST ast = Parser.GetAST(src);
		return InterpreteASTNode(ast);
	}

	/*
	 * Private Methods
	 */

	private IRuntimeValue InterpreteASTNode(IStmt ast) =>
		ast.Kind switch {
			ASTNode.Program => EvalProgramBody(((AST)ast).Body),
			ASTNode.Var => _scope.GetVarVal(((VarName)ast).Symbol!),
			ASTNode.Number => new RuntimeNum(((NumberLiteral)ast).Value),
			ASTNode.Text => new RuntimeText(((TextLiteral)ast).Value),
			ASTNode.Null => new RuntimeNull(),
			ASTNode.Boolean => new RuntimeBool(((BooleanLiteral)ast).Value),
			ASTNode.BinaryExpression => EvalBinaryExpr((BinaryExpression)ast),
			ASTNode.FunDeclaration => EvalFunDeclaration((FunDeclaration)ast),
			ASTNode.FunCall => ResolveFunCall((FunCall)ast),
			ASTNode.Condition => EvalConditionalExpr((Condition)ast),
			ASTNode.Math => EvalMathFunction((MathFun)ast),
			ASTNode.Print => EvalPrintFunction(),
			_ => new RuntimeNull()
		} ?? throw new InvalidOperationException();

	/*
	 * BUILT-IN Functions
	 */

	private RuntimeNum EvalMathFunction(MathFun fun) =>
		fun.FunType switch {
			MathFunType.Log => new RuntimeNum(float.Log(Param<float>("x"), Param<float>("base"))),
			MathFunType.Cos => new RuntimeNum(float.Cos(Param<float>("grade"))),
			MathFunType.Sin => new RuntimeNum(float.Sin(Param<float>("grade"))),
			_ => throw new ArgumentOutOfRangeException()
		};

	private RuntimeNull EvalPrintFunction() {
		Console.WriteLine(Param<object>("src"));
		return new RuntimeNull();
	}

	private T Param<T>(string var) => (T)_scope.GetVarVal(var)!.Value!;
	/*
	 * -------- Statements ----------
	 */

	private IRuntimeValue EvalProgramBody(List<IStmt> body) {
		IRuntimeValue last = new RuntimeNull();
		foreach (IStmt stmt in body)
			if (stmt.Kind == ASTNode.VarDeclaration)
				EvalVarDeclaration((VarDeclaration)stmt);
			else last = InterpreteASTNode(stmt);
		return last;
	}

	private void EvalVarDeclaration(VarDeclaration decl) {
		_scope = _scope.DeclareVar(decl.Sym!, InterpreteASTNode(decl.Value));
	}

	private RuntimeNull EvalFunDeclaration(FunDeclaration funContext) {
		IStmt body = funContext.Body;
		Scope temp = _scope;
		if (_scope.VarCount > 0)
			body = EvalVars(funContext.Body, funContext.Args);
		_scope = temp;
		_scope.DeclareFun(funContext.Sym!, funContext.Args, body);
		return new RuntimeNull();
	}

	private IStmt EvalVars(IStmt stmt, string[] funArgs) {
		switch (stmt.Kind) {
			case ASTNode.Program: {
				AST newBody = new();
				foreach (IStmt statement in ((AST)stmt).Body)
					newBody.Body.Add(EvalVars(statement, funArgs));
				return newBody;
			}
			case ASTNode.BinaryExpression: {
				var bin = (BinaryExpression)stmt;
				var left = (Expression)EvalVars(bin.Left, funArgs);
				var right = (Expression)EvalVars(bin.Right, funArgs);
				return new BinaryExpression(left, bin.Op, right);
			}
			case ASTNode.Condition: {
				var con = (Condition)stmt;
				var conExpr = (Expression)EvalVars(con.ConditionExpr, funArgs);
				IStmt posExpr = EvalVars(con.Positive, funArgs);
				IStmt negativeExpr = EvalVars(con.Negative, funArgs);
				return new Condition(conExpr, posExpr, negativeExpr);
			}
			case ASTNode.FunCall: {
				var call = (FunCall)stmt;
				return new FunCall(call.Symbol, call.Args.Select(expr => (Expression)EvalVars(expr, funArgs)).ToArray());
			}
			case ASTNode.VarDeclaration: {
				EvalVarDeclaration((VarDeclaration)stmt);
				return new NullLiteral();
			}
			// Target one
			case ASTNode.Var: {
				if (funArgs.Contains(((VarName)stmt).Symbol)) break;
				IRuntimeValue value = InterpreteASTNode(stmt);
				return ResolveASTNode(value);
			}
		}

		return stmt;
	}

	private IStmt ResolveASTNode(IRuntimeValue val) =>
		val.Type switch {
			RuntimeType.Number => new NumberLiteral(val.Value!.ToString()!),
			RuntimeType.Text => new TextLiteral(val.Value!.ToString()!),
			RuntimeType.Bool => new BooleanLiteral(val.Value!.ToString()!),
			_ => new NullLiteral()
		};

	private IRuntimeValue ResolveFunCall(FunCall ast) {
		_scope.GetFunContext(ast.Symbol!).Deconstruct(out string?[] paramNames, out IStmt bodyContext);
		// create Function Scope
		Scope funEnv = new(_scope);

		// verify if the numbers of args are the same in the calling and the declaration
		if (ast.Args.Length != paramNames.Length)
			throw new Exception($"{ast.Symbol} receive {paramNames.Length} args and you're giving {ast.Args.Length}.");

		// give value to the params
		for (int i = 0; i < paramNames.Length; i++)
			funEnv.DeclareVar(paramNames[i]!, InterpreteASTNode(ast.Args[i]));

		// Evaluate Body function in his own Scope
		Scope temp = _scope;
		_scope = funEnv;
		IRuntimeValue callRes = InterpreteASTNode(bodyContext);
		_scope = temp;

		return callRes;
	}

	/*
	 * ------ EXPRESSIONS ------------
	 */

	private IRuntimeValue EvalBinaryExpr(BinaryExpression expr) {
		IRuntimeValue left = InterpreteASTNode(expr.Left);
		IRuntimeValue right = InterpreteASTNode(expr.Right);

		return expr.Op switch {
			"+" or "-" or "/" or "*" or "%" or "^" => EvalMathExpr(left, right, expr.Op),
			">" or "<" or "==" or ">=" or "<=" => EvalComparativeExpr(left, right, expr.Op),
			"@" => EvalConcatExpr(left, right),
			_ => throw new Exception("not valid operator :p .")
		};
	}

	private RuntimeNum EvalMathExpr(IRuntimeValue left, IRuntimeValue right, string? op) {
		CheckNumMembersType(left, right, $"Can't operate {op} for not number members.");
		Func<IRuntimeValue, float> get = GetRuntimeVal<float>;

		return op switch {
			"+" => new RuntimeNum(get(left) + get(right)),
			"-" => new RuntimeNum(get(left) - get(right)),
			"*" => new RuntimeNum(get(left) * get(right)),
			"/" => new RuntimeNum(get(left) / get(right)),
			"%" => new RuntimeNum(get(left) % get(right)),
			"^" => new RuntimeNum((float)Math.Pow(get(left), get(right))),
			_ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
		};
	}


	private RuntimeBool EvalComparativeExpr(IRuntimeValue left, IRuntimeValue right, string? op) {
		// if members are different type the they are not the same
		if (left.Type != right.Type)
			return new RuntimeBool(false);

		if (op == "==") {
			if (left.Value == null && right.Value == null)
				return new RuntimeBool(true);
			if (left.Value == null || right.Value == null)
				return new RuntimeBool(false);
			return new RuntimeBool(left.Value!.ToString() == right.Value!.ToString());
		}

		CheckNumMembersType(left, right, $"Can't operate {op} for not number members.");
		Func<IRuntimeValue, float> getNum = GetRuntimeVal<float>;

		return op switch {
			">=" => new RuntimeBool(getNum(left) >= getNum(right)),
			"<=" => new RuntimeBool(getNum(left) <= getNum(right)),
			">" => new RuntimeBool(getNum(left) > getNum(right)),
			"<" => new RuntimeBool(getNum(left) < getNum(right)),
			_ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
		};
	}

	private RuntimeText EvalConcatExpr(IRuntimeValue left, IRuntimeValue right) =>
		new($"{GetRuntimeVal<object>(left)}{GetRuntimeVal<object>(right)}");

	private IRuntimeValue EvalConditionalExpr(Condition condition) {
		// Interprete Conditional Expression
		IRuntimeValue conditionResult = InterpreteASTNode(condition.ConditionExpr);

		// verify if the conditional expression is a Boolean expression
		if (conditionResult.Type != RuntimeType.Bool)
			throw new Exception("The condition expression must to return a Boolean value");

		// decide which expression interprete
		if ((bool)conditionResult.Value!) return InterpreteASTNode(condition.Positive);
		return InterpreteASTNode(condition.Negative);
	}

	// Generic Get Value
	private static T GetRuntimeVal<T>(IRuntimeValue num) => (T)num.Value!;

	private static void CheckNumMembersType(IRuntimeValue left, IRuntimeValue right, string msg) {
		if (RuntimeType.Number != left.Type || RuntimeType.Number != right.Type)
			throw new Exception(msg);
	}
}