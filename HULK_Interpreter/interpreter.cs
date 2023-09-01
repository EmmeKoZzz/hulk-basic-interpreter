namespace HULK_Interpreter;

using HULK_libs;

public class Interpreter {
	private readonly Scope _scope;

	public Interpreter(Scope scope) {
		_scope = scope;
	}
	/*
	 * Public Methods
	 */

	public IRuntimeValue Interprete(string src) {
		return InterpreteASTNode(Parser.GetAST(src), _scope);
	}

	/*
	 * Private Methods
	 */

	private IRuntimeValue InterpreteASTNode(IStmt ast, Scope scope) =>
		(ast.Kind switch {
				ASTNode.Program => EvalProgramBody(((AST)ast).Body, scope),
				ASTNode.Var => scope.GetVarVal(((VarName)ast).Symbol),
				ASTNode.Number => new Number(((NumberLiteral)ast).Value),
				ASTNode.Text => new Text(((TextLiteral)ast).Value),
				ASTNode.Null => new Null(),
				ASTNode.Boolean => new Boolean(((BooleanLiteral)ast).Value),
				ASTNode.BinaryExpression => EvalBinaryExpr((BinaryExpression)ast, scope),
				ASTNode.VarDeclaration => scope.DeclareVar(((VarDeclaration)ast).Sym,
				                                           InterpreteASTNode(((VarDeclaration)ast).Value, scope)),
				ASTNode.FunDeclaration => EvalFunDeclaration((FunDeclaration)ast, scope), //scope.DeclareFun((FunDeclaration)ast),
				ASTNode.FunCall => ResolveFunCall((FunCall)ast, scope),
				_ => throw new ArgumentOutOfRangeException()
			})!;

	/*
	 * -------- Statements ----------
	 */

	private IRuntimeValue EvalProgramBody(List<IStmt> body, Scope scope) {
		IRuntimeValue last = new Null();
		foreach (IStmt stmt in body) last = InterpreteASTNode(stmt, scope);
		return last;
	}

	private Null EvalFunDeclaration(FunDeclaration funContext, Scope env) {
		IStmt body = funContext.Body;
		if (env.VarCount > 0)
			body = EvalVars(funContext.Body, funContext.Args);
		env.DeclareFun(funContext.Sym, funContext.Args, body);
		return new Null();
	}

	private IStmt EvalVars(IStmt stmt, string[] funArgs) {
		switch (stmt.Kind) {
			case ASTNode.BinaryExpression: {
				var bin = (BinaryExpression)stmt;
				var left = (Expression)EvalVars(bin.Left, funArgs);
				var right = (Expression)EvalVars(bin.Right, funArgs);
				return new BinaryExpression(left, bin.Op, right);
			}
			case ASTNode.Var: {
				if (funArgs.Contains((stmt as VarName)!.Symbol)) break;
				IRuntimeValue value = InterpreteASTNode(stmt, _scope);
				return ResolveASTNode(value);
			}
		}

		return stmt;
	}

	private IStmt ResolveASTNode(IRuntimeValue val) =>
		val.Type switch {
			RuntimeType.Number => new NumberLiteral(val.Value.ToString()!),
			RuntimeType.Text => new TextLiteral(val.Value.ToString()!),
			RuntimeType.Bool => new BooleanLiteral(val.Value.ToString()!),
			_ => new NullLiteral()
		};

	private IRuntimeValue ResolveFunCall(FunCall ast, Scope scope) {
		scope.GetFunContext(ast.Symbol).Deconstruct(out string[] paramNames, out IStmt bodyContext);
		// create Function Scope
		Scope funEnv = new(scope);

		// verify if the numbers of args are the same in the calling and the declaration
		if (ast.Args.Length != paramNames.Length)
			throw new Exception($"{ast.Symbol} receive {paramNames.Length} args and you're giving {ast.Args.Length}.");

		// give value to the params
		for (int i = 0; i < paramNames.Length; i++)
			funEnv.DeclareVar(paramNames[i], InterpreteASTNode(ast.Args[i], scope));
		// Scope Created

		// interprete the function body with the function environment
		return InterpreteASTNode(bodyContext, funEnv);
	}

	/*
	 * ------ EXPRESSIONS ------------
	 */

	private IRuntimeValue? EvalBinaryExpr(BinaryExpression expr, Scope scope) {
		IRuntimeValue left = InterpreteASTNode(expr.Left, scope);
		IRuntimeValue right = InterpreteASTNode(expr.Right, scope);

		return expr.Op switch {
			"+" or "-" or "/" or "*" or "%" or "^" => EvalMathExpr(left, right, expr.Op),
			">" or "<" or "==" or ">=" or "<=" => EvalComparativeExpr(left, right, expr.Op),
			"@" => EvalConcatExpr(left, right),
			_ => throw new Exception("not valid operator :p .")
		};
	}

	private Number? EvalMathExpr(IRuntimeValue left, IRuntimeValue right, string op) {
		CheckNumMembersType(left, right, $"Can't operate {op} for not number members.");
		Func<IRuntimeValue, float> get = GetRuntimeVal<float>;

		return op switch {
			"+" => new Number(get(left) + get(right)),
			"-" => new Number(get(left) - get(right)),
			"*" => new Number(get(left) * get(right)),
			"/" => new Number(get(left) / get(right)),
			"%" => new Number(get(left) % get(right)),
			"^" => new Number((float)Math.Pow(get(left), get(right))),
			_ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
		};
	}


	private Boolean? EvalComparativeExpr(IRuntimeValue left, IRuntimeValue right, string op) {
		// if members are different type the they are not the same
		if (left.Type != right.Type)
			return new Boolean(false);

		if (op == "==")
			return new Boolean(left.Value.ToString() == right.Value.ToString());

		CheckNumMembersType(left, right, $"Can't operate {op} for not number members.");
		Func<IRuntimeValue, float> getNum = GetRuntimeVal<float>;

		return op switch {
			">=" => new Boolean(getNum(left) >= getNum(right)),
			"<=" => new Boolean(getNum(left) <= getNum(right)),
			">" => new Boolean(getNum(left) > getNum(right)),
			"<" => new Boolean(getNum(left) < getNum(right)),
			_ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
		};
	}

	private Text? EvalConcatExpr(IRuntimeValue left, IRuntimeValue right) =>
		new($"{GetRuntimeVal<object>(left)}{GetRuntimeVal<object>(right)}");


	// Generic Get Value
	private static T GetRuntimeVal<T>(IRuntimeValue num) => (T)num.Value;

	private static void CheckNumMembersType(IRuntimeValue left, IRuntimeValue right, string msg) {
		if (RuntimeType.Number != left.Type || RuntimeType.Number != right.Type)
			throw new Exception(msg);
	}
}