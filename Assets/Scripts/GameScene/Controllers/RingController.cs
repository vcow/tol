using GameScene.Signals;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.Animations;
using Zenject;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class RingController : MonoBehaviour
	{
		[field: SerializeField] public RingColor RingType { get; private set; }
		[SerializeField] private float _spring;
		[SerializeField] private float _breakForce;
		[SerializeField] private float _damper;

		[Inject] private readonly SignalBus _signalBus;

		private readonly CompositeDisposable _disposables = new();
		private readonly Subject<Unit> _breakJoinObservable = new();
		private GameObject _connection;

		private void Start()
		{
			_connection = new GameObject($"{gameObject.name}_Joint",
				typeof(Rigidbody), typeof(ParentConstraint));
			var rigidbody = _connection.GetComponent<Rigidbody>();
			rigidbody.isKinematic = true;
			rigidbody.useGravity = false;

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

			var connectPosition = connectedObject.position;
			var connectPositionLocal = connectPosition - transform.position;
			_connection.transform.position = connectPosition;

			var joint = gameObject.AddComponent<ConfigurableJoint>();
			joint.anchor = connectPositionLocal;
			joint.connectedBody = _connection.GetComponent<Rigidbody>();
			joint.breakForce = _breakForce;
			joint.xMotion = ConfigurableJointMotion.Limited;
			joint.yMotion = ConfigurableJointMotion.Limited;
			joint.zMotion = ConfigurableJointMotion.Limited;
			joint.angularXMotion = ConfigurableJointMotion.Limited;
			joint.angularYMotion = ConfigurableJointMotion.Limited;
			joint.angularZMotion = ConfigurableJointMotion.Limited;
			joint.angularXLimitSpring = new SoftJointLimitSpring { spring = _spring, damper = _damper };
			joint.highAngularXLimit = new SoftJointLimit { limit = 1f };
			joint.lowAngularXLimit = new SoftJointLimit { limit = 0.5f };

			var constraint = _connection.GetComponent<ParentConstraint>();
			ClearConstraintSources(constraint);
			constraint.AddSource(new ConstraintSource
			{
				sourceTransform = connectedObject,
				weight = 1f
			});
			constraint.constraintActive = true;
		}

		private void OnJointBreak(float breakForce)
		{
			Release();
			_breakJoinObservable.OnNext(Unit.Default);
		}

		public void Release()
		{
			var constraint = _connection.GetComponent<ParentConstraint>();
			constraint.constraintActive = false;
			ClearConstraintSources(constraint);

			var joint = GetComponent<ConfigurableJoint>();
			Destroy(joint);
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