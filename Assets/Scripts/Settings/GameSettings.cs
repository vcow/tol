using System;
using System.Linq;
using Models;
using UnityEngine;
using Zenject;

namespace Settings
{
	[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings")]
	public sealed class GameSettings : ScriptableObjectInstaller<GameSettings>
	{
		[Serializable]
		private class RingRecord
		{
			public RingColor _ringType;
			public Color _ringColor;
			public GameObject _ringPrefab;
		}

		[Header("Rings Settings"), SerializeField] private RingRecord[] _rings;

		public override void InstallBindings()
		{
			Container.Bind<GameSettings>().FromInstance(this).AsSingle();
		}

		public (Color color, GameObject prefab) GetRingSettings(RingColor ringType)
		{
			var record = _rings.First(record => record._ringType == ringType);
			return (record._ringColor, record._ringPrefab);
		}
	}
}