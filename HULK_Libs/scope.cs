namespace HULK_libs;

internal enum EnvDict {
	Vars,
	Functions
}

public class Scope {
	private readonly Scope? _upperScope;
	private Dictionary<string, IRuntimeValue?> Vars { get; }
	private Dictionary<string, Tuple<string[], IStmt>> Functions { get; }
	public int VarCount => _upperScope != null ? Vars.Count + _upperScope.VarCount : Vars.Count;

	public Scope(Scope? parent = null) {
		_upperScope = parent;
		Vars = new Dictionary<string, IRuntimeValue?>();
		Functions = new Dictionary<string, Tuple<string[], IStmt>>();
	}

	// Handle Vars Behavior

	public Scope DeclareVar(string name, IRuntimeValue? value) {
		Declaration(Vars.TryAdd(name, value), "Can't Declare a Variable that it's already declare.");
		return new Scope(this);
	}

	public IRuntimeValue? GetVarVal(string name) {
		// Search in local scope first
		if (Vars.TryGetValue(name, out IRuntimeValue? val)) return val;
		// Search in the parent Scope
		if (SearchUpperScope(name, EnvDict.Vars, out object? value)) return (IRuntimeValue?)value;
		// Else the Variable doesn't exist
		throw new Exception($"'{name}' var doesn't exist.");
	}

	// Handle Functions Behavior
	// SETTER

	public void DeclareFun(string sym, string[] args, IStmt body) {
		Scope env = this;
		// all functions get declare in the global Scope
		do
			env = env._upperScope ?? env;
		while (env._upperScope != null);

		Declaration(
			env.Functions.TryAdd(sym, new Tuple<string[], IStmt>(args, body)),
			"Can't Declare a Function that it's already declare.");
	}

	// CALL

	public Tuple<string[], IStmt> GetFunContext(string name) {
		// Search in local scope first
		if (Functions.TryGetValue(name, out Tuple<string[], IStmt>? val)) return val;
		// Search in the parent Scope
		if (SearchUpperScope(name, EnvDict.Functions, out object? objVal)) return (Tuple<string[], IStmt>)objVal!;
		// Else the Function doesn't exit
		throw new Exception($"'{name}' var doesn't exist.");
	}

// Aux Methods

	private static void Declaration(bool success, string errMsg) {
		if (success) return;
		throw new Exception(errMsg);
	}


	private bool SearchUpperScope(string name, EnvDict dict, out object? value) {
		value = null;
		Scope? upper = _upperScope;

		while (true) {
			// if no exist parent scope
			if (upper == null) return false;

			// if the data exist
			// for vars
			if (dict == EnvDict.Vars && upper.Vars.TryGetValue(name, out IRuntimeValue? runVal)) {
				value = runVal!;
				return true;
			}

			// for functions
			if (dict == EnvDict.Functions &&
			    upper.Functions.TryGetValue(name, out Tuple<string[], IStmt>? funContext)) {
				value = funContext;
				return true;
			}

			upper = upper._upperScope;
		}
	}
}