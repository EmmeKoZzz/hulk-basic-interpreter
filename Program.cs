namespace HULK_Interpreter;

class Program {
	public static void Main() {
		Welcome();
		while (true) {
			if (Ask(out string input)) continue;
			Console.WriteLine(input);
			return;
		}
	}

	static void Welcome() =>
		Console.WriteLine(
			"HI, this is a Simplify HULK Interpreter (SHULKI):\n " +
			"Write the expression to be interpreted or use the command 'run <filepath>' to read it from file."
		);


	static bool Ask(out string answer) {
		Console.Write("> ");
		answer = Console.ReadLine() ?? "";
		return String.IsNullOrEmpty(answer);
	}
}