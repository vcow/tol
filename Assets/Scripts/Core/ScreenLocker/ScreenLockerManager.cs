using System;
using System.Collections.Generic;
using System.Linq;
using Core.Activatable;
using UnityEngine;
using UnityEngine.Assertions;
using Utils.TouchHelper;
using Zenject;
using Object = UnityEngine.Object;

namespace Core.ScreenLocker
{
	public sealed class ScreenLockerManager : IScreenLockerManager, IDisposable
	{
		private readonly DiContainer _container;
		private readonly Dictionary<LockerType, BaseScreenLocker> _screenLockerPrefabs;
		private readonly Dictionary<LockerType, BaseScreenLocker> _activeLockers = new();
		private readonly Dictionary<BaseScreenLocker, Action> _lockCompleteCallbacks = new();
		private readonly Dictionary<BaseScreenLocker, Action<LockerType>> _unlockCompleteCallbacks = new();

		private int _lockId;

		public ScreenLockerManager(DiContainer container, IScreenLockerSettings settings)
		{
			_container = container;

			_screenLockerPrefabs = settings.ScreenLockers != null
				? settings.ScreenLockers.GroupBy(record => record.LockerType)
					.Select(lockers =>
					{
						var locker = lockers.First();
#if DEBUG || UNITY_EDITOR
						var numLockers = lockers.Count();
						if (numLockers > 1)
						{
							Debug.LogErrorFormat("There are {0} lockers, specified for the {1} type.",
								numLockers, locker.LockerType);
						}
#endif
						return locker;
					})
					.ToDictionary(locker => locker.LockerType)
				: new Dictionary<LockerType, BaseScreenLocker>();
		}

		void IDisposable.Dispose()
		{
			foreach (var activeLocker in _activeLockers.Values)
			{
				if (!activeLocker) continue;

				activeLocker.Force();

				activeLocker.ActivatableStateChangedEvent -= OnLockerStateChanged;
				Object.Destroy(activeLocker.gameObject);
			}

			_activeLockers.Clear();
			_lockCompleteCallbacks.Clear();
			_unlockCompleteCallbacks.Clear();
		}

		private void OnLockerStateChanged(IActivatable activatable, ActivatableState state)
		{
			var locker = (BaseScreenLocker)activatable;

			switch (state)
			{
				case ActivatableState.Active: // locked
					locker.ActivatableStateChangedEvent -= OnLockerStateChanged;

					if (_lockCompleteCallbacks.Remove(locker, out var lockCallback))
					{
						lockCallback.Invoke();
					}

					break;
				case ActivatableState.Inactive: // unlocked
					locker.ActivatableStateChangedEvent -= OnLockerStateChanged;

					if (_activeLockers.TryGetValue(locker.LockerType, out var activeLocker) &&
					    activeLocker == locker)
					{
						_activeLockers.Remove(locker.LockerType);
					}

					if (_unlockCompleteCallbacks.Remove(locker, out var unlockCallback))
					{
						unlockCallback.Invoke(locker.LockerType);
					}

					Object.Destroy(locker.gameObject);

					IsLocked = _activeLockers.Count > 0;
					break;
			}
		}

		public void SetScreenLocker(LockerType type, BaseScreenLocker baseScreenLockerPrefab)
		{
			_screenLockerPrefabs[type] = baseScreenLockerPrefab;
		}

		// 	IScreenLockerManager

		public bool IsLocked
		{
			get => _lockId > 0;
			private set
			{
				if (value == IsLocked) return;

				if (value)
				{
					Assert.IsTrue(_lockId == 0);
					_lockId = TouchHelper.Lock();
				}
				else
				{
					Assert.IsTrue(_lockId > 0);
					TouchHelper.Unlock(_lockId);
					_lockId = 0;
				}
			}
		}

