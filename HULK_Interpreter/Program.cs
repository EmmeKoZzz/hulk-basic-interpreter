namespace HULK_Interpreter;

using HULK_libs;

internal static class Program {
	/**
	 * Main Program
	 */
	public static void Main() {
		// WELCOME PROMPT
		View.Welcome();
		Scope mainScope = SetupGlobalScope();

		// PROGRAM
		while (true) {
			// Take the input and check if isn't empty or null
			//if (!View.Ask(out string input)) continue;
			var input = "/home/EmmeKoZzz/Programming/UNI/hulk-basic-interpreter/Expressions.txt";
			//if (input is "q" or "exit") break;

			// Take the input and check the validity of the expression
			if (!Input.IsExpression(input, out string[] expressions) &&
			    !Input.IsFilePath(input, out expressions)) continue;

			// Compute all expressions
			foreach (string expression in expressions) {
				Console.WriteLine($">>> {expression};");
				Scope exprScope = new(mainScope);
				IRuntimeValue interpretation = new Interpreter(exprScope).Interprete(expression);
				if (interpretation.Type != RuntimeType.Null)
					Console.WriteLine(interpretation.Value);
			}

			break;
		}
	}

	private static Scope SetupGlobalScope() {
		Scope env = new();
		
		// built-in Variables
		env.DeclareVar("PI", new RuntimeNum(float.Pi));
		env.DeclareVar("E", new RuntimeNum(float.E));
		env.DeclareVar("TAU", new RuntimeNum(float.Tau));
		
		// built-in Functions
		env.DeclareFun("print", new[] { "x" }, Parser.GetAST("x"));
		env.DeclareFun("log",new []{"x","base"}, new MathFun(MathFunType.Log));
		env.DeclareFun("cos",new []{"grade"}, new MathFun(MathFunType.Cos));
		env.DeclareFun("sin",new []{"grade"}, new MathFun(MathFunType.Sin));
		
		// return global
		return env;
	}
}