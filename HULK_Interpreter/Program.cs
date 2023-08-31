namespace HULK_Interpreter;
using HULK_libs;

internal static class Program {
	/**
	 * Main Program
	 */
	public static void Main() {
		// WELCOME PROMPT
		View.Welcome();
		Scope mainScope = new (null);
		
		// PROGRAM
		while (true) {
			// Take the input and check if isn't empty or null
			//if (!View.Ask(out string input)) continue;
			var input = "/home/EmmeKoZzz/Programming/UNI/hulk-basic-interpreter/Expressions.txt";
			if (input is "q" or "exit") break;

			// Take the input and check the validity of the expression
			if (!Input.IsExpression(input, out string[] expressions) &&
			    !Input.IsFilePath(input, out expressions)) continue;
			
			// Compute all expressions
			foreach (string expression in expressions) {
				Console.WriteLine($">>> {expression};");
				Scope exprScope = new(mainScope);
				Console.WriteLine(new Interpreter(exprScope).Interprete(expression));
			}
			break;
		}
	}
}