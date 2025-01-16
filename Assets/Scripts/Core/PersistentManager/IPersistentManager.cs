using Cysharp.Threading.Tasks;

namespace Core.PersistentManager
{
	public interface IPersistentManager
	{
		UniTask<bool> GetBool(string key, bool defaultValue);
		UniTask<int> GetInt(string key, int defaultValue);
		UniTask<string> GetString(string key, string defaultValue);
		void SetBool(string key, bool value);
		void SetInt(string key, int value);
		void SetString(string key, string value);
	}
}