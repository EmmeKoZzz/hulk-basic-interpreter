namespace HULK_Interpreter;
using libs;

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
			if (!Ask(out string input)) continue;
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