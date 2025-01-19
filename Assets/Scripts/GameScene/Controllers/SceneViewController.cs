using System.Collections.Generic;
using Core.Utils;
using Core.Utils.TouchHelper;
using GameScene.Logic;
using GameScene.Signals;
using Models;
using Settings;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class SceneViewController : MonoBehaviour
	{
		[SerializeField] private CinemachineTargetGroup _cinemachineTargetGroup;
		[SerializeField] private GameObject _catchMarker;
		[Header("Towers"), SerializeField] private TowerBaseController _tower1;
		[SerializeField] private TowerBaseController _tower2;
		[SerializeField] private TowerBaseController _tower3;

		[Inject] private readonly LevelModel _levelModel;
		[Inject] private readonly GameSettings _gameSettings;
		[Inject] private readonly DiContainer _container;
		[Inject] private readonly SignalBus _signalBus;
		[Inject] private readonly GameLogic _gameLogic;

		private (GameObject handler, RingController ring) _caught;
		private Camera _camera;
		private float _dragPlaneDistance;
		private Vector3 _startDragPosition;
		private Vector3 _startMousePosition;

		private void Start()
		{
			_catchMarker.SetActive(false);

			_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			Assert.IsNotNull(_camera);

			var towersHeight = _levelModel.NumColors;
			_tower1.Height = towersHeight;
			_tower2.Height = towersHeight;
			_tower3.Height = towersHeight;

			PopulateRingsFromInitialState(_levelModel.InitialState.tower1, _tower1);
			PopulateRingsFromInitialState(_levelModel.InitialState.tower2, _tower2);
			PopulateRingsFromInitialState(_levelModel.InitialState.tower3, _tower3);
			_cinemachineTargetGroup.DoUpdate();

			_signalBus.Subscribe<ThrowRingSignal>(OnThrowRing);
		}

		private void OnDestroy()
		{
			_signalBus.Unsubscribe<ThrowRingSignal>(OnThrowRing);
		}

		private void OnThrowRing()
		{
			Assert.IsFalse(_caught == default);
			ThrowRing();
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

				_cinemachineTargetGroup.Targets.Add(new CinemachineTargetGroup.Target
				{
					Object = ringInstance.transform,
					Radius = 1f,
					Weight = 1f
				});
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
				else if (touch.phase == TouchPhase.Moved && _caught != default)
				{
					MoveCaughtObject(touch.position);
				}
				else if (_caught != default)
				{
					ReleaseCaughtObject();
				}
			}
			else if (_caught != default)
			{
				ReleaseCaughtObject();
			}
		}

		private void CatchObject(Vector2 touchPoint)
		{
			if (_caught != default)
			{
				ReleaseCaughtObject();
			}

			var ray = _camera.ScreenPointToRay(touchPoint);
			if (!Physics.Raycast(ray, out var ringHitInfo, _camera.farClipPlane, LayerMask.GetMask("Ring")))
			{
				return;
			}

			var caughtRing = ringHitInfo.transform.GetComponent<RingController>();
			Assert.IsNotNull(caughtRing);

			if (!_gameLogic.CanTakeRing(caughtRing.RingType))
			{
				_signalBus.TryFire<CatchWrongRingSignal>();
				return;
			}

			if (_catchMarker)
			{
				var markerTransform = _catchMarker.transform;
				markerTransform.position = ringHitInfo.point;
				_catchMarker.SetActive(true);
				caughtRing.JoinTo(markerTransform);
				_caught = (_catchMarker, caughtRing);
			}
			else
			{
				var catcher = new GameObject("Catcher").GetComponent<Transform>();
				catcher.position = ringHitInfo.point;
				caughtRing.JoinTo(catcher);
				_caught = (catcher.gameObject, caughtRing);
			}

			_startDragPosition = _caught.handler.transform.position;
			_dragPlaneDistance = (_startDragPosition - _camera.transform.position).magnitude;
			_startMousePosition = _camera.ScreenToWorldPoint(new Vector3(touchPoint.x, touchPoint.y, _dragPlaneDistance));
		}

		private void MoveCaughtObject(Vector2 touchPoint)
		{
			Assert.IsFalse(_caught == default);

			var position = _camera.ScreenToWorldPoint(new Vector3(touchPoint.x, touchPoint.y, _dragPlaneDistance));
			_caught.handler.transform.position = _startDragPosition + (position - _startMousePosition);
		}

		private void ReleaseCaughtObject()
		{
			Assert.IsFalse(_caught == default);
			_caught.ring.Release();
			ThrowRing();
		}

		private void ThrowRing()
		{
			if (_catchMarker)
			{
				Assert.IsTrue(_caught.handler == _catchMarker);
				_catchMarker.SetActive(false);
			}
			else
			{
				Destroy(_caught.handler);
			}

			_caught = default;
		}

		private void OnValidate()
		{
			Assert.IsNotNull(_tower1, "_tower1 != null");
			Assert.IsNotNull(_tower2, "_tower2 != null");
			Assert.IsNotNull(_tower3, "_tower3 != null");
			Assert.IsNotNull(_cinemachineTargetGroup, "_cinemachineTargetGroup != null");
		}
	}
}