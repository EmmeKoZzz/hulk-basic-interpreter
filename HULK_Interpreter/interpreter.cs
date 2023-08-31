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

	public string Interprete(string src) => InterpreteASTNode(Parser.GetAST(src)).Value.ToString() ?? string.Empty;

	/*
	 * Private Methods
	 */

	private IRuntimeValue InterpreteASTNode(IStmt ast) =>
		ast.Kind switch {
			ASTNode.Program => EvalProgramBody(((AST)ast).Body),
			ASTNode.Var => _scope.GetVarVal(((VarName)ast).Symbol),
			ASTNode.Number => new Number(((NumberLiteral)ast).Value),
			ASTNode.Text => new Text(((TextLiteral)ast).Value),
			ASTNode.Null => new Null(),
			ASTNode.Boolean => new Boolean(((BooleanLiteral)ast).Value),
			ASTNode.BinaryExpression => EvalBinaryExpr((BinaryExpression)ast),
			ASTNode.VarDeclaration => EvalVarDeclaration((VarDeclaration)ast),
			_ => throw new ArgumentOutOfRangeException()
		};

	private IRuntimeValue EvalProgramBody(List<IStmt> body) {
		IRuntimeValue last = new Null();
		foreach (IStmt stmt in body) last = InterpreteASTNode(stmt);
		return last;
	}

	private IRuntimeValue EvalBinaryExpr(BinaryExpression expr) {
		IRuntimeValue left = InterpreteASTNode(expr.Left);
		IRuntimeValue right = InterpreteASTNode(expr.Right);

		return expr.Op switch {
			"+" or "-" or "/" or "*" or "%" or "^" => EvalMathExpr(left, right, expr.Op),
			">" or "<" or "==" or ">=" or "<=" => EvalComparativeExpr(left, right, expr.Op),
			"@" => EvalConcatExpr(left, right),
			_ => throw new NotImplementedException()
		};
	}

	private Number EvalMathExpr(IRuntimeValue left, IRuntimeValue right, string op) {
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


	private Boolean EvalComparativeExpr(IRuntimeValue left, IRuntimeValue right, string op) {
		if (op == "==") return EqualRuntimeType();

		CheckNumMembersType(left, right, $"Can't operate {op} for not number members.");
		Func<IRuntimeValue, float> getNum = GetRuntimeVal<float>;

		return op switch {
			">=" => new Boolean(getNum(left) >= getNum(right)),
			"<=" => new Boolean(getNum(left) <= getNum(right)),
			">" => new Boolean(getNum(left) > getNum(right)),
			"<" => new Boolean(getNum(left) < getNum(right)),
			_ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
		};

		Boolean EqualRuntimeType() {
			// if members are different type the they are not the same
			if (left.Type != right.Type)
				return new Boolean(false);

			return left.Type switch {
				RuntimeType.Number => new Boolean(GetRuntimeVal<float>(left) == GetRuntimeVal<float>(right)),
				RuntimeType.Bool => new Boolean(GetRuntimeVal<bool>(left) == GetRuntimeVal<bool>(right)),
				RuntimeType.Text => new Boolean(GetRuntimeVal<string>(left) == GetRuntimeVal<string>(right)),
				_ => new Boolean(true)
			};
		}
	}

	private Text EvalConcatExpr(IRuntimeValue left, IRuntimeValue right) =>
		new($"{GetRuntimeVal<object>(left)}{GetRuntimeVal<object>(right)}");

	// ------- Statements ----------

	private Null EvalVarDeclaration(VarDeclaration declaration) {
		bool success = _scope.DeclareVar(declaration.Sym, InterpreteASTNode(declaration.Value));
		if (success) return new Null();
		throw new Exception("Can't Declare a Variable that it's already declare.");
	}

	// Generic Get Value
	private static T GetRuntimeVal<T>(IRuntimeValue num) => (T)num.Value;

	private static void CheckNumMembersType(IRuntimeValue left, IRuntimeValue right, string msg) {
		if (RuntimeType.Number != left.Type || RuntimeType.Number != right.Type)
			throw new Exception(msg);
	}
}