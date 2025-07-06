using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002AE RID: 686
	public class DragArea : ThunderBehaviour
	{
		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06002017 RID: 8215 RVA: 0x000DA2CA File Offset: 0x000D84CA
		public Vector3 Normal
		{
			get
			{
				return base.transform.forward;
			}
		}

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06002018 RID: 8216 RVA: 0x000DA2D7 File Offset: 0x000D84D7
		public float Area
		{
			get
			{
				return this.surfaceDimension.x * this.surfaceDimension.y;
			}
		}

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x06002019 RID: 8217 RVA: 0x000DA2F0 File Offset: 0x000D84F0
		public Vector3 Velocity
		{
			get
			{
				return this.GetVelocity();
			}
		}

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x0600201A RID: 8218 RVA: 0x000DA2F8 File Offset: 0x000D84F8
		public Vector3 Drag
		{
			get
			{
				return this.GetDrag(this.Velocity, this.currentFluid);
			}
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x0600201B RID: 8219 RVA: 0x000DA30C File Offset: 0x000D850C
		public Vector3 Lift
		{
			get
			{
				return this.GetLift();
			}
		}

		// Token: 0x0600201C RID: 8220 RVA: 0x000DA314 File Offset: 0x000D8514
		private void Awake()
		{
			this.SetFluidAir();
			if (this.listenToItemCallbacks)
			{
				this.CheckIfIsItemAndCacheValues();
			}
			this.waterHandler = new WaterHandler(false, true);
			this.waterHandler.OnWaterEnter += this.OnWaterEnter;
			this.waterHandler.OnWaterExit += this.OnWaterExit;
		}

		// Token: 0x0600201D RID: 8221 RVA: 0x000DA370 File Offset: 0x000D8570
		private void Start()
		{
			UnityEvent unityEvent = this.onStart;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		/// <summary>
		/// Caches the parent item, and hooks itself to its callbacks
		/// </summary>
		// Token: 0x0600201E RID: 8222 RVA: 0x000DA384 File Offset: 0x000D8584
		private void CheckIfIsItemAndCacheValues()
		{
			this.item = base.GetComponentInParent<Item>();
			if (this.item)
			{
				this.item.OnGrabEvent += this.OnObjectGrabbed;
				this.item.OnUngrabEvent += this.OnObjectReleased;
				this.item.OnSnapEvent += this.OnSnap;
			}
		}

		/// <summary>
		/// Updates the water handler
		/// </summary>
		// Token: 0x0600201F RID: 8223 RVA: 0x000DA3F0 File Offset: 0x000D85F0
		private void UpdateWater()
		{
			Vector3 position = this.center.position;
			this.waterHandler.Update(position, position.y - this.surfaceDimension.y / 2f, position.y + this.surfaceDimension.y / 2f, Mathf.Max(this.surfaceDimension.x, this.surfaceDimension.y) / 2f, this.Velocity);
		}

		/// <summary>
		/// When the water handler detects that the area is immersed, set the fluid to water.
		/// </summary>
		// Token: 0x06002020 RID: 8224 RVA: 0x000DA46C File Offset: 0x000D866C
		private void OnWaterEnter()
		{
			this.SetFluidWater();
		}

		/// <summary>
		/// When the water handler detects that the area is not immersed anymore, set the fluid back to default.
		/// </summary>
		// Token: 0x06002021 RID: 8225 RVA: 0x000DA474 File Offset: 0x000D8674
		private void OnWaterExit()
		{
			this.SetFluidAir();
		}

		/// <summary>
		/// Called when the parent item snaps to an holder.
		/// </summary>
		/// <param name="holder"></param>
		// Token: 0x06002022 RID: 8226 RVA: 0x000DA47C File Offset: 0x000D867C
		private void OnSnap(Holder holder)
		{
			UnityEvent unityEvent = this.onItemSnaps;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		/// <summary>
		/// Called when the parent item is grabbed.
		/// Caches the grabbing creature to the different values (if used).
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="ragdollhand"></param>
		// Token: 0x06002023 RID: 8227 RVA: 0x000DA490 File Offset: 0x000D8690
		private void OnObjectGrabbed(Handle handle, RagdollHand ragdollhand)
		{
			UnityEvent<Handle, RagdollHand> unityEvent = this.onItemIsGrabbed;
			if (unityEvent != null)
			{
				unityEvent.Invoke(handle, ragdollhand);
			}
			this.creature = ragdollhand.creature;
			PhysicBody physicBody = this.creature.player ? this.creature.player.locomotion.physicBody : this.creature.locomotion.physicBody;
			if (this.dragGrabbingCreatureBody && !this.bodiesToDrag.Contains(physicBody))
			{
				this.bodiesToDrag.Add(physicBody);
			}
			if (this.liftGrabbingCreatureBody && !this.bodiesToLift.Contains(physicBody))
			{
				this.bodiesToLift.Add(physicBody);
			}
			this.defaultVelocityOrigin = this.velocityOrigin;
			if (this.useCreatureLocomotionAsVelocityOrigin)
			{
				this.velocityOrigin = physicBody.transform;
			}
		}

		/// <summary>
		/// Called when the parent item is released.
		/// un-caches the grabbing creature to the different values (if used).
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="ragdollhand"></param>
		// Token: 0x06002024 RID: 8228 RVA: 0x000DA55C File Offset: 0x000D875C
		private void OnObjectReleased(Handle handle, RagdollHand ragdollhand, bool throwing)
		{
			UnityEvent<Handle, RagdollHand> unityEvent = this.onItemIsUnGrabbed;
			if (unityEvent != null)
			{
				unityEvent.Invoke(handle, ragdollhand);
			}
			this.creature = ragdollhand.creature;
			PhysicBody rb = this.creature.player ? this.creature.player.locomotion.physicBody : this.creature.locomotion.physicBody;
			if (this.dragGrabbingCreatureBody)
			{
				this.bodiesToDrag.Remove(rb);
			}
			if (this.liftGrabbingCreatureBody)
			{
				this.bodiesToLift.Remove(rb);
			}
			if (this.useCreatureLocomotionAsVelocityOrigin)
			{
				this.velocityOrigin = this.defaultVelocityOrigin;
			}
		}

		// Token: 0x06002025 RID: 8229 RVA: 0x000DA600 File Offset: 0x000D8800
		protected override void ManagedOnEnable()
		{
			if (this.velocityType == DragArea.VelocityType.Estimated)
			{
				this.BeginEstimatingVelocity();
			}
			UnityEvent unityEvent = this.onStart;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x06002026 RID: 8230 RVA: 0x000DA620 File Offset: 0x000D8820
		protected override void ManagedOnDisable()
		{
			if (this.velocityType == DragArea.VelocityType.Estimated)
			{
				this.FinishEstimatingVelocity();
			}
			UnityEvent unityEvent = this.onStop;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			this.waterHandler.Reset();
		}

		// Token: 0x06002027 RID: 8231 RVA: 0x000DA64C File Offset: 0x000D884C
		private void OnDestroy()
		{
			UnityEvent unityEvent = this.onStop;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			if (this.item)
			{
				this.item.OnGrabEvent -= this.OnObjectGrabbed;
				this.item.OnUngrabEvent -= this.OnObjectReleased;
				this.item.OnSnapEvent -= this.OnSnap;
			}
			this.waterHandler.OnWaterEnter -= this.OnWaterEnter;
			this.waterHandler.OnWaterExit -= this.OnWaterExit;
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x06002028 RID: 8232 RVA: 0x000DA6EA File Offset: 0x000D88EA
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate | ManagedLoops.Update;
			}
		}

		// Token: 0x06002029 RID: 8233 RVA: 0x000DA6ED File Offset: 0x000D88ED
		protected internal override void ManagedUpdate()
		{
			if (this.checkForWater)
			{
				this.UpdateWater();
			}
		}

		// Token: 0x0600202A RID: 8234 RVA: 0x000DA700 File Offset: 0x000D8900
		protected internal override void ManagedFixedUpdate()
		{
			Vector3 drag = this.Drag;
			if (this.bodiesToDrag != null)
			{
				for (int i = 0; i < this.bodiesToDrag.Count; i++)
				{
					this.bodiesToDrag[i].AddForceAtPosition(drag, this.center.position, ForceMode.Force);
				}
			}
			Vector3 lift = this.Lift;
			if (this.bodiesToLift != null)
			{
				for (int j = 0; j < this.bodiesToLift.Count; j++)
				{
					this.bodiesToLift[j].AddForceAtPosition(lift, this.center.position, ForceMode.Force);
				}
			}
			if (this.preventUpdatesWhenCreatureTurns && this.CheckIfCreatureIsTurning())
			{
				return;
			}
			if (this.onAreaMoves == null && this.onAreaStartDragging == null && this.onAreaDrags == null && this.onAreaPulls == null && this.onAreaStopDragging == null)
			{
				return;
			}
			float rawVelocityMagnitude = this.Velocity.magnitude;
			UnityEvent<float> unityEvent = this.onAreaMoves;
			if (unityEvent != null)
			{
				unityEvent.Invoke(rawVelocityMagnitude);
			}
			if (Mathf.Abs(drag.magnitude) > 0.1f)
			{
				if (!this.dragging)
				{
					UnityEvent<float> unityEvent2 = this.onAreaStartDragging;
					if (unityEvent2 != null)
					{
						unityEvent2.Invoke(rawVelocityMagnitude);
					}
					this.dragging = true;
					return;
				}
				UnityEvent<float> unityEvent3 = this.onAreaDrags;
				if (unityEvent3 == null)
				{
					return;
				}
				unityEvent3.Invoke(rawVelocityMagnitude);
				return;
			}
			else if (this.dragging)
			{
				this.dragging = false;
				UnityEvent<float> unityEvent4 = this.onAreaStopDragging;
				if (unityEvent4 == null)
				{
					return;
				}
				unityEvent4.Invoke(rawVelocityMagnitude);
				return;
			}
			else
			{
				UnityEvent<float> unityEvent5 = this.onAreaPulls;
				if (unityEvent5 == null)
				{
					return;
				}
				unityEvent5.Invoke(rawVelocityMagnitude);
				return;
			}
		}

		/// <summary>
		/// Checks if the player is currently turning with the thumbstick
		/// </summary>
		/// <returns>True if the player is turning, false otherwise</returns>
		// Token: 0x0600202B RID: 8235 RVA: 0x000DA874 File Offset: 0x000D8A74
		private bool CheckIfCreatureIsTurning()
		{
			return this.creature && this.creature.locomotion && (this.creature.locomotion.turnSmoothDirection != 0f || this.creature.locomotion.turnSnapDirection != 0f || this.creature.locomotion.turnSmoothSnapDirection != 0f);
		}

		/// <summary>
		/// Get the velocity of the area using the velocityType.
		/// </summary>
		/// <returns>The velocity of the area, using the velocityType</returns>
		// Token: 0x0600202C RID: 8236 RVA: 0x000DA8F0 File Offset: 0x000D8AF0
		public Vector3 GetVelocity()
		{
			DragArea.VelocityType velocityType = this.velocityType;
			if (velocityType == DragArea.VelocityType.Estimated)
			{
				return this.GetVelocityEstimate();
			}
			if (velocityType != DragArea.VelocityType.FromRigidbody)
			{
				return Vector3.zero;
			}
			if (!this.rbToGetVelocityFrom)
			{
				return Vector3.zero;
			}
			return this.rbToGetVelocityFrom.GetPointVelocity(this.center.position);
		}

		/// <summary>
		/// Estimate and convert the drag as a convenient float that depends on the angle between the area velocity and the area normal.
		/// Eased by the dragAngleEasing curve.
		/// </summary>
		/// <param name="velocity">Velocity of the area in the current fluid.</param>
		/// <returns>The current drag "amount".</returns>
		// Token: 0x0600202D RID: 8237 RVA: 0x000DA944 File Offset: 0x000D8B44
		public float GetDragForce(Vector3 velocity)
		{
			float normalizedDot = Vector3.Dot(velocity.normalized, -this.Normal);
			float easing = this.dragAngleEasing.Evaluate(Mathf.Abs(normalizedDot));
			return Vector3.Dot(velocity, -this.Normal) * easing;
		}

		/// <summary>
		/// Used to quantify the drag or resistance of an object in a fluid environment, such as air or water.
		/// This is a big simplification of real life drag, for gameplay purposes.
		/// </summary>
		/// <param name="velocity">Velocity of the area.</param>
		/// <param name="fluid">Current fluid the area is in</param>
		/// <returns>The estimated drag force in the direction of the area normal.</returns>
		// Token: 0x0600202E RID: 8238 RVA: 0x000DA990 File Offset: 0x000D8B90
		public Vector3 GetDrag(Vector3 velocity, DragArea.Fluid fluid)
		{
			Vector3 velocityInFluid = Mathf.Pow(this.Area, 2f) * fluid.MassDensity * fluid.FlowVelocity(velocity);
			if (!this.twoSided && Vector3.Dot(this.Normal, velocity) < 0f)
			{
				return Vector3.zero;
			}
			float dragForce = this.GetDragForce(velocityInFluid);
			return this.Normal * dragForce * this.dragCoef;
		}

		/// <summary>
		/// Estimate the lift, hugely simplified to be the drag force multiplied by a ratio.
		/// This allows to move creatures and objects in the drag direction (ie. the inverted area velocity)
		/// </summary>
		/// <returns>The lift force.</returns>
		// Token: 0x0600202F RID: 8239 RVA: 0x000DAA03 File Offset: 0x000D8C03
		public Vector3 GetLift()
		{
			return this.Drag * this.dragToLiftRatio;
		}

		/// <summary>
		/// Set the current fluid to air (defined in gamedata)
		/// </summary>
		// Token: 0x06002030 RID: 8240 RVA: 0x000DAA16 File Offset: 0x000D8C16
		public void SetFluidAir()
		{
			this.SetFluid(Catalog.gameData.airFluidData);
		}

		/// <summary>
		/// Set the current fluid to water (defined in gamedata)
		/// </summary>
		// Token: 0x06002031 RID: 8241 RVA: 0x000DAA28 File Offset: 0x000D8C28
		public void SetFluidWater()
		{
			this.SetFluid(Catalog.gameData.waterFluidData);
		}

		/// <summary>
		/// Set the current fluid to the one given.
		/// </summary>
		/// <param name="fluid">Fluid to set.</param>
		// Token: 0x06002032 RID: 8242 RVA: 0x000DAA3A File Offset: 0x000D8C3A
		public void SetFluid(DragArea.Fluid fluid)
		{
			this.currentFluid = fluid;
		}

		/// <summary>
		/// Reset velocity samples and start estimating the velocity of the hand.
		/// </summary>
		// Token: 0x06002033 RID: 8243 RVA: 0x000DAA43 File Offset: 0x000D8C43
		public void BeginEstimatingVelocity()
		{
			this.FinishEstimatingVelocity();
			this.velocitySamples = new Vector3[this.velocityAverageFrames];
			this.routine = base.StartCoroutine(this.EstimateVelocityCoroutine());
		}

		/// <summary>
		/// Stops the velocity estimation routine.
		/// </summary>
		// Token: 0x06002034 RID: 8244 RVA: 0x000DAA6E File Offset: 0x000D8C6E
		public void FinishEstimatingVelocity()
		{
			if (this.routine != null)
			{
				base.StopCoroutine(this.routine);
				this.routine = null;
			}
		}

		/// <summary>
		/// Compute the acceleration estimation from the taken samples
		/// </summary>
		/// <returns>Acceleration estimation</returns>
		// Token: 0x06002035 RID: 8245 RVA: 0x000DAA8C File Offset: 0x000D8C8C
		public Vector3 GetAccelerationEstimate()
		{
			if (this.velocitySamples == null)
			{
				return Vector3.zero;
			}
			Vector3 average = Vector3.zero;
			for (int i = 2 + this.sampleCount - this.velocitySamples.Length; i < this.sampleCount; i++)
			{
				if (i >= 2)
				{
					int first = i - 2;
					int second = i - 1;
					Vector3 v = this.velocitySamples[first % this.velocitySamples.Length];
					Vector3 v2 = this.velocitySamples[second % this.velocitySamples.Length];
					average += v2 - v;
				}
			}
			return average * (1f / Time.deltaTime);
		}

		/// <summary>
		/// Compute the velocity estimation from the taken samples
		/// </summary>
		/// <returns>Velocity estimation</returns>
		// Token: 0x06002036 RID: 8246 RVA: 0x000DAB2C File Offset: 0x000D8D2C
		public Vector3 GetVelocityEstimate()
		{
			if (this.velocitySamples == null)
			{
				return Vector3.zero;
			}
			Vector3 velocity = Vector3.zero;
			int velocitySampleCount = Mathf.Min(this.sampleCount, this.velocitySamples.Length);
			if (velocitySampleCount == 0)
			{
				return velocity;
			}
			for (int i = 0; i < velocitySampleCount; i++)
			{
				velocity += this.velocitySamples[i];
			}
			return velocity * (1f / (float)velocitySampleCount);
		}

		/// <summary>
		/// Routine that samples and estimate linear velocity.
		/// </summary>
		/// <returns></returns>
		// Token: 0x06002037 RID: 8247 RVA: 0x000DAB95 File Offset: 0x000D8D95
		private IEnumerator EstimateVelocityCoroutine()
		{
			this.sampleCount = 0;
			Vector3 pos = this.center.position;
			Vector3 previousPosition = this.velocityOrigin ? (pos - this.velocityOrigin.position) : pos;
			for (;;)
			{
				yield return new WaitForEndOfFrame();
				float velocityFactor = 1f / Time.deltaTime;
				int i = this.sampleCount % this.velocitySamples.Length;
				this.sampleCount++;
				Vector3 position = this.center.position;
				Vector3 originPos = this.velocityOrigin ? this.velocityOrigin.position : Vector3.zero;
				this.velocitySamples[i] = velocityFactor * (position - originPos - previousPosition);
				previousPosition = position - originPos;
			}
			yield break;
		}

		// Token: 0x06002038 RID: 8248 RVA: 0x000DABA4 File Offset: 0x000D8DA4
		private void OnDrawGizmosSelected()
		{
			Matrix4x4 matrix = Gizmos.matrix;
			Transform t = base.transform;
			Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, new Vector3(this.surfaceDimension.x, this.surfaceDimension.y, 0f));
			Gizmos.color = new Color(1f, 0.3f, 0.1f, 0.1f);
			Gizmos.DrawCube(Vector3.zero + Vector3.forward / 100f, Vector3.one);
			Gizmos.color = new Color(1f, 0.3f, 0.1f);
			Gizmos.DrawWireCube(Vector3.zero + Vector3.forward / 100f, Vector3.one);
			Gizmos.matrix = matrix;
			Gizmos.color = new Color(1f, 0.3f, 0.1f);
			DragArea.ArrowGizmo(this.center.position, this.Normal, 0.075f, 0.05f, 20f);
			Gizmos.color = new Color(0.1f, 0.3f, 1f);
			if (this.twoSided)
			{
				DragArea.ArrowGizmo(this.center.position, -this.Normal, 0.075f, 0.05f, 20f);
			}
			Gizmos.color = new Color(1f, 0.3f, 0.1f);
			DragArea.ArrowGizmo(t.position, this.Drag, this.Drag.magnitude, 0.1f, 20f);
			Gizmos.color = new Color(0.1f, 0.3f, 1f);
			DragArea.ArrowGizmo(t.position, this.Lift, this.Lift.magnitude, 0.1f, 20f);
			Gizmos.color = new Color(0.35f, 0.5f, 1f);
			Vector3 position = this.center.position;
			Gizmos.DrawLine(position + this.center.up * this.surfaceDimension.y / 2f, position - this.center.up * this.surfaceDimension.y / 2f);
			float i = 10f;
			int j = 0;
			while ((float)j < i)
			{
				Vector3 a = t.position - base.transform.right * this.surfaceDimension.x / 2f;
				float easing = this.dragAngleEasing.Evaluate((float)j / i);
				float invertedEasing = this.dragAngleEasing.Evaluate(1f - (float)j / i);
				Gizmos.matrix = Matrix4x4.TRS(a + base.transform.right * ((float)j * (this.surfaceDimension.x / 2f / i)), t.rotation, new Vector3(this.surfaceDimension.x / i, this.surfaceDimension.y, 0f));
				Gizmos.color = new Color(1f, 0.3f, 0.1f, easing);
				Gizmos.DrawCube(Vector3.zero + Vector3.forward / 100f, Vector3.one);
				Gizmos.matrix = Matrix4x4.TRS(a + base.transform.right * (this.surfaceDimension.x / 2f + (float)j * (this.surfaceDimension.x / 2f / i)), t.rotation, new Vector3(this.surfaceDimension.x / i, this.surfaceDimension.y, 0f));
				Gizmos.color = new Color(1f, 0.3f, 0.1f, invertedEasing);
				Gizmos.DrawCube(Vector3.zero + Vector3.forward / 100f, Vector3.one);
				j++;
			}
			Gizmos.matrix = matrix;
		}

		// Token: 0x06002039 RID: 8249 RVA: 0x000DAFD0 File Offset: 0x000D91D0
		private static void ArrowGizmo(Vector3 pos, Vector3 normal, float magnitude, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20f)
		{
			if (magnitude <= 0.0001f)
			{
				return;
			}
			if (normal.sqrMagnitude <= 0.0001f)
			{
				return;
			}
			Vector3 direction = normal * magnitude;
			Gizmos.DrawRay(pos, direction);
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}

		// Token: 0x04001F1F RID: 7967
		[Header("Velocity")]
		[Tooltip("Type of velocity to use. Estimated mode does not need any rigidbody")]
		public DragArea.VelocityType velocityType;

		// Token: 0x04001F20 RID: 7968
		[Tooltip("Rigidbody to get the velocity from. Only used when velocityType = 'FromRigidbody'.")]
		public Rigidbody rbToGetVelocityFrom;

		// Token: 0x04001F21 RID: 7969
		[Tooltip("Number of frames used to estimate the velocity. Only used when velocityType = 'Estimated'.")]
		public int velocityAverageFrames = 5;

		// Token: 0x04001F22 RID: 7970
		[Header("Coefficients")]
		[Tooltip("Multiplies the drag by this value")]
		public float dragCoef = 1f;

		// Token: 0x04001F23 RID: 7971
		[Tooltip("Percentage of drag to convert into lift")]
		public float dragToLiftRatio = 0.5f;

		// Token: 0x04001F24 RID: 7972
		[Tooltip("Eases the drag across the normal. When drag is in the same direction than the normal, we sample at 1. when it's perpendicular, we sample at 0.")]
		public AnimationCurve dragAngleEasing = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		// Token: 0x04001F25 RID: 7973
		[Header("Bodies")]
		[Tooltip("Rigidbodies that will be dragged by the area.")]
		public List<PhysicBody> bodiesToDrag;

		// Token: 0x04001F26 RID: 7974
		[Tooltip("Rigidbodies that will be lifted by the area.")]
		public List<PhysicBody> bodiesToLift;

		// Token: 0x04001F27 RID: 7975
		[Tooltip("Will drag the creature grabbing this item (locomotion)")]
		public bool dragGrabbingCreatureBody;

		// Token: 0x04001F28 RID: 7976
		[Tooltip("Will lift the creature grabbing this item (locomotion)")]
		public bool liftGrabbingCreatureBody;

		// Token: 0x04001F29 RID: 7977
		[Tooltip("Will use the creature locomotion position as the velocity origin")]
		public bool useCreatureLocomotionAsVelocityOrigin;

		// Token: 0x04001F2A RID: 7978
		[Header("Surface")]
		[Tooltip("Area surface as a plane")]
		public Vector2 surfaceDimension;

		// Token: 0x04001F2B RID: 7979
		[Tooltip("Point to add the drag forces at")]
		public Transform center;

		// Token: 0x04001F2C RID: 7980
		[Tooltip("Origin used to estimate the velocity. Only used when velocityType = 'Estimated'")]
		public Transform velocityOrigin;

		// Token: 0x04001F2D RID: 7981
		[Tooltip("Is the area two sided? If not, drag will only apply in one way")]
		public bool twoSided = true;

		// Token: 0x04001F2E RID: 7982
		[Header("Item")]
		[Tooltip("Is the area on an Item? If yes, then item callbacks will be used (grab, un-grab, etc.)")]
		public bool listenToItemCallbacks = true;

		// Token: 0x04001F2F RID: 7983
		[Header("Misc")]
		[Tooltip("Prevent drag & lift to be computed when the creature turns (causing fast and abrupt changes of position)")]
		public bool preventUpdatesWhenCreatureTurns;

		// Token: 0x04001F30 RID: 7984
		[Tooltip("Check if the area enters and exit from the water")]
		public bool checkForWater = true;

		// Token: 0x04001F31 RID: 7985
		[NonSerialized]
		public DragArea.Fluid currentFluid;

		// Token: 0x04001F32 RID: 7986
		public UnityEvent onStart;

		// Token: 0x04001F33 RID: 7987
		public UnityEvent onStop;

		// Token: 0x04001F34 RID: 7988
		[Header("This event is fired (every physic frames) when the area moves in any direction")]
		public UnityEvent<float> onAreaMoves;

		// Token: 0x04001F35 RID: 7989
		[Header("This event is fired (only once) when we first detect a motion that causes drag")]
		public UnityEvent<float> onAreaStartDragging;

		// Token: 0x04001F36 RID: 7990
		[Header("This event is fired (every physic frames) when we detect a motion that causes drag")]
		public UnityEvent<float> onAreaDrags;

		// Token: 0x04001F37 RID: 7991
		[Header("This event is fired (every physic frames) when we detect a motion that is not causing drag")]
		public UnityEvent<float> onAreaPulls;

		// Token: 0x04001F38 RID: 7992
		[Header("This event is fired (only once) when we stop detecting a motion that causes drag")]
		public UnityEvent<float> onAreaStopDragging;

		// Token: 0x04001F39 RID: 7993
		[Header("Item related events")]
		public UnityEvent onItemSnaps;

		// Token: 0x04001F3A RID: 7994
		public UnityEvent<Handle, RagdollHand> onItemIsGrabbed;

		// Token: 0x04001F3B RID: 7995
		public UnityEvent<Handle, RagdollHand> onItemIsUnGrabbed;

		// Token: 0x04001F3C RID: 7996
		private bool dragging;

		// Token: 0x04001F3D RID: 7997
		private Coroutine routine;

		// Token: 0x04001F3E RID: 7998
		private int sampleCount;

		// Token: 0x04001F3F RID: 7999
		private Vector3[] velocitySamples;

		// Token: 0x04001F40 RID: 8000
		private Item item;

		// Token: 0x04001F41 RID: 8001
		private Creature creature;

		// Token: 0x04001F42 RID: 8002
		private WaterHandler waterHandler;

		// Token: 0x04001F43 RID: 8003
		private Transform defaultVelocityOrigin;

		/// <summary>
		/// Struct used to define fluids (air, water, etc.).
		/// The higher the density, the higher the drag.
		/// Flow is made to simulate world space "current".
		/// </summary>
		// Token: 0x02000947 RID: 2375
		[Serializable]
		public struct Fluid
		{
			/// <summary>
			/// Over simplification of the real world mass densities.
			/// Simplified to return the density only.
			/// </summary>
			// Token: 0x1700056C RID: 1388
			// (get) Token: 0x06004309 RID: 17161 RVA: 0x0018E65E File Offset: 0x0018C85E
			public float MassDensity
			{
				get
				{
					return this.density;
				}
			}

			// Token: 0x0600430A RID: 17162 RVA: 0x0018E666 File Offset: 0x0018C866
			public Fluid(float density, Vector3 flow)
			{
				this.density = density;
				this.flow = flow;
			}

			// Token: 0x0600430B RID: 17163 RVA: 0x0018E676 File Offset: 0x0018C876
			public Vector3 FlowVelocity(Vector3 velocity)
			{
				return velocity + this.flow;
			}

			// Token: 0x0400443E RID: 17470
			public float density;

			// Token: 0x0400443F RID: 17471
			public Vector3 flow;
		}

		// Token: 0x02000948 RID: 2376
		public enum VelocityType
		{
			// Token: 0x04004441 RID: 17473
			Estimated,
			// Token: 0x04004442 RID: 17474
			FromRigidbody
		}
	}
}
