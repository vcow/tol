using System.Collections.Generic;
using Core.Utils;
using Core.Utils.TouchHelper;
using Models;
using Settings;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class SceneViewController : MonoBehaviour
	{
		[SerializeField] private Transform _catchMarker;
		[Header("Towers"), SerializeField] private TowerBaseController _tower1;
		[SerializeField] private TowerBaseController _tower2;
		[SerializeField] private TowerBaseController _tower3;

		[Inject] private readonly LevelModel _levelModel;
		[Inject] private readonly GameSettings _gameSettings;
		[Inject] private readonly DiContainer _container;

		private GameObject _caughtObject;
		private Camera _camera;

		private void Start()
		{
			_catchMarker.gameObject.SetActive(false);

			_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			Assert.IsNotNull(_camera);

			var towersHeight = _levelModel.NumColors;
			_tower1.Height = towersHeight;
			_tower2.Height = towersHeight;
			_tower3.Height = towersHeight;

			PopulateRingsFromInitialState(_levelModel.InitialState.tower1, _tower1);
			PopulateRingsFromInitialState(_levelModel.InitialState.tower2, _tower2);
			PopulateRingsFromInitialState(_levelModel.InitialState.tower3, _tower3);
		}

		private void PopulateRingsFromInitialState(IReadOnlyList<RingColor> rings, TowerBaseController tower)
		{
			var yOffset = 0f;
			var baseBounds = tower.Bounds;
			foreach (var ringColor in rings)
			{
				var ringPrefab = _gameSettings.GetRingSettings(ringColor).prefab;
				var ringInstance = _container.InstantiatePrefab(ringPrefab);
				var ringBounds = ringInstance.GetBounds();
				var ringPosition = new Vector3(baseBounds.center.x,
					baseBounds.max.y + ringBounds.center.y - ringBounds.min.y + yOffset,
					baseBounds.center.z);
				yOffset += ringBounds.size.y;
				ringInstance.transform.position = ringPosition;
			}
		}

		private void Update()
		{
			if (TouchHelper.GetTouch(out var touch))
			{
				if (touch.phase == TouchPhase.Began)
				{
					CatchObject(touch.position);
				}
				else if (touch.phase == TouchPhase.Moved && _caughtObject != null)
				{
					MoveCaughtObject(touch.position);
				}
				else if (_caughtObject != null)
				{
					ReleaseCaughtObject();
				}
			}
			else if (_caughtObject != null)
			{
				ReleaseCaughtObject();
			}
		}

		private void CatchObject(Vector2 touchPoint)
		{
			if (_caughtObject != null)
			{
				ReleaseCaughtObject();
			}

			var ray = _camera.ScreenPointToRay(touchPoint);
			if (!Physics.Raycast(ray, out var ringHitInfo, _camera.farClipPlane, LayerMask.GetMask("Ring")))
			{
				return;
			}

			if (!Physics.Raycast(ray, out var forceHitInfo, _camera.farClipPlane, LayerMask.GetMask("ForceRaycaster")))
			{
				Debug.LogWarning("Can't cast ForceRaycaster collider.");
				return;
			}

			var caughtRing = ringHitInfo.transform.GetComponent<RingController>();
			Assert.IsNotNull(caughtRing);

			if (_catchMarker)
			{
				var ringBounds = caughtRing.gameObject.GetBounds();
				var normalVector = ringHitInfo.point - ringBounds.center;
				var angXZ = Mathf.Atan2(normalVector.x, normalVector.z);

				_catchMarker.position = ringHitInfo.point;
				_catchMarker.localRotation = Quaternion.Euler(0, angXZ * Mathf.Rad2Deg, 0);

				_catchMarker.gameObject.SetActive(true);
			}
		}

		private void MoveCaughtObject(Vector2 touchPoint)
		{
			Assert.IsNotNull(_caughtObject);

			var ray = _camera.ScreenPointToRay(touchPoint);
			if (!Physics.Raycast(ray, out var hitInfo, _camera.farClipPlane, LayerMask.GetMask("ForceRaycaster")))
			{
				Debug.LogWarning("Can't cast ForceRaycaster collider.");
				ReleaseCaughtObject();
				return;
			}
		}

		private void ReleaseCaughtObject()
		{
			Assert.IsNotNull(_caughtObject);

			if (_catchMarker)
			{
				_catchMarker.gameObject.SetActive(false);
			}
		}

		private void OnValidate()
		{
			Assert.IsNotNull(_tower1, "_tower1 != null");
			Assert.IsNotNull(_tower2, "_tower2 != null");
			Assert.IsNotNull(_tower3, "_tower3 != null");
		}
	}
}