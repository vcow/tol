using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class StateGUIController : MonoBehaviour
	{
		[SerializeField] private TowerGUIController _tower1;
		[SerializeField] private TowerGUIController _tower2;
		[SerializeField] private TowerGUIController _tower3;

		public TowerGUIController this[int index] => index switch
		{
			0 => _tower1,
			1 => _tower2,
			2 => _tower3,
			_ => throw new IndexOutOfRangeException()
		};

		private void OnValidate()
		{
			Assert.IsNotNull(_tower1, "_tower1 != null");
			Assert.IsNotNull(_tower2, "_tower2 != null");
			Assert.IsNotNull(_tower3, "_tower3 != null");
		}
	}
}