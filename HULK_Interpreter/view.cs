namespace HULK_Interpreter;

public static class View {
	// INPUT
	public static bool Ask(out string answer) {
		Console.Write("> ");
		answer = Console.ReadLine() ?? "";
		return !string.IsNullOrEmpty(answer);
	}
	
	// MESSAGES
	public static void Welcome() =>
		Console.WriteLine(
			"HI, this is a Simplify H.U.L.K Interpreter (S.H.U.L.K.I):\n " +
			"Write the expression to be interpreted or specify the filepath to read it from file."
		);

	// ERRORS
	public static void NotValidExpressionError() =>
		Console.WriteLine("It's not a valid expression.");
}