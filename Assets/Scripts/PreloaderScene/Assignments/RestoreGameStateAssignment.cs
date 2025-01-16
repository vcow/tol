using System;
using System.Linq;
using Core.Assignments;
using Core.PersistentManager;
using Core.SoundManager;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace PreloaderScene.Assignments
{
	public class RestoreGameStateAssignment : IAssignment
	{
		private readonly IPersistentManager _persistentManager;
		private readonly ISoundManager _soundManager;

		private bool _completed;

		public RestoreGameStateAssignment(IPersistentManager persistentManager, ISoundManager soundManager)
		{
			_persistentManager = persistentManager;
			_soundManager = soundManager;
		}

		public async void Start()
		{
			await RestoreSound();
			await RestoreLocalLanguage();
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

		private async UniTask RestoreSound()
		{
			var soundIsOn = await _persistentManager.GetBool(Const.SoundOnPersistentKey, true);
			var musicIsOn = await _persistentManager.GetBool(Const.MusicOnPersistentKey, true);
			_soundManager.SoundIsOn = soundIsOn;
			_soundManager.MusicIsOn = musicIsOn;
		}

		private async UniTask RestoreLocalLanguage()
		{
			var value = await _persistentManager.GetString(Const.LanguagePersistentKey, Application.systemLanguage.ToString());
			var lang = Enum.TryParse<SystemLanguage>(value, out var result) ? result : Application.systemLanguage;
			var locale = lang switch
			{
				SystemLanguage.Russian => LocalizationSettings.AvailableLocales.Locales.First(locale => locale.Identifier.Code == "ru-RU"),
				_ => LocalizationSettings.AvailableLocales.Locales.First(locale => locale.Identifier.Code == "en-US")
			};

			if (locale != await LocalizationSettings.SelectedLocaleAsync)
			{
				await ChangeLocale(locale);
			}
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
	}
}