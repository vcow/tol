using System;
using Core.Activatable;
using Core.ScreenLocker;
using DG.Tweening;
using UnityEngine;

namespace UI.ScreenLockers
{
	[DisallowMultipleComponent, RequireComponent(typeof(CanvasGroup))]
	public sealed class GameScreenLocker : BaseScreenLocker
	{
		private const float MinLockDelayTimeSec = 1f;
		private const float DeactivateDuration = 1f;

		private readonly Lazy<CanvasGroup> _canvasGroup;

		private Tween _tween;
		private float _activateTime;

		public GameScreenLocker()
		{
			_canvasGroup = new Lazy<CanvasGroup>(GetComponent<CanvasGroup>);
		}

		public override void Activate(bool immediately = false)
		{
			_tween?.Kill();
			_tween = null;

			gameObject.SetActive(true);
			_canvasGroup.Value.alpha = 1;

			_activateTime = Time.time;
			ActivatableState = ActivatableState.Active;
		}

		public override void Deactivate(bool immediately = false)
		{
			_tween?.Kill();
			if (immediately)
			{
				_tween = null;
				gameObject.SetActive(false);
				_canvasGroup.Value.alpha = 0;
				ActivatableState = ActivatableState.Inactive;
			}
			else
			{
				_tween = _canvasGroup.Value.DOFade(0f, DeactivateDuration).SetEase(Ease.Linear)
					.OnComplete(() =>
					{
						_tween = null;
						gameObject.SetActive(false);
						ActivatableState = ActivatableState.Inactive;
					});

				var delayTimeSec = MinLockDelayTimeSec - (Time.time - _activateTime);
				if (delayTimeSec > 0.01f)
				{
					_tween.SetDelay(delayTimeSec);
				}

				ActivatableState = ActivatableState.ToInactive;
			}
		}

		public override LockerType LockerType => LockerType.GameLoader;

		public override void Force()
		{
			switch (ActivatableState)
			{
				case ActivatableState.ToInactive:
					Deactivate(true);
					break;
			}
		}
	}
}