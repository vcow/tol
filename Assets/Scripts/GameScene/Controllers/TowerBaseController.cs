using UnityEngine;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class TowerBaseController : MonoBehaviour
	{
		[SerializeField] private Collider _base;
		[SerializeField] private GameObject[] _pins;

		private int? _height;

		public Bounds Bounds => _base.bounds;

		public int Height
		{
			get => _height ?? 0;
			set
			{
				if (value == _height)
				{
					return;
				}

				_height = value;
				if (value > _pins.Length)
				{
					Debug.LogError($"Max height for the TowerBase is {_pins.Length}. Received {value}.");
				}

				var actualHeight = Mathf.Min(value, _pins.Length);
				for (var i = 0; i < _pins.Length; ++i)
				{
					var pin = _pins[i];
					pin.SetActive(i < actualHeight);
				}
			}
		}

		private void Start()
		{
			if (!_height.HasValue)
			{
				Height = 1;
			}
		}
	}
}