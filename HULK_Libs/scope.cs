namespace HULK_libs;

using System.Diagnostics.CodeAnalysis;

internal enum EnvDict {
	Vars,
	Functions
}

public class Scope {
	private readonly Scope? _upperScope;
	private Dictionary<string, IRuntimeValue?> Vars { get; }
	private Dictionary<string, Tuple<string[], IStmt>> Functions { get; }
	public int VarCount => Vars.Count;

	public Scope(Scope? parent) {
		_upperScope = parent;
		Vars = new Dictionary<string, IRuntimeValue?>();
		Functions = new Dictionary<string, Tuple<string[], IStmt>>();
	}

	// Handle Vars Behavior

	public Null? DeclareVar(string name, IRuntimeValue? value) =>
		Declaration(Vars.TryAdd(name, value), "Can't Declare a Variable that it's already declare.");

	public IRuntimeValue? GetVarVal(string name) {
		// Search in local scope first
		if (Vars.TryGetValue(name, out IRuntimeValue? val)) return val;
		// Search in the parent Scope
		if (SearchVarUpperScope(name, EnvDict.Vars, out object? value)) return (IRuntimeValue?)value;
		// Else the Variable doesn't exist
		throw new Exception($"'{name}' var doesn't exist.");
	}

	// Handle Functions Behavior
	// SETTER

	public Null DeclareFun(string sym,string[] args, IStmt body) {
		Scope env = _upperScope ?? this;
		return Declaration(
			env.Functions.TryAdd(sym, new Tuple<string[], IStmt>(args, body)),
			"Can't Declare a Function that it's already declare.")!;
	}
	
	// CALL

	public Tuple<string[], IStmt> GetFunContext(string name) {
		// Search in local scope first
		if (Functions.TryGetValue(name, out Tuple<string[], IStmt>? val)) return val;
		// Search in the parent Scope
		if (SearchVarUpperScope(name, EnvDict.Functions, out object? objVal)) return (Tuple<string[], IStmt>)objVal!;
		// Else the Function doesn't exit
		throw new Exception($"'{name}' var doesn't exist.");
	}

// Aux Methods

	private static Null? Declaration(bool success, string errMsg) {
		if (success) return new Null();
		throw new Exception(errMsg);
	}


	private bool SearchVarUpperScope(string name, EnvDict dict, out object? value) {
		value = null;
		Scope? upper = _upperScope;
		while (true) {
			if (upper == null) return false;
			switch (dict) {
				case EnvDict.Vars:
					if (upper.Vars.TryGetValue(name, out IRuntimeValue? runVal)) {
						value = runVal;
						return true;
					}

					break;
				case EnvDict.Functions:
					if (upper.Functions.TryGetValue(name, out Tuple<string[], IStmt>? funContext)) {
						value = funContext;
						return true;
					}

					break;
			}

			upper = upper._upperScope;
		}
	}
}