namespace HULK_Interpreter;
using HULK_libs;

internal static class Program {
	/**
	 * Main Program
	 */
	public static void Main() {
		// WELCOME PROMPT
		View.Welcome();

		// PROGRAM
		while (true) {
			// Take the input and check if isn't empty or null
			//if (!View.Ask(out string input)) continue;
			const string input = "/home/EmmeKoZzz/Programming/UNI/hulk-basic-interpreter/Expressions.txt";

			// Take the input and check the validity of the expression
			if (!Syntax.IsExpression(input, out string[] expressions) &&
			    !Syntax.IsFilePath(input, out expressions)) continue;

			// Compute all expressions
			foreach (string expression in expressions) ComputeExpression(expression);

			return;
		}
	}

	/**
	 * Compute Expression
	 */
	private static void ComputeExpression(string expression) {
		Basics.Print($"Expression: {expression}");
		
	}
}