namespace OwlDomain.Common.Threading;

/// <summary>
/// Represents a scope that will release the relevant <see cref="SemaphoreSlim"/> when the scope is disposed.
/// </summary>
public readonly struct SemaphoreScope : IDisposable
{
	#region Fields
	private readonly SemaphoreSlim _semaphore;
	private readonly int _releaseCount;
	#endregion

	#region Properties
	/// <summary>Whether the semaphore has been entered.</summary>
	public bool HasEntered => _releaseCount > 0;
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="SemaphoreScope"/>.</summary>
	/// <param name="semaphore">The semaphore to release when the scope is disposed.</param>
	/// <param name="releaseCount">
	/// The amount of times to release the given <paramref name="semaphore"/>, if a value of zero
	/// is provided, then the <paramref name="semaphore"/> is considered to not have been entered.
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="releaseCount"/> is less than zero.</exception>
	public SemaphoreScope(SemaphoreSlim semaphore, int releaseCount)
	{
		Guard.IsGreaterThanOrEqualTo(releaseCount, 0, nameof(releaseCount));

		_semaphore = semaphore;
		_releaseCount = releaseCount;
	}
	#endregion

	#region Methods
	/// <summary>Releases the semaphore that the scope is for.</summary>
	public void Dispose()
	{
		if (_releaseCount > 0)
			_semaphore.Release(_releaseCount);
	}
	#endregion
}

/// <summary>
/// Contains various extensions related to the <see cref="SemaphoreSlim"/>.
/// </summary>
public static class SemaphoreSlimExtensions
{
	extension(SemaphoreSlim semaphore)
	{
		#region Methods
		/// <summary>Blocks the current thread until it can enter the <paramref name="semaphore"/>.</summary>
		/// <returns>A scope, which when disposed, will release the <paramref name="semaphore"/> once.</returns>
		public SemaphoreScope Lock()
		{
			semaphore.Wait();
			return new(semaphore, releaseCount: 1);
		}

		/// <summary>Asynchronously waits to enter the <paramref name="semaphore"/>.</summary>
		/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
		/// <returns>A scope, which when disposed, will release the <paramref name="semaphore"/> once.</returns>
		/// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
		public async ValueTask<SemaphoreScope> LockAsync(CancellationToken cancellation = default)
		{
			await semaphore.WaitAsync(cancellation).ConfigureAwait(false);
			return new(semaphore, releaseCount: 1);
		}

		/// <summary>Blocks the current thread until it can enter the <paramref name="semaphore"/>, or the given <paramref name="timeout"/> runs out.</summary>
		/// <param name="timeout">
		/// The maximum amount of time to allow before giving up on entering the <paramref name="semaphore"/>.
		/// A value of -1 millisecond represents an infinite timeout.
		/// </param>
		/// <returns>A scope, which when disposed, will release the <paramref name="semaphore"/> if it was successfully entered.</returns>
		/// <remarks>The property <see cref="SemaphoreScope.HasEntered"/> on the returned scope can be checked to see if the semaphore was entered.</remarks>
		public SemaphoreScope Lock(TimeSpan timeout)
		{
			if (semaphore.Wait(timeout))
				return new(semaphore, releaseCount: 1);

			return new(semaphore, releaseCount: 0);
		}

		/// <summary>Asynchronously waits to enter the <paramref name="semaphore"/>, unless the <paramref name="timeout"/> runs out before that can happen.</summary>
		/// <param name="timeout">
		/// The maximum amount of time to allow before giving up on entering the <paramref name="semaphore"/>.
		/// A value of -1 millisecond represents an infinite timeout.
		/// </param>
		/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
		/// <returns>A scope, which when disposed, will release the <paramref name="semaphore"/> if it was successfully entered.</returns>
		/// <remarks>The property <see cref="SemaphoreScope.HasEntered"/> on the returned scope can be checked to see if the semaphore was entered.</remarks>
		/// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
		public async Task<SemaphoreScope> LockAsync(TimeSpan timeout, CancellationToken cancellation = default)
		{
			if (await semaphore.WaitAsync(timeout, cancellation).ConfigureAwait(false))
				return new(semaphore, releaseCount: 1);

			return new(semaphore, releaseCount: 0);
		}

		/// <summary>Blocks the current thread until it can enter the <paramref name="semaphore"/>.</summary>
		/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
		/// <returns>A scope, which when disposed, will release the <paramref name="semaphore"/> once.</returns>
		/// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
		public SemaphoreScope Lock(CancellationToken cancellation)
		{
			semaphore.Wait(cancellation);
			return new(semaphore, releaseCount: 1);
		}
		#endregion
	}
}
