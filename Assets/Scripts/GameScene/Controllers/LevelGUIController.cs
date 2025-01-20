using Models;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class LevelGUIController : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _level;

		[Inject] private readonly LevelModel _levelModel;

		private void Start()
		{
			_level.text = (_levelModel.Index + 1).ToString();
		}

		private void OnValidate()
		{
			Assert.IsNotNull(_level, "_level != null");
		}
	}
}