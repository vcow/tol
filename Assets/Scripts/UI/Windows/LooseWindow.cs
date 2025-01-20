using Core.Utils.TouchHelper;
using Core.WindowManager.Template;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace UI.Windows
{
	public sealed class LooseWindow : BaseScaleFxPopup<DialogButtonType>
	{
		public const string Id = nameof(LooseWindow);

		[SerializeField] private TextMeshProUGUI _playerName;

		[Inject] private readonly IPlayerModel _playerModel;

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
			_lock = TouchHelper.Lock();

			Result = DialogButtonType.None;
			_playerName.text = _playerModel.Name;
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

		public void OnReplay()
		{
			Result = DialogButtonType.Ok;
			Close();
		}

		protected override void OnValidate()
		{
			Assert.IsNotNull(_playerName, "_playerName != null");
			base.OnValidate();
		}
	}
}