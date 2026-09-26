namespace OwlDomain.Common.Events.Async;

/// <summary>Represents a delegate for an <see cref="IAsyncEvent"/> callback.</summary>
/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
/// <returns>A task that represents the asynchronous operation.</returns>
public delegate ValueTask AsyncEventCallback(CancellationToken cancellation = default);

/// <summary>
/// Represents an asynchronous event that doesn't have any arguments.
/// </summary>
public interface IAsyncEvent : IAsyncEventBase<AsyncEventCallback>
{
}

/// <inheritdoc cref="IAsyncEvent"/>
public sealed class AsyncEvent : AsyncEventBase<AsyncEventCallback>, IAsyncEvent
{
	#region Methods
	/// <summary>Raises the registered event callbacks.</summary>
	/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async ValueTask RaiseAsync(CancellationToken cancellation = default)
	{
		using (await RaiseLockAsync(cancellation).ConfigureAwait(false))
		{
			IReadOnlyList<AsyncEventCallback> callbacks = GetCallbacks();
			if (IsConcurrent)
			{
				IEnumerable<Task> tasks = callbacks.Select(callback => callback.Invoke(cancellation).AsTask());
				await Task.WhenAll(tasks).ConfigureAwait(ContinueOnCapturedContext);
			}
			else
			{
				foreach (AsyncEventCallback callback in callbacks)
					await callback.Invoke(cancellation).ConfigureAwait(ContinueOnCapturedContext);
			}
		}
	}
	#endregion
}
