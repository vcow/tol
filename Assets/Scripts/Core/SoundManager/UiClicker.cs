using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Core.SoundManager
{
	/// <summary>
	/// This component reproduces the "click" sound when user pointer up and/or down on the UI element.
	/// </summary>
	[DisallowMultipleComponent]
	public sealed class UiClicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField] private string _mouseDownSound;
		[SerializeField] private string _mouseUpSound;
		[SerializeField] private string _mouseEnterSound;
		[SerializeField] private string _mouseExitSound;

		[Inject] private readonly ISoundManager _soundManager;

		public void OnPointerDown(PointerEventData eventData)
		{
			if (string.IsNullOrEmpty(_mouseDownSound))
			{
				return;
			}

			var selectable = GetComponent<Selectable>();
			if (!selectable || selectable.interactable)
			{
				_soundManager.PlaySound(_mouseDownSound);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (string.IsNullOrEmpty(_mouseUpSound))
			{
				return;
			}

			var selectable = GetComponent<Selectable>();
			if (!selectable || selectable.interactable)
			{
				_soundManager.PlaySound(_mouseUpSound);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (string.IsNullOrEmpty(_mouseEnterSound))
			{
				return;
			}

			var selectable = GetComponent<Selectable>();
			if (!selectable || selectable.interactable)
			{
				_soundManager.PlaySound(_mouseEnterSound);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (string.IsNullOrEmpty(_mouseExitSound))
			{
				return;
			}

			var selectable = GetComponent<Selectable>();
			if (!selectable || selectable.interactable)
			{
				_soundManager.PlaySound(_mouseExitSound);
			}
		}
	}
}