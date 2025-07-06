using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using ThunderRoad.AI;
using UnityEngine;
using UnityEngine.AI;

namespace ThunderRoad
{
	// Token: 0x02000162 RID: 354
	public class BrainModuleMove : BrainData.Module
	{
		// Token: 0x17000117 RID: 279
		// (get) Token: 0x06001130 RID: 4400 RVA: 0x0007A44A File Offset: 0x0007864A
		// (set) Token: 0x06001131 RID: 4401 RVA: 0x0007A452 File Offset: 0x00078652
		public float turnOffsetDegrees { get; protected set; }

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x06001132 RID: 4402 RVA: 0x0007A45B File Offset: 0x0007865B
		private Vector3[] currentCorners
		{
			get
			{
				NavMeshAgent navMeshAgent = this.navMeshAgent;
				Vector3[] array;
				if (navMeshAgent == null)
				{
					array = null;
				}
				else
				{
					NavMeshPath path = navMeshAgent.path;
					array = ((path != null) ? path.corners : null);
				}
				return array ?? new Vector3[0];
			}
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06001133 RID: 4403 RVA: 0x0007A485 File Offset: 0x00078685
		// (set) Token: 0x06001134 RID: 4404 RVA: 0x0007A48D File Offset: 0x0007868D
		public List<Vector3> path { get; protected set; }

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06001135 RID: 4405 RVA: 0x0007A496 File Offset: 0x00078696
		// (set) Token: 0x06001136 RID: 4406 RVA: 0x0007A49E File Offset: 0x0007869E
		[JsonIgnore]
		public bool returningToNavmesh { get; protected set; }

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x06001137 RID: 4407 RVA: 0x0007A4A7 File Offset: 0x000786A7
		// (set) Token: 0x06001138 RID: 4408 RVA: 0x0007A4AF File Offset: 0x000786AF
		public List<Item> interactingDoors { get; protected set; } = new List<Item>();

		// Token: 0x06001139 RID: 4409 RVA: 0x0007A4B8 File Offset: 0x000786B8
		public override void Load(Creature creature)
		{
			base.Load(creature);
			this.navMeshAgent = creature.brain.navMeshAgent;
			this.navMeshAgent.updatePosition = true;
			this.navMeshAgent.areaMask = (int)this.areaMask;
			if (this.locomotionOverride)
			{
				creature.locomotion.SetSpeedModifier(this, this.locomotionForwardSpeedMultiplier, this.locomotionBackwardSpeedMultiplier, this.locomotionStrafeSpeedMultiplier, this.locomotionRunSpeedMultiplier, 1f, 1f);
			}
			creature.ragdoll.OnGrabEvent += this.RagdollGrabbed;
			Catalog.LoadAssetAsync<AnimationClip>(this.doorReachClip, delegate(AnimationClip clip)
			{
				this.reachClip = clip;
			}, "BrainModuleMove");
			if (this.path == null)
			{
				this.path = new List<Vector3>();
			}
		}

		// Token: 0x0600113A RID: 4410 RVA: 0x0007A578 File Offset: 0x00078778
		public override void Unload()
		{
			base.Unload();
			base.creature.ragdoll.OnGrabEvent -= this.RagdollGrabbed;
		}

		// Token: 0x0600113B RID: 4411 RVA: 0x0007A59C File Offset: 0x0007879C
		private void RagdollGrabbed(RagdollHand ragdollHand, HandleRagdoll handleRagdoll)
		{
			this.StopMove();
		}

		// Token: 0x0600113C RID: 4412 RVA: 0x0007A5A4 File Offset: 0x000787A4
		public IEnumerator PlaceTempObstacle(Vector3 position, float duration)
		{
			this.lastPlaceTime = Time.time;
			NavMeshObstacle newObstacle = new GameObject("TemporaryObstacle").AddComponent<NavMeshObstacle>();
			newObstacle.transform.position = position;
			CapsuleCollider locomotionCapsule = base.creature.currentLocomotion.capsuleCollider;
			newObstacle.shape = NavMeshObstacleShape.Capsule;
			newObstacle.center = locomotionCapsule.center;
			newObstacle.radius = locomotionCapsule.radius;
			newObstacle.height = locomotionCapsule.height;
			yield return Yielders.ForSeconds(duration);
			UnityEngine.Object.Destroy(newObstacle.gameObject);
			yield break;
		}

		// Token: 0x0600113D RID: 4413 RVA: 0x0007A5C4 File Offset: 0x000787C4
		public override void Update()
		{
			Creature thisCreature = base.creature;
			if (thisCreature.isPlayer)
			{
				return;
			}
			thisCreature.brain.onNavmesh = false;
			if (thisCreature.state != Creature.State.Alive || thisCreature.ragdoll.standingUp || thisCreature.brain.isIncapacitated || thisCreature.brain.isUnconscious)
			{
				this.StopMove();
				return;
			}
			Vector3 creaturePos = thisCreature.transform.position;
			Vector3 nextPosition = this.navMeshAgent.nextPosition;
			thisCreature.brain.onNavmesh = (thisCreature.locomotion.isGrounded && Mathf.Abs(thisCreature.transform.InverseTransformPoint(nextPosition).y) < thisCreature.morphology.height * 0.5f && (nextPosition.ToXZ().PointInRadius(creaturePos.ToXZ(), 0.001f) || this.navMeshAgent.isOnOffMeshLink));
			Vector3 movement = creaturePos.ToXZ() - this.lastPosition.ToXZ();
			this.ProcessNavMesh(creaturePos, movement);
			this.PreventTooClose(creaturePos);
			this.ProcessTurnRequests(creaturePos);
			this.ProcessMove(creaturePos);
		}

		// Token: 0x0600113E RID: 4414 RVA: 0x0007A6DC File Offset: 0x000788DC
		private void ProcessNavMesh(Vector3 creaturePos, Vector3 movement)
		{
			if (!base.creature.brain.onNavmesh)
			{
				if (this.moveType != BrainModuleMove.MoveType.InDirection)
				{
					this.returningToNavmesh = false;
				}
				if (!this.returningToNavmesh && base.creature.locomotion.isGrounded && this.moveType == BrainModuleMove.MoveType.ToPoint)
				{
					NavMeshHit hit;
					NavMesh.SamplePosition(creaturePos, out hit, base.creature.morphology.height * 0.75f, -1);
					this.ReturnToNavMesh(hit.position);
					return;
				}
			}
			else
			{
				if (this.navMeshAgent.isOnOffMeshLink)
				{
					OffMeshLink offMeshLink = this.navMeshAgent.currentOffMeshLinkData.offMeshLink;
					HingeDrive doorDrive = (offMeshLink != null) ? offMeshLink.GetComponentInParent<HingeDrive>() : null;
					if (offMeshLink != null && offMeshLink.area == 3 && doorDrive && !this.reach)
					{
						this.reachHandle = creaturePos.GetClosestObject(doorDrive.handles);
						if (this.reachHandle.item)
						{
							this.interactingDoors.Add(this.reachHandle.item);
						}
						base.creature.ragdoll.forcePhysic.Add(this);
						base.creature.ragdoll.SetState(Ragdoll.State.Standing, true);
						this.reachSide = ((base.creature.transform.InverseTransformPoint(this.reachHandle.transform.position).x > 0f) ? Side.Left : Side.Right);
						base.creature.PlayUpperAnimation(this.reachClip, false, 1f, null, this.reachSide == Side.Left, true);
						this.stuckDuration = 0f;
						this.reach = true;
					}
				}
				else
				{
					if (this.reach)
					{
						this.reach = false;
					}
					if (this.interactingDoors.Count > 0)
					{
						this.interactingDoors.Clear();
					}
					base.creature.ragdoll.forcePhysic.Remove(this);
				}
				if (this.returningToNavmesh)
				{
					this.StopMove();
					if (this.previousNavTarget != null)
					{
						this.navMeshAgent.SetDestination(this.previousNavTarget.Value);
						this.useLegs = true;
						this.lastMoveTime = Time.time;
						this.navMeshAgent.isStopped = false;
						this.navMeshAgent.speed = this.moveSpeedRatio * base.creature.currentLocomotion.forwardSpeed;
						this.navPosition = this.previousNavTarget.Value;
						this.moveType = BrainModuleMove.MoveType.ToPoint;
					}
					this.returningToNavmesh = false;
				}
				if (this.moveType != BrainModuleMove.MoveType.None && movement.PointInRadius(Vector3.zero, this.stuckRadius))
				{
					this.stuckDuration += Time.fixedDeltaTime;
					if (this.stuckDuration > this.stuckMaxDuration && Time.time - this.lastMoveTime > 3f * this.stuckMaxDuration)
					{
						this.StopMove();
						return;
					}
				}
				else
				{
					this.lastPosition = creaturePos;
					this.stuckDuration = 0f;
					this.StopTurn(this);
					this.LeaveExclusiveStack(this);
				}
			}
		}

		// Token: 0x0600113F RID: 4415 RVA: 0x0007A9E0 File Offset: 0x00078BE0
		private void PreventTooClose(Vector3 creaturePos)
		{
			if (this.preventTooClose && (base.creature.brain.state == Brain.State.Combat || base.creature.brain.state == Brain.State.Investigate) && !this.returningToNavmesh)
			{
				Vector3 tooCloseAverage = Vector3.zero;
				int tooCloseCount = 0;
				int allActiveCount = Creature.allActive.Count;
				float tooCloseDistanceSqr = this.tooCloseDistance * this.tooCloseDistance;
				for (int i = 0; i < allActiveCount; i++)
				{
					Creature otherCreature = Creature.allActive[i];
					if (otherCreature.state == Creature.State.Alive && !(otherCreature == base.creature))
					{
						Vector3 diff = otherCreature.transform.position;
						diff.x -= creaturePos.x;
						diff.y = 0f;
						diff.z -= creaturePos.z;
						if (diff.sqrMagnitude <= tooCloseDistanceSqr)
						{
							tooCloseCount++;
							tooCloseAverage -= diff.normalized;
						}
					}
				}
				if (tooCloseCount > 0)
				{
					if (Mathf.Approximately(tooCloseAverage.sqrMagnitude, 0f))
					{
						tooCloseAverage -= base.creature.transform.forward.ToXZ().normalized;
					}
					tooCloseAverage /= (float)tooCloseCount;
					if (this.moveType != BrainModuleMove.MoveType.InDirection)
					{
						this.StopMove();
					}
					tooCloseAverage = base.creature.transform.InverseTransformDirection(tooCloseAverage);
					this.UpdateMoveCycleDirection(tooCloseAverage, 1f, 0.65f);
				}
			}
		}

		// Token: 0x06001140 RID: 4416 RVA: 0x0007AB5C File Offset: 0x00078D5C
		private void ProcessMove(Vector3 creaturePos)
		{
			if (this.path == null)
			{
				this.path = new List<Vector3>();
			}
			if (this.moveType != BrainModuleMove.MoveType.None)
			{
				float currentRunSpeedRatio = 0f;
				if (this.moveType != BrainModuleMove.MoveType.ToPoint || !this.navMeshAgent.enabled || this.navMeshAgent.pathPending)
				{
					this.path.Clear();
				}
				if (this.moveType == BrainModuleMove.MoveType.ToPoint && this.navMeshAgent.enabled && !this.navMeshAgent.pathPending && base.creature.brain.onNavmesh)
				{
					bool updateEdgeAvoidance = false;
					int corners = this.navMeshAgent.path.GetCornersNonAlloc(this.navMeshCornerBuffer);
					if (this.path.Count > 0)
					{
						for (int i = 1; i < corners - 1; i++)
						{
							if (i > 1)
							{
								int index = corners - i;
								if (!this.path.Contains(this.navMeshCornerBuffer[index]))
								{
									this.path.Clear();
									break;
								}
							}
						}
					}
					if (this.path.Count == 0)
					{
						this.path = new List<Vector3>();
						for (int j = 0; j < corners; j++)
						{
							this.path.Add(this.navMeshCornerBuffer[j]);
						}
						updateEdgeAvoidance = true;
					}
					else if (this.path.Count > 1 && Utils.InverseLerpVector3(this.path[0], this.path[1], Utils.ClosestPointOnLine(this.path[0], this.path[1], creaturePos)) >= 0.999f)
					{
						this.path.RemoveAt(0);
						updateEdgeAvoidance = true;
					}
					if (this.path.Count < 2)
					{
						this.path.Clear();
					}
					if (updateEdgeAvoidance)
					{
						List<Vector3> path = this.path;
						if (path != null && path.Count == 2)
						{
							updateEdgeAvoidance = false;
						}
					}
					if (this.path.Count > 0 && updateEdgeAvoidance)
					{
						Vector3 cornerForward = (this.path[1] - this.path[0]).normalized;
						if (this.path.Count > 2)
						{
							cornerForward = (cornerForward + (this.path[2] - this.path[1]).normalized) / 2f;
						}
						Vector3 pathRight = Vector3.Cross(Vector3.up, cornerForward).normalized;
						NavMeshHit rightHit;
						bool rightHitEdge = NavMesh.Raycast(this.path[1] + pathRight * (this.edgeAvoidanceDistance * 0.05f), this.path[1] + pathRight * (this.edgeAvoidanceDistance * 0.95f), out rightHit, (int)this.areaMask);
						NavMeshHit leftHit;
						bool leftHitEdge = NavMesh.Raycast(this.path[1] + pathRight * (-this.edgeAvoidanceDistance * 0.05f), this.path[1] + pathRight * (-this.edgeAvoidanceDistance * 0.95f), out leftHit, (int)this.areaMask);
						if (leftHitEdge || rightHitEdge)
						{
							if (leftHitEdge && rightHitEdge)
							{
								this.path[1] = (leftHit.position + rightHit.position) / 2f;
							}
							else
							{
								this.path[1] = this.path[1] + pathRight * (leftHitEdge ? this.edgeAvoidanceDistance : (-this.edgeAvoidanceDistance));
							}
						}
					}
					float remainingDistance = this.navMeshAgent.remainingDistance;
					currentRunSpeedRatio = ((remainingDistance > this.runDistance) ? ((base.creature.brain.slowZones.Count == 0) ? this.runSpeedRatio : base.creature.brain.slowZones[0].runSpeed) : 0f);
					this.navMeshAgent.speed = base.creature.locomotion.forwardSpeed * this.moveSpeedRatio;
					this.moveDirection = ((this.path.Count == 0 || this.path.Count < 2) ? Vector3.zero : (this.path[1].ToXZ() - creaturePos.ToXZ()));
					float frameMoveDistance = base.creature.locomotion.horizontalSpeed * Time.fixedDeltaTime;
					if ((this.path.Count > 0 && this.path.Count == 2 && this.moveDirection.sqrMagnitude <= frameMoveDistance * frameMoveDistance) || creaturePos.ToXZ().PointInRadius(this.navPosition.ToXZ(), this.navReachDistance) || (this.navMeshAgent.hasPath && remainingDistance <= frameMoveDistance))
					{
						this.moveDirection = Vector3.zero;
						this.path.Clear();
					}
				}
				if (this.moveType == BrainModuleMove.MoveType.InDirection && Time.time - this.lastMoveTime > this.directionHold)
				{
					this.StopMove();
				}
				if (this.moveType != BrainModuleMove.MoveType.None)
				{
					if (base.creature.locomotion.groundAngle > 25f)
					{
						this.TurnTo(this.moveDirection.normalized, this, 1f);
						this.TopExclusiveStack(this);
					}
					else
					{
						this.LeaveExclusiveStack(this);
						this.StopTurn(this);
					}
				}
				base.creature.locomotion.MoveWeighted(this.moveDirection.normalized, base.creature.transform, base.creature.GetAnimatorHeightRatio(), this.moveSpeedRatio, currentRunSpeedRatio, this.useAcceleration ? this.acceleration : 0f);
				if (this.navMeshAgent.nextPosition.y < base.creature.transform.position.y)
				{
					base.creature.transform.position = new Vector3(base.creature.transform.position.x, this.navMeshAgent.nextPosition.y, base.creature.transform.position.z);
				}
			}
		}

		// Token: 0x06001141 RID: 4417 RVA: 0x0007B17C File Offset: 0x0007937C
		private void ProcessTurnRequests(Vector3 creaturePos)
		{
			int turnRequestsCount = this.turnRequests.Count;
			for (int i = 0; i < turnRequestsCount; i++)
			{
				BrainModuleMove.TurnRequest turnRequest = this.turnRequests[i];
				if ((!this.returningToNavmesh || turnRequest.handler == this) && (this.exclusiveHandlerStack.IsNullOrEmpty() || turnRequest.handler == this.exclusiveHandlerStack[0]))
				{
					if (turnRequest.turnMode != BrainModuleMove.TurnMode.None)
					{
						Vector3 targetDirection = turnRequest.turnDirection;
						if (turnRequest.turnMode == BrainModuleMove.TurnMode.MoveDirection)
						{
							targetDirection = this.moveDirection.normalized;
						}
						else if (turnRequest.turnMode == BrainModuleMove.TurnMode.Transform)
						{
							if (turnRequest.turnTarget == null)
							{
								Debug.LogWarning("Trying to turn to a null transform, ignoring turn request. Request from " + turnRequest.handler.ToString());
								goto IL_EF;
							}
							Vector3 turnTargetPosition = turnRequest.turnTarget.position;
							targetDirection = new Vector3(turnTargetPosition.x, creaturePos.y, turnTargetPosition.z) - creaturePos;
						}
						this.UpdateTurn(targetDirection, turnRequest.turnSpeedRatio);
						return;
					}
					break;
				}
				IL_EF:;
			}
		}

		// Token: 0x06001142 RID: 4418 RVA: 0x0007B284 File Offset: 0x00079484
		public virtual void UpdateTurnNavDirection(float turnSpeedRatio)
		{
			this.UpdateTurn(this.navMeshAgent.desiredVelocity.normalized, turnSpeedRatio);
		}

		// Token: 0x06001143 RID: 4419 RVA: 0x0007B2AB File Offset: 0x000794AB
		public virtual void TopExclusiveStack(object handler)
		{
			this.LeaveExclusiveStack(handler);
			this.exclusiveHandlerStack.Insert(0, handler);
		}

		// Token: 0x06001144 RID: 4420 RVA: 0x0007B2C1 File Offset: 0x000794C1
		public virtual void LeaveExclusiveStack(object handler)
		{
			if (this.exclusiveHandlerStack.Contains(handler))
			{
				this.exclusiveHandlerStack.Remove(handler);
			}
		}

		// Token: 0x06001145 RID: 4421 RVA: 0x0007B2DE File Offset: 0x000794DE
		public virtual void SetTurnOffset(object setter, float value)
		{
			List<object> list = this.exclusiveHandlerStack;
			if ((list == null || list.Count != 0) && setter != this.exclusiveHandlerStack[0])
			{
				return;
			}
			this.turnOffsetDegrees = value;
		}

		// Token: 0x06001146 RID: 4422 RVA: 0x0007B310 File Offset: 0x00079510
		public virtual void UpdateTurn(Transform turnTarget, float turnSpeedRatio)
		{
			Vector3 creaturePos = base.creature.transform.position;
			Vector3 targetDirection = new Vector3(turnTarget.position.x, creaturePos.y, turnTarget.position.z) - creaturePos;
			this.UpdateTurn(targetDirection, turnSpeedRatio);
		}

		// Token: 0x06001147 RID: 4423 RVA: 0x0007B360 File Offset: 0x00079560
		public virtual void UpdateTurn(Vector3 targetDirection, float turnSpeedRatio)
		{
			float angle = this.GetTargetAngle(targetDirection) * base.creature.locomotion.turnSpeed * this.baseTurnSpeedMultiplier * turnSpeedRatio * Time.fixedDeltaTime;
			base.creature.transform.Rotate(base.creature.transform.up, angle);
			int creatureRegions = base.creature.ragdoll.ragdollRegions.Count;
			if (creatureRegions > 1)
			{
				for (int i = 1; i < creatureRegions; i++)
				{
					foreach (RagdollPart ragdollPart in base.creature.ragdoll.ragdollRegions[i].parts)
					{
						ragdollPart.transform.RotateAround(base.creature.transform.position, base.creature.transform.up, -angle);
					}
				}
			}
		}

		// Token: 0x06001148 RID: 4424 RVA: 0x0007B45C File Offset: 0x0007965C
		public virtual float GetTargetAngle(Vector3 targetDirection)
		{
			return Vector3.SignedAngle(base.creature.transform.forward, Quaternion.Euler(0f, this.turnOffsetDegrees, 0f) * targetDirection, base.creature.transform.up);
		}

		// Token: 0x06001149 RID: 4425 RVA: 0x0007B4AC File Offset: 0x000796AC
		public Vector3 GetRandomPointAround(Vector3 position, Vector3 direction, float angle, float minRadius, float maxRadius)
		{
			float randomRadius = UnityEngine.Random.Range(minRadius, maxRadius);
			Vector3 horizontalDirection = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
			return position + Quaternion.AngleAxis(angle, Vector3.up) * horizontalDirection * randomRadius;
		}

		// Token: 0x0600114A RID: 4426 RVA: 0x0007B4F4 File Offset: 0x000796F4
		public bool TrySampleCirclePosition(Vector3 position, float circleMinRadius, float circleMaxRadius, float circleAngle, bool unobstructedPathFirst, out Vector3 navPosition)
		{
			if (unobstructedPathFirst)
			{
				for (int i = 0; i < this.samplePositionCircleSteps; i++)
				{
					float angle = circleAngle + (float)(i * (360 / this.samplePositionCircleSteps));
					NavMeshHit navHit;
					NavMeshHit navRaycastHit;
					if (NavMesh.SamplePosition(this.GetRandomPointAround(position, base.creature.transform.position - position, angle, circleMinRadius, circleMaxRadius), out navHit, 1f, -1) && !NavMesh.Raycast(navHit.position, position, out navRaycastHit, -1))
					{
						navPosition = navHit.position;
						return true;
					}
				}
			}
			NavMeshHit navHit2;
			if (NavMesh.SamplePosition(this.GetRandomPointAround(position, base.creature.transform.position - position, circleAngle, circleMinRadius, circleMaxRadius), out navHit2, 1f, -1))
			{
				navPosition = navHit2.position;
				return true;
			}
			navPosition = Vector3.zero;
			return false;
		}

		// Token: 0x0600114B RID: 4427 RVA: 0x0007B5C8 File Offset: 0x000797C8
		public State UpdateMoveCycle(Vector3 targetPosition, float targetMinRadius, float targetMaxRadius, float moveSpeedRatio = 1f, float runSpeedRatio = 1f, float targetRadiusAngle = 0f, bool unobstructedPathFirst = false, float repathMinDelay = 0f, float repathMaxDelay = 0f)
		{
			if (!this.allowMove)
			{
				return State.FAILURE;
			}
			if (this.returningToNavmesh)
			{
				return State.RUNNING;
			}
			if (this.moveType == BrainModuleMove.MoveType.ToPoint)
			{
				if (this.navMeshAgent.pathPending)
				{
					return State.RUNNING;
				}
				float targetToNavDistance = Vector3.Distance(targetPosition.ToXZ(), this.navMeshAgent.destination.ToXZ());
				if (targetToNavDistance < targetMinRadius || targetToNavDistance >= targetMaxRadius || Mathf.Abs(targetPosition.y - this.navMeshAgent.destination.y) >= 1f)
				{
					this.StopMove();
				}
				else
				{
					if (this.navMeshAgent.remainingDistance > this.navReachDistance * base.creature.transform.lossyScale.y)
					{
						base.creature.StopAnimation(false);
						return State.RUNNING;
					}
					this.StopMove();
					this.previousNavTarget = null;
					return State.SUCCESS;
				}
			}
			if (base.creature.brain.isAttacking)
			{
				return State.FAILURE;
			}
			if (repathMinDelay > 0f && repathMaxDelay > repathMinDelay && Time.time - this.lastMoveTime < this.nextMoveDelay)
			{
				float creatureToTargetDistance = base.creature.brain.GetHorizontalDistance(targetPosition);
				if (creatureToTargetDistance >= targetMinRadius && creatureToTargetDistance < targetMaxRadius && Mathf.Abs(base.creature.transform.position.y - targetPosition.y) < 1f)
				{
					base.creature.StopAnimation(false);
					return State.RUNNING;
				}
			}
			if (!this.TrySampleCirclePosition(targetPosition, targetMinRadius, targetMaxRadius, targetRadiusAngle, unobstructedPathFirst, out this.navPosition))
			{
				return State.FAILURE;
			}
			if (!this.navMeshAgent.Warp(base.creature.transform.position))
			{
				return State.FAILURE;
			}
			if (!this.navMeshAgent.SetDestination(this.navPosition))
			{
				return State.FAILURE;
			}
			if (this.moveType == BrainModuleMove.MoveType.None)
			{
				this.useLegs = true;
				this.lastMoveTime = Time.time;
				this.nextMoveDelay = UnityEngine.Random.Range(repathMinDelay, repathMaxDelay);
				this.navMeshAgent.isStopped = false;
				this.navMeshAgent.speed = moveSpeedRatio * base.creature.currentLocomotion.forwardSpeed;
				this.moveSpeedRatio = moveSpeedRatio;
				this.runSpeedRatio = runSpeedRatio;
				this.previousNavTarget = new Vector3?(this.navPosition);
				this.moveType = BrainModuleMove.MoveType.ToPoint;
				base.creature.StopAnimation(false);
				return State.RUNNING;
			}
			return State.FAILURE;
		}

		// Token: 0x0600114C RID: 4428 RVA: 0x0007B800 File Offset: 0x00079A00
		public State UpdateMoveCycleDirection(Vector3 direction, float speedRatio = 1f, float minHoldTime = 0.5f)
		{
			if (!this.allowMove)
			{
				return State.FAILURE;
			}
			if (this.moveType == BrainModuleMove.MoveType.InDirection)
			{
				this.moveDirection = (base.creature.transform.right * direction.x + base.creature.transform.up * direction.y + base.creature.transform.forward * direction.z).normalized * (base.creature.locomotion.forwardSpeed * speedRatio);
				this.lastMoveTime = Time.time;
				this.directionHold = minHoldTime;
				return State.RUNNING;
			}
			if (base.creature.brain.isAttacking)
			{
				return State.FAILURE;
			}
			if (this.moveType == BrainModuleMove.MoveType.None)
			{
				this.useLegs = true;
				this.lastMoveTime = Time.time;
				this.moveDirection = (base.creature.transform.right * direction.x + base.creature.transform.up * direction.y + base.creature.transform.forward * direction.z).normalized * (base.creature.locomotion.forwardSpeed * speedRatio);
				this.directionHold = minHoldTime;
				this.moveType = BrainModuleMove.MoveType.InDirection;
				base.creature.StopAnimation(false);
				return State.RUNNING;
			}
			return State.FAILURE;
		}

		// Token: 0x0600114D RID: 4429 RVA: 0x0007B988 File Offset: 0x00079B88
		public void StopMove()
		{
			this.StopTurn(this);
			this.LeaveExclusiveStack(this);
			this.returningToNavmesh = false;
			this.useLegs = false;
			base.creature.locomotion.MoveStop();
			this.navMeshAgent.Warp(base.creature.transform.position);
			if (this.navMeshAgent.enabled && this.navMeshAgent.isOnNavMesh)
			{
				this.navMeshAgent.isStopped = true;
			}
			this.navMeshAgent.speed = 0f;
			this.navPosition = Vector3.zero;
			this.moveDirection = Vector3.zero;
			this.moveType = BrainModuleMove.MoveType.None;
		}

		// Token: 0x0600114E RID: 4430 RVA: 0x0007BA30 File Offset: 0x00079C30
		public void ReturnToNavMesh(Vector3 navMeshPosition)
		{
			Vector3 backDirection = base.creature.transform.InverseTransformPoint(navMeshPosition).ToXZ();
			if (backDirection.sqrMagnitude < 1.0000001E-06f && Mathf.Abs(navMeshPosition.y - base.creature.transform.position.y) < 0.01f)
			{
				Debug.LogWarning(base.creature.name + " is too close to the closest NavMesh position to move to it! It is shifting instead.");
				base.creature.transform.position = navMeshPosition;
				base.creature.brain.onNavmesh = base.creature.currentLocomotion.isGrounded;
				return;
			}
			this.StopMove();
			this.UpdateMoveCycleDirection(backDirection.normalized, 1f, 0.5f);
			this.TurnTo(navMeshPosition, this, 1f);
			this.TopExclusiveStack(this);
			this.returningToNavmesh = true;
		}

		// Token: 0x0600114F RID: 4431 RVA: 0x0007BB10 File Offset: 0x00079D10
		public bool TryGetTurnRequest(object handler, out BrainModuleMove.TurnRequest turnRequest)
		{
			int turnRequestsCount = this.turnRequests.Count;
			for (int i = 0; i < turnRequestsCount; i++)
			{
				BrainModuleMove.TurnRequest tr = this.turnRequests[i];
				if (tr.handler == handler)
				{
					turnRequest = tr;
					return true;
				}
			}
			turnRequest = null;
			return false;
		}

		// Token: 0x06001150 RID: 4432 RVA: 0x0007BB54 File Offset: 0x00079D54
		public void TurnToNavDirection(object handler, float turnSpeedRatio = 1f)
		{
			BrainModuleMove.TurnRequest turnRequest;
			if (this.TryGetTurnRequest(handler, out turnRequest))
			{
				turnRequest.turnMode = BrainModuleMove.TurnMode.MoveDirection;
				turnRequest.turnSpeedRatio = turnSpeedRatio;
				return;
			}
			this.turnRequests.Add(new BrainModuleMove.TurnRequest(handler, turnSpeedRatio));
		}

		// Token: 0x06001151 RID: 4433 RVA: 0x0007BB90 File Offset: 0x00079D90
		public void TurnTo(Vector3 direction, object handler, float turnSpeedRatio = 1f)
		{
			if (direction == Vector3.zero)
			{
				return;
			}
			BrainModuleMove.TurnRequest turnRequest;
			if (this.TryGetTurnRequest(handler, out turnRequest))
			{
				turnRequest.turnMode = BrainModuleMove.TurnMode.Direction;
				turnRequest.turnDirection = direction;
				turnRequest.turnSpeedRatio = turnSpeedRatio;
				return;
			}
			this.turnRequests.Add(new BrainModuleMove.TurnRequest(handler, direction, turnSpeedRatio));
		}

		// Token: 0x06001152 RID: 4434 RVA: 0x0007BBE0 File Offset: 0x00079DE0
		public void TurnTo(Transform transform, object handler, float turnSpeedRatio = 1f)
		{
			if (transform == null)
			{
				return;
			}
			BrainModuleMove.TurnRequest turnRequest;
			if (this.TryGetTurnRequest(handler, out turnRequest))
			{
				turnRequest.turnMode = BrainModuleMove.TurnMode.Transform;
				turnRequest.turnTarget = transform;
				turnRequest.turnSpeedRatio = turnSpeedRatio;
				return;
			}
			this.turnRequests.Add(new BrainModuleMove.TurnRequest(handler, transform, turnSpeedRatio));
		}

		// Token: 0x06001153 RID: 4435 RVA: 0x0007BC2C File Offset: 0x00079E2C
		public void StopTurn(object handler)
		{
			BrainModuleMove.TurnRequest turnRequest;
			if (this.TryGetTurnRequest(handler, out turnRequest))
			{
				this.turnRequests.Remove(turnRequest);
			}
		}

		// Token: 0x06001154 RID: 4436 RVA: 0x0007BC54 File Offset: 0x00079E54
		public override void OnDrawGizmos()
		{
			if (this.moveType == BrainModuleMove.MoveType.ToPoint)
			{
				int cornerCount = base.creature.brain.navMeshAgent.path.corners.Length;
				for (int i = 0; i < cornerCount - 1; i++)
				{
					Debug.DrawLine(base.creature.brain.navMeshAgent.path.corners[i], base.creature.brain.navMeshAgent.path.corners[i + 1], Color.blue);
				}
				Debug.DrawLine(base.creature.transform.position + new Vector3(0f, 0.05f, 0f), this.navPosition + new Vector3(0f, 0.05f, 0f), Color.white);
				int pathCount = this.path.Count;
				for (int j = 0; j < pathCount - 1; j++)
				{
					Debug.DrawLine(this.path[j] + new Vector3(0f, -0.05f, 0f), this.path[j + 1] + new Vector3(0f, -0.05f, 0f), Color.green);
				}
			}
			Debug.DrawLine(base.creature.transform.position, base.creature.transform.position + new Vector3(0f, 2f, 0f), Color.green);
			Debug.DrawLine(this.navMeshAgent.nextPosition, this.navMeshAgent.nextPosition + new Vector3(0f, 2f, 0f), Color.yellow);
		}

		// Token: 0x04000F39 RID: 3897
		[Header("General")]
		public bool allowMove = true;

		// Token: 0x04000F3A RID: 3898
		public float runDistance = 3f;

		// Token: 0x04000F3B RID: 3899
		public bool useAcceleration = true;

		// Token: 0x04000F3C RID: 3900
		public float acceleration = 0.3f;

		// Token: 0x04000F3D RID: 3901
		public float baseTurnSpeedMultiplier = 4f;

		// Token: 0x04000F3E RID: 3902
		public float navReachDistance = 0.05f;

		// Token: 0x04000F3F RID: 3903
		public float strafeMinDelay;

		// Token: 0x04000F40 RID: 3904
		public float strafeMaxDelay = 5f;

		// Token: 0x04000F41 RID: 3905
		public int samplePositionCircleSteps = 6;

		// Token: 0x04000F42 RID: 3906
		public NavmeshArea areaMask = ~NavmeshArea.NotWalkable;

		// Token: 0x04000F43 RID: 3907
		[Header("Close or stuck detection")]
		public float stuckRadius = 0.01f;

		// Token: 0x04000F44 RID: 3908
		public float stuckMaxDuration = 0.5f;

		// Token: 0x04000F45 RID: 3909
		public bool preventTooClose = true;

		// Token: 0x04000F46 RID: 3910
		public float tooCloseDistance = 0.7f;

		// Token: 0x04000F47 RID: 3911
		public float edgeAvoidanceDistance = 0.1f;

		// Token: 0x04000F48 RID: 3912
		[Header("Locomotion override")]
		public bool locomotionOverride;

		// Token: 0x04000F49 RID: 3913
		public float locomotionForwardSpeedMultiplier = 1f;

		// Token: 0x04000F4A RID: 3914
		public float locomotionBackwardSpeedMultiplier = 1f;

		// Token: 0x04000F4B RID: 3915
		public float locomotionStrafeSpeedMultiplier = 1f;

		// Token: 0x04000F4C RID: 3916
		public float locomotionRunSpeedMultiplier = 1f;

		// Token: 0x04000F4D RID: 3917
		[Header("Doors")]
		public float reachTime = 0.5f;

		// Token: 0x04000F4E RID: 3918
		public AnimationCurve reachCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		// Token: 0x04000F4F RID: 3919
		public string doorReachClip = "Bas.Animation.Interaction.ReachDoorHandleLeft";

		// Token: 0x04000F50 RID: 3920
		[Header("Moving")]
		[NonSerialized]
		public BrainModuleMove.MoveType moveType;

		// Token: 0x04000F51 RID: 3921
		[NonSerialized]
		protected Vector3 navPosition;

		// Token: 0x04000F52 RID: 3922
		[NonSerialized]
		protected Vector3 moveDirection;

		// Token: 0x04000F53 RID: 3923
		[NonSerialized]
		protected float moveSpeedRatio;

		// Token: 0x04000F54 RID: 3924
		[NonSerialized]
		protected float runSpeedRatio;

		// Token: 0x04000F56 RID: 3926
		[Header("Turning")]
		[NonSerialized]
		public List<BrainModuleMove.TurnRequest> turnRequests = new List<BrainModuleMove.TurnRequest>();

		// Token: 0x04000F57 RID: 3927
		[NonSerialized]
		public List<object> exclusiveHandlerStack = new List<object>();

		// Token: 0x04000F58 RID: 3928
		protected float lastMoveTime;

		// Token: 0x04000F59 RID: 3929
		protected float nextMoveDelay;

		// Token: 0x04000F5A RID: 3930
		protected float directionHold;

		// Token: 0x04000F5B RID: 3931
		protected NavMeshAgent navMeshAgent;

		// Token: 0x04000F5C RID: 3932
		protected float strafeTime;

		// Token: 0x04000F5D RID: 3933
		protected float strafeDelay;

		// Token: 0x04000F5F RID: 3935
		protected Vector3 lastPosition;

		// Token: 0x04000F60 RID: 3936
		protected float stuckDuration;

		// Token: 0x04000F62 RID: 3938
		protected Vector3? previousNavTarget;

		// Token: 0x04000F63 RID: 3939
		protected float lastPlaceTime;

		// Token: 0x04000F64 RID: 3940
		protected AnimationClip reachClip;

		// Token: 0x04000F65 RID: 3941
		protected Handle reachHandle;

		// Token: 0x04000F66 RID: 3942
		protected Side reachSide;

		// Token: 0x04000F67 RID: 3943
		protected bool reach;

		// Token: 0x04000F69 RID: 3945
		private Vector3[] navMeshCornerBuffer = new Vector3[1000];

		// Token: 0x02000734 RID: 1844
		[Serializable]
		public class TurnRequest
		{
			// Token: 0x06003C03 RID: 15363 RVA: 0x0017B9AB File Offset: 0x00179BAB
			public TurnRequest(object handler, Vector3 turnDirection, float turnSpeedRatio)
			{
				this.handler = handler;
				this.turnMode = BrainModuleMove.TurnMode.Direction;
				this.turnDirection = turnDirection;
				this.turnSpeedRatio = turnSpeedRatio;
			}

			// Token: 0x06003C04 RID: 15364 RVA: 0x0017B9CF File Offset: 0x00179BCF
			public TurnRequest(object handler, Transform turnTarget, float turnSpeedRatio)
			{
				this.handler = handler;
				this.turnMode = BrainModuleMove.TurnMode.Transform;
				this.turnTarget = turnTarget;
				this.turnSpeedRatio = turnSpeedRatio;
			}

			// Token: 0x06003C05 RID: 15365 RVA: 0x0017B9F3 File Offset: 0x00179BF3
			public TurnRequest(object handler, float turnSpeedRatio)
			{
				this.handler = handler;
				this.turnMode = BrainModuleMove.TurnMode.MoveDirection;
				this.turnSpeedRatio = turnSpeedRatio;
			}

			// Token: 0x04003C5B RID: 15451
			public object handler;

			// Token: 0x04003C5C RID: 15452
			public BrainModuleMove.TurnMode turnMode;

			// Token: 0x04003C5D RID: 15453
			public Vector3 turnDirection;

			// Token: 0x04003C5E RID: 15454
			public Transform turnTarget;

			// Token: 0x04003C5F RID: 15455
			public float turnSpeedRatio;
		}

		// Token: 0x02000735 RID: 1845
		public enum TurnMode
		{
			// Token: 0x04003C61 RID: 15457
			None,
			// Token: 0x04003C62 RID: 15458
			MoveDirection,
			// Token: 0x04003C63 RID: 15459
			Transform,
			// Token: 0x04003C64 RID: 15460
			Direction
		}

		// Token: 0x02000736 RID: 1846
		public enum MoveType
		{
			// Token: 0x04003C66 RID: 15462
			None,
			// Token: 0x04003C67 RID: 15463
			ToPoint,
			// Token: 0x04003C68 RID: 15464
			InDirection
		}
	}
}
