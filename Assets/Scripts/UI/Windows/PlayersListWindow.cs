using System;
using Core.WindowManager;
using Core.WindowManager.Template;
using Models;
using Settings;
using StartScene.Signals;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Zenject;

namespace UI.Windows
{
	public sealed class PlayersListWindow : BaseScaleFxPopup<DialogButtonType>
	{
		public const string Id = nameof(PlayersListWindow);

		private readonly CompositeDisposable _disposables = new();

		[Header("Buttons"), SerializeField] private Button _continueButton;
		[SerializeField] private Button _replayButton;
		[SerializeField] private Button _deleteButton;

		[Header("List"), SerializeField] private Transform _listContainer;
		[SerializeField] private PlayerListItemController _listItemPrefab;
		[SerializeField] private ToggleGroup _toggleGroup;

		[Inject] private readonly DiContainer _container;
		[Inject] private readonly IGameModel _gameModel;
		[Inject] private readonly SignalBus _signalBus;
		[Inject] private readonly IWindowManager _windowManager;
		[Inject] private readonly LevelsProvider _levelsProvider;

		private IPlayerModel _selectedItem;

		protected override string GetWindowId()
		{
			return Id;
		}

		protected override void DoSetArgs(object[] args)
		{
		}

		public void OnClose()
		{
			Close();
		}

		public void OnContinue()
		{
			Assert.IsNotNull(_selectedItem);
			_signalBus.TryFire(new StartPlayGameSignal(_selectedItem.Name, _selectedItem.LastLevel.Value + 1));
		}

		public void OnReplay()
		{
			Assert.IsNotNull(_selectedItem);
		}

		public void OnDelete()
		{
			Assert.IsNotNull(_selectedItem);
			var messageWindow = (MessageWindow)_windowManager.ShowWindow(MessageWindow.Id,
				new object[]
				{
					("delete_player_title", "delete_player_message"),
					new object[] { _selectedItem.Name },
					DialogButtonType.YesNo
				});
			IDisposable closeHandler = null;
			closeHandler = Observable.FromEvent<CloseWindowHandler, (IWindow window, DialogButtonType result)>(
					h => (window, result) => h((window, result)),
					h => messageWindow.CloseWindowEvent += h,
					h => messageWindow.CloseWindowEvent -= h)
				.Subscribe(tuple =>
				{
					// ReSharper disable once AccessToModifiedClosure
					_disposables.Remove(closeHandler);

					if (tuple.result == DialogButtonType.Yes)
					{
						_signalBus.TryFire(new RemovePlayerSignal(_selectedItem.Name));
					}
				})
				.AddTo(_disposables);
		}

		private void Start()
		{
			DisableAllButtons();

			_gameModel.Players.ObserveAdd().Subscribe(OnAddItem).AddTo(_disposables);
			_gameModel.Players.ObserveRemove().Subscribe(OnRemoveItem).AddTo(_disposables);
			for (var i = 0; i < _gameModel.Players.Count; ++i)
			{
				var playerModel = _gameModel.Players[i];
				CreateListItem(playerModel, i == 0);
			}
		}

		private void OnAddItem(CollectionAddEvent<IPlayerModel> evt)
		{
			CreateListItem(evt.Value, false);
		}

		private void OnRemoveItem(CollectionRemoveEvent<IPlayerModel> evt)
		{
			foreach (Transform child in _listContainer)
			{
				var item = child.GetComponent<PlayerListItemController>();
				if (item.PlayerModel != evt.Value)
				{
					continue;
				}

				item.Toggle.onValueChanged.RemoveAllListeners();
				if (item.Toggle.isOn)
				{
					DisableAllButtons();
					_selectedItem = null;
				}

				Destroy(item.gameObject);
				break;
			}
		}

		private void DisableAllButtons()
		{
			_continueButton.interactable = false;
			_deleteButton.interactable = false;
			_replayButton.interactable = false;
		}

		private void CreateListItem(IPlayerModel playerModel, bool selectItem)
		{
			var item = _container.InstantiatePrefabForComponent<PlayerListItemController>(
				_listItemPrefab, _listContainer, new object[]
				{
					playerModel
				});
			item.Toggle.isOn = selectItem;
			item.Toggle.group = _toggleGroup;
			item.Toggle.onValueChanged.AddListener(b =>
			{
				if (b)
				{
					OnSelectItem(playerModel);
				}
			});

			if (selectItem)
			{
				OnSelectItem(playerModel);
			}
		}

		private void OnSelectItem(IPlayerModel playerModel)
		{
			Assert.IsNotNull(playerModel);
			_selectedItem = playerModel;

			_continueButton.interactable = playerModel.LastLevel.Value < _levelsProvider.Levels.Count;
			_deleteButton.interactable = true;
			_replayButton.interactable = playerModel.LastLevel.Value > 0;
		}

		protected override void OnDestroy()
		{
			_disposables.Dispose();

			foreach (Transform child in _listContainer)
			{
				var item = child.GetComponent<PlayerListItemController>();
				item.Toggle.onValueChanged.RemoveAllListeners();
			}

			base.OnDestroy();
		}

		protected override void OnValidate()
		{
			Assert.IsNotNull(_continueButton, "_continueButton != null");
			Assert.IsNotNull(_replayButton, "_replayButton != null");
			Assert.IsNotNull(_deleteButton, "_deleteButton != null");
			Assert.IsNotNull(_listContainer, "_listContainer != null");
			Assert.IsNotNull(_listItemPrefab, "_listItemPrefab != null");
			Assert.IsNotNull(_toggleGroup, "_toggleGroup != null");

			base.OnValidate();
		}
	}
}