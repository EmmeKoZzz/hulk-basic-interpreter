namespace HULK_Interpreter;

class Program {
	/**
	 * Main Program
	 */
	public static void Main() {
		Welcome();
		while (true) {
			// Take the input and check if isn't empty or null
			if (!Ask(out string input)) continue;
			// Take the input and check the validity of the expression
			if (!Sintax.IsExpression(input, out string expression)) {
				Console.WriteLine("It's not a valid expression, check the ';' at the end :).");
				continue;
			}
			//
			Console.WriteLine(expression);
			return;
		}
	}
	
	/**
	 * Methods
	 */
	static bool Ask(out string answer) {
		Console.Write("> ");
		answer = Console.ReadLine() ?? "";
		return !String.IsNullOrEmpty(answer);
	}
	static void Welcome() =>
		Console.WriteLine(
			"HI, this is a Simplify HULK Interpreter (SHULKI):\n " +
			"Write the expression to be interpreted or use the command 'run <filepath>' to read it from file."
		);
	
}