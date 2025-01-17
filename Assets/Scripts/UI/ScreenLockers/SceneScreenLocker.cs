using System;
using Core.Activatable;
using Core.ScreenLocker;
using DG.Tweening;
using UnityEngine;

namespace UI.ScreenLockers
{
	[DisallowMultipleComponent, RequireComponent(typeof(CanvasGroup))]
	public sealed class SceneScreenLocker : BaseScreenLocker
	{
		private const float AppearDuration = 0.7f;
		private const float DisappearDuration = 0.7f;

		private Tween _tween;
		private readonly Lazy<CanvasGroup> _canvasGroup;

		public SceneScreenLocker()
		{
			_canvasGroup = new Lazy<CanvasGroup>(GetComponent<CanvasGroup>);
		}

		public override void Activate(bool immediately = false)
		{
			if (this.IsActiveOrActivated())
			{
				return;
			}

			_tween?.Kill();
			if (immediately)
			{
				_tween = null;
				_canvasGroup.Value.alpha = 1f;
				ActivatableState = ActivatableState.Active;
			}
			else
			{
				_canvasGroup.Value.alpha = 0f;
				_tween = _canvasGroup.Value.DOFade(1f, AppearDuration).SetEase(Ease.Linear)
					.OnComplete(() =>
					{
						_tween = null;
						ActivatableState = ActivatableState.Active;
					});
				ActivatableState = ActivatableState.ToActive;
			}
		}

		public override void Deactivate(bool immediately = false)
		{
			if (this.IsInactiveOrDeactivated())
			{
				return;
			}

			_tween?.Kill();
			if (immediately)
			{
				_tween = null;
				_canvasGroup.Value.alpha = 0f;
				ActivatableState = ActivatableState.Inactive;
			}
			else
			{
				_canvasGroup.Value.alpha = 1f;
				_tween = _canvasGroup.Value.DOFade(0f, DisappearDuration).SetEase(Ease.Linear)
					.OnComplete(() =>
					{
						_tween = null;
						ActivatableState = ActivatableState.Inactive;
					});
				ActivatableState = ActivatableState.ToInactive;
			}
		}

		public override LockerType LockerType => LockerType.SceneLoader;

		public override void Force()
		{
			_tween?.Complete(true);
			switch (ActivatableState)
			{
				case ActivatableState.ToActive:
					_canvasGroup.Value.alpha = 1f;
					ActivatableState = ActivatableState.Active;
					break;
				case ActivatableState.ToInactive:
					_canvasGroup.Value.alpha = 0f;
					ActivatableState = ActivatableState.Inactive;
					break;
			}
		}

		protected override void OnDestroy()
		{
			_tween?.Kill();
			base.OnDestroy();
		}
	}
}