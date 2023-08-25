namespace HULK_libs;
using models;

public static class Basics {
	// print in argument
	public static void Print(string input) => Console.WriteLine(input);

	// Compute Algebra
	public static string MathOp(string x, string y, MathOps op) {
		if (!int.TryParse(x, out int numX) || !int.TryParse(y, out int numY)) return $"{x}{y}";

		return op switch {
			MathOps.Sum => (numX + numY).ToString(),
			MathOps.Rest => (numX - numY).ToString(),
			_ => throw new Exception("How f*ck did you get here!?")
		};
	}
}