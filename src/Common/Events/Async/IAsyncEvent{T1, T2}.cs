namespace OwlDomain.Common.Events.Async;

/// <summary>Represents a delegate for an <see cref="IAsyncEvent"/> callback.</summary>
/// <typeparam name="T1">The type of the 1st argument that is passed from the event.</typeparam>
/// <typeparam name="T2">The type of the 2nd argument that is passed from the event.</typeparam>
/// <param name="argument1">The 1st argument that is passed from the event.</param>
/// <param name="argument2">The 2nd argument that is passed from the event.</param>
/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
/// <returns>A task that represents the asynchronous operation.</returns>
public delegate ValueTask AsyncEventCallback<T1, T2>(T1 argument1, T2 argument2, CancellationToken cancellation = default);

/// <summary>
/// Represents an asynchronous event that accepts two arguments.
/// </summary>
/// <typeparam name="T1">The type of the 1st event argument.</typeparam>
/// <typeparam name="T2">The type of the 2nd event argument.</typeparam>
public interface IAsyncEvent<T1, T2> : IAsyncEventBase<AsyncEventCallback<T1, T2>>
{
}

/// <inheritdoc cref="IAsyncEvent{T1, T2}"/>
public sealed class AsyncEvent<T1, T2> : AsyncEventBase<AsyncEventCallback<T1, T2>>, IAsyncEvent<T1, T2>
{
	#region Methods
	/// <summary>Raises the registered event callbacks.</summary>
	/// <param name="argument1">The 1st argument that will be passed to the registered callbacks.</param>
	/// <param name="argument2">The 2nd argument that will be passed to the registered callbacks.</param>
	/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public async ValueTask RaiseAsync(T1 argument1, T2 argument2, CancellationToken cancellation = default)
	{
		using (await RaiseLockAsync(cancellation).ConfigureAwait(false))
		{
			IReadOnlyList<AsyncEventCallback<T1, T2>> callbacks = GetCallbacks();
			if (IsConcurrent)
			{
				IEnumerable<Task> tasks = callbacks.Select(callback => callback.Invoke(argument1, argument2, cancellation).AsTask());
				await Task.WhenAll(tasks).ConfigureAwait(ContinueOnCapturedContext);
			}
			else
			{
				foreach (AsyncEventCallback<T1, T2> callback in callbacks)
					await callback.Invoke(argument1, argument2, cancellation).ConfigureAwait(ContinueOnCapturedContext);
			}
		}
	}
	#endregion
}
