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

		private const int MinNameLength = 3;

		private readonly CompositeDisposable _disposables = new();

		[SerializeField] private TMP_InputField _nameInput;
		[SerializeField] private Button _okButton;

		[Inject] private readonly IGameModel _gameModel;
		[Inject] private readonly SignalBus _signalBus;

		private string[] _names;

		private void Start()
		{
			_names = _gameModel.Players.Select(model => model.Name).ToArray();
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
			var isValid = value.Length >= MinNameLength && !_names.Contains(value);
			_okButton.interactable = isValid;
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