using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class HandsGUIController : MonoBehaviour
	{
		[SerializeField] private Image _hand;
		[SerializeField] private Image _ring;
		[SerializeField] private RingsSpriteProvider _ringsSpriteProvider;

		private void OnValidate()
		{
			Assert.IsNotNull(_hand, "_hand != null");
			Assert.IsNotNull(_ring, "_ring != null");
			Assert.IsNotNull(_ringsSpriteProvider, "_ringsSpriteProvider != null");
		}
	}
}