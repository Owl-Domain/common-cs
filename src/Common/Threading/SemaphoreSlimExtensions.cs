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
	extension(SemaphoreSlim)
	{
		#region Functions
		/// <summary>Creates a semaphore that has the initial count, and the maximum count set to the given <paramref name="count"/> value.</summary>
		/// <param name="count">The initial and maximum count of the semaphore.</param>
		/// <returns>The configured semaphore.</returns>
		/// <remarks>
		/// This function exists because by default a semaphore has
		/// <see href="https://source.dot.net/#System.Private.CoreLib/src/runtime/src/libraries/System.Private.CoreLib/src/System/Threading/SemaphoreSlim.cs,ead13858a2ff9c7c"> no maximum count</see>.<br/>
		/// <br/>
		/// This causes a problem because it makes it easy to create a semaphore that will behave differently
		/// to what you'd intuit it to do, and as such it won't throw the <see cref="SemaphoreFullException"/>
		/// even though you might expect one.<br/>
		/// <br/>
		/// This means that debugging would be more annoying since by default the semaphore won't
		/// throw if it's been released more times than it has been acquired.
		/// </remarks>
		public static SemaphoreSlim Create(int count = 1) => new(initialCount: count, maxCount: count);
		#endregion
	}

	extension(SemaphoreSlim semaphore)
	{
		#region Methods
		/// <summary>Blocks the current thread until it can enter the semaphore.</summary>
		/// <returns>A scope, which when disposed, will release the semaphore once.</returns>
		public SemaphoreScope Lock()
		{
			semaphore.Wait();
			return new(semaphore, releaseCount: 1);
		}

		/// <summary>Asynchronously waits to enter the semaphore.</summary>
		/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
		/// <returns>A scope, which when disposed, will release the semaphore once.</returns>
		/// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
		public async ValueTask<SemaphoreScope> LockAsync(CancellationToken cancellation = default)
		{
			await semaphore.WaitAsync(cancellation).ConfigureAwait(false);
			return new(semaphore, releaseCount: 1);
		}

		/// <summary>Blocks the current thread until it can enter the semaphore, or the given <paramref name="timeout"/> runs out.</summary>
		/// <param name="timeout">
		/// The maximum amount of time to allow before giving up on entering the semaphore.
		/// A value of -1 millisecond represents an infinite timeout.
		/// </param>
		/// <returns>A scope, which when disposed, will release the semaphore if it was successfully entered.</returns>
		/// <remarks>The property <see cref="SemaphoreScope.HasEntered"/> on the returned scope can be checked to see if the semaphore was entered.</remarks>
		public SemaphoreScope Lock(TimeSpan timeout)
		{
			if (semaphore.Wait(timeout))
				return new(semaphore, releaseCount: 1);

			return new(semaphore, releaseCount: 0);
		}

		/// <summary>Asynchronously waits to enter the semaphore, unless the <paramref name="timeout"/> runs out before that can happen.</summary>
		/// <param name="timeout">
		/// The maximum amount of time to allow before giving up on entering the semaphore.
		/// A value of -1 millisecond represents an infinite timeout.
		/// </param>
		/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
		/// <returns>A scope, which when disposed, will release the semaphore if it was successfully entered.</returns>
		/// <remarks>The property <see cref="SemaphoreScope.HasEntered"/> on the returned scope can be checked to see if the semaphore was entered.</remarks>
		/// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
		public async Task<SemaphoreScope> LockAsync(TimeSpan timeout, CancellationToken cancellation = default)
		{
			if (await semaphore.WaitAsync(timeout, cancellation).ConfigureAwait(false))
				return new(semaphore, releaseCount: 1);

			return new(semaphore, releaseCount: 0);
		}

		/// <summary>Blocks the current thread until it can enter the semaphore.</summary>
		/// <param name="cancellation">A cancellation token that can be used to cancel the operation.</param>
		/// <returns>A scope, which when disposed, will release the semaphore once.</returns>
		/// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
		public SemaphoreScope Lock(CancellationToken cancellation)
		{
			semaphore.Wait(cancellation);
			return new(semaphore, releaseCount: 1);
		}
		#endregion
	}
}
