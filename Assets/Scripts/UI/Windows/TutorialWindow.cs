using Core.Activatable;
using Core.Utils.TouchHelper;
using Core.WindowManager.Template;
using DG.Tweening;
using UnityEngine;

namespace UI.Windows
{
	[DisallowMultipleComponent, RequireComponent(typeof(CanvasGroup))]
	public sealed class TutorialWindow : BasePopupWindow<DialogButtonType>
	{
		public const string Id = nameof(TutorialWindow);

		private const float AppearDuration = 1f;
		private const float DisappearDuration = 1f;

		private Tween _tween;
		private int? _lock;

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

			_tween?.Kill();
			var canvasGroup = GetComponent<CanvasGroup>();
			canvasGroup.alpha = 0f;
			_tween = canvasGroup.DOFade(1f, AppearDuration).SetEase(Ease.Linear)
				.OnComplete(() =>
				{
					_tween = null;
					ActivatableState = ActivatableState.Active;
				});

			ActivatableState = ActivatableState.ToActive;
		}

		protected override void DoDeactivate(bool immediately)
		{
			if (this.IsInactiveOrDeactivated())
			{
				return;
			}

			_tween?.Kill();
			var canvasGroup = GetComponent<CanvasGroup>();
			_tween = canvasGroup.DOFade(0f, DisappearDuration).SetEase(Ease.Linear)
				.OnComplete(() =>
				{
					_tween = null;
					ActivatableState = ActivatableState.Inactive;
				});

			ActivatableState = ActivatableState.ToInactive;
		}

		private void Start()
		{
			_lock = TouchHelper.Lock();
		}

		protected override void OnDestroy()
		{
			_tween?.Kill();
			if (_lock.HasValue)
			{
				TouchHelper.Unlock(_lock.Value);
			}

			base.OnDestroy();
		}

		public void OnClose()
		{
			Close();
		}
	}
}