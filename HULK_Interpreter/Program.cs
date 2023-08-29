using System.Text.RegularExpressions;

namespace HULK_Interpreter;

using HULK_libs;
using static HULK_libs.Lexer;

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
			if (!IsExpression(input, out string[] expressions) &&
			    !IsFilePath(input, out expressions)) continue;

			// Compute all expressions
			foreach (string expression in expressions) {
				Console.WriteLine($">>> {expression}");
				Token[] b = Tokenize(expression);
			}

			View.Ask(out string a);
			if (a is "q" or "exit") break;
		}
	}

	// verify if is a valid expression
	private static bool IsExpression(string input, out string[] expression) {
		expression = new[] { input.Trim().Replace(";", "") };
		string trimmed = input.TrimEnd();
		return trimmed.Length != 0 && trimmed[^1] == ';';
	}

	// verify if the string (input) is correct path to a plainFile
	private static bool IsFilePath(string input, out string[] expressions) {
		expressions = Array.Empty<string>();

		if (!Regex.Match(input, @"^((\.{2}/)+|(\./)|(/)|([A-Z]:\\))(?!.*/\./).*(?<![/\\])\.txt$")
		          .Success) return false;

		List<string> expressionsList = new();

		StreamReader reader = new(input);
		while (reader.ReadLine() is { } line) {
			if (!string.IsNullOrEmpty(line) && IsExpression(line, out string[] expression))
				expressionsList.Add(expression[0]);
		}

		expressions = expressionsList.ToArray();
		return true;
	}
}