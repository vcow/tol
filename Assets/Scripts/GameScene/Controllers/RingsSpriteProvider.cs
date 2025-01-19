using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;

namespace GameScene.Controllers
{
	[CreateAssetMenu(fileName = "RingsSpriteProvider", menuName = "Game/Rings Sprite Provider")]
	public class RingsSpriteProvider : ScriptableObject
	{
		[Serializable]
		private class Record
		{
			public RingColor _ringType;
			public Sprite _sprite;
		}

		[SerializeField] private Record[] _sprites = Array.Empty<Record>();

		private readonly Lazy<Dictionary<RingColor, Sprite>> _spritesMap;

		public RingsSpriteProvider()
		{
			_spritesMap = new Lazy<Dictionary<RingColor, Sprite>>(() => _sprites.ToDictionary(record => record._ringType, record => record._sprite));
		}

		public IReadOnlyDictionary<RingColor, Sprite> SpritesMap => _spritesMap.Value;
	}
}