using System;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Windows
{
	[DisallowMultipleComponent, RequireComponent(typeof(Toggle))]
	public sealed class PlayerListItemController : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _name;
		[SerializeField] private TextMeshProUGUI _scores;

		[Inject] private readonly IPlayerModel _playerModel;

		private readonly Lazy<Toggle> _toggle;

		public Toggle Toggle => _toggle.Value;
		public IPlayerModel PlayerModel => _playerModel;

		public PlayerListItemController()
		{
			_toggle = new Lazy<Toggle>(GetComponent<Toggle>);
		}

		private void Start()
		{
			_name.text = _playerModel.Name;
			_scores.text = _playerModel.Scores.Value.ToString();
		}
	}
}