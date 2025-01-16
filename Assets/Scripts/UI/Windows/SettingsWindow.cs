using System;
using System.Linq;
using Core.Activatable;
using Core.PersistentManager;
using Core.SoundManager;
using Core.WindowManager.Template;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Zenject;

namespace UI.Windows
{
	[RequireComponent(typeof(RawImage))]
	public sealed class SettingsWindow : BasePopupWindow<DialogButtonType>
	{
		public const string Id = nameof(SettingsWindow);

		private const float AppearDuration = 0.7f;
		private const float DisappearDuration = 0.5f;

		[Header("Sound"), SerializeField] private Toggle _soundOn;
		[SerializeField] private Toggle _musicOn;
		[Header("Language"), SerializeField] private Toggle _eng;
		[SerializeField] private Toggle _rus;

		[Inject] private readonly IPersistentManager _persistentManager;
		[Inject] private readonly ISoundManager _soundManager;

		private Color _initialBlendColor;
		private Tween _tween;

		private void Awake()
		{
			_initialBlendColor = Blend.color;
		}

		protected override string GetWindowId()
		{
			return Id;
		}

		protected override void DoSetArgs(object[] args)
		{
		}

		protected override void DoActivate(bool immediately)
		{
			if (this.IsActiveOrActivated())
			{
				return;
			}

			var canvasGroup = Popup.GetComponent<CanvasGroup>();
			_tween?.Kill();
			if (immediately)
			{
				_tween = null;
				Blend.color = _initialBlendColor;
				canvasGroup.alpha = 1;
				canvasGroup.interactable = true;
				Popup.localScale = Vector3.one;
				ActivatableState = ActivatableState.Active;
			}
			else
			{
				Blend.color = new Color(_initialBlendColor.r, _initialBlendColor.g, _initialBlendColor.b, 0f);
				canvasGroup.alpha = 0f;
				canvasGroup.interactable = false;
				Popup.localScale = Vector3.one * 0.01f;
				ActivatableState = ActivatableState.ToActive;
				_tween = DOTween.Sequence()
					.Append(Blend.DOFade(_initialBlendColor.a, AppearDuration).SetEase(Ease.Linear))
					.Join(canvasGroup.DOFade(1f, AppearDuration * 0.3f).SetEase(Ease.Linear))
					.Join(Popup.DOScale(Vector3.one, AppearDuration).SetEase(Ease.OutBack))
					.OnComplete(() =>
					{
						_tween = null;
						canvasGroup.interactable = true;
						ActivatableState = ActivatableState.Active;
					});
			}
		}

		protected override void DoDeactivate(bool immediately)
		{
			if (this.IsInactiveOrDeactivated())
			{
				return;
			}

			var canvasGroup = Popup.GetComponent<CanvasGroup>();
			canvasGroup.interactable = false;
			_tween?.Kill();
			if (immediately)
			{
				_tween = null;
				Blend.color = new Color(_initialBlendColor.r, _initialBlendColor.g, _initialBlendColor.b, 0f);
				Popup.localScale = Vector3.one * 0.01f;
				ActivatableState = ActivatableState.Inactive;
			}
			else
			{
				Blend.color = _initialBlendColor;
				canvasGroup.alpha = 1f;
				Popup.localScale = Vector3.one;
				ActivatableState = ActivatableState.ToInactive;
				_tween = DOTween.Sequence()
					.Append(Blend.DOFade(0f, DisappearDuration).SetEase(Ease.Linear))
					.Join(Popup.DOScale(Vector3.one * 0.01f, DisappearDuration).SetEase(Ease.InBack))
					.OnComplete(() =>
					{
						_tween = null;
						ActivatableState = ActivatableState.Inactive;
					});
			}
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
			_tween?.Kill();

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

		private void OnValidate()
		{
			Assert.IsNotNull(_soundOn, "_soundOn != null");
			Assert.IsNotNull(_musicOn, "_musicOn != null");
			Assert.IsNotNull(_eng, "_eng != null");
			Assert.IsNotNull(_rus, "_rus != null");
			Assert.IsNotNull(Popup, "Popup != null");
			Assert.IsNotNull(Popup.GetComponent<CanvasGroup>(), "Popup must have CanvasGroup.");
		}
	}
}