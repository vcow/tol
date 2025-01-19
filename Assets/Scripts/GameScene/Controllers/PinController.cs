using UnityEngine;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent, RequireComponent(typeof(Collider))]
	public sealed class PinController : MonoBehaviour
	{
		[SerializeField] private int _towerIndex;
		[SerializeField] private int _pinIndex;
	}
}