namespace OwlDomain.Common.Disposable;

/// <summary>
/// Represents a scope, which when disposed will invoke the registered callback.
/// </summary>
public readonly struct DisposableScope : IDisposable
{
	#region Fields
	private readonly Action _callback;
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="DisposableScope"/>.</summary>
	/// <param name="callback">The callback to invoke when the scope is disposed.</param>
	public DisposableScope(Action callback) => _callback = callback;
	#endregion

	#region Methods
	/// <summary>Calls the associated callback.</summary>
	public void Dispose() => _callback.Invoke();
	#endregion
}
