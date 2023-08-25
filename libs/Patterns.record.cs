using System.Text.RegularExpressions;
namespace HULK_Interpreter.libs;

public static class Patterns {
	// Describe a plain file path
	public static readonly Regex FilePath = new(@"^((\.{2}/)+|(\./)|(/)|([A-Z]:\\))(?!.*/\./).*(?<![/\\])\.txt$");
}