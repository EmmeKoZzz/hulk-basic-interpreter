using System.Text.RegularExpressions;

namespace HULK_Interpreter; 

public static class Input {
	// verify if is a valid expression
	public static bool IsExpression(string input, out string[] expression) {
		expression = new[] { input.Trim().Replace(";", "") };
		string trimmed = input.TrimEnd();
		return trimmed.Length != 0 && trimmed[^1] == ';';
	}

	// verify if the string (input) is correct path to a plainFile
	public static bool IsFilePath(string input, out string[] expressions) {
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