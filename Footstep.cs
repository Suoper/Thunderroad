using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000259 RID: 601
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Footstep.html")]
	public class Footstep : ThunderBehaviour
	{
		// Token: 0x140000B3 RID: 179
		// (add) Token: 0x06001A5D RID: 6749 RVA: 0x000AFFB8 File Offset: 0x000AE1B8
		// (remove) Token: 0x06001A5E RID: 6750 RVA: 0x000AFFEC File Offset: 0x000AE1EC
		public static event Footstep.FootStepEvent OnFootStepEvent;

		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06001A5F RID: 6751 RVA: 0x000B001F File Offset: 0x000AE21F
		// (set) Token: 0x06001A60 RID: 6752 RVA: 0x000B0027 File Offset: 0x000AE227
		public float stepIntensityMultiplier { get; private set; } = 1f;

		// Token: 0x140000B4 RID: 180
		// (add) Token: 0x06001A61 RID: 6753 RVA: 0x000B0030 File Offset: 0x000AE230
		// (remove) Token: 0x06001A62 RID: 6754 RVA: 0x000B0068 File Offset: 0x000AE268
		public event Footstep.StepEvent OnStep;

		// Token: 0x06001A63 RID: 6755 RVA: 0x000B00A0 File Offset: 0x000AE2A0
		private void Awake()
		{
			this.materialData = Catalog.GetData<MaterialData>(this.materialId, true);
			this.defaultLayer = GameManager.GetLayer(LayerName.Default);
			this.locomotionOnlyLayer = GameManager.GetLayer(LayerName.LocomotionOnly);
			EventManager.onPossess += this.OnPossessionEvent;
			EventManager.onUnpossess += this.OnUnpossessionEvent;
			this.OnStep += this.PlayStepEffects;
		}

		// Token: 0x06001A64 RID: 6756 RVA: 0x000B010C File Offset: 0x000AE30C
		private void Start()
		{
			this.creature = base.GetComponentInParent<Creature>();
			if (this.creature != null)
			{
				this.creatureLocomotion = this.creature.locomotion;
				this.footLeft = this.creature.GetFoot(Side.Left).toesAnchor;
				this.footRight = this.creature.GetFoot(Side.Right).toesAnchor;
			}
		}

		// Token: 0x06001A65 RID: 6757 RVA: 0x000B0174 File Offset: 0x000AE374
		private void OnDestroy()
		{
			this.OnStep -= this.PlayStepEffects;
			if (this.creatureLocomotion)
			{
				this.creatureLocomotion.OnGroundEvent -= this.OnLocomotionGroundEvent;
			}
			EventManager.onPossess -= this.OnPossessionEvent;
			EventManager.onUnpossess -= this.OnUnpossessionEvent;
		}

		// Token: 0x06001A66 RID: 6758 RVA: 0x000B01D9 File Offset: 0x000AE3D9
		public void AddStepVolumeMultiplier(object handler, float multiplier)
		{
			this.playerStepIntensityMultipliers[handler] = multiplier;
			this.UpdateMultiplier();
		}

		// Token: 0x06001A67 RID: 6759 RVA: 0x000B01EE File Offset: 0x000AE3EE
		public void RemoveStepVolumeMultiplier(object handler)
		{
			this.playerStepIntensityMultipliers.Remove(handler);
			this.UpdateMultiplier();
		}

		// Token: 0x06001A68 RID: 6760 RVA: 0x000B0204 File Offset: 0x000AE404
		private void UpdateMultiplier()
		{
			this.stepIntensityMultiplier = 1f;
			foreach (KeyValuePair<object, float> kvp in this.playerStepIntensityMultipliers)
			{
				this.stepIntensityMultiplier *= kvp.Value;
				if (this.stepIntensityMultiplier == 0f)
				{
					break;
				}
			}
		}

		/// <summary>
		/// Caches the player's creature and locomotion.
		/// Updates on ground event binding.
		/// Caches the creature's feet.
		/// </summary>
		/// <param name="creature">Player's creature</param>
		/// <param name="eventTime"></param>
		// Token: 0x06001A69 RID: 6761 RVA: 0x000B0280 File Offset: 0x000AE480
		private void OnPossessionEvent(Creature creature, EventTime eventTime)
		{
			if (base.gameObject.activeInHierarchy && eventTime == EventTime.OnEnd && creature == this.creature)
			{
				this.creatureLocomotion.OnGroundEvent -= this.OnLocomotionGroundEvent;
				this.creatureLocomotion = creature.player.locomotion;
				this.creatureLocomotion.OnGroundEvent += this.OnLocomotionGroundEvent;
				this.footLeft = creature.GetFoot(Side.Left).toesAnchor;
				this.footRight = creature.GetFoot(Side.Right).toesAnchor;
			}
		}

		/// <summary>
		/// Un-cache the player's creature and locomotion. Updates on ground event binding
		/// </summary>
		/// <param name="creature">Player's creature</param>
		/// <param name="eventTime"></param>
		// Token: 0x06001A6A RID: 6762 RVA: 0x000B0310 File Offset: 0x000AE510
		private void OnUnpossessionEvent(Creature creature, EventTime eventTime)
		{
			if (base.gameObject.activeInHierarchy && eventTime == EventTime.OnEnd && creature == this.creature)
			{
				this.creatureLocomotion.OnGroundEvent -= this.OnLocomotionGroundEvent;
				this.creatureLocomotion = creature.locomotion;
				this.creatureLocomotion.OnGroundEvent += this.OnLocomotionGroundEvent;
			}
		}

		/// <summary>
		/// When the locomotion is grounded (ie. falling), this method gets called.
		/// It forces the feet to play their sounds.
		/// </summary>
		/// <param name="groundPoint">Position of the fall</param>
		/// <param name="velocity">Velocity of the fall</param>
		/// <param name="groundCollider">Collider of the ground object</param>
		// Token: 0x06001A6B RID: 6763 RVA: 0x000B0378 File Offset: 0x000AE578
		private void OnLocomotionGroundEvent(Locomotion locomotion, Vector3 groundPoint, Vector3 velocity, Collider groundCollider)
		{
			float time = Time.time;
			if (time < this.lastFallTime + this.fallMinDelay)
			{
				return;
			}
			if (this.quietLanding && this.creature.currentLocomotion.isCrouched)
			{
				return;
			}
			this.CheckFootLeft(this.footLeft.position, true);
			this.CheckFootRight(this.footRight.position, true);
			this.lastFallTime = time;
		}

		/// <summary>
		/// Picks effects from the groundCollider's material, and play on the given ground point.
		/// </summary>
		/// <param name="groundPoint">Where to spawn the effects (world)</param>
		/// <param name="groundCollider">GroundCollider the creature is currently standing on</param>
		/// <param name="speed">Speed of the effects to play</param>
		/// <param name="intensity">Intensity of the effects to play</param>
		// Token: 0x06001A6C RID: 6764 RVA: 0x000B03E4 File Offset: 0x000AE5E4
		private void PlayOnGroundEffects(Vector3 groundPoint, Collider groundCollider, float speed, float intensity)
		{
			MaterialData.Collision materialCollision = this.materialData.GetCollision(Animator.StringToHash(groundCollider.material.name));
			EffectInstance effectInstance = EffectInstance.Spawn((materialCollision == null) ? this.materialData.defaultEffects : materialCollision.effects, groundPoint, Quaternion.LookRotation(Vector3.up), intensity * (this.creature.isPlayer ? this.stepIntensityMultiplier : 1f), speed * (this.creature.isPlayer ? this.stepIntensityMultiplier : 1f), null, null, true, null, true);
			effectInstance.SetNoise(true);
			effectInstance.source = this.creature;
			effectInstance.Play(0, false, false);
		}

		/// <summary>
		/// Converts the creature locomotion velocity to a [0 ; 1] float, using its magnitude.
		/// The value is clamped between 2 thresholds values (different when falling).
		/// </summary>
		/// <returns>A velocity magnitude clamped between 2 thresholds values, different when falling.</returns>
		// Token: 0x06001A6D RID: 6765 RVA: 0x000B048C File Offset: 0x000AE68C
		private float GetFootSpeedRatio(bool falling = false)
		{
			if (!this.creatureLocomotion)
			{
				return 0f;
			}
			float factor = 1f;
			if (falling)
			{
				factor = this.fallingIntensityFactor;
			}
			else if (this.creatureLocomotion.isCrouched)
			{
				factor = this.crouchingIntensityFactor;
			}
			Vector2 thresholds = falling ? this.minMaxFallingVelocity : this.minMaxStandingVelocity;
			return Mathf.InverseLerp(thresholds.x, thresholds.y, this.creatureLocomotion.velocity.magnitude) * factor;
		}

		// Token: 0x1700019C RID: 412
		// (get) Token: 0x06001A6E RID: 6766 RVA: 0x000B0507 File Offset: 0x000AE707
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		/// <summary>
		/// Keep track of the steps cool-downs and calls related stepping methods
		/// </summary>
		// Token: 0x06001A6F RID: 6767 RVA: 0x000B050C File Offset: 0x000AE70C
		protected internal override void ManagedUpdate()
		{
			if (this.footLeft == null || this.footRight == null)
			{
				return;
			}
			float time = Time.time;
			if (time >= this.lastStepTimeLeft + this.stepMinDelay)
			{
				this.CheckFootLeft(this.footLeft.position, false);
			}
			if (time >= this.lastStepTimeRight + this.stepMinDelay)
			{
				this.CheckFootRight(this.footRight.position, false);
			}
		}

		/// <summary>
		/// Checks if the left foot has stepped.
		/// </summary>
		/// <param name="footLeftPosition">Position of the left foot.</param>
		/// <param name="forceStep">Force the sound to play even when on cooldown (used on fall).</param>
		// Token: 0x06001A70 RID: 6768 RVA: 0x000B0580 File Offset: 0x000AE780
		private void CheckFootLeft(Vector3 footLeftPosition, bool forceStep = false)
		{
			float offsetThreshold = this.creatureLocomotion.IsRunning ? this.footstepDetectionRunningHeightThreshold : this.footstepDetectionHeightThreshold;
			bool stepped = footLeftPosition.y - this.creatureLocomotion.groundHit.point.y <= offsetThreshold;
			if (!this.hasSteppedLeft)
			{
				if (stepped || forceStep)
				{
					this.hasSteppedLeft = true;
					this.lastStepTimeLeft = Time.time;
					Footstep.StepEvent onStep = this.OnStep;
					if (onStep == null)
					{
						return;
					}
					onStep(footLeftPosition, Side.Left, this.GetFootSpeedRatio(forceStep));
					return;
				}
			}
			else if (forceStep)
			{
				this.hasSteppedLeft = true;
				this.lastStepTimeLeft = Time.time;
				Footstep.StepEvent onStep2 = this.OnStep;
				if (onStep2 == null)
				{
					return;
				}
				onStep2(footLeftPosition, Side.Left, this.GetFootSpeedRatio(true));
				return;
			}
			else if (!stepped)
			{
				this.hasSteppedLeft = false;
			}
		}

		/// <summary>
		/// Checks if the right foot has stepped
		/// </summary>
		/// <param name="footRightPosition">position of the right foot</param>
		/// <param name="forceStep">Force the sound to play even when on cooldown (used on fall)</param>
		// Token: 0x06001A71 RID: 6769 RVA: 0x000B0640 File Offset: 0x000AE840
		private void CheckFootRight(Vector3 footRightPosition, bool forceStep = false)
		{
			float offsetThreshold = this.creatureLocomotion.IsRunning ? this.footstepDetectionRunningHeightThreshold : this.footstepDetectionHeightThreshold;
			bool stepped = footRightPosition.y - this.creatureLocomotion.groundHit.point.y <= offsetThreshold;
			if (!this.hasSteppedRight)
			{
				if (stepped || forceStep)
				{
					this.hasSteppedRight = true;
					this.lastStepTimeRight = Time.time;
					Footstep.StepEvent onStep = this.OnStep;
					if (onStep == null)
					{
						return;
					}
					onStep(footRightPosition, Side.Right, this.GetFootSpeedRatio(forceStep));
					return;
				}
			}
			else if (forceStep)
			{
				this.hasSteppedRight = true;
				this.lastStepTimeRight = Time.time;
				Footstep.StepEvent onStep2 = this.OnStep;
				if (onStep2 == null)
				{
					return;
				}
				onStep2(footRightPosition, Side.Right, this.GetFootSpeedRatio(true));
				return;
			}
			else if (!stepped)
			{
				this.hasSteppedRight = false;
			}
		}

		/// <summary>
		/// Choose and play footsteps effects.
		/// </summary>
		/// <param name="position">Where to spawn the effect (world)</param>
		/// <param name="side">Side of the foot currently stepping</param>
		/// <param name="velocity">Velocity of the creature</param>
		// Token: 0x06001A72 RID: 6770 RVA: 0x000B0700 File Offset: 0x000AE900
		private void PlayStepEffects(Vector3 position, Side side, float velocity)
		{
			if (!Player.local)
			{
				return;
			}
			if (!Player.local.creature)
			{
				return;
			}
			Vector3 footPosition = position;
			Footstep.FootStepEvent onFootStepEvent = Footstep.OnFootStepEvent;
			if (onFootStepEvent != null)
			{
				onFootStepEvent(this, footPosition, velocity * (this.creature.isPlayer ? this.stepIntensityMultiplier : 1f), velocity * (this.creature.isPlayer ? this.stepIntensityMultiplier : 1f), this.creature.waterHandler.inWater);
			}
			if (this.creature.waterHandler.inWater)
			{
				footPosition.y = this.creature.waterHandler.waterSurfacePosition.y;
				EffectInstance effectInstance = Catalog.gameData.water.footstepEffectData.Spawn(footPosition, Quaternion.LookRotation(Vector3.up), null, null, true, null, false, this.creature.waterHandler.submergedRatio, velocity, Array.Empty<Type>());
				effectInstance.SetNoise(true);
				effectInstance.source = this.creature;
				effectInstance.Play(0, false, false);
				return;
			}
			if (this.materialData == null)
			{
				return;
			}
			if (this.creatureLocomotion == null)
			{
				return;
			}
			if (this.usePerFootRaycastCheck && this.PerFootRaycast(position, side, velocity))
			{
				return;
			}
			Collider floorCollider = this.creatureLocomotion.groundHit.collider;
			if (floorCollider == null)
			{
				return;
			}
			int floorColliderLayer = floorCollider.gameObject.layer;
			if (floorColliderLayer != this.defaultLayer && floorColliderLayer != this.locomotionOnlyLayer)
			{
				return;
			}
			this.PlayOnGroundEffects(footPosition, floorCollider, velocity, velocity);
		}

		/// <summary>
		/// Casts a ray on the position, sampling above and under the foot.
		/// Plays only the effects of the highest collider.
		/// </summary>
		/// <param name="position">Position of the ray to cast</param>
		/// <param name="side">Side of the foot</param>
		/// <param name="velocity">Velocity of the player</param>
		/// <returns>True if it found and played an effect, false otherwise</returns>
		// Token: 0x06001A73 RID: 6771 RVA: 0x000B087C File Offset: 0x000AEA7C
		private bool PerFootRaycast(Vector3 position, Side side, float velocity)
		{
			float raycastHeight = (this.creatureLocomotion.IsRunning ? this.footstepDetectionRunningHeightThreshold : this.footstepDetectionHeightThreshold) * 2.5f;
			int hitCount = Physics.SphereCastNonAlloc(position + Vector3.up * raycastHeight, 0.05f, Vector3.down, this.hits, raycastHeight + raycastHeight / 10f, ThunderRoadSettings.current.groundLayer, QueryTriggerInteraction.Collide);
			if (hitCount > 0)
			{
				float maxHeight = this.hits[0].point.y;
				int maxHeightHitIndex = 0;
				for (int i = 1; i < hitCount; i++)
				{
					if (this.hits[i].collider.gameObject.layer == this.defaultLayer || this.hits[i].collider.gameObject.layer == this.locomotionOnlyLayer)
					{
						float h = this.hits[i].point.y;
						if (h > maxHeight)
						{
							maxHeight = h;
							maxHeightHitIndex = i;
						}
					}
				}
				RaycastHit currentHit = this.hits[maxHeightHitIndex];
				bool isWaterCollision = this.waterMaterialHash == Animator.StringToHash(currentHit.collider.material.name);
				this.PlayOnGroundEffects(position, currentHit.collider, velocity, isWaterCollision ? 0.1f : velocity);
				return true;
			}
			return false;
		}

		// Token: 0x06001A74 RID: 6772 RVA: 0x000B09D4 File Offset: 0x000AEBD4
		private void OnDrawGizmosSelected()
		{
			Vector3 floorPoint = base.transform.position;
			Vector3 floorNormal = Vector3.up;
			if (this.creatureLocomotion)
			{
				floorPoint = this.creatureLocomotion.groundHit.point;
				floorNormal = this.creatureLocomotion.groundHit.normal;
			}
			Vector3 footLeftPosition = base.transform.position;
			Vector3 footRightPosition = base.transform.position;
			if (this.footLeft)
			{
				footLeftPosition = this.footLeft.position;
			}
			if (this.footRight)
			{
				footRightPosition = this.footRight.position;
			}
			Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(floorPoint + floorNormal * this.footstepDetectionHeightThreshold, Quaternion.identity, new Vector3(1f, 0f, 1f));
			Gizmos.DrawWireSphere(Vector3.zero, 0.2f);
			Gizmos.DrawWireSphere(Vector3.zero, 0.8f);
			Gizmos.matrix = matrix;
			Matrix4x4 matrix2 = Gizmos.matrix;
			Gizmos.color = new Color(1f, 0.65f, 0.7f);
			Gizmos.matrix = Matrix4x4.TRS(floorPoint + floorNormal * this.footstepDetectionRunningHeightThreshold, Quaternion.identity, new Vector3(1f, 0f, 1f));
			Gizmos.DrawWireSphere(Vector3.zero, 0.1f);
			Gizmos.DrawWireSphere(Vector3.zero, 0.7f);
			Gizmos.matrix = matrix2;
			Gizmos.color = (this.hasSteppedLeft ? Color.red : Color.yellow);
			Gizmos.DrawLine(footLeftPosition, new Vector3(footLeftPosition.x, floorPoint.y, footLeftPosition.z));
			Gizmos.color = (this.hasSteppedRight ? Color.red : Color.yellow);
			Gizmos.DrawLine(footRightPosition, new Vector3(footRightPosition.x, floorPoint.y, footRightPosition.z));
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(footLeftPosition, 0.015f);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(footRightPosition, 0.015f);
			float raycastHeight = this.footstepDetectionHeightThreshold * 2f;
			Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
			Gizmos.DrawLine(footLeftPosition + Vector3.up * raycastHeight, footLeftPosition + Vector3.down * (raycastHeight + raycastHeight / 10f));
			Gizmos.DrawLine(footRightPosition + Vector3.up * raycastHeight, footRightPosition + Vector3.down * (raycastHeight + raycastHeight / 10f));
		}

		/// <summary>
		/// Id of the material to retrieve the sounds from (in catalog).
		/// </summary>
		// Token: 0x04001914 RID: 6420
		[Tooltip("Id of the material to retrieve the sounds from (in catalog).")]
		public string materialId = "Footstep";

		/// <summary>
		/// If true, on each step we trigger a raycast sampling above and under the foot.
		/// We only keep the higher collider and play its effects. We discard the others.
		/// </summary>
		// Token: 0x04001915 RID: 6421
		[Tooltip("Enables on each step a ray sampling above and under the foot. Plays only the effects of the highest collider. (Needs to be true for water planes)")]
		public bool usePerFootRaycastCheck = true;

		/// <summary>
		/// Velocity thresholds, used in an inverse lerp when walking and running.
		/// </summary>
		// Token: 0x04001916 RID: 6422
		[Tooltip("Velocity thresholds, used in an inverse lerp when walking and running.")]
		public Vector2 minMaxStandingVelocity = new Vector2(0f, 8f);

		/// <summary>
		/// Velocity thresholds, used in an inverse lerp when falling.
		/// </summary>
		// Token: 0x04001917 RID: 6423
		[Tooltip("Velocity thresholds, used in an inverse lerp when falling.")]
		public Vector2 minMaxFallingVelocity = new Vector2(0.1f, 10f);

		/// <summary>
		/// Factor used to tweak the intensity of the footsteps when falling.
		/// </summary>
		// Token: 0x04001918 RID: 6424
		[Tooltip("Factor used to tweak the intensity of the footsteps when falling.")]
		public float fallingIntensityFactor = 1.25f;

		/// <summary>
		/// Factor used to tweak the intensity of the footsteps when crouching.
		/// </summary>
		// Token: 0x04001919 RID: 6425
		[Tooltip("Factor used to tweak the intensity of the footsteps when crouching.")]
		public float crouchingIntensityFactor = 0.35f;

		/// <summary>
		/// Cool-down delay in second between two steps. Different timers are used for each foot.
		/// </summary>
		// Token: 0x0400191A RID: 6426
		[Tooltip("Cool-down delay in second between two steps. Different timers are used for each foot.")]
		public float stepMinDelay = 0.2f;

		/// <summary>
		/// Cool-down delay in second between two falls.
		/// </summary>
		// Token: 0x0400191B RID: 6427
		[Tooltip("Cool-down delay in second between two falls.")]
		public float fallMinDelay = 0.2f;

		/// <summary>
		/// Height used to detect footsteps, it's added to the locomotion ground point (in meters).
		/// </summary>
		// Token: 0x0400191C RID: 6428
		[Tooltip("Height used to detect footsteps, it's added to the locomotion ground point (in meters).")]
		public float footstepDetectionHeightThreshold = 0.045f;

		/// <summary>
		/// Height used to detect footsteps while running, it's added to the locomotion ground point (in meters).
		/// </summary>
		// Token: 0x0400191D RID: 6429
		[Tooltip("Height used to detect footsteps while running, it's added to the locomotion ground point (in meters).")]
		public float footstepDetectionRunningHeightThreshold = 0.08f;

		/// <summary>
		/// RaycastHit buffer, used to sample the floor material per foot.
		/// </summary>
		// Token: 0x0400191E RID: 6430
		private readonly RaycastHit[] hits = new RaycastHit[4];

		// Token: 0x0400191F RID: 6431
		private int waterMaterialHash = Animator.StringToHash("Water (Instance)");

		// Token: 0x04001920 RID: 6432
		protected MaterialData materialData;

		// Token: 0x04001921 RID: 6433
		public Creature creature;

		// Token: 0x04001922 RID: 6434
		protected Locomotion creatureLocomotion;

		// Token: 0x04001923 RID: 6435
		private int defaultLayer;

		// Token: 0x04001924 RID: 6436
		private int locomotionOnlyLayer;

		// Token: 0x04001925 RID: 6437
		private float lastStepTimeRight;

		// Token: 0x04001926 RID: 6438
		private float lastStepTimeLeft;

		// Token: 0x04001927 RID: 6439
		private float lastFallTime;

		// Token: 0x04001928 RID: 6440
		private Transform footLeft;

		// Token: 0x04001929 RID: 6441
		private Transform footRight;

		// Token: 0x0400192A RID: 6442
		private bool hasSteppedLeft;

		// Token: 0x0400192B RID: 6443
		private bool hasSteppedRight;

		// Token: 0x0400192C RID: 6444
		public bool quietLanding;

		// Token: 0x0400192E RID: 6446
		private Dictionary<object, float> playerStepIntensityMultipliers = new Dictionary<object, float>();

		// Token: 0x020008AC RID: 2220
		// (Invoke) Token: 0x0600410E RID: 16654
		public delegate void FootStepEvent(Footstep footstep, Vector3 position, float intensity, float speed, bool inWater);

		// Token: 0x020008AD RID: 2221
		// (Invoke) Token: 0x06004112 RID: 16658
		public delegate void StepEvent(Vector3 position, Side side, float velocity);
	}
}
