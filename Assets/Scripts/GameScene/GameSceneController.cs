using System;
using Core.ScreenLocker;
using Core.SoundManager;
using Core.WindowManager;
using Core.WindowManager.Template;
using GameScene.Logic;
using GameScene.Signals;
using Models;
using Settings;
using UI.Windows;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene
{
	[DisallowMultipleComponent]
	public sealed class GameSceneController : MonoBehaviour
	{
		private readonly CompositeDisposable _disposables = new();

		[Inject] private readonly IScreenLockerManager _screenLockerManager;
		[Inject] private readonly IWindowManager _windowManager;
		[Inject] private readonly ISoundManager _soundManager;
		[Inject] private readonly GameLogic _gameLogic;
		[Inject] private readonly PlayerModelController _playerModelController;
		[Inject] private readonly LevelsProvider _levelsProvider;
		[Inject] private readonly ZenjectSceneLoader _sceneLoader;
		[Inject] private readonly SignalBus _signalBus;
		[InjectOptional] private readonly bool _doNotShowTutorial;

		private void Start()
		{
			if (_screenLockerManager.IsLocked)
			{
				_screenLockerManager.Unlock(OnSceneUnlock);
			}
			else
			{
				OnSceneUnlock(LockerType.Undefined);
			}

			_gameLogic.GameResult.First(result => result == GameLogic.Result.Win).Subscribe(_ => OnWin()).AddTo(_disposables);
			_gameLogic.GameResult.First(result => result == GameLogic.Result.Lose).Subscribe(_ => OnLose()).AddTo(_disposables);

			_signalBus.Subscribe<CatchWrongRingSignal>(OnCatchWrongRing);
			_gameLogic.Step.Skip(1).Subscribe(_ => _soundManager.PlaySound("bell_ding")).AddTo(_disposables);

			_soundManager.PlayMusic("Together");
		}

		private void OnCatchWrongRing()
		{
			_soundManager.PlaySound("error_sound");
		}

		private void OnDestroy()
		{
			_signalBus.Unsubscribe<CatchWrongRingSignal>(OnCatchWrongRing);
			_disposables.Dispose();
		}

		private void OnSceneUnlock(LockerType _)
		{
			if (!_doNotShowTutorial && _playerModelController.Model.LastLevel.Value < 0)
			{
				_windowManager.ShowWindow(TutorialWindow.Id);
			}
		}

		public void OnOpenSettings()
		{
			_windowManager.ShowWindow(SettingsWindow.Id);
		}

		private void OnWin()
		{
			var winWindow = (WinWindow)_windowManager.ShowWindow(WinWindow.Id);
			IDisposable closeHandler = null;
			closeHandler = Observable.FromEvent<BaseWindow<BasePopupWindow<DialogButtonType>, DialogButtonType>.CloseWindowHandler, (IWindow window, DialogButtonType result)>(
					h => (window, result) => h((window, result)),
					h => winWindow.CloseWindowEvent += h,
					h => winWindow.CloseWindowEvent -= h)
				.Subscribe(tuple =>
				{
					// ReSharper disable once AccessToModifiedClosure
					_disposables.Remove(closeHandler);
					switch (tuple.result)
					{
						case DialogButtonType.Ok:
							PlayLevel(_playerModelController.Model.LastLevel.Value + 1);
							break;
						case DialogButtonType.Cancel:
							ReturnToStartScene();
							break;
						default:
							throw new NotSupportedException($"Result {tuple.result} isn't supported.");
					}
				})
				.AddTo(_disposables);
		}

		private void OnLose()
		{
			var looseWindow = (LooseWindow)_windowManager.ShowWindow(LooseWindow.Id);
			IDisposable closeHandler = null;
			closeHandler = Observable.FromEvent<BaseWindow<BasePopupWindow<DialogButtonType>, DialogButtonType>.CloseWindowHandler, (IWindow window, DialogButtonType result)>(
					h => (window, result) => h((window, result)),
					h => looseWindow.CloseWindowEvent += h,
					h => looseWindow.CloseWindowEvent -= h)
				.Subscribe(tuple =>
				{
					// ReSharper disable once AccessToModifiedClosure
					_disposables.Remove(closeHandler);
					switch (tuple.result)
					{
						case DialogButtonType.Ok:
							PlayLevel(_playerModelController.Model.LastLevel.Value + 1);
							break;
						case DialogButtonType.Cancel:
							ReturnToStartScene();
							break;
						default:
							throw new NotSupportedException($"Result {tuple.result} isn't supported.");
					}
				})
				.AddTo(_disposables);
		}

		private void ReturnToStartScene()
		{
			Assert.IsFalse(_screenLockerManager.IsLocked);
			_screenLockerManager.Lock(LockerType.SceneLoader,
				() => _sceneLoader.LoadSceneAsync(Const.StartSceneName));
		}

		private void PlayLevel(int levelIndex)
		{
			if (!_levelsProvider.Levels.TryGetValue(levelIndex, out var levelModel))
			{
				Debug.LogError($"Can't find level {levelIndex}.");
				ReturnToStartScene();
				return;
			}

			Assert.IsFalse(_screenLockerManager.IsLocked);
			_screenLockerManager.Lock(LockerType.SceneLoader,
				() =>
				{
					_sceneLoader.LoadSceneAsync(Const.GameSceneName, extraBindings: container =>
					{
						container.Bind<bool>().FromInstance(true).AsTransient(); // _doNotShowTutorial
						container.Bind<PlayerModelController>().FromInstance(_playerModelController).AsSingle();
						container.Bind<LevelModel>().FromInstance(levelModel).AsSingle();
					});
				}
			);
		}
	}
}