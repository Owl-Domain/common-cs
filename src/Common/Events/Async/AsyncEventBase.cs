namespace OwlDomain.Common.Events.Async;

/// <summary>
/// Represents a base interface to help with implementing asynchronous events.
/// </summary>
/// <typeparam name="TCallback">The type of the callback that the event uses.</typeparam>
public interface IAsyncEventBase<TCallback>
	where TCallback : notnull
{
	#region Properties
	/// <summary>Whether the registered event callbacks will be raised concurrently, in parallel, instead of sequentially.</summary>
	bool IsConcurrent { get; }

	/// <summary>Whether the callbacks will be invoked on the captured context.</summary>
	bool ContinueOnCapturedContext { get; }
	#endregion

	#region Methods
	/// <summary>Subscribes to the asynchronous event.</summary>
	/// <param name="callback">The callback to invoke when the event is fired.</param>
	void Subscribe(TCallback callback);

	/// <summary>Unsubscribes from the asynchronous event.</summary>
	/// <param name="callback">The callback that was used when subscribing to the event.</param>
	void Unsubscribe(TCallback callback);
	#endregion
}

/// <summary>
/// Represents a base class to help with implementing asynchronous events.
/// </summary>
/// <typeparam name="TCallback">The type of the callback that the event uses.</typeparam>
public abstract class AsyncEventBase<TCallback> : IAsyncEventBase<TCallback>
	where TCallback : notnull
{
	#region Fields
	private readonly List<TCallback> _callbacks = [];
	private readonly SemaphoreSlim _callbacksLock = SemaphoreSlim.Create();
	private readonly SemaphoreSlim _raiseLock = SemaphoreSlim.Create();
	#endregion

	#region Properties
	/// <inheritdoc/>
	public bool IsConcurrent { get; set; } = true;

	/// <inheritdoc/>
	public bool ContinueOnCapturedContext { get; set; } = false;
	#endregion

	#region Methods
	/// <inheritdoc/>
	public void Subscribe(TCallback callback)
	{
		using (_callbacksLock.Lock())
			_callbacks.Add(callback);
	}

	/// <inheritdoc/>
	public void Unsubscribe(TCallback callback)
	{
		using (_callbacksLock.Lock())
			_callbacks.Remove(callback);
	}

	/// <summary>Gets a copy of the registered callbacks.</summary>
	/// <returns>A copy of the registered callbacks.</returns>
	/// <remarks>A copy is returned so that it is unaffected by subscribe/unsubscribe calls from the within the registered callbacks.</remarks>
	protected IReadOnlyList<TCallback> GetCallbacks()
	{
		using (_callbacksLock.Lock())
			return _callbacks.ToArray();
	}

	/// <summary>Asynchronously enters a scope during which a secondary raise will be blocked.</summary>
	/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A scope which when disposed will allow the event to be raised again.</returns>
	protected async ValueTask<SemaphoreScope> RaiseLockAsync(CancellationToken cancellation = default)
	{
		return await _raiseLock.LockAsync(cancellation).ConfigureAwait(false);
	}
	#endregion
}
