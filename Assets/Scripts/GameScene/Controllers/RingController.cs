using System.Collections.Generic;
using GameScene.Signals;
using UniRx;
using UnityEngine;
using UnityEngine.Animations;
using Zenject;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class RingController : MonoBehaviour
	{
		[SerializeField] private Transform[] _anchors;
		[SerializeField] private float _spring;
		[SerializeField] private float _breakForce;
		[SerializeField] private float _damper;

		[Inject] private readonly SignalBus _signalBus;

		private readonly CompositeDisposable _disposables = new();
		private readonly List<GameObject> _connections = new();
		private readonly Subject<Unit> _breakJoinObservable = new();

		private void Start()
		{
			var ringName = gameObject.name;
			for (var i = 0; i < _anchors.Length; ++i)
			{
				var connect = new GameObject($"{ringName}_Spring{i + 1}Connect",
					typeof(Rigidbody), typeof(ParentConstraint));
				var rigidbody = connect.GetComponent<Rigidbody>();
				rigidbody.isKinematic = true;
				rigidbody.useGravity = false;
				_connections.Add(connect);
			}

			_breakJoinObservable.ThrottleFrame(1)
				.Subscribe(_ => _signalBus.TryFire<ThrowRingSignal>())
				.AddTo(_disposables);
		}

		private void OnDestroy()
		{
			_breakJoinObservable.OnCompleted();
			_disposables.Dispose();
		}

		public void JoinTo(Transform connectedObject)
		{
			Release();

			for (var i = 0; i < _anchors.Length; ++i)
			{
				var anchor = _anchors[i];
				var connection = _connections[i];

				connection.transform.position = anchor.position;

				var spring = gameObject.AddComponent<SpringJoint>();
				spring.anchor = anchor.localPosition;
				spring.connectedBody = connection.GetComponent<Rigidbody>();
				spring.spring = _spring;
				spring.breakForce = _breakForce;
				spring.damper = _damper;

				var constraint = connection.GetComponent<ParentConstraint>();
				ClearConstraintSources(constraint);
				var offset = anchor.position - connectedObject.position;
				constraint.AddSource(new ConstraintSource
				{
					sourceTransform = connectedObject,
					weight = 1f
				});
				constraint.SetTranslationOffset(0, offset);
				constraint.constraintActive = true;
			}
		}

		private void OnJointBreak(float breakForce)
		{
			Release();
			_breakJoinObservable.OnNext(Unit.Default);
		}

		public void Release()
		{
			foreach (var connection in _connections)
			{
				var constraint = connection.GetComponent<ParentConstraint>();
				constraint.constraintActive = false;
				ClearConstraintSources(constraint);
			}

			var springs = GetComponents<SpringJoint>();
			foreach (var spring in springs)
			{
				Destroy(spring);
			}
		}

		private void ClearConstraintSources(ParentConstraint constraint)
		{
			for (var i = constraint.sourceCount - 1; i >= 0; --i)
			{
				constraint.RemoveSource(i);
			}
		}
	}
}