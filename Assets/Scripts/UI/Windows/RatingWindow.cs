using System.Linq;
using Core.WindowManager.Template;
using Models;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace UI.Windows
{
	public sealed class RatingWindow : BaseScaleFxPopup<DialogButtonType>
	{
		public const string Id = nameof(RatingWindow);

		[SerializeField] private Transform _listContainer;
		[SerializeField] private RatingListItemController _listItemPrefab;

		[Inject] private readonly IGameModel _gameModel;
		[Inject] private readonly DiContainer _container;

		protected override string GetWindowId()
		{
			return Id;
		}

		protected override void DoSetArgs(object[] args)
		{
		}

		public void OnCLose()
		{
			Close();
		}

		private void Start()
		{
			var sortedList = _gameModel.Players.OrderByDescending(model => model.Scores.Value);
			foreach (var playerModel in sortedList)
			{
				_container.InstantiatePrefabForComponent<RatingListItemController>(_listItemPrefab, _listContainer,
					new object[] { playerModel });
			}
		}

		protected override void OnValidate()
		{
			Assert.IsNotNull(_listContainer, "_listContainer != null");
			Assert.IsNotNull(_listItemPrefab, "_listItemPrefab != null");

			base.OnValidate();
		}
	}
}