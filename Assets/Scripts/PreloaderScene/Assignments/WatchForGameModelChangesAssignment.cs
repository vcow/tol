using System;
using Core.Assignments;
using Core.PersistentManager;
using Models;
using UniRx;
using UnityEngine.Assertions;

namespace PreloaderScene.Assignments
{
	public class WatchForGameModelChangesAssignment : IAssignment, IDisposable
	{
		private readonly CompositeDisposable _disposables = new();
		private readonly IPersistentManager _persistentManager;
		private readonly GameModelController _gameModelController;

		private bool _completed;

		public WatchForGameModelChangesAssignment(IPersistentManager persistentManager, GameModelController gameModelController)
		{
			_persistentManager = persistentManager;
			_gameModelController = gameModelController;
		}

		public void Start()
		{
			_gameModelController.ObserveModelChanged
				.ThrottleFrame(1)
				.Subscribe(OnModelChanged)
				.AddTo(_disposables);
			Completed = true;
		}

		private void OnModelChanged(Unit _)
		{
			var rawData = _gameModelController.ToString();
			_persistentManager.SetString(Const.GameModelPersistentKey, rawData);
		}

		public bool Completed
		{
			get => _completed;
			private set
			{
				if (value == _completed)
				{
					return;
				}

				_completed = value;
				Assert.IsTrue(value);
				CompleteEvent?.Invoke(this);
			}
		}

		public event AssignmentCompleteHandler CompleteEvent;

		public void Dispose()
		{
			_disposables.Dispose();
		}
	}
}