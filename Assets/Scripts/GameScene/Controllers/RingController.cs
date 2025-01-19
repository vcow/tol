using System;
using Models;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

namespace GameScene.Controllers
{
	[DisallowMultipleComponent]
	public sealed class RingController : MonoBehaviour
	{
		[field: SerializeField] public RingColor RingType { get; private set; }
		[SerializeField] private float _spring;
		[SerializeField] private float _breakForce;
		[SerializeField] private float _damper;

		// ReSharper disable InconsistentNaming
		public UnityEvent<RingColor> onCatch;
		public UnityEvent<RingColor> onRelease;
		// ReSharper restore InconsistentNaming

		private GameObject _connection;

		private void Start()
		{
			_connection = new GameObject($"{gameObject.name}_Joint",
				typeof(Rigidbody), typeof(ParentConstraint));
			var rb = _connection.GetComponent<Rigidbody>();
			rb.isKinematic = true;
			rb.useGravity = false;
		}

		private void OnDestroy()
		{
			onCatch.RemoveAllListeners();
			onRelease.RemoveAllListeners();
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

			onCatch.Invoke(RingType);
		}

		private void OnJointBreak(float breakForce)
		{
			Release();
		}

		private void OnTriggerEnter(Collider other)
		{
			Debug.Log("TRIGGER!!!");
		}

		public void Release()
		{
			var constraint = _connection.GetComponent<ParentConstraint>();
			constraint.constraintActive = false;
			ClearConstraintSources(constraint);

			var joint = GetComponent<ConfigurableJoint>();
			if (joint)
			{
				Destroy(joint);
				onRelease.Invoke(RingType);
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