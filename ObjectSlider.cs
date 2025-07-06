using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002E5 RID: 741
	public class ObjectSlider : MonoBehaviour
	{
		// Token: 0x060023AF RID: 9135 RVA: 0x000F44FC File Offset: 0x000F26FC
		protected void Awake()
		{
			this.navMeshObstacle = base.GetComponentInChildren<NavMeshObstacle>();
			if (this.navMeshObstacle)
			{
				this.haveNavMeshObstacle = true;
			}
			this.jointDrive = default(JointDrive);
			this.startTransform = new GameObject("SliderStart").transform;
			this.startTransform.SetParent(base.transform.parent);
			this.startTransform.position = base.transform.position;
			this.startTransform.rotation = base.transform.rotation;
			this.joint = base.gameObject.GetComponent<ConfigurableJoint>();
			if (!this.joint)
			{
				this.joint = base.gameObject.AddComponent<ConfigurableJoint>();
			}
			this.joint.autoConfigureConnectedAnchor = false;
			if (base.transform.parent)
			{
				this.joint.connectedBody = base.transform.parent.GetComponentInParent<Rigidbody>();
			}
			if (this.joint.connectedBody)
			{
				this.joint.connectedAnchor = this.joint.connectedBody.transform.InverseTransformPoint(base.transform.position);
				this.joint.connectedAnchor += new Vector3(0f, this.maxheight * 0.5f, 0f);
			}
			else
			{
				this.joint.connectedAnchor = new Vector3(base.transform.position.x, base.transform.position.y + this.maxheight * 0.5f, base.transform.position.z);
			}
			this.joint.xMotion = ConfigurableJointMotion.Locked;
			this.joint.yMotion = ConfigurableJointMotion.Limited;
			this.joint.zMotion = ConfigurableJointMotion.Locked;
			this.joint.angularXMotion = ConfigurableJointMotion.Locked;
			this.joint.angularYMotion = ConfigurableJointMotion.Locked;
			this.joint.angularZMotion = ConfigurableJointMotion.Locked;
			SoftJointLimit softJointLimit = default(SoftJointLimit);
			softJointLimit.limit = this.maxheight * 0.5f;
			this.joint.linearLimit = softJointLimit;
			if (AreaManager.Instance && AreaManager.Instance.CurrentArea != null)
			{
				this.currentArea = AreaManager.Instance.CurrentArea.FindRecursive(base.transform.position);
			}
		}

		// Token: 0x060023B0 RID: 9136 RVA: 0x000F4758 File Offset: 0x000F2958
		protected void Start()
		{
			this.collisionHandler = base.GetComponentInParent<CollisionHandler>();
			this.SetPosition(this.positionOnStart);
			if (this.driveEnabled)
			{
				this.SetDrive(this.drivePositionOnStart);
			}
			this.initialized = true;
		}

		// Token: 0x060023B1 RID: 9137 RVA: 0x000F478D File Offset: 0x000F298D
		protected void OnEnable()
		{
			if (this.initialized && this.driveEnabled)
			{
				this.SetDrive(this.drivePositionOnStart);
			}
		}

		// Token: 0x060023B2 RID: 9138 RVA: 0x000F47AB File Offset: 0x000F29AB
		public float GetDrivePosition()
		{
			return Mathf.InverseLerp(this.maxheight * 0.5f, -this.maxheight * 0.5f, this.joint.targetPosition.y);
		}

		// Token: 0x060023B3 RID: 9139 RVA: 0x000F47DB File Offset: 0x000F29DB
		public void Open()
		{
			this.SetDrive(1f);
		}

		// Token: 0x060023B4 RID: 9140 RVA: 0x000F47E8 File Offset: 0x000F29E8
		public void Close()
		{
			this.SetDrive(0f);
		}

		// Token: 0x060023B5 RID: 9141 RVA: 0x000F47F5 File Offset: 0x000F29F5
		public void HoldDrive()
		{
			this.SetDrive(this.currentPosition);
		}

		// Token: 0x060023B6 RID: 9142 RVA: 0x000F4803 File Offset: 0x000F2A03
		public void StopDrive()
		{
			this.collisionHandler.RemovePhysicModifier(this);
			this.driveEnabled = false;
			this.jointDrive.maximumForce = 0f;
			this.joint.yDrive = this.jointDrive;
		}

		// Token: 0x060023B7 RID: 9143 RVA: 0x000F4839 File Offset: 0x000F2A39
		public void SetPosition(float linearPosition)
		{
			base.transform.position = this.startTransform.position + base.transform.up * this.maxheight * linearPosition;
		}

		// Token: 0x060023B8 RID: 9144 RVA: 0x000F4874 File Offset: 0x000F2A74
		public void SetDrive(float linearPosition)
		{
			if (linearPosition > this.currentPosition)
			{
				this.jointDrive.positionSpring = this.driveSpring.x;
				this.jointDrive.positionDamper = this.driveDamper.x;
				this.jointDrive.maximumForce = this.driveMaxForce.x;
			}
			else
			{
				this.jointDrive.positionSpring = this.driveSpring.y;
				this.jointDrive.positionDamper = this.driveDamper.y;
				this.jointDrive.maximumForce = this.driveMaxForce.y;
			}
			this.joint.yDrive = this.jointDrive;
			this.driveEnabled = true;
			this.collisionHandler.SetPhysicModifier(this, new float?(0f), 1f, -1f, -1f, -1f, null);
			if (this.dynamicDriveCoroutine != null)
			{
				base.StopCoroutine(this.dynamicDriveCoroutine);
			}
			if (this.dynamicDriveEnabled)
			{
				this.dynamicDriveCoroutine = base.StartCoroutine(this.DynamicDriveCoroutine(linearPosition));
			}
			else
			{
				this.SetJointTargetPosition(linearPosition);
			}
			this.drivePositionOnStart = linearPosition;
		}

		// Token: 0x060023B9 RID: 9145 RVA: 0x000F4992 File Offset: 0x000F2B92
		protected void SetJointTargetPosition(float linearPosition)
		{
			this.joint.targetPosition = new Vector3(0f, Mathf.Lerp(this.maxheight * 0.5f, -this.maxheight * 0.5f, linearPosition), 0f);
		}

		// Token: 0x060023BA RID: 9146 RVA: 0x000F49CD File Offset: 0x000F2BCD
		private IEnumerator DynamicDriveCoroutine(float targetLinearPosition)
		{
			this.SetJointTargetPosition(this.currentPosition);
			float time = 0f;
			while (Mathf.Abs(this.currentPosition - targetLinearPosition) > this.reachOffset)
			{
				if (targetLinearPosition > this.currentPosition)
				{
					this.SetJointTargetPosition(this.currentPosition + this.dynamicDriveSpeed * this.dynamicDriveCurve.Evaluate(Mathf.InverseLerp(0f, this.dynamicDriveCurveDuration, time)));
				}
				else
				{
					this.SetJointTargetPosition(this.currentPosition - this.dynamicDriveSpeed * this.dynamicDriveCurve.Evaluate(Mathf.InverseLerp(0f, this.dynamicDriveCurveDuration, time)));
				}
				time += Time.deltaTime;
				yield return Yielders.EndOfFrame;
			}
			this.SetJointTargetPosition(targetLinearPosition);
			this.dynamicDriveCoroutine = null;
			yield break;
		}

		// Token: 0x060023BB RID: 9147 RVA: 0x000F49E4 File Offset: 0x000F2BE4
		protected void Update()
		{
			this.currentPosition = Mathf.InverseLerp(0f, this.maxheight, this.startTransform.InverseTransformPoint(base.transform.position).y);
			if (this.haveNavMeshObstacle)
			{
				if (this.currentPosition > this.navMeshObstacleMaxHeight / this.maxheight)
				{
					if (this.navMeshObstacle.enabled)
					{
						this.navMeshObstacle.enabled = false;
					}
				}
				else if (!this.navMeshObstacle.enabled)
				{
					this.navMeshObstacle.enabled = true;
				}
			}
			if (this.currentPosition < this.reachOffset)
			{
				if (this.state != ObjectSlider.State.Close)
				{
					if (this.MakeSound() && this.effectAudioReachEnd != null && this.collisionHandler.physicBody.velocity.magnitude > this.audioReachMinVelocity)
					{
						this.effectAudioReachEnd.SetIntensity(Mathf.InverseLerp(this.audioReachMinVelocity, this.audioReachMaxVelocity, this.collisionHandler.physicBody.velocity.magnitude));
						this.effectAudioReachEnd.Play();
					}
					this.state = ObjectSlider.State.Close;
					return;
				}
			}
			else if (this.currentPosition > 1f - this.reachOffset)
			{
				if (this.state != ObjectSlider.State.Open)
				{
					if (this.MakeSound() && this.effectAudioReachStart != null && this.collisionHandler.physicBody.velocity.magnitude > this.audioReachMinVelocity)
					{
						this.effectAudioReachStart.SetIntensity(Mathf.InverseLerp(this.audioReachMinVelocity, this.audioReachMaxVelocity, this.collisionHandler.physicBody.velocity.magnitude));
						this.effectAudioReachStart.Play();
					}
					this.state = ObjectSlider.State.Open;
					return;
				}
			}
			else if (this.state != ObjectSlider.State.InBetween)
			{
				this.state = ObjectSlider.State.InBetween;
			}
		}

		// Token: 0x060023BC RID: 9148 RVA: 0x000F4BBC File Offset: 0x000F2DBC
		private bool MakeSound()
		{
			if (Level.current && !Level.current.loaded)
			{
				return false;
			}
			if (this.currentArea == null)
			{
				if (!AreaManager.Instance || AreaManager.Instance.CurrentArea == null)
				{
					return true;
				}
				this.currentArea = AreaManager.Instance.CurrentArea.FindRecursive(base.transform.position);
				if (this.currentArea == null)
				{
					return true;
				}
			}
			if (!this.currentArea.IsSpawned)
			{
				return false;
			}
			Area area = this.currentArea.SpawnedArea;
			return area.initialized && !area.isCulled && !area.isHidden;
		}

		// Token: 0x060023BD RID: 9149 RVA: 0x000F4C6C File Offset: 0x000F2E6C
		protected void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawRay(Application.isPlaying ? this.startTransform.position : base.transform.position, base.transform.up * this.maxheight);
		}

		// Token: 0x040022B7 RID: 8887
		[Header("General")]
		public float maxheight = 2.7f;

		// Token: 0x040022B8 RID: 8888
		[Range(0f, 1f)]
		public float positionOnStart;

		// Token: 0x040022B9 RID: 8889
		public float reachOffset = 0.01f;

		// Token: 0x040022BA RID: 8890
		public float navMeshObstacleMaxHeight = 2f;

		// Token: 0x040022BB RID: 8891
		[Header("Drive")]
		public bool driveEnabled = true;

		// Token: 0x040022BC RID: 8892
		[Range(0f, 1f)]
		public float drivePositionOnStart;

		// Token: 0x040022BD RID: 8893
		[Header("Dynamic drive")]
		public bool dynamicDriveEnabled = true;

		// Token: 0x040022BE RID: 8894
		public float dynamicDriveSpeed = 0.05f;

		// Token: 0x040022BF RID: 8895
		public float dynamicDriveCurveDuration = 3f;

		// Token: 0x040022C0 RID: 8896
		public AnimationCurve dynamicDriveCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x040022C1 RID: 8897
		[Header("Drive (Forward / Reverse)")]
		public Vector2 driveSpring = new Vector2(5000f, 5000f);

		// Token: 0x040022C2 RID: 8898
		public Vector2 driveDamper = new Vector2(1000f, 1000f);

		// Token: 0x040022C3 RID: 8899
		public Vector2 driveMaxForce = new Vector2(5000f, 5000f);

		// Token: 0x040022C4 RID: 8900
		[Header("Audio")]
		public float audioReachMinVelocity = 0.1f;

		// Token: 0x040022C5 RID: 8901
		public float audioReachMaxVelocity = 1f;

		// Token: 0x040022C6 RID: 8902
		public FxModule effectAudioReachStart;

		// Token: 0x040022C7 RID: 8903
		public FxModule effectAudioReachEnd;

		// Token: 0x040022C8 RID: 8904
		[Header("Event")]
		public UnityEvent targetReachEvent = new UnityEvent();

		// Token: 0x040022C9 RID: 8905
		[Header("Instance")]
		[NonSerialized]
		public ObjectSlider.State state;

		// Token: 0x040022CA RID: 8906
		[NonSerialized]
		public float currentPosition;

		// Token: 0x040022CB RID: 8907
		[NonSerialized]
		public CollisionHandler collisionHandler;

		// Token: 0x040022CC RID: 8908
		protected ConfigurableJoint joint;

		// Token: 0x040022CD RID: 8909
		protected Coroutine dynamicDriveCoroutine;

		// Token: 0x040022CE RID: 8910
		protected JointDrive jointDrive;

		// Token: 0x040022CF RID: 8911
		protected Transform startTransform;

		// Token: 0x040022D0 RID: 8912
		protected bool initialized;

		// Token: 0x040022D1 RID: 8913
		protected NavMeshObstacle navMeshObstacle;

		// Token: 0x040022D2 RID: 8914
		protected bool haveNavMeshObstacle;

		// Token: 0x040022D3 RID: 8915
		protected SpawnableArea currentArea;

		// Token: 0x020009CE RID: 2510
		public enum State
		{
			// Token: 0x040045F2 RID: 17906
			Close,
			// Token: 0x040045F3 RID: 17907
			Open,
			// Token: 0x040045F4 RID: 17908
			InBetween
		}
	}
}
