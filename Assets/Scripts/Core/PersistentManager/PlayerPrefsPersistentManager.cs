using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.PersistentManager
{
	public sealed class PlayerPrefsPersistentManager : IPersistentManager
	{
		public UniTask<bool> GetBool(string key, bool defaultValue)
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return UniTask.FromResult(defaultValue);
			}

			var value = PlayerPrefs.GetInt(key);
			return UniTask.FromResult(value > 0);
		}

		public UniTask<string> GetString(string key, string defaultValue)
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return UniTask.FromResult(defaultValue);
			}

			return UniTask.FromResult(PlayerPrefs.GetString(key));
		}

		public UniTask<int> GetInt(string key, int defaultValue)
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return UniTask.FromResult(defaultValue);
			}

			return UniTask.FromResult(PlayerPrefs.GetInt(key));
		}

		public void SetBool(string key, bool value)
		{
			PlayerPrefs.SetInt(key, value ? 1 : 0);
		}

		public void SetInt(string key, int value)
		{
			PlayerPrefs.SetInt(key, value);
		}

		public void SetString(string key, string value)
		{
			PlayerPrefs.SetString(key, value);
		}
	}
}