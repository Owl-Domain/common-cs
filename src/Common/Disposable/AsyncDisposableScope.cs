namespace OwlDomain.Common.Disposable;

/// <summary>
/// Represents a scope, which when disposed will invoke the registered callback.
/// </summary>
public readonly struct AsyncDisposableScope : IAsyncDisposable
{
	#region Fields
	private readonly Delegate _callback;
	private readonly bool _continueOnCapturedContext;
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="AsyncDisposableScope"/>.</summary>
	/// <param name="callback">The asynchronous callback to invoke when the scope is disposed.</param>
	/// <param name="continueOnCapturedContext">Whether the <paramref name="callback"/> should be marshalled back onto the captured context.</param>
	public AsyncDisposableScope(Func<ValueTask> callback, bool continueOnCapturedContext = false)
	{
		_callback = callback;
		_continueOnCapturedContext = continueOnCapturedContext;
	}

	/// <summary>Creates a new <see cref="AsyncDisposableScope"/>.</summary>
	/// <param name="callback">The asynchronous callback to invoke when the scope is disposed.</param>
	/// <param name="continueOnCapturedContext">Whether the <paramref name="callback"/> should be marshalled back onto the captured context.</param>
	public AsyncDisposableScope(Func<Task> callback, bool continueOnCapturedContext = false)
	{
		_callback = callback;
		_continueOnCapturedContext = continueOnCapturedContext;
	}
	#endregion

	#region Methods
	/// <summary>Calls the associated asynchronous callback.</summary>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async ValueTask DisposeAsync()
	{
		if (_callback is Func<ValueTask> valueTask)
		{
			await valueTask.Invoke().ConfigureAwait(_continueOnCapturedContext);
			return;
		}

		if (_callback is Func<Task> task)
		{
			await task.Invoke().ConfigureAwait(_continueOnCapturedContext);
			return;
		}

		// Note(Nightowl): I don't know if .GetType() here will actually return what I expect it to, but it should be unreachable anyway;
		ThrowHelper.ThrowInvalidOperationException($"A callback ({_callback}) of the type ({_callback.GetType()}) was somehow encountered.");
	}
	#endregion
}
