using Core.Utils.TouchHelper;
using Core.WindowManager.Template;
using Models;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Zenject;

namespace UI.Windows
{
	public sealed class WinWindow : BaseScaleFxPopup<DialogButtonType>
	{
		private const float DisabledButtonsAlpha = 0.5f;

		public const string Id = nameof(WinWindow);

		[SerializeField] private TextMeshProUGUI _playerName;
		[SerializeField] private TextMeshProUGUI _scores;
		[SerializeField] private Button _nextButton;

		[Inject] private readonly IPlayerModel _playerModel;
		[Inject] private readonly LevelsProvider _levelsProvider;

		private int? _lock;

		protected override string GetWindowId()
		{
			return Id;
		}

		protected override void DoSetArgs(object[] args)
		{
		}

		private void Start()
		{
			Result = DialogButtonType.None;
			_lock = TouchHelper.Lock();

			var nextButtonCanvasGroup = _nextButton.GetComponent<CanvasGroup>();
			Assert.IsNotNull(nextButtonCanvasGroup);

			if (_playerModel.LastLevel.Value < _levelsProvider.Levels.Count - 1)
			{
				_nextButton.interactable = true;
				nextButtonCanvasGroup.alpha = 1f;
			}
			else
			{
				_nextButton.interactable = false;
				nextButtonCanvasGroup.alpha = DisabledButtonsAlpha;
			}

			_playerName.text = _playerModel.Name;
			_scores.text = _playerModel.Scores.Value.ToString();
		}

		protected override void OnDestroy()
		{
			if (_lock.HasValue)
			{
				TouchHelper.Unlock(_lock.Value);
			}

			base.OnDestroy();
		}

		public void OnExit()
		{
			Result = DialogButtonType.Cancel;
			Close();
		}

		public void OnPlayNext()
		{
			Result = DialogButtonType.Ok;
			Close();
		}

		protected override void OnValidate()
		{
			Assert.IsNotNull(_playerName, "_playerName != null");
			Assert.IsNotNull(_scores, "_scores != null");
			Assert.IsNotNull(_nextButton, "_nextButton != null");
			Assert.IsNotNull(_scores, "_scores != null");

			base.OnValidate();
		}
	}
}