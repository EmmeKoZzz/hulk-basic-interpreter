namespace HULK_Interpreter.libs;

public static class Syntax {
	// verify if the string (input) is a valid expression
	public static bool IsExpression(string input, out string[] expression) {
		expression = new[] { input.Trim().Replace(";", "") };
		return input.TrimEnd()[^1] == ';';
	}

	// verify if the string (input) is correct path to a plainFile
	public static bool IsFilePath(string input, out string[] expressions) {
		expressions = Array.Empty<string>();
		
		if (!Patterns.FilePath.Match(input).Success) return false;

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