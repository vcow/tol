using GameScene.Logic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class StepsGUIController : MonoBehaviour
	{
		private readonly CompositeDisposable _disposables = new();

		[SerializeField] private TextMeshProUGUI _value;

		[Inject] private readonly GameLogic _gameLogic;

		private void Start()
		{
			_gameLogic.Step.Subscribe(i => _value.text = $"{i}/{_gameLogic.MaxSteps}").AddTo(_disposables);
		}

		private void OnDestroy()
		{
			_disposables.Dispose();
		}

		private void OnValidate()
		{
			Assert.IsNotNull(_value, "_value != null");
		}
	}
}