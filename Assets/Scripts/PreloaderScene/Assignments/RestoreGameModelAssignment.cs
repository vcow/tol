using Core.Assignments;
using Core.PersistentManager;
using Models;
using UnityEngine.Assertions;

namespace PreloaderScene.Assignments
{
	public class RestoreGameModelAssignment : IAssignment
	{
		private readonly IPersistentManager _persistentManager;
		private readonly GameModelController _gameModelController;

		private bool _completed;

		public RestoreGameModelAssignment(IPersistentManager persistentManager, GameModelController gameModelController)
		{
			_persistentManager = persistentManager;
			_gameModelController = gameModelController;
		}

		public async void Start()
		{
			var rawData = await _persistentManager.GetString(Const.GameModelPersistentKey, string.Empty);
			if (!string.IsNullOrEmpty(rawData))
			{
				_gameModelController.RestoreGameModel(rawData);
			}

			Completed = true;
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
	}
}