		public void Lock(LockerType type, Action completeCallback)
		{
			if (_activeLockers.TryGetValue(type, out var oldLocker))
			{
				oldLocker.Force();

				oldLocker.ActivatableStateChangedEvent -= OnLockerStateChanged;
				Object.Destroy(oldLocker.gameObject);

				if (_activeLockers.Remove(type))
				{
					// This locker should have be removed from the active lockers in the OnLockerStateChanged handler,
					// if not then he isn't send the ActivatableStateChangedEvent during the Force() call.
					Debug.LogWarningFormat("The locker of type {0} hasn't change his activatable state during the " +
					                       "Force() action.", oldLocker.LockerType);
				}

				if (_lockCompleteCallbacks.Remove(oldLocker, out var oldLockCallback))
				{
					oldLockCallback.Invoke();
				}

				if (_unlockCompleteCallbacks.Remove(oldLocker, out var oldUnlockCallback))
				{
					oldUnlockCallback.Invoke(oldLocker.LockerType);
				}
			}

			if (!_screenLockerPrefabs.TryGetValue(type, out var prefab))
			{
				Debug.LogErrorFormat("There is no screen prefab for the {0} lock type.", type);
				IsLocked = _activeLockers.Count > 0;
				completeCallback?.Invoke();
				return;
			}

			var locker = _container.InstantiatePrefab(prefab).GetComponent<BaseScreenLocker>();
			if (!locker)
			{
				throw new Exception("Screen locker must implements IScreenLocker.");
			}

			_activeLockers.Add(type, locker);

			IsLocked = true;

			if (locker.IsInactive())
			{
				if (completeCallback != null)
				{
					_lockCompleteCallbacks.Add(locker, completeCallback);
				}

				locker.ActivatableStateChangedEvent += OnLockerStateChanged;
				locker.Activate();
			}
			else if (locker.IsActive())
			{
				completeCallback?.Invoke();
			}
			else
			{
				Debug.LogErrorFormat("The locker {0} is in wrong initial state {1}.",
					locker.LockerType, locker.ActivatableState);
				completeCallback?.Invoke();
			}
		}

		public void Unlock(Action<LockerType> completeCallback, LockerType? type = null)
		{
			var unlocked = new List<BaseScreenLocker>();
			if (type.HasValue)
			{
				if (_activeLockers.TryGetValue(type.Value, out var locker))
				{
					unlocked.Add(locker);
				}
			}
			else
			{
				unlocked.AddRange(_activeLockers.Values);
			}

			if (unlocked.Count <= 0)
			{
				completeCallback?.Invoke(LockerType.Undefined);
				return;
			}

			foreach (var locker in unlocked)
			{
				if (!locker.IsActive())
				{
					locker.Force();

					if (_lockCompleteCallbacks.TryGetValue(locker, out var lockCallback))
					{
						// This callback should be called and removed from the lock complete callbacks in the
						// OnLockerStateChanged handler, if not then locker isn't send ActivatableStateChanged event
						// during the Force() call.
						Debug.LogWarningFormat("The locker of type {0} hasn't change his activatable state during " +
						                       "the Force() action.", locker.LockerType);
						_lockCompleteCallbacks.Remove(locker);
						lockCallback.Invoke();
					}
				}

				if (locker.IsActive())
				{
					if (completeCallback != null)
					{
						_unlockCompleteCallbacks.Add(locker, completeCallback);
					}

					locker.ActivatableStateChangedEvent += OnLockerStateChanged;
					locker.Deactivate();
				}
				else
				{
					Debug.LogErrorFormat("The locker {0} that is been unlocked wasn't switched to the Active state.",
						locker.LockerType);

					completeCallback?.Invoke(locker.LockerType);

					locker.ActivatableStateChangedEvent -= OnLockerStateChanged;
					Object.Destroy(locker.gameObject);

					_activeLockers.Remove(locker.LockerType);
					_lockCompleteCallbacks.Remove(locker);
					_unlockCompleteCallbacks.Remove(locker);
				}
			}

			IsLocked = _activeLockers.Count > 0;
		}

		// 	\IScreenLockerManager
	}
}