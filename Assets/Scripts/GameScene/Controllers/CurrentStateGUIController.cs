using GameScene.Logic;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class CurrentStateGUIController : MonoBehaviour
	{
		private readonly CompositeDisposable _disposables = new();

		[SerializeField] private StateGUIController _state;

		[Inject] private readonly GameLogic _gameLogic;

		private void Start()
		{
			var height = _gameLogic.NumColors;
			_state[0].SetState(_gameLogic.Tower1, height);
			_state[1].SetState(_gameLogic.Tower2, height);
			_state[2].SetState(_gameLogic.Tower3, height);

			_gameLogic.Tower1.ObserveCountChanged()
				.Subscribe(_ => _state[0].SetState(_gameLogic.Tower1))
				.AddTo(_disposables);
			_gameLogic.Tower2.ObserveCountChanged()
				.Subscribe(_ => _state[1].SetState(_gameLogic.Tower2))
				.AddTo(_disposables);
			_gameLogic.Tower3.ObserveCountChanged()
				.Subscribe(_ => _state[2].SetState(_gameLogic.Tower3))
				.AddTo(_disposables);
		}

		private void OnDestroy()
		{
			_disposables.Dispose();
		}

		private void OnValidate()
		{
			Assert.IsNotNull(_state, "_state != null");
		}
	}
}