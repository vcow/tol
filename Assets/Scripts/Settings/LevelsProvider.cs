using System.Collections.Generic;
using Core.Assignments;
using Models;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Settings
{
	[CreateAssetMenu(fileName = "LevelsProvider", menuName = "Game/Levels Provider")]
	public class LevelsProvider : ScriptableObjectInstaller<LevelsProvider>, IAssignment
	{
		[SerializeField] private TextAsset[] _levels;

		private readonly Dictionary<int, LevelModel> _levelModels = new();
		private bool _completed;

		public override void InstallBindings()
		{
			Container.Bind<LevelsProvider>().FromInstance(this).AsSingle();
		}

		void IAssignment.Start()
		{
			for (var i = 0; i < _levels.Length; ++i)
			{
				var levelAsset = _levels[i];
				var levelModel = new LevelModel(i, levelAsset.text);
				_levelModels.Add(i, levelModel);
			}

			Completed = true;
		}

		public IReadOnlyDictionary<int, LevelModel> Levels => _levelModels;

		public bool Completed
		{
			get => _completed;
			private set
			{
				if (value == _completed)
				{
					return;
				}

				_completed = value;
				Assert.IsTrue(_completed);
				CompleteEvent?.Invoke(this);
			}
		}

		public event AssignmentCompleteHandler CompleteEvent;
	}
}