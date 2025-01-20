using System.Linq;
using Core.WindowManager.Template;
using Models;
using StartScene.Signals;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Zenject;

namespace UI.Windows
{
	public class NewPlayerWindow : BaseScaleFxPopup<DialogButtonType>
	{
		public const string Id = nameof(NewPlayerWindow);

		private const float DisabledButtonsAlpha = 0.5f;

		private const int MinNameLength = 3;

		private readonly CompositeDisposable _disposables = new();

		[SerializeField] private TMP_InputField _nameInput;
		[SerializeField] private Button _okButton;

		[Inject] private readonly IGameModel _gameModel;
		[Inject] private readonly SignalBus _signalBus;

		private string[] _names;
		private CanvasGroup _okButtonCanvasGroup;

		private void Start()
		{
			_okButtonCanvasGroup = _okButton.GetComponent<CanvasGroup>();
			Assert.IsTrue(_okButtonCanvasGroup);

			_names = _gameModel.Players.Select(model => model.Name).ToArray();

			OnNameValueChanged(string.Empty);
		}

		protected override string GetWindowId()
		{
			return Id;
		}

		protected override void DoSetArgs(object[] args)
		{
		}

		public void OnNameValueChanged(string value)
		{
			if (value.Length >= MinNameLength && !_names.Contains(value))
			{
				_okButton.interactable = true;
				_okButtonCanvasGroup.alpha = 1f;
			}
			else
			{
				_okButton.interactable = false;
				_okButtonCanvasGroup.alpha = DisabledButtonsAlpha;
			}
		}

		public void OnClose()
		{
			Close();
		}

		public void OnOk()
		{
			var newPlayerName = _nameInput.text;
			_gameModel.Players.ObserveAdd()
				.First(evt => evt.Value.Name == newPlayerName)
				.Subscribe(evt => _signalBus.TryFire(new StartPlayGameSignal(evt.Value.Name, 0)))
				.AddTo(_disposables);

			Close();
			_signalBus.TryFire(new AddPlayerSignal(newPlayerName));
		}

		protected override void OnDestroy()
		{
			_disposables.Dispose();
			base.OnDestroy();
		}

		protected override void OnValidate()
		{
			Assert.IsNotNull(_nameInput, "_nameInput != null");
			Assert.IsNotNull(_okButton, "_okButton != null");

			base.OnValidate();
		}
	}
}