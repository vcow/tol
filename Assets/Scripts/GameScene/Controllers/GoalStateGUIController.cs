using Models;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class GoalStateGUIController : MonoBehaviour
	{
		[SerializeField] private StateGUIController _state;

		[Inject] private readonly LevelModel _levelModel;

		private void Start()
		{
			var (tower1, tower2, tower3) = _levelModel.GoalState;
			var height = _levelModel.NumColors;
			_state[0].SetState(tower1, height);
			_state[1].SetState(tower2, height);
			_state[2].SetState(tower3, height);
		}

		private void OnValidate()
		{
			Assert.IsNotNull(_state, "_state != null");
		}
	}
}