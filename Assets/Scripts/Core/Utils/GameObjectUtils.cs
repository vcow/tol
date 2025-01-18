using UnityEngine;

namespace Core.Utils
{
	public static class GameObjectUtils
	{
		public static Bounds GetBounds(this GameObject obj)
		{
			var colliders = obj.GetComponentsInChildren<Collider>();
			var result = new Bounds(obj.transform.position, Vector3.zero);
			foreach (var c in colliders)
			{
				result.Encapsulate(c.bounds);
			}

			return result;
		}
	}
}