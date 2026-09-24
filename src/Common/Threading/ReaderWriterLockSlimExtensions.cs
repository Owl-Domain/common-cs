namespace OwlDomain.Common.Threading;

/// <summary>
/// Represents a scope that will release a read lock acquired from a <see cref="ReaderWriterLockSlim"/>.
/// </summary>
public readonly struct ReadScope : IDisposable
{
	#region Fields
	private readonly ReaderWriterLockSlim _lock;
	#endregion

	#region Properties
	/// <summary>Whether the read lock has been entered.</summary>
	public readonly bool HasEntered { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="ReadScope"/>.</summary>
	/// <param name="lock">The reader/writer lock to release when the scope is disposed.</param>
	/// <param name="hasEntered">Whether the <paramref name="lock"/> successfully entered the read lock.</param>
	public ReadScope(ReaderWriterLockSlim @lock, bool hasEntered)
	{
		_lock = @lock;
		HasEntered = hasEntered;
	}
	#endregion

	#region Methods
	/// <summary>Releases the read lock if it has been successfully acquired.</summary>
	public readonly void Dispose()
	{
		if (HasEntered)
			_lock.ExitReadLock();
	}
	#endregion
}

/// <summary>
/// Represents a scope that will release an upgradeable read lock acquired from a <see cref="ReaderWriterLockSlim"/>.
/// </summary>
public readonly struct UpgradeableReadScope : IDisposable
{
	#region Fields
	private readonly ReaderWriterLockSlim _lock;
	#endregion

	#region Properties
	/// <summary>Whether the upgradeable read lock has been entered.</summary>
	public readonly bool HasEntered { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="UpgradeableReadScope"/>.</summary>
	/// <param name="lock">The reader/writer lock to release when the scope is disposed.</param>
	/// <param name="hasEntered">Whether the <paramref name="lock"/> successfully entered the upgradeable read lock.</param>
	public UpgradeableReadScope(ReaderWriterLockSlim @lock, bool hasEntered)
	{
		_lock = @lock;
		HasEntered = hasEntered;
	}
	#endregion

	#region Methods
	/// <summary>Releases the upgradeable read lock if it has been successfully acquired.</summary>
	public readonly void Dispose()
	{
		if (HasEntered)
			_lock.ExitUpgradeableReadLock();
	}
	#endregion
}

/// <summary>
/// Represents a scope that will release a write lock acquired from a <see cref="ReaderWriterLockSlim"/>.
/// </summary>
public readonly struct WriteScope : IDisposable
{
	#region Fields
	private readonly ReaderWriterLockSlim _lock;
	#endregion

	#region Properties
	/// <summary>Whether the write lock has been entered.</summary>
	public readonly bool HasEntered { get; }
	#endregion

	#region Constructors
	/// <summary>Creates a new <see cref="WriteScope"/>.</summary>
	/// <param name="lock">The reader/writer lock to release when the scope is disposed.</param>
	/// <param name="hasEntered">Whether the <paramref name="lock"/> successfully entered the write lock.</param>
	public WriteScope(ReaderWriterLockSlim @lock, bool hasEntered)
	{
		_lock = @lock;
		HasEntered = hasEntered;
	}
	#endregion

	#region Methods
	/// <summary>Releases the write lock if it has been successfully acquired.</summary>
	public readonly void Dispose()
	{
		if (HasEntered)
			_lock.ExitWriteLock();
	}
	#endregion
}

/// <summary>
/// Contains various extensions related to the <see cref="ReaderWriterLockSlim"/>.
/// </summary>
public static class ReaderWriterLockSlimExtensions
{
	extension(ReaderWriterLockSlim @lock)
	{
		#region Read methods
		/// <summary>Enters a read lock.</summary>
		/// <returns>A scope, which when disposed, will release the read lock.</returns>
		public ReadScope ReadLock()
		{
			@lock.EnterReadLock();
			return new(@lock, hasEntered: true);
		}

		/// <summary>Tries to enter a read lock.</summary>
		/// <param name="timeout">
		/// The maximum amount of time to allow before giving up on entering the
		/// read lock. A value of -1 millisecond represents an infinite timeout.
		/// </param>
		/// <returns>A scope, which when disposed, will release the read lock.</returns>
		/// <remarks>The property <see cref="ReadScope.HasEntered"/> on the returned scope can be checked to see if the read lock was entered.</remarks>
		public ReadScope ReadLock(TimeSpan timeout)
		{
			if (@lock.TryEnterReadLock(timeout))
				return new(@lock, hasEntered: true);

			return new(@lock, hasEntered: false);
		}
		#endregion

		#region Upgradeable read methods
		/// <summary>Enters an upgradeable read lock.</summary>
		/// <returns>A scope, which when disposed, will release the upgradeable read lock.</returns>
		public UpgradeableReadScope UpgradeableReadLock()
		{
			@lock.EnterUpgradeableReadLock();
			return new(@lock, hasEntered: true);
		}

		/// <summary>Tries to enter an upgradeable read lock.</summary>
		/// <param name="timeout">
		/// The maximum amount of time to allow before giving up on entering the
		/// upgradeable read lock. A value of -1 millisecond represents an infinite timeout.
		/// </param>
		/// <returns>A scope, which when disposed, will release the upgradeable read lock.</returns>
		/// <remarks>The property <see cref="UpgradeableReadScope.HasEntered"/> on the returned scope can be checked to see if the upgradeable read lock was entered.</remarks>
		public UpgradeableReadScope UpgradeableReadLock(TimeSpan timeout)
		{
			if (@lock.TryEnterUpgradeableReadLock(timeout))
				return new(@lock, hasEntered: true);

			return new(@lock, hasEntered: false);
		}
		#endregion

		#region Write methods
		/// <summary>Enters a read lock.</summary>
		/// <returns>A scope, which when disposed, will release the read lock.</returns>
		public WriteScope WriteLock()
		{
			@lock.EnterWriteLock();
			return new(@lock, hasEntered: true);
		}

		/// <summary>Tries to enter a write lock.</summary>
		/// <param name="timeout">
		/// The maximum amount of time to allow before giving up on entering the
		/// write lock. A value of -1 millisecond represents an infinite timeout.
		/// </param>
		/// <returns>A scope, which when disposed, will release the write lock.</returns>
		/// <remarks>The property <see cref="WriteScope.HasEntered"/> on the returned scope can be checked to see if the write lock was entered.</remarks>
		public WriteScope WriteLock(TimeSpan timeout)
		{
			if (@lock.TryEnterWriteLock(timeout))
				return new(@lock, hasEntered: true);

			return new(@lock, hasEntered: false);
		}
		#endregion
	}
}
