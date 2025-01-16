using UnityEngine;

namespace Utils
{
	[DisallowMultipleComponent]
	public sealed class DontDestroyOnLoad : MonoBehaviour
	{
		private void Start()
		{
			DontDestroyOnLoad(gameObject);
		}
	}
}