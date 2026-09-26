namespace OwlDomain.Common.Events.Async;

/// <summary>Represents a delegate for an <see cref="IAsyncEvent"/> callback.</summary>
/// <typeparam name="T">The type of the argument that is passed from the event.</typeparam>
/// <param name="argument">The argument that is passed from the event.</param>
/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
/// <returns>A task that represents the asynchronous operation.</returns>
public delegate ValueTask AsyncEventCallback<T>(T argument, CancellationToken cancellation = default);

/// <summary>
/// Represents an asynchronous event that accepts one argument.
/// </summary>
/// <typeparam name="T">The type of the event argument.</typeparam>
public interface IAsyncEvent<T> : IAsyncEventBase<AsyncEventCallback<T>>
{
}

/// <inheritdoc cref="IAsyncEvent{T}"/>
public sealed class AsyncEvent<T> : AsyncEventBase<AsyncEventCallback<T>>, IAsyncEvent<T>
{
	#region Methods
	/// <summary>Raises the registered event callbacks.</summary>
	/// <param name="argument">The argument that will be passed to the registered callbacks.</param>
	/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async ValueTask RaiseAsync(T argument, CancellationToken cancellation = default)
	{
		using (await RaiseLockAsync(cancellation).ConfigureAwait(false))
		{
			IReadOnlyList<AsyncEventCallback<T>> callbacks = GetCallbacks();
			if (IsConcurrent)
			{
				IEnumerable<Task> tasks = callbacks.Select(callback => callback.Invoke(argument, cancellation).AsTask());
				await Task.WhenAll(tasks).ConfigureAwait(ContinueOnCapturedContext);
			}
			else
			{
				foreach (AsyncEventCallback<T> callback in callbacks)
					await callback.Invoke(argument, cancellation).ConfigureAwait(ContinueOnCapturedContext);
			}
		}
	}
	#endregion
}
