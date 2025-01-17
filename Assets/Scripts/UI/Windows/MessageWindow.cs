using System;
using System.Linq;
using Core.WindowManager.Template;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Zenject;

namespace UI.Windows
{
	public sealed class MessageWindow : BaseScaleFxPopup<DialogButtonType>
	{
		[Serializable]
		public class ButtonRecord
		{
			public DialogButtonType _type;
			public Button _prefab;
		}

		public const string Id = nameof(MessageWindow);

		[SerializeField] private TextMeshProUGUI _title;
		[SerializeField] private TextMeshProUGUI _message;
		[Header("Buttons"), SerializeField] private Transform _buttonsContainer;
		[SerializeField] private ButtonRecord[] _buttonPrefabs;

		[Inject] private readonly DiContainer _container;

		private DialogButtonType _buttons = DialogButtonType.Ok;
		private string _titleKey;
		private string _messageKey;
		private object[] _messageArgs;

		protected override string GetWindowId()
		{
			return Id;
		}

		protected override void DoSetArgs(object[] args)
		{
			foreach (var arg in args)
			{
				switch (arg)
				{
					case DialogButtonType buttons:
						_buttons = buttons;
						break;
					case (string title, string message):
						_titleKey = title;
						_messageKey = message;
						break;
					case object[] messageArgs:
						_messageArgs = messageArgs;
						break;
					default:
						throw new NotSupportedException($"The argument of type {arg.GetType()} isn't supported in the MessageWindow.");
				}
			}
		}

		private void Start()
		{
			Result = DialogButtonType.None;

			var title = string.IsNullOrEmpty(_titleKey) ? string.Empty : LocalizationSettings.StringDatabase.GetLocalizedString(_titleKey);
			var message = string.IsNullOrEmpty(_messageKey) ? string.Empty : LocalizationSettings.StringDatabase.GetLocalizedString(_messageKey);
			if (_messageArgs != null)
			{
				message = string.Format(message, _messageArgs);
			}

			_title.text = title;
			_message.text = message;

			TryAddButton(DialogButtonType.Ok);
			TryAddButton(DialogButtonType.Yes);
			TryAddButton(DialogButtonType.No);
			TryAddButton(DialogButtonType.Cancel);

			return;

			void TryAddButton(DialogButtonType buttonType)
			{
				if ((_buttons & buttonType) == 0)
				{
					return;
				}

				var prefab = _buttonPrefabs.Single(record => record._type == buttonType)._prefab;
				var button = _container.InstantiatePrefab(prefab.gameObject, _buttonsContainer).GetComponent<Button>();
				button.gameObject.name = $"{buttonType}Button";
				button.onClick.AddListener(() =>
				{
					Result = buttonType;
					Close();
				});
			}
		}

		protected override void OnDestroy()
		{
			foreach (Transform child in _buttonsContainer)
			{
				var button = child.GetComponent<Button>();
				button.onClick.RemoveAllListeners();
			}

			base.OnDestroy();
		}

		protected override void OnValidate()
		{
			Assert.IsNotNull(_title, "_title != null");
			Assert.IsNotNull(_message, "_message != null");
			Assert.IsNotNull(_buttonsContainer, "_buttonsContainer != null");

			base.OnValidate();
		}
	}
}