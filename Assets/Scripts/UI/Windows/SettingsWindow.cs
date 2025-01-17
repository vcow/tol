using System;
using System.Linq;
using Core.PersistentManager;
using Core.SoundManager;
using Core.WindowManager.Template;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Zenject;

namespace UI.Windows
{
	public sealed class SettingsWindow : BaseScaleFxPopup<DialogButtonType>
	{
		public const string Id = nameof(SettingsWindow);

		[Header("Sound"), SerializeField] private Toggle _soundOn;
		[SerializeField] private Toggle _musicOn;
		[Header("Language"), SerializeField] private Toggle _eng;
		[SerializeField] private Toggle _rus;

		[Inject] private readonly IPersistentManager _persistentManager;
		[Inject] private readonly ISoundManager _soundManager;

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

		private async void Start()
		{
			var soundIsOn = await _persistentManager.GetBool(Const.SoundOnPersistentKey, true);
			var musicIsOn = await _persistentManager.GetBool(Const.MusicOnPersistentKey, true);

			var value = await _persistentManager.GetString(Const.LanguagePersistentKey, Application.systemLanguage.ToString());
			var lang = Enum.TryParse<SystemLanguage>(value, out var result) ? result : Application.systemLanguage;

			_soundOn.isOn = soundIsOn;
			_musicOn.isOn = musicIsOn;

			_soundOn.onValueChanged.AddListener(val =>
			{
				_persistentManager.SetBool(Const.SoundOnPersistentKey, val);
				_soundManager.SoundIsOn = val;
			});

			_musicOn.onValueChanged.AddListener(val =>
			{
				_persistentManager.SetBool(Const.MusicOnPersistentKey, val);
				_soundManager.MusicIsOn = val;
			});

			if (lang == SystemLanguage.Russian)
			{
				_rus.isOn = true;
			}
			else
			{
				_eng.isOn = true;
			}

			_rus.onValueChanged.AddListener(val =>
			{
				if (val)
				{
					SetLocalization(SystemLanguage.Russian);
				}
			});

			_eng.onValueChanged.AddListener(val =>
			{
				if (val)
				{
					SetLocalization(SystemLanguage.English);
				}
			});
		}

		protected override void OnDestroy()
		{
			_soundOn.onValueChanged.RemoveAllListeners();
			_musicOn.onValueChanged.RemoveAllListeners();

			_rus.onValueChanged.RemoveAllListeners();
			_eng.onValueChanged.RemoveAllListeners();

			base.OnDestroy();
		}

		private async void SetLocalization(SystemLanguage language)
		{
			var locale = language switch
			{
				SystemLanguage.Russian => LocalizationSettings.AvailableLocales.Locales.First(locale => locale.Identifier.Code == "ru-RU"),
				_ => LocalizationSettings.AvailableLocales.Locales.First(locale => locale.Identifier.Code == "en-US")
			};

			var canvasGroup = Popup.GetComponent<CanvasGroup>();
			canvasGroup.interactable = false;
			if (locale != await LocalizationSettings.SelectedLocaleAsync)
			{
				await ChangeLocale(locale);
			}

			canvasGroup.interactable = true;
			_persistentManager.SetString(Const.LanguagePersistentKey, language.ToString());
		}

		private UniTask ChangeLocale(Locale locale)
		{
			var result = new UniTaskCompletionSource();
			LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
			LocalizationSettings.SelectedLocale = locale;

			return result.Task;

			void OnSelectedLocaleChanged(Locale newLocale)
			{
				Assert.IsTrue(newLocale == locale);
				LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
				result.TrySetResult();
			}
		}

		protected override void OnValidate()
		{
			Assert.IsNotNull(_soundOn, "_soundOn != null");
			Assert.IsNotNull(_musicOn, "_musicOn != null");
			Assert.IsNotNull(_eng, "_eng != null");
			Assert.IsNotNull(_rus, "_rus != null");

			base.OnValidate();
		}
	}
}