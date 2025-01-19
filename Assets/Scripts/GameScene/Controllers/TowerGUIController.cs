using System.Collections.Generic;
using Models;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class TowerGUIController : MonoBehaviour
	{
		[SerializeField] private GameObject[] _pins;
		[SerializeField] private Image[] _rings;
		[SerializeField] private RingsSpriteProvider _ringsSpriteProvider;

		private int _towerHeight;

		public void SetState(IEnumerable<RingColor> state, int? towerHeight = null)
		{
			if (towerHeight.HasValue)
			{
				_towerHeight = towerHeight.Value;
				Assert.IsTrue(_towerHeight > 0 && _towerHeight <= _pins.Length);

				for (var i = 0; i < _pins.Length; ++i)
				{
					var pin = _pins[i];
					pin.gameObject.SetActive(i < _towerHeight);
				}
			}

			using (var stateEnumerator = state.GetEnumerator())
			{
				foreach (var ring in _rings)
				{
					if (stateEnumerator.MoveNext())
					{
						var sprite = _ringsSpriteProvider.SpritesMap[stateEnumerator.Current];
						ring.sprite = sprite;
						ring.gameObject.SetActive(true);
					}
					else
					{
						ring.gameObject.SetActive(false);
					}
				}
			}
		}

		private void OnValidate()
		{
			Assert.IsNotNull(_ringsSpriteProvider, "_ringsSpriteProvider != null");
		}
	}
}