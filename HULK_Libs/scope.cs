namespace HULK_libs;

public class Scope {
	private readonly Scope? _upperScope;
	private Dictionary<string, IRuntimeValue> Vars { get; }


	public Scope(Scope? parent) {
		_upperScope = parent;
		Vars = new Dictionary<string, IRuntimeValue>();
	}

	// Handle Vars Behavior
	public bool DeclareVar(string name, IRuntimeValue value) => Vars.TryAdd(name, value);

	public IRuntimeValue GetVarVal(string name) {
		if (Vars.TryGetValue(name, out IRuntimeValue? val) ||
		    SearchUpperScope(name, _upperScope, out val)) return val;
		throw new Exception($"'{name}' var doesn't exist.");
	}

	private static bool SearchUpperScope(string name, Scope? upper, [MaybeNullWhen(false)] out IRuntimeValue value) {
		value = null;
		while (true) {
			if (upper == null) return false;
			if (upper.Vars.TryGetValue(name, out value)) return true;
			upper = upper._upperScope;
		}
	}
}