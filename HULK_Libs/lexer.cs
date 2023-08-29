using System.Text.RegularExpressions;

namespace HULK_libs;

public enum TokenType {
	Identifier,
	FunctionDeclarator,

	// H.U.L.K Value Types
	Number,
	Text,
	BinaryValue,

	// Operators
	BinaryOperator,

	// Group Tokens
	OpenParen,
	CloseParen,
	OpenVar,
	CloseVar,

	// Conjunction
	ColonConjunction
}

public struct Token {
	public TokenType Key { get; }
	public string Value { get; }

	private Token(TokenType type, string src) => (Key, Value) = (type, src);
	public static Token Init(TokenType type, string src) => new Token(type, src);
}

public static class Lexer {
	// Regex patterns
	private static bool IsAlpha(char c) => Regex.Match(c.ToString(), "[a-zA-Z_]").Success;
	private static bool IsDigit(char c) => Regex.Match(c.ToString(), @"\d").Success;
	private static string GetToken(string src, string pattern) => Regex.Match(src, pattern).Value;

	// Aux Methods
	private static readonly Func<TokenType, string, Token> InitTk = Token.Init;

	// Properties
	private static readonly Dictionary<string, TokenType> Keywords = new() {
		{ "let", TokenType.OpenVar },
		{ "in", TokenType.CloseVar },
		{ "function", TokenType.FunctionDeclarator },
		{ "true", TokenType.BinaryValue },
		{ "false", TokenType.BinaryValue }
	};

	// Set Means to Expression parts;
	public static Token[] Tokenize(string expression) {
		// the list of reserved word for the language
		var tokens = new List<Token>();

		// This loop will build the tokens List
		while (expression.Length > 0) {
			char at = expression[0];
			string match;
			Token tk;

			// Single Tokens
			switch (at) {
				case ' ':
					expression = expression[1..];
					continue;
				case '+' or '-' or '/' or '*' or '%' or '^' or '>' or '<' or '=': {
					match = expression is [_, '=', ..] ? expression[..2] : at.ToString();
					expression = AddToken(expression, InitTk(TokenType.BinaryOperator, match));
					continue;
				}
				case '"' or '\'': {
					match = GetToken(expression[1..], """(?<!")[^"]+|(?<!')[^']+""");
					tk = InitTk(TokenType.Text, match);
					expression = AddToken(expression, tk, 2);
					continue;
				}
				case ',':
					expression = AddToken(expression, InitTk(TokenType.ColonConjunction, at.ToString()));
					continue;
				case '(':
					expression = AddToken(expression, InitTk(TokenType.OpenParen, at.ToString()));
					continue;
				case ')':
					expression = AddToken(expression, InitTk(TokenType.CloseParen, at.ToString()));
					continue;
			}

			// Multiple Characters Tokens
			if (IsAlpha(at)) {
				match = GetToken(expression, @"^\w+");
				tk = Keywords.TryGetValue(match, out TokenType type)
					     ? InitTk(type, match)
					     : InitTk(TokenType.Identifier, match);
				expression = AddToken(expression, tk);
				continue;
			}

			if (expression is ['!', '=', ..]) {
				expression = AddToken(expression, InitTk(TokenType.BinaryOperator, expression[..2]));
			}

			if (!IsDigit(at)) throw new Exception("char unrecognizable: " + at);

			match = GetToken(expression, @"^\d+");
			tk = InitTk(TokenType.Number, match);
			expression = AddToken(expression, tk);
		}

		/***
		 * Return the Array of tokens
		 */
		return tokens.ToArray();

		// Add token to the list and reduce the expression
		string AddToken(string src, Token tk, int overload = 0) {
			tokens.Add(tk);
			return src[(tk.Value.Length + overload) ..];
		}
	}
}