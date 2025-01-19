using UnityEngine;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent, RequireComponent(typeof(Collider))]
	public sealed class PinController : MonoBehaviour
	{
		[field: SerializeField] public int TowerIndex { get; private set; }
		[field: SerializeField] public int PinIndex { get; private set; }
	}
}