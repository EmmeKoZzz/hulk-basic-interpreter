namespace HULK_Interpreter;

public class Sintax {
	public static bool IsExpression(string input, out string expression) {
		string[] checker = input.Split(';');
		expression = checker[0].Replace(" ", "");
		return checker.Length > 1;
	}
}