using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000273 RID: 627
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Locomotion.html")]
	[AddComponentMenu("ThunderRoad/Locomotion")]
	[RequireComponent(typeof(Rigidbody))]
	public class Locomotion : ThunderBehaviour
	{
		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06001C55 RID: 7253 RVA: 0x000BD1C2 File Offset: 0x000BB3C2
		// (set) Token: 0x06001C56 RID: 7254 RVA: 0x000BD1CA File Offset: 0x000BB3CA
		public bool locomotionError { get; private set; }

		// Token: 0x140000C4 RID: 196
		// (add) Token: 0x06001C57 RID: 7255 RVA: 0x000BD1D4 File Offset: 0x000BB3D4
		// (remove) Token: 0x06001C58 RID: 7256 RVA: 0x000BD20C File Offset: 0x000BB40C
		public event Locomotion.GroundEvent OnGroundEvent;

		// Token: 0x140000C5 RID: 197
		// (add) Token: 0x06001C59 RID: 7257 RVA: 0x000BD244 File Offset: 0x000BB444
		// (remove) Token: 0x06001C5A RID: 7258 RVA: 0x000BD27C File Offset: 0x000BB47C
		public event Locomotion.CrouchEvent OnCrouchEvent;

		// Token: 0x140000C6 RID: 198
		// (add) Token: 0x06001C5B RID: 7259 RVA: 0x000BD2B4 File Offset: 0x000BB4B4
		// (remove) Token: 0x06001C5C RID: 7260 RVA: 0x000BD2EC File Offset: 0x000BB4EC
		public event Locomotion.CollisionEvent OnCollisionEnterEvent;

		// Token: 0x140000C7 RID: 199
		// (add) Token: 0x06001C5D RID: 7261 RVA: 0x000BD324 File Offset: 0x000BB524
		// (remove) Token: 0x06001C5E RID: 7262 RVA: 0x000BD35C File Offset: 0x000BB55C
		public event Locomotion.FlyEvent OnFlyEvent;

		// Token: 0x140000C8 RID: 200
		// (add) Token: 0x06001C5F RID: 7263 RVA: 0x000BD394 File Offset: 0x000BB594
		// (remove) Token: 0x06001C60 RID: 7264 RVA: 0x000BD3CC File Offset: 0x000BB5CC
		public event Action OnJumpEvent;

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x06001C61 RID: 7265 RVA: 0x000BD401 File Offset: 0x000BB601
		public bool IsRunning
		{
			get
			{
				return this.isGrounded && this.prevVelocity.sqrMagnitude > 20f;
			}
		}

		// Token: 0x06001C62 RID: 7266 RVA: 0x000BD420 File Offset: 0x000BB620
		protected void Awake()
		{
			this.physicBody = base.gameObject.GetPhysicBody();
			this.physicBody.freezeRotation = true;
			this.physicBody.isKinematic = true;
			this.capsuleCollider = base.gameObject.AddComponent<CapsuleCollider>();
			this.capsuleCollider.radius = this.colliderRadius;
			this.capsuleCollider.height = this.colliderHeight;
			this.capsuleCollider.center = new Vector3(0f, Mathf.Max(this.colliderHeight / 2f, this.colliderRadius), 0f);
			this.capsuleCollider.material = this.colliderGroundMaterial;
			this.player = base.GetComponent<Player>();
			this.creature = base.GetComponent<Creature>();
			this.globalMoveSpeedMultiplier = new FloatHandler();
			if (this.player)
			{
				base.gameObject.layer = GameManager.GetLayer(LayerName.PlayerLocomotion);
				return;
			}
			base.gameObject.layer = GameManager.GetLayer(LayerName.BodyLocomotion);
		}

		// Token: 0x06001C63 RID: 7267 RVA: 0x000BD520 File Offset: 0x000BB720
		protected override void ManagedOnEnable()
		{
			if (this.initialized)
			{
				this.globalMoveSpeedMultiplier = new FloatHandler();
				this.physicBody.velocity = Vector3.zero;
				this.physicBody.angularVelocity = Vector3.zero;
				this.moveDirection = Vector3.zero;
				this.horizontalSpeed = 0f;
				this.verticalSpeed = 0f;
				this.physicBody.isKinematic = false;
				this.capsuleCollider.isTrigger = false;
				this.MoveStop();
			}
		}

		// Token: 0x06001C64 RID: 7268 RVA: 0x000BD5A0 File Offset: 0x000BB7A0
		protected override void ManagedOnDisable()
		{
			this.physicBody.isKinematic = true;
			this.capsuleCollider.isTrigger = true;
			this.horizontalSpeed = 0f;
			this.verticalSpeed = 0f;
			this.velocity = Vector3.zero;
			this.angularSpeed = 0f;
			this.moveDirection = Vector3.zero;
		}

		// Token: 0x06001C65 RID: 7269 RVA: 0x000BD5FC File Offset: 0x000BB7FC
		public void Init()
		{
			this.groundMask = ThunderRoadSettings.current.groundLayer;
			if (this.creature)
			{
				if (this.creature.data.overrideGroundMask)
				{
					this.groundMask = this.creature.data.groundMask;
				}
				this.physicBody.mass = this.creature.data.locomotionMass;
				this.forwardSpeed = this.creature.data.locomotionForwardSpeed;
				this.backwardSpeed = this.creature.data.locomotionBackwardSpeed;
				this.strafeSpeed = this.creature.data.locomotionStrafeSpeed;
				this.runSpeedAdd = this.creature.data.locomotionRunSpeedAdd;
				this.crouchSpeed = this.creature.data.locomotionCrouchSpeed;
				this.horizontalAirSpeed = this.creature.data.locomotionAirSpeed;
				this.jumpGroundForce = this.creature.data.locomotionJumpForce;
				this.jumpClimbVerticalMultiplier = this.creature.data.locomotionJumpClimbVerticalMultiplier;
				this.jumpClimbHorizontalMultiplier = this.creature.data.locomotionJumpClimbHorizontalMultiplier;
				this.jumpClimbVerticalMaxVelocityRatio = this.creature.data.jumpClimbVerticalMaxVelocityRatio;
				this.groundDrag = this.creature.data.locomotionGroundDrag;
				this.flyDrag = this.creature.data.locomotionFlyDrag;
				this.jumpMaxDuration = this.creature.data.locomotionJumpMaxDuration;
			}
			if (base.enabled)
			{
				this.physicBody.isKinematic = false;
				this.capsuleCollider.isTrigger = false;
			}
			this.initialized = true;
			this.startGroundCheck = true;
			this.orgMass = this.physicBody.mass;
			this.orgDrag = this.physicBody.drag;
			this.orgAngularDrag = this.physicBody.angularDrag;
			this.orgSleepThreshold = this.physicBody.sleepThreshold;
		}

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x06001C66 RID: 7270 RVA: 0x000BD7FC File Offset: 0x000BB9FC
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate | ManagedLoops.Update;
			}
		}

		// Token: 0x06001C67 RID: 7271 RVA: 0x000BD800 File Offset: 0x000BBA00
		protected internal override void ManagedUpdate()
		{
			if (!this.collideWithPlayer)
			{
				UnityEngine.Object x = this.ignoredPlayerCreatureLocomotion;
				Creature currentCreature = Player.currentCreature;
				UnityEngine.Object y;
				if (currentCreature == null)
				{
					y = null;
				}
				else
				{
					Locomotion locomotion = currentCreature.locomotion;
					y = ((locomotion != null) ? locomotion.capsuleCollider : null);
				}
				if (x != y)
				{
					if (this.ignoredPlayerCreatureLocomotion != null)
					{
						Physics.IgnoreCollision(this.capsuleCollider, this.ignoredPlayerCreatureLocomotion, false);
					}
					Creature currentCreature2 = Player.currentCreature;
					Collider collider2;
					if (currentCreature2 == null)
					{
						collider2 = null;
					}
					else
					{
						Locomotion locomotion2 = currentCreature2.locomotion;
						collider2 = ((locomotion2 != null) ? locomotion2.capsuleCollider : null);
					}
					this.ignoredPlayerCreatureLocomotion = collider2;
					if (this.ignoredPlayerCreatureLocomotion != null)
					{
						Physics.IgnoreCollision(this.capsuleCollider, this.ignoredPlayerCreatureLocomotion, true);
						Collider collider3 = this.capsuleCollider;
						Creature currentCreature3 = Player.currentCreature;
						Collider collider4;
						if (currentCreature3 == null)
						{
							collider4 = null;
						}
						else
						{
							Locomotion currentLocomotion = currentCreature3.currentLocomotion;
							collider4 = ((currentLocomotion != null) ? currentLocomotion.capsuleCollider : null);
						}
						Physics.IgnoreCollision(collider3, collider4, true);
						foreach (Collider collider in Player.local.globalOffsetTransform.GetComponentsInChildren<Collider>())
						{
							Physics.IgnoreCollision(this.capsuleCollider, collider, true);
						}
					}
				}
			}
			this.UpdateGrounded(this.startGroundCheck);
			if (this.startGroundCheck)
			{
				this.startGroundCheck = false;
			}
			this.CrouchCheck();
			this.TestMove(base.transform, Vector3.up);
			float deltaTime = Time.deltaTime;
			if (this.physicBody.isKinematic)
			{
				if (Time.deltaTime > 0f)
				{
					this.velocity = (base.transform.position - this.prevPosition) / deltaTime;
				}
				this.horizontalSpeed = new Vector3(this.velocity.x, 0f, this.velocity.z).magnitude;
				this.verticalSpeed = this.velocity.y;
				this.prevPosition = base.transform.position;
			}
			else
			{
				this.velocity = this.prevVelocity;
				this.horizontalSpeed = new Vector3(this.velocity.x, 0f, this.velocity.z).magnitude;
				this.verticalSpeed = this.velocity.y;
				this.prevVelocity = this.physicBody.velocity;
			}
			if (deltaTime > 0f)
			{
				this.angularSpeed = Mathf.DeltaAngle(0f, (base.transform.rotation * Quaternion.Inverse(this.prevRotation)).eulerAngles.y) / deltaTime;
			}
			this.prevRotation = base.transform.rotation;
		}

		// Token: 0x06001C68 RID: 7272 RVA: 0x000BDA7C File Offset: 0x000BBC7C
		public void UpdateGrounded(bool forceInvokeFlyGround = false)
		{
			Vector3 vector3Up = Vector3.up;
			if (Level.master)
			{
				Vector3 lossyScale = base.transform.lossyScale;
				if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration, BuildSettings.ContentFlagBehaviour.Discard))
				{
					foreach (Creature otherCreature in Creature.allActive)
					{
						if (otherCreature.isKilled && otherCreature.ragdoll.state == Ragdoll.State.Inert)
						{
							otherCreature.ragdoll.SetPartsLayer(LayerName.Ragdoll);
						}
					}
				}
				if (Physics.SphereCast(this.capsuleCollider.transform.TransformPoint(this.capsuleCollider.center), this.capsuleCollider.radius * 0.99f * lossyScale.y, -vector3Up, out this.groundHit, 1000f, this.groundMask, QueryTriggerInteraction.Ignore))
				{
					this.groundAngle = Vector3.Angle(vector3Up, this.groundHit.normal);
					if (Mathf.Clamp(this.groundHit.distance - (this.capsuleCollider.height / 2f - this.capsuleCollider.radius * 0.99f) * lossyScale.y, 0f, float.PositiveInfinity) > this.groundDetectionDistance)
					{
						if (forceInvokeFlyGround || this.isGrounded)
						{
							this.groundHit.normal = vector3Up;
							this.OnFly(forceInvokeFlyGround);
						}
					}
					else if (forceInvokeFlyGround || !this.isGrounded)
					{
						this.OnGround(this.groundHit.point, this.velocity, this.capsuleCollider, forceInvokeFlyGround);
					}
				}
				else if (forceInvokeFlyGround || this.isGrounded)
				{
					this.groundHit.distance = 1000f;
					this.groundHit.normal = vector3Up;
					this.groundAngle = 0f;
					this.OnFly(forceInvokeFlyGround);
				}
				if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration, BuildSettings.ContentFlagBehaviour.Discard))
				{
					foreach (Creature otherCreature2 in Creature.allActive)
					{
						if (otherCreature2.isKilled && otherCreature2.ragdoll.state == Ragdoll.State.Inert)
						{
							otherCreature2.ragdoll.RefreshPartsLayer();
						}
					}
				}
			}
		}

		// Token: 0x06001C69 RID: 7273 RVA: 0x000BDCBC File Offset: 0x000BBEBC
		private void TestMove(Transform t, Vector3 vector3Up)
		{
			if (this.testMove)
			{
				this.moveDirection = Quaternion.Euler(0f, t.rotation.eulerAngles.y, 0f) * new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * this.forwardSpeed;
				float axisTurn = Input.GetAxis("Rotation");
				if (axisTurn > 0.1f || axisTurn < -0.1f)
				{
					t.RotateAround(t.position, vector3Up, axisTurn * this.turnSpeed * 3f);
				}
			}
		}

		// Token: 0x06001C6A RID: 7274 RVA: 0x000BDD60 File Offset: 0x000BBF60
		protected void CrouchCheck()
		{
			if (!this.player || !this.allowCrouch)
			{
				return;
			}
			if (this.player.creature == null)
			{
				return;
			}
			if (this.player.creature.GetAnimatorHeightRatio() > this.crouchHeightRatio)
			{
				if (this.isCrouched)
				{
					this.OnCrouch(false);
					return;
				}
			}
			else if (!this.isCrouched)
			{
				this.OnCrouch(true);
			}
		}

		// Token: 0x06001C6B RID: 7275 RVA: 0x000BDDCE File Offset: 0x000BBFCE
		protected void OnCrouch(bool isCrouching)
		{
			this.isCrouched = isCrouching;
			Locomotion.CrouchEvent onCrouchEvent = this.OnCrouchEvent;
			if (onCrouchEvent == null)
			{
				return;
			}
			onCrouchEvent(isCrouching);
		}

		// Token: 0x06001C6C RID: 7276 RVA: 0x000BDDE8 File Offset: 0x000BBFE8
		protected void OnGround(Vector3 groundPoint, Vector3 velocity, Collider groundCollider, bool silent = false)
		{
			this.isGrounded = true;
			this.capsuleCollider.material = this.colliderGroundMaterial;
			this.physicBody.drag = this.groundDrag * this.groundDragMultiplier;
			if (silent)
			{
				return;
			}
			Locomotion.GroundEvent onGroundEvent = this.OnGroundEvent;
			if (onGroundEvent == null)
			{
				return;
			}
			onGroundEvent(this, groundPoint, velocity, groundCollider);
		}

		// Token: 0x06001C6D RID: 7277 RVA: 0x000BDE40 File Offset: 0x000BC040
		protected void OnFly(bool silent = false)
		{
			this.isGrounded = false;
			this.capsuleCollider.material = this.colliderFlyMaterial;
			this.physicBody.drag = this.flyDrag * this.flyDragMultiplier;
			if (silent)
			{
				return;
			}
			Locomotion.FlyEvent onFlyEvent = this.OnFlyEvent;
			if (onFlyEvent == null)
			{
				return;
			}
			onFlyEvent(this);
		}

		// Token: 0x06001C6E RID: 7278 RVA: 0x000BDE94 File Offset: 0x000BC094
		public void SetCapsuleCollider(float height)
		{
			this.capsuleCollider.height = height;
			if (height < this.capsuleCollider.radius)
			{
				this.capsuleCollider.radius = height;
			}
			this.capsuleCollider.center = new Vector3(this.capsuleCollider.center.x, Mathf.Max(this.capsuleCollider.height / 2f, this.capsuleCollider.radius), this.capsuleCollider.center.z);
		}

		// Token: 0x06001C6F RID: 7279 RVA: 0x000BDF18 File Offset: 0x000BC118
		private void OnCollisionEnter(Collision collision)
		{
			Locomotion.<>c__DisplayClass128_0 CS$<>8__locals1 = new Locomotion.<>c__DisplayClass128_0();
			CS$<>8__locals1.<>4__this = this;
			if (collision.collider.gameObject.layer == GameManager.GetLayer(LayerName.LocomotionOnly))
			{
				CS$<>8__locals1.otherCreature = collision.collider.GetComponentInParent<Creature>();
				if (CS$<>8__locals1.otherCreature != null)
				{
					Physics.IgnoreCollision(this.capsuleCollider, collision.collider);
					List<Collider> colliders;
					if (this.ignoredLocomotionOnlyColliders.TryGetValue(CS$<>8__locals1.otherCreature, out colliders))
					{
						colliders.Add(collision.collider);
						return;
					}
					this.ignoredLocomotionOnlyColliders.Add(CS$<>8__locals1.otherCreature, new List<Collider>
					{
						collision.collider
					});
					CS$<>8__locals1.otherCreature.OnDespawnEvent += CS$<>8__locals1.<OnCollisionEnter>g__UnignoreCollision|0;
					return;
				}
			}
			Locomotion.CollisionEvent onCollisionEnterEvent = this.OnCollisionEnterEvent;
			if (onCollisionEnterEvent == null)
			{
				return;
			}
			onCollisionEnterEvent(collision);
		}

		// Token: 0x06001C70 RID: 7280 RVA: 0x000BDFEC File Offset: 0x000BC1EC
		public void MoveWeighted(Vector3 direction, Transform bodyTransform, float heightRatio, float moveSpeedRatio = 1f, float runSpeedRatio = 0f, float acceleration = 0f)
		{
			if (!this.allowMove)
			{
				return;
			}
			if (this.player)
			{
				heightRatio = this.player.creature.GetAnimatorHeightRatio();
			}
			float moveSpeed = this.horizontalAirSpeed;
			if (this.isGrounded)
			{
				if (heightRatio > this.crouchHeightRatio)
				{
					Vector3 normalized = bodyTransform.forward.ToXZ().normalized;
					Vector3 directionNormalized = direction.normalized;
					float forwardsRatio = Mathf.Clamp01(Vector3.Dot(normalized, directionNormalized));
					float backwardRatio = Mathf.Clamp01(Vector3.Dot(-normalized, directionNormalized));
					float strafeRatio = Mathf.Abs(Vector3.Dot(bodyTransform.right.ToXZ().normalized, directionNormalized));
					float num = this.forwardSpeed * this.forwardSpeedMultiplier * forwardsRatio * moveSpeedRatio;
					float backwardSpeedResult = this.backwardSpeed * this.backwardSpeedMultiplier * backwardRatio * moveSpeedRatio;
					float strafeSpeedResult = this.strafeSpeed * this.strafeSpeedMultiplier * strafeRatio * moveSpeedRatio;
					float runSpeedResult = (forwardsRatio > this.runDot) ? (this.runSpeedAdd * this.runSpeedMultiplier * runSpeedRatio) : 0f;
					moveSpeed = (num + backwardSpeedResult + strafeSpeedResult + runSpeedResult) * Mathf.Clamp01(base.transform.lossyScale.y);
				}
				else
				{
					moveSpeed = this.crouchSpeed * this.crouchSpeedMultiplier * Mathf.Clamp01(base.transform.lossyScale.y);
				}
			}
			Player player = this.player;
			bool flag;
			if (player == null)
			{
				Creature creature = this.creature;
				flag = (creature != null && creature.isSwimming);
			}
			else
			{
				flag = player.creature.isSwimming;
			}
			if (flag)
			{
				moveSpeed = this.waterSpeed;
			}
			this.moveDirection.x = direction.x * moveSpeed;
			this.moveDirection.z = direction.z * moveSpeed;
			if (acceleration > 0f)
			{
				Vector3 moveDir = Vector3.SmoothDamp(this.velocity, this.moveDirection, ref this.accelerationCurrentSpeed, acceleration);
				this.moveDirection.x = moveDir.x;
				this.moveDirection.z = moveDir.z;
			}
		}

		// Token: 0x06001C71 RID: 7281 RVA: 0x000BE1DC File Offset: 0x000BC3DC
		public void MoveVertical(float directionY)
		{
			if (!this.isGrounded && this.verticalAirSpeed > 0f)
			{
				this.moveDirection.y = directionY * this.verticalAirSpeed;
				return;
			}
			this.moveDirection.y = 0f;
		}

		// Token: 0x06001C72 RID: 7282 RVA: 0x000BE217 File Offset: 0x000BC417
		public void MoveStop()
		{
			this.moveDirection = Vector3.zero;
		}

		// Token: 0x06001C73 RID: 7283 RVA: 0x000BE224 File Offset: 0x000BC424
		protected internal override void ManagedFixedUpdate()
		{
			if (this.allowTurn)
			{
				float targetAngle = 0f;
				if (this.turnSmoothDirection != 0f)
				{
					targetAngle = this.turnSmoothDirection * this.turnSpeed * TimeManager.GetTimeStepMultiplier();
				}
				else if (this.turnSnapDirection != 0f && Time.time - this.snapTurnTime > this.snapTurnDelay)
				{
					targetAngle = this.turnSnapDirection * this.turnSpeed * (this.snapTurnDelay / Time.fixedDeltaTime) * TimeManager.GetTimeStepMultiplier();
					this.snapTurnTime = Time.time;
				}
				else if (this.turnSmoothSnapDirection == 0f)
				{
					this.smoothSnapTurn = false;
				}
				else if (Time.time - this.snapTurnTime > this.snapTurnDelay)
				{
					this.smoothSnapTurn = !this.smoothSnapTurn;
					this.snapTurnTime = Time.time;
				}
				else if (this.smoothSnapTurn)
				{
					targetAngle = this.turnSmoothSnapDirection * this.turnSpeed * 2f * TimeManager.GetTimeStepMultiplier();
				}
				if (targetAngle != 0f)
				{
					Vector3 colliderPosition = base.transform.TransformPoint(this.capsuleCollider.center);
					base.transform.RotateAround(colliderPosition, base.transform.up, targetAngle);
				}
			}
			if (this.allowMove)
			{
				this.locomotionError = true;
				if (!float.IsNaN(this.moveDirection.x) && !float.IsNaN(this.moveDirection.z))
				{
					if (this.moveDirection.x != 0f || this.moveDirection.z != 0f)
					{
						Vector3 force = new Vector3(this.moveDirection.x, 0f, this.moveDirection.z) * TimeManager.GetTimeStepMultiplier() * this.globalMoveSpeedMultiplier;
						float upSlopeMult = this.moveForceMultiplierByAngleCurve.Evaluate(this.groundAngle);
						Creature currentCreature = Player.currentCreature;
						float downSlopeMult = (((currentCreature != null) ? currentCreature.currentLocomotion : null) == this) ? 1f : Mathf.Clamp01(1f / upSlopeMult);
						Vector3 projectedForce = Vector3.ProjectOnPlane(force, this.isGrounded ? this.groundHit.normal : Vector3.up).normalized * (force.magnitude * ((!this.isGrounded) ? 1f : ((Vector3.Angle(force, this.groundHit.normal) > 90f) ? upSlopeMult : downSlopeMult)));
						if (!float.IsNaN(projectedForce.x) && !float.IsNaN(projectedForce.y) && !float.IsNaN(projectedForce.z))
						{
							this.locomotionError = false;
							this.physicBody.AddForce(projectedForce, this.isGrounded ? ForceMode.VelocityChange : this.airForceMode);
						}
					}
					if (this.moveDirection.y != 0f)
					{
						this.locomotionError = false;
						this.physicBody.AddForce(new Vector3(0f, this.moveDirection.y, 0f), this.verticalForceMode);
					}
				}
			}
			if (this.isJumping)
			{
				if (this.jumpTime > 0f)
				{
					Vector3 force2 = new Vector3(Utils.CalculateRatio(this.jumpTime, 0f, this.jumpMaxDuration, 0f, this.jumpForce.x), Utils.CalculateRatio(this.jumpTime, 0f, this.jumpMaxDuration, 0f, this.jumpForce.y), Utils.CalculateRatio(this.jumpTime, 0f, this.jumpMaxDuration, 0f, this.jumpForce.z)) * TimeManager.GetTimeStepMultiplier();
					this.physicBody.AddForce(force2 * this.jumpForceMultiplier, ForceMode.VelocityChange);
					this.jumpTime -= Time.deltaTime;
				}
				else
				{
					this.jumpTime = 0f;
					this.isJumping = false;
				}
			}
			if (this.customGravity != 0f)
			{
				this.physicBody.AddForce(this.customGravity * Physics.gravity, ForceMode.Acceleration);
			}
		}

		// Token: 0x06001C74 RID: 7284 RVA: 0x000BE634 File Offset: 0x000BC834
		public void Move(Vector3 direction)
		{
			if (!this.allowMove)
			{
				return;
			}
			if (this.isGrounded)
			{
				this.moveDirection.x = direction.x * this.forwardSpeed * this.forwardSpeedMultiplier;
				this.moveDirection.z = direction.z * this.forwardSpeed * this.forwardSpeedMultiplier;
				return;
			}
			this.moveDirection.x = direction.x * this.horizontalAirSpeed;
			this.moveDirection.z = direction.z * this.horizontalAirSpeed;
		}

		// Token: 0x06001C75 RID: 7285 RVA: 0x000BE6C4 File Offset: 0x000BC8C4
		public void Jump(bool active)
		{
			if (!this.allowJump)
			{
				return;
			}
			if (!active)
			{
				this.isJumping = false;
				return;
			}
			if (this.isGrounded)
			{
				if (!this.isJumping)
				{
					this.jumpForce = new Vector3(0f, this.jumpGroundForce, 0f);
					this.jumpTime = this.jumpMaxDuration;
					this.isJumping = true;
					if (this.player && this.player.creature && this.player.creature.data.jumpEffectData != null)
					{
						EffectInstance effectInstance = this.player.creature.data.jumpEffectData.Spawn(this.player.creature.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
						effectInstance.source = Player.currentCreature;
						effectInstance.Play(0, false, false);
					}
					Action onJumpEvent = this.OnJumpEvent;
					if (onJumpEvent != null)
					{
						onJumpEvent();
					}
					if (this.player && this.player.creature)
					{
						Action onJumpEvent2 = this.player.creature.locomotion.OnJumpEvent;
						if (onJumpEvent2 == null)
						{
							return;
						}
						onJumpEvent2();
						return;
					}
				}
			}
			else if (this.player && this.player.creature && !this.isJumping)
			{
				bool jumpClimb = false;
				if (this.player.creature.handLeft.grabbedHandle && (this.player.creature.handLeft.grabbedHandle.data.forceClimbing || (this.player.creature.handLeft.grabbedHandle.item && this.player.creature.handLeft.grabbedHandle.item.data.grabAndGripClimb)))
				{
					this.player.creature.handLeft.UnGrab(true);
					jumpClimb = true;
				}
				if (this.player.creature.handRight.grabbedHandle && (this.player.creature.handRight.grabbedHandle.data.forceClimbing || (this.player.creature.handRight.grabbedHandle.item && this.player.creature.handRight.grabbedHandle.item.data.grabAndGripClimb)))
				{
					this.player.creature.handRight.UnGrab(true);
					jumpClimb = true;
				}
				if (this.player.creature.handLeft.climb.isGripping && this.player.creature.handLeft.climb.gripItem && this.player.creature.handLeft.climb.gripItem.data.grabAndGripClimb)
				{
					jumpClimb = true;
				}
				if (this.player.creature.handRight.climb.isGripping && this.player.creature.handRight.climb.gripItem && this.player.creature.handRight.climb.gripItem.data.grabAndGripClimb)
				{
					jumpClimb = true;
				}
				if (this.player.creature.handLeft.climb.isGripping && (this.player.creature.handLeft.climb.gripPhysicBody.isKinematic || this.player.creature.handLeft.climb.gripPhysicBody.gameObject.CompareTag("AllowJumpClimb")))
				{
					jumpClimb = true;
				}
				if (this.player.creature.handRight.climb.isGripping && (this.player.creature.handRight.climb.gripPhysicBody.isKinematic || this.player.creature.handRight.climb.gripPhysicBody.gameObject.CompareTag("AllowJumpClimb")))
				{
					jumpClimb = true;
				}
				if (this.player.creature.climber.footLeft.state == FeetClimber.Foot.State.Posed)
				{
					jumpClimb = true;
					this.player.creature.climber.footLeft.state = FeetClimber.Foot.State.Idle;
				}
				if (this.player.creature.climber.footRight.state == FeetClimber.Foot.State.Posed)
				{
					jumpClimb = true;
					this.player.creature.climber.footRight.state = FeetClimber.Foot.State.Idle;
				}
				if (jumpClimb)
				{
					float jumpForceX = this.player.head.cam.transform.forward.normalized.x * this.jumpGroundForce * this.jumpClimbHorizontalMultiplier;
					float jumpForceY = this.jumpGroundForce * this.jumpClimbVerticalMultiplier * Mathf.InverseLerp(this.jumpClimbVerticalMaxVelocityRatio, 0f, this.physicBody.velocity.y);
					float jumpForceZ = this.player.head.cam.transform.forward.normalized.z * this.jumpGroundForce * this.jumpClimbHorizontalMultiplier;
					this.jumpForce = new Vector3(jumpForceX, jumpForceY, jumpForceZ);
					this.jumpTime = this.jumpMaxDuration;
					this.isJumping = true;
					if (this.player && this.player.creature && this.player.creature.data.jumpEffectData != null)
					{
						EffectInstance effectInstance2 = this.player.creature.data.jumpEffectData.Spawn(this.player.creature.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
						effectInstance2.source = Player.currentCreature;
						effectInstance2.Play(0, false, false);
					}
					Action onJumpEvent3 = this.OnJumpEvent;
					if (onJumpEvent3 != null)
					{
						onJumpEvent3();
					}
					if (this.player && this.player.creature)
					{
						Action onJumpEvent4 = this.player.creature.locomotion.OnJumpEvent;
						if (onJumpEvent4 == null)
						{
							return;
						}
						onJumpEvent4();
					}
				}
			}
		}

		// Token: 0x06001C76 RID: 7286 RVA: 0x000BED20 File Offset: 0x000BCF20
		public void Turn(float speed, Locomotion.TurnMode turnMode)
		{
			if (turnMode == Locomotion.TurnMode.Disabled)
			{
				return;
			}
			if (turnMode == Locomotion.TurnMode.Smooth)
			{
				this.turnSmoothDirection = speed;
				return;
			}
			if (turnMode == Locomotion.TurnMode.Instant)
			{
				this.turnSnapDirection = speed;
				return;
			}
			if (turnMode == Locomotion.TurnMode.Snap)
			{
				this.turnSmoothSnapDirection = speed;
			}
		}

		// Token: 0x06001C77 RID: 7287 RVA: 0x000BED49 File Offset: 0x000BCF49
		public void StartShrinkCollider()
		{
			if (this.stopShrinkColliderCoroutine != null)
			{
				base.StopCoroutine(this.stopShrinkColliderCoroutine);
			}
			this.capsuleCollider.radius = this.colliderShrinkMinRadius;
			this.colliderIsShrinking = true;
		}

		// Token: 0x06001C78 RID: 7288 RVA: 0x000BED78 File Offset: 0x000BCF78
		public void StopShrinkCollider()
		{
			if (this.capsuleCollider.radius < this.colliderRadius)
			{
				if (this.stopShrinkColliderCoroutine != null)
				{
					base.StopCoroutine(this.stopShrinkColliderCoroutine);
				}
				this.stopShrinkColliderCoroutine = base.StartCoroutine(this.StopShrinkColliderCoroutine());
			}
			this.colliderIsShrinking = false;
		}

		// Token: 0x06001C79 RID: 7289 RVA: 0x000BEDC5 File Offset: 0x000BCFC5
		protected IEnumerator StopShrinkColliderCoroutine()
		{
			float time = 0f;
			while (time < this.colliderGrowDuration)
			{
				this.capsuleCollider.radius = Mathf.Lerp(this.colliderShrinkMinRadius, this.colliderRadius, Mathf.InverseLerp(0f, this.colliderGrowDuration, time));
				time += Time.deltaTime;
				yield return null;
			}
			yield break;
		}

		// Token: 0x06001C7A RID: 7290 RVA: 0x000BEDD4 File Offset: 0x000BCFD4
		public bool SphereCastGround(float castLenght, out RaycastHit raycastHit, out float groundDistance)
		{
			groundDistance = 0f;
			Vector3 colliderPosition = base.transform.TransformPoint(this.capsuleCollider.center);
			Ray ray = new Ray(new Vector3(colliderPosition.x, base.transform.position.y + this.capsuleCollider.height / 2f, colliderPosition.z), -base.transform.up);
			bool returnValue = false;
			if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration, BuildSettings.ContentFlagBehaviour.Discard))
			{
				foreach (Creature otherCreature in Creature.allActive)
				{
					if (otherCreature.isKilled && otherCreature.ragdoll.state == Ragdoll.State.Inert)
					{
						otherCreature.ragdoll.SetPartsLayer(LayerName.Ragdoll);
					}
				}
			}
			if (Physics.SphereCast(ray, this.capsuleCollider.radius * 0.99f, out raycastHit, castLenght, this.groundMask, QueryTriggerInteraction.Ignore))
			{
				groundDistance = raycastHit.distance - this.capsuleCollider.height / 2f;
				returnValue = true;
			}
			if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration, BuildSettings.ContentFlagBehaviour.Discard))
			{
				foreach (Creature otherCreature2 in Creature.allActive)
				{
					if (otherCreature2.isKilled && otherCreature2.ragdoll.state == Ragdoll.State.Inert)
					{
						otherCreature2.ragdoll.RefreshPartsLayer();
					}
				}
			}
			return returnValue;
		}

		// Token: 0x06001C7B RID: 7291 RVA: 0x000BEF64 File Offset: 0x000BD164
		public void SetAllSpeedModifiers(object handler, float multiplier)
		{
			this.SetSpeedModifier(handler, multiplier, multiplier, multiplier, multiplier, 1f, multiplier);
		}

		// Token: 0x06001C7C RID: 7292 RVA: 0x000BEF78 File Offset: 0x000BD178
		public void SetSpeedModifier(object handler, float forwardSpeedMultiplier = 1f, float backwardSpeedMultiplier = 1f, float strafeSpeedMultiplier = 1f, float runSpeedMultiplier = 1f, float jumpForceMultiplier = 1f, float crouchSpeedModifier = 1f)
		{
			Locomotion.SpeedModifier speedModifier = null;
			int speedModifiersCount = this.speedModifiers.Count;
			for (int i = 0; i < speedModifiersCount; i++)
			{
				Locomotion.SpeedModifier p = this.speedModifiers[i];
				if (p.handler == handler)
				{
					speedModifier = p;
					break;
				}
			}
			if (speedModifier != null)
			{
				speedModifier.forwardSpeedMultiplier = forwardSpeedMultiplier;
				speedModifier.backwardSpeedMultiplier = backwardSpeedMultiplier;
				speedModifier.strafeSpeedMultiplier = strafeSpeedMultiplier;
				speedModifier.runSpeedMultiplier = runSpeedMultiplier;
				speedModifier.jumpForceMultiplier = jumpForceMultiplier;
				speedModifier.crouchSpeedModifier = crouchSpeedModifier;
			}
			else
			{
				speedModifier = new Locomotion.SpeedModifier(handler, forwardSpeedMultiplier, backwardSpeedMultiplier, strafeSpeedMultiplier, runSpeedMultiplier, jumpForceMultiplier, crouchSpeedModifier);
				this.speedModifiers.Add(speedModifier);
			}
			this.RefreshSpeedModifiers();
		}

		// Token: 0x06001C7D RID: 7293 RVA: 0x000BF010 File Offset: 0x000BD210
		public void RemoveSpeedModifier(object handler)
		{
			for (int i = 0; i < this.speedModifiers.Count; i++)
			{
				if (this.speedModifiers[i].handler == handler)
				{
					this.speedModifiers.RemoveAtIgnoreOrder(i);
					i--;
				}
			}
			this.RefreshSpeedModifiers();
		}

		// Token: 0x06001C7E RID: 7294 RVA: 0x000BF05D File Offset: 0x000BD25D
		public void ClearSpeedModifiers()
		{
			this.speedModifiers.Clear();
			this.RefreshSpeedModifiers();
		}

		// Token: 0x06001C7F RID: 7295 RVA: 0x000BF070 File Offset: 0x000BD270
		public void RefreshSpeedModifiers()
		{
			if (this.speedModifiers.Count == 0)
			{
				this.forwardSpeedMultiplier = 1f;
				this.backwardSpeedMultiplier = 1f;
				this.strafeSpeedMultiplier = 1f;
				this.runSpeedMultiplier = 1f;
				this.jumpForceMultiplier = 1f;
				return;
			}
			float resultForwardSpeedMultiplier = 1f;
			float resultBackwardSpeedMultiplier = 1f;
			float resultStrafeSpeedMultiplier = 1f;
			float resultRunSpeedMultiplier = 1f;
			float resultJumpForceMultiplier = 1f;
			foreach (Locomotion.SpeedModifier speedModifier in this.speedModifiers)
			{
				resultForwardSpeedMultiplier *= speedModifier.forwardSpeedMultiplier;
				resultBackwardSpeedMultiplier *= speedModifier.backwardSpeedMultiplier;
				resultStrafeSpeedMultiplier *= speedModifier.strafeSpeedMultiplier;
				resultRunSpeedMultiplier *= speedModifier.runSpeedMultiplier;
				resultJumpForceMultiplier *= speedModifier.jumpForceMultiplier;
			}
			this.forwardSpeedMultiplier = resultForwardSpeedMultiplier;
			this.backwardSpeedMultiplier = resultBackwardSpeedMultiplier;
			this.strafeSpeedMultiplier = resultStrafeSpeedMultiplier;
			this.runSpeedMultiplier = resultRunSpeedMultiplier;
			this.jumpForceMultiplier = resultJumpForceMultiplier;
		}

		// Token: 0x06001C80 RID: 7296 RVA: 0x000BF17C File Offset: 0x000BD37C
		public void SetPhysicModifier(object handler, float? gravityMultiplier = null, float massMultiplier = -1f, float dragMultiplier = -1f, int duplicateId = -1)
		{
			if (gravityMultiplier == null && massMultiplier == -1f && dragMultiplier == -1f)
			{
				return;
			}
			Locomotion.PhysicModifier physicModifier = null;
			int physicModifiersCount = this.physicModifiers.Count;
			for (int i = 0; i < physicModifiersCount; i++)
			{
				Locomotion.PhysicModifier p = this.physicModifiers[i];
				if (p.handler == handler)
				{
					physicModifier = p;
					break;
				}
			}
			if (physicModifier != null)
			{
				physicModifier.gravityMultiplier = gravityMultiplier;
				physicModifier.massMultiplier = massMultiplier;
				physicModifier.dragMultiplier = dragMultiplier;
			}
			else
			{
				physicModifier = new Locomotion.PhysicModifier(handler, gravityMultiplier, massMultiplier, dragMultiplier, duplicateId);
				this.physicModifiers.Add(physicModifier);
			}
			this.RefreshPhysicModifiers();
		}

		// Token: 0x06001C81 RID: 7297 RVA: 0x000BF214 File Offset: 0x000BD414
		public void RemovePhysicModifier(object handler)
		{
			for (int i = 0; i < this.physicModifiers.Count; i++)
			{
				if (this.physicModifiers[i].handler == handler)
				{
					this.physicModifiers.RemoveAtIgnoreOrder(i);
					i--;
				}
			}
			this.RefreshPhysicModifiers();
		}

		// Token: 0x06001C82 RID: 7298 RVA: 0x000BF264 File Offset: 0x000BD464
		public void ClearPhysicModifiers()
		{
			for (int i = 0; i < this.physicModifiers.Count; i++)
			{
				if (this.physicModifiers[i].effectInstance != null)
				{
					this.physicModifiers[i].effectInstance.End(false, -1f);
				}
			}
			this.physicModifiers.Clear();
			this.RefreshPhysicModifiers();
		}

		// Token: 0x06001C83 RID: 7299 RVA: 0x000BF2C8 File Offset: 0x000BD4C8
		public void RefreshPhysicModifiers()
		{
			if (!this.physicBody)
			{
				return;
			}
			if (this.physicModifiers.Count == 0)
			{
				if (this.player && this.player.creature)
				{
					this.physicBody.mass = this.player.creature.data.locomotionMass;
				}
				else if (this.creature)
				{
					this.physicBody.mass = this.creature.data.locomotionMass;
				}
				else
				{
					this.physicBody.mass = this.orgMass;
				}
				this.physicBody.useGravity = true;
				this.customGravity = 0f;
				this.groundDragMultiplier = 1f;
				this.flyDragMultiplier = 1f;
			}
			else
			{
				float totalGravityMultiplier = 1f;
				float totalMassMultiplier = 1f;
				float totalDragMultiplier = 1f;
				HashSet<int> duplicates = new HashSet<int>();
				foreach (Locomotion.PhysicModifier physicModifier in this.physicModifiers)
				{
					if (physicModifier.duplicateId == -1 || duplicates.Add(physicModifier.duplicateId))
					{
						if (physicModifier.gravityMultiplier != null)
						{
							totalGravityMultiplier *= physicModifier.gravityMultiplier.Value;
						}
						if (physicModifier.massMultiplier > 0f)
						{
							totalMassMultiplier *= physicModifier.massMultiplier;
						}
						if (physicModifier.dragMultiplier >= 0f)
						{
							totalDragMultiplier *= physicModifier.dragMultiplier;
						}
					}
				}
				if (totalGravityMultiplier == 1f)
				{
					this.customGravity = 0f;
					this.physicBody.useGravity = true;
				}
				else if (totalGravityMultiplier <= 0f)
				{
					this.customGravity = 0f;
					this.physicBody.useGravity = false;
				}
				else
				{
					this.customGravity = totalGravityMultiplier;
					this.physicBody.useGravity = false;
				}
				if (this.player && this.player.creature)
				{
					this.physicBody.mass = totalMassMultiplier * this.player.creature.data.locomotionMass;
				}
				else if (this.creature)
				{
					this.physicBody.mass = totalMassMultiplier * this.creature.data.locomotionMass;
				}
				else
				{
					this.physicBody.mass = totalMassMultiplier * this.orgMass;
				}
				this.groundDragMultiplier = totalDragMultiplier;
				this.flyDragMultiplier = totalDragMultiplier;
			}
			PhysicBody physicBody = this.physicBody;
			float drag;
			if (this.isGrounded)
			{
				Player player = this.player;
				bool? flag;
				if (player == null)
				{
					flag = null;
				}
				else
				{
					Creature creature = player.creature;
					flag = ((creature != null) ? new bool?(creature.isSwimming) : null);
				}
				bool? flag2 = flag;
				bool flag3;
				if (flag2 == null)
				{
					Creature creature2 = this.creature;
					flag3 = (creature2 != null && creature2.isSwimming);
				}
				else
				{
					flag3 = flag2.GetValueOrDefault();
				}
				if (!flag3)
				{
					drag = this.groundDrag * this.groundDragMultiplier;
					goto IL_2E8;
				}
			}
			drag = this.flyDrag * this.flyDragMultiplier;
			IL_2E8:
			physicBody.drag = drag;
		}

		// Token: 0x04001AFE RID: 6910
		[Header("Ground movement")]
		public bool allowMove = true;

		// Token: 0x04001AFF RID: 6911
		public bool allowTurn = true;

		// Token: 0x04001B00 RID: 6912
		public bool allowJump = true;

		// Token: 0x04001B01 RID: 6913
		public bool allowCrouch = true;

		// Token: 0x04001B02 RID: 6914
		public AnimationCurve moveForceMultiplierByAngleCurve;

		// Token: 0x04001B03 RID: 6915
		public bool testMove;

		// Token: 0x04001B04 RID: 6916
		public float forwardSpeed = 0.2f;

		// Token: 0x04001B05 RID: 6917
		public float backwardSpeed = 0.2f;

		// Token: 0x04001B06 RID: 6918
		public float strafeSpeed = 0.2f;

		// Token: 0x04001B07 RID: 6919
		public float runSpeedAdd = 0.1f;

		// Token: 0x04001B08 RID: 6920
		public float crouchSpeed = 0.1f;

		// Token: 0x04001B09 RID: 6921
		public FloatHandler globalMoveSpeedMultiplier;

		// Token: 0x04001B0A RID: 6922
		protected float forwardSpeedMultiplier = 1f;

		// Token: 0x04001B0B RID: 6923
		protected float backwardSpeedMultiplier = 1f;

		// Token: 0x04001B0C RID: 6924
		protected float strafeSpeedMultiplier = 1f;

		// Token: 0x04001B0D RID: 6925
		protected float runSpeedMultiplier = 1f;

		// Token: 0x04001B0E RID: 6926
		protected float crouchSpeedMultiplier = 1f;

		// Token: 0x04001B0F RID: 6927
		protected float jumpForceMultiplier = 1f;

		// Token: 0x04001B10 RID: 6928
		public float forwardAngle = 10f;

		// Token: 0x04001B11 RID: 6929
		public float backwardAngle = 10f;

		// Token: 0x04001B12 RID: 6930
		public float runDot = 0.75f;

		// Token: 0x04001B13 RID: 6931
		public bool runEnabled = true;

		// Token: 0x04001B14 RID: 6932
		public float crouchHeightRatio = 0.8f;

		// Token: 0x04001B15 RID: 6933
		public float turnSpeed = 1f;

		// Token: 0x04001B16 RID: 6934
		private float snapTurnTime;

		// Token: 0x04001B17 RID: 6935
		private bool smoothSnapTurn;

		// Token: 0x04001B18 RID: 6936
		[Header("Jump / Fall")]
		public float horizontalAirSpeed = 0.02f;

		// Token: 0x04001B19 RID: 6937
		public float verticalAirSpeed;

		// Token: 0x04001B1A RID: 6938
		public float waterSpeed = 0.08f;

		// Token: 0x04001B1B RID: 6939
		public ForceMode airForceMode = ForceMode.VelocityChange;

		// Token: 0x04001B1C RID: 6940
		public float jumpGroundForce = 0.3f;

		// Token: 0x04001B1D RID: 6941
		public float jumpClimbVerticalMultiplier = 0.8f;

		// Token: 0x04001B1E RID: 6942
		public float jumpClimbVerticalMaxVelocityRatio = 20f;

		// Token: 0x04001B1F RID: 6943
		public float jumpClimbHorizontalMultiplier = 1f;

		// Token: 0x04001B20 RID: 6944
		public float jumpMaxDuration = 0.6f;

		// Token: 0x04001B21 RID: 6945
		[Header("Turn")]
		public float turnSmoothDirection;

		// Token: 0x04001B22 RID: 6946
		public float turnSnapDirection;

		// Token: 0x04001B23 RID: 6947
		public float turnSmoothSnapDirection;

		// Token: 0x04001B24 RID: 6948
		public Vector3 moveDirection;

		// Token: 0x04001B25 RID: 6949
		public ForceMode verticalForceMode = ForceMode.VelocityChange;

		// Token: 0x04001B26 RID: 6950
		[Header("Colliders")]
		public float colliderRadius = 0.3f;

		// Token: 0x04001B27 RID: 6951
		public float colliderShrinkMinRadius = 0.05f;

		// Token: 0x04001B28 RID: 6952
		public float colliderGrowDuration = 2f;

		// Token: 0x04001B29 RID: 6953
		public float colliderHeight = 1f;

		// Token: 0x04001B2A RID: 6954
		[Tooltip("Only enable this on ")]
		public bool collideWithPlayer = true;

		// Token: 0x04001B2B RID: 6955
		[Header("Ground detection")]
		public float groundDetectionDistance = 0.05f;

		// Token: 0x04001B2C RID: 6956
		public PhysicMaterial colliderGroundMaterial;

		// Token: 0x04001B2D RID: 6957
		public float groundDrag = 3f;

		// Token: 0x04001B2E RID: 6958
		protected float groundDragMultiplier = 1f;

		// Token: 0x04001B2F RID: 6959
		public PhysicMaterial colliderFlyMaterial;

		// Token: 0x04001B30 RID: 6960
		public float flyDrag = 1f;

		// Token: 0x04001B31 RID: 6961
		protected float flyDragMultiplier = 1f;

		// Token: 0x04001B32 RID: 6962
		[NonSerialized]
		public PhysicBody physicBody;

		// Token: 0x04001B33 RID: 6963
		[NonSerialized]
		public CapsuleCollider capsuleCollider;

		// Token: 0x04001B34 RID: 6964
		protected float orgMass;

		// Token: 0x04001B35 RID: 6965
		protected float orgDrag;

		// Token: 0x04001B36 RID: 6966
		protected float orgAngularDrag;

		// Token: 0x04001B37 RID: 6967
		protected float orgSleepThreshold;

		// Token: 0x04001B38 RID: 6968
		public float customGravity;

		// Token: 0x04001B39 RID: 6969
		[NonSerialized]
		public Vector3 prevVelocity;

		// Token: 0x04001B3A RID: 6970
		[NonSerialized]
		public Vector3 prevPosition;

		// Token: 0x04001B3B RID: 6971
		[NonSerialized]
		public Quaternion prevRotation;

		// Token: 0x04001B3C RID: 6972
		[NonSerialized]
		public Vector3 velocity;

		// Token: 0x04001B3D RID: 6973
		[NonSerialized]
		public float horizontalSpeed;

		// Token: 0x04001B3E RID: 6974
		[NonSerialized]
		public float verticalSpeed;

		// Token: 0x04001B3F RID: 6975
		[NonSerialized]
		public float angularSpeed;

		// Token: 0x04001B40 RID: 6976
		[NonSerialized]
		public bool isCrouched;

		// Token: 0x04001B41 RID: 6977
		[NonSerialized]
		public LayerMask groundMask;

		// Token: 0x04001B42 RID: 6978
		[NonSerialized]
		public bool isGrounded;

		// Token: 0x04001B43 RID: 6979
		[NonSerialized]
		public RaycastHit groundHit;

		// Token: 0x04001B44 RID: 6980
		[NonSerialized]
		public float groundAngle;

		// Token: 0x04001B46 RID: 6982
		private Vector3 accelerationCurrentSpeed;

		// Token: 0x04001B47 RID: 6983
		[NonSerialized]
		public bool isJumping;

		// Token: 0x04001B48 RID: 6984
		protected Vector3 jumpForce;

		// Token: 0x04001B49 RID: 6985
		protected float jumpTime;

		// Token: 0x04001B4A RID: 6986
		[NonSerialized]
		public bool jumpCharging;

		// Token: 0x04001B4B RID: 6987
		protected float jumpChargingTime;

		// Token: 0x04001B4C RID: 6988
		[NonSerialized]
		public float snapTurnDelay = 0.25f;

		// Token: 0x04001B4D RID: 6989
		[NonSerialized]
		public bool colliderIsShrinking;

		// Token: 0x04001B53 RID: 6995
		[NonSerialized]
		public Player player;

		// Token: 0x04001B54 RID: 6996
		[NonSerialized]
		public Creature creature;

		// Token: 0x04001B55 RID: 6997
		protected bool initialized;

		// Token: 0x04001B56 RID: 6998
		protected bool startGroundCheck = true;

		// Token: 0x04001B57 RID: 6999
		private Collider ignoredPlayerCreatureLocomotion;

		// Token: 0x04001B58 RID: 7000
		protected Coroutine stopShrinkColliderCoroutine;

		// Token: 0x04001B59 RID: 7001
		private Dictionary<Creature, List<Collider>> ignoredLocomotionOnlyColliders = new Dictionary<Creature, List<Collider>>();

		// Token: 0x04001B5A RID: 7002
		public List<Locomotion.SpeedModifier> speedModifiers;

		// Token: 0x04001B5B RID: 7003
		public List<Locomotion.PhysicModifier> physicModifiers;

		// Token: 0x020008E0 RID: 2272
		public enum GroundDetection
		{
			// Token: 0x040042FF RID: 17151
			Raycast,
			// Token: 0x04004300 RID: 17152
			Collision
		}

		// Token: 0x020008E1 RID: 2273
		public enum TurnMode
		{
			// Token: 0x04004302 RID: 17154
			Instant,
			// Token: 0x04004303 RID: 17155
			Snap,
			// Token: 0x04004304 RID: 17156
			Smooth,
			// Token: 0x04004305 RID: 17157
			Disabled
		}

		// Token: 0x020008E2 RID: 2274
		public enum CrouchMode
		{
			// Token: 0x04004307 RID: 17159
			Disabled,
			// Token: 0x04004308 RID: 17160
			Hold,
			// Token: 0x04004309 RID: 17161
			Toggle
		}

		// Token: 0x020008E3 RID: 2275
		// (Invoke) Token: 0x060041C2 RID: 16834
		public delegate void GroundEvent(Locomotion locomotion, Vector3 groundPoint, Vector3 velocity, Collider groundCollider);

		// Token: 0x020008E4 RID: 2276
		// (Invoke) Token: 0x060041C6 RID: 16838
		public delegate void CrouchEvent(bool isCrouching);

		// Token: 0x020008E5 RID: 2277
		// (Invoke) Token: 0x060041CA RID: 16842
		public delegate void CollisionEvent(Collision collision);

		// Token: 0x020008E6 RID: 2278
		// (Invoke) Token: 0x060041CE RID: 16846
		public delegate void FlyEvent(Locomotion locomotion);

		// Token: 0x020008E7 RID: 2279
		[Serializable]
		public class SpeedModifier
		{
			// Token: 0x060041D1 RID: 16849 RVA: 0x0018BF4C File Offset: 0x0018A14C
			public SpeedModifier(object handler, float forwardSpeedMultiplier, float backwardSpeedMultiplier, float strafeSpeedMultiplier, float runSpeedMultiplier, float jumpForceMultiplier, float crouchSpeedModifier)
			{
				this.handler = handler;
				this.forwardSpeedMultiplier = forwardSpeedMultiplier;
				this.backwardSpeedMultiplier = backwardSpeedMultiplier;
				this.strafeSpeedMultiplier = strafeSpeedMultiplier;
				this.runSpeedMultiplier = runSpeedMultiplier;
				this.jumpForceMultiplier = jumpForceMultiplier;
				this.crouchSpeedModifier = crouchSpeedModifier;
			}

			// Token: 0x0400430A RID: 17162
			[NonSerialized]
			public object handler;

			// Token: 0x0400430B RID: 17163
			public float forwardSpeedMultiplier;

			// Token: 0x0400430C RID: 17164
			public float backwardSpeedMultiplier;

			// Token: 0x0400430D RID: 17165
			public float strafeSpeedMultiplier;

			// Token: 0x0400430E RID: 17166
			public float runSpeedMultiplier;

			// Token: 0x0400430F RID: 17167
			public float jumpForceMultiplier;

			// Token: 0x04004310 RID: 17168
			public float crouchSpeedModifier;
		}

		// Token: 0x020008E8 RID: 2280
		[Serializable]
		public class PhysicModifier
		{
			// Token: 0x060041D2 RID: 16850 RVA: 0x0018BF89 File Offset: 0x0018A189
			public PhysicModifier(object handler, float? gravityMultiplier, float massMultiplier, float dragMultiplier, int duplicateId = -1)
			{
				this.handler = handler;
				this.gravityMultiplier = gravityMultiplier;
				this.massMultiplier = massMultiplier;
				this.dragMultiplier = dragMultiplier;
				this.duplicateId = duplicateId;
			}

			// Token: 0x04004311 RID: 17169
			[NonSerialized]
			public object handler;

			// Token: 0x04004312 RID: 17170
			public float? gravityMultiplier;

			// Token: 0x04004313 RID: 17171
			public float massMultiplier;

			// Token: 0x04004314 RID: 17172
			public float dragMultiplier;

			// Token: 0x04004315 RID: 17173
			public int duplicateId;

			// Token: 0x04004316 RID: 17174
			[NonSerialized]
			public EffectInstance effectInstance;
		}
	}
}
