using Core.Activatable;
using Core.WindowManager.Template;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

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

		protected override void OnDestroy()
		{
			_tween?.Kill();
			base.OnDestroy();
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