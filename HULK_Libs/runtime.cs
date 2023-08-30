namespace HULK_libs;

public enum RuntimeType {
	Number,
	Text,
	Bool,
	Null
}

public interface IRuntimeValue {
	RuntimeType Type { get; }
	object Value { get; }
}

public abstract class RuntimeValue<TValue> : IRuntimeValue {
	public RuntimeType Type { get; }
	public object Value { get; }

	protected RuntimeValue(RuntimeType type, TValue value) {
		Type = type;
		Value = value ?? throw new ArgumentNullException(nameof(value));
	}
}

public class Number : RuntimeValue<float> {
	public Number(float value) : base(RuntimeType.Number, value) { }
}

public class Text : RuntimeValue<string> {
	public Text(string value) : base(RuntimeType.Text, value) { }
}

public class Boolean : RuntimeValue<bool> {
	public Boolean(bool value) : base(RuntimeType.Bool, value) { }
}

public class Null : RuntimeValue<string> {
	public Null() : base(RuntimeType.Null, "null") { }
}