namespace HULK_libs;

public enum RuntimeType {
	Number,
	Text,
	Bool,
	Null,
}

public interface IRuntimeValue {
	RuntimeType Type { get; }
	object? Value { get; }
}

public abstract class RuntimeValue<TValue> : IRuntimeValue {
	public RuntimeType Type { get; }
	public object? Value { get; }

	protected RuntimeValue(RuntimeType type, TValue? value) {
		Type = type;
		Value = value;
	}
}

public class RuntimeNum : RuntimeValue<float> {
	public RuntimeNum(float value) : base(RuntimeType.Number, value) { }
}

public class RuntimeText : RuntimeValue<string> {
	public RuntimeText(string? value) : base(RuntimeType.Text, value) { }
}

public class RuntimeBool : RuntimeValue<bool> {
	public RuntimeBool(bool value) : base(RuntimeType.Bool, value) { }
}

public class RuntimeNull : RuntimeValue<object?> {
	public RuntimeNull(string? errMsg = null) : base(RuntimeType.Null, errMsg) { }
}