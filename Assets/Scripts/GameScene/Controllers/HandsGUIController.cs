using DG.Tweening;
using GameScene.Logic;
using GameScene.Signals;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Zenject;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class HandsGUIController : MonoBehaviour
	{
		private const float HandBlinkDuration = 0.5f;
		private const float HandBlinkDelay = 3f;

		private readonly CompositeDisposable _disposables = new();

		[SerializeField] private Image _hand;
		[SerializeField] private Image _ring;
		[SerializeField] private RingsSpriteProvider _ringsSpriteProvider;

		[Inject] private readonly GameLogic _gameLogic;
		[Inject] private readonly SignalBus _signalBus;

		private Tween _tween;

		private void Start()
		{
			_gameLogic.Hands.Subscribe(OnHandsChanged).AddTo(_disposables);

			_signalBus.Subscribe<CatchRingSignal>(StopHandBlink);
			_signalBus.Subscribe<ThrowRingSignal>(StartHandBlink);
		}

		private void OnHandsChanged(RingColor? ringInHands)
		{
			if (ringInHands.HasValue)
			{
				var sprite = _ringsSpriteProvider.SpritesMap[ringInHands.Value];
				_ring.sprite = sprite;
				gameObject.SetActive(true);
			}
			else
			{
				StopHandBlink();
				gameObject.SetActive(false);
			}
		}

		private void StartHandBlink()
		{
			if (_tween != null)
			{
				Debug.LogError("Hand blink wasn't stopped correctly.");
				_tween.Kill();
			}

			_hand.color = new Color(1f, 1f, 1f, 0f);
			_tween = _hand.DOFade(1f, HandBlinkDuration).SetEase(Ease.InQuad)
				.SetLoops(-1, LoopType.Yoyo).SetDelay(HandBlinkDelay);
		}

		private void StopHandBlink()
		{
			if (_tween != null)
			{
				_tween.Kill();
				_tween = null;
			}

			_hand.color = Color.white;
		}

		private void OnDestroy()
		{
			_tween?.Kill();

			_signalBus.Unsubscribe<CatchRingSignal>(StopHandBlink);
			_signalBus.Unsubscribe<ThrowRingSignal>(StartHandBlink);

			_disposables.Dispose();
		}

		private void OnValidate()
		{
			Assert.IsNotNull(_hand, "_hand != null");
			Assert.IsNotNull(_ring, "_ring != null");
			Assert.IsNotNull(_ringsSpriteProvider, "_ringsSpriteProvider != null");
		}
	}
}