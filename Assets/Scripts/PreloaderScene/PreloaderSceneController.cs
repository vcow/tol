using Core.Assignments;
using Core.ScreenLocker;
using PreloaderScene.Assignments;
using Settings;
using UnityEngine;
using Zenject;

namespace PreloaderScene
{
	public sealed class PreloaderSceneController : MonoBehaviour
	{
		[Inject] private readonly IScreenLockerManager _screenLockerManager;
		[Inject] private readonly ZenjectSceneLoader _sceneLoader;

		[Inject] private readonly RestoreGameStateAssignment _restoreGameStateAssignment;
		[Inject] private readonly RestoreGameModelAssignment _restoreGameModelAssignment;
		[Inject] private readonly WatchForGameModelChangesAssignment _watchForGameModelChangesAssignment;
		[Inject] private readonly LevelsProvider _levelsProvider;

		private void Start()
		{
			_screenLockerManager.Lock(LockerType.GameLoader, DoInitialize);
		}

		private void DoInitialize()
		{
			var initializeQueue = new AssignmentQueue();

			initializeQueue.Add(_levelsProvider);
			initializeQueue.Add(_restoreGameStateAssignment);
			initializeQueue.Add(_restoreGameModelAssignment);
			initializeQueue.Add(_watchForGameModelChangesAssignment);
			// TODO: Add initialized objects here.

			initializeQueue.CompleteEvent += OnInitializeComplete;

			initializeQueue.Start();
		}

		private async void OnInitializeComplete(IAssignment assignment)
		{
			assignment.CompleteEvent -= OnInitializeComplete;
			await _sceneLoader.LoadSceneAsync(Const.StartSceneName);
		}
	}
}