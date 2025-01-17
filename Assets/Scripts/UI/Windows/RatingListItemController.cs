using System;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Windows
{
	[DisallowMultipleComponent]
	public sealed class RatingListItemController : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _name;
		[SerializeField] private TextMeshProUGUI _scores;

		[Inject] private readonly IPlayerModel _playerModel;

		private readonly Lazy<Toggle> _toggle;

		private void Start()
		{
			_name.text = _playerModel.Name;
			_scores.text = _playerModel.Scores.Value.ToString();
		}
	}
}