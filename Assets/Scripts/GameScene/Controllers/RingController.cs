using Models;
using UniRx;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
	public sealed class RingController : MonoBehaviour
	{
		private const float MuteOnStartDelayTime = 1f;

		[field: SerializeField] public RingColor RingType { get; private set; }
		[SerializeField] private float _spring;
		[SerializeField] private float _breakForce;
		[SerializeField] private float _damper;
		[SerializeField] private LayerMask _collideSoundLayerMask;

		// ReSharper disable InconsistentNaming
		public UnityEvent<RingColor> onCatch;
		public UnityEvent<RingColor> onRelease;
		public UnityEvent<RingColor, int, int> onSleepAtPosition;
		// ReSharper restore InconsistentNaming

		private PinController _pin;
		private GameObject _connection;
		private readonly CompositeDisposable _disposables = new();

		private AudioSource _audioSource;

		private float _startTime = float.MaxValue;
		private bool _initializeInStartPosition;

		private CharacterJoint _inPositionLockJoint;

		private void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
		}

		private void Start()
		{
			_startTime = Time.time;
			_connection = new GameObject($"{gameObject.name}_Joint",
				typeof(Rigidbody), typeof(ParentConstraint));
			var rb = _connection.GetComponent<Rigidbody>();
			rb.isKinematic = true;
			rb.useGravity = false;

			DetectSleepAtPosition();
		}

		private void OnDestroy()
		{
			_disposables.Dispose();

			onCatch.RemoveAllListeners();
			onRelease.RemoveAllListeners();
			onSleepAtPosition.RemoveAllListeners();
		}

		public void JoinTo(Transform connectedObject)
		{
			if (!_initializeInStartPosition)
			{
				return;
			}

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

			onCatch.Invoke(RingType);
		}

		private void OnJointBreak(float breakForce)
		{
			Release();
		}

		private void OnTriggerStay(Collider other)
		{
			_pin = other.GetComponent<PinController>();
		}

		private void OnTriggerExit(Collider other)
		{
			var pin = other.GetComponent<PinController>();
			if (pin == _pin)
			{
				_pin = null;
			}
		}

		private void OnCollisionEnter(Collision other)
		{
			if (Time.time - _startTime < MuteOnStartDelayTime)
			{
				return;
			}

			var otherLayerMask = 1 << other.collider.gameObject.layer;
			if ((_collideSoundLayerMask.value & otherLayerMask) == 0 ||
			    _audioSource.isPlaying)
			{
				return;
			}

			_audioSource.Play();
		}

		public void Release()
		{
			UnlockRing();

			_disposables.Clear();

			var constraint = _connection.GetComponent<ParentConstraint>();
			constraint.constraintActive = false;
			ClearConstraintSources(constraint);

			var joint = GetComponent<ConfigurableJoint>();
			if (joint)
			{
				Destroy(joint);
				onRelease.Invoke(RingType);

				DetectSleepAtPosition();
			}
		}

		private void DetectSleepAtPosition()
		{
			var rb = GetComponent<Rigidbody>();
			if (rb.IsSleeping())
			{
				if (_pin)
				{
					LockRing();
				}

				return;
			}

			rb.ObserveEveryValueChanged(r => r.IsSleeping())
				.First(b => b)
				.Subscribe(_ =>
				{
					_disposables.Clear();
					if (_pin)
					{
						LockRing();
					}
				})
				.AddTo(_disposables);
		}

		private void ClearConstraintSources(ParentConstraint constraint)
		{
			for (var i = constraint.sourceCount - 1; i >= 0; --i)
			{
				constraint.RemoveSource(i);
			}
		}

		private void LockRing()
		{
			Assert.IsNotNull(_pin);
			UnlockRing();

			var pinRb = _pin.GetComponentInParent<Rigidbody>();
			Assert.IsNotNull(pinRb);

			_inPositionLockJoint = gameObject.AddComponent<CharacterJoint>();
			_inPositionLockJoint.connectedBody = pinRb;

			if (_initializeInStartPosition)
			{
				onSleepAtPosition.Invoke(RingType, _pin.TowerIndex, _pin.PinIndex);
			}
			else
			{
				_initializeInStartPosition = true;
			}
		}

		private void UnlockRing()
		{
			if (_inPositionLockJoint)
			{
				Destroy(_inPositionLockJoint);
				_inPositionLockJoint = null;
			}
		}
	}
}