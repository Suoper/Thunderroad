using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x0200034B RID: 843
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/ItemMagnet.html")]
	[AddComponentMenu("ThunderRoad/Item magnet")]
	[RequireComponent(typeof(Collider))]
	public class ItemMagnet : MonoBehaviour
	{
		// Token: 0x1400012E RID: 302
		// (add) Token: 0x06002763 RID: 10083 RVA: 0x0010FC84 File Offset: 0x0010DE84
		// (remove) Token: 0x06002764 RID: 10084 RVA: 0x0010FCBC File Offset: 0x0010DEBC
		public event ItemMagnet.ItemEvent OnItemCatchEvent;

		// Token: 0x1400012F RID: 303
		// (add) Token: 0x06002765 RID: 10085 RVA: 0x0010FCF4 File Offset: 0x0010DEF4
		// (remove) Token: 0x06002766 RID: 10086 RVA: 0x0010FD2C File Offset: 0x0010DF2C
		public event ItemMagnet.ItemEvent OnItemReleaseEvent;

		// Token: 0x06002767 RID: 10087 RVA: 0x0010FD61 File Offset: 0x0010DF61
		public List<ValueDropdownItem<string>> GetAllHolderSlots()
		{
			return Catalog.GetDropdownHolderSlots("None");
		}

		// Token: 0x06002768 RID: 10088 RVA: 0x0010FD6D File Offset: 0x0010DF6D
		protected virtual void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.RefreshJoints();
		}

		// Token: 0x06002769 RID: 10089 RVA: 0x0010FD83 File Offset: 0x0010DF83
		private void OnDisable()
		{
			this.ReleaseAllItems();
		}

		// Token: 0x0600276A RID: 10090 RVA: 0x0010FD8B File Offset: 0x0010DF8B
		protected virtual void Awake()
		{
			this.rb = base.GetComponentInParent<Rigidbody>();
			this.trigger = base.GetComponent<Collider>();
			this.trigger.gameObject.layer = Common.GetLayer(LayerName.MovingItem);
		}

		// Token: 0x0600276B RID: 10091 RVA: 0x0010FDBC File Offset: 0x0010DFBC
		public virtual bool ItemAllowed(Item item)
		{
			if (item != null)
			{
				ItemData data = item.data;
				if (((data != null) ? data.slot : null) != null)
				{
					return this.SlotAllowed(item.data.slot);
				}
			}
			return false;
		}

		// Token: 0x0600276C RID: 10092 RVA: 0x0010FDE8 File Offset: 0x0010DFE8
		public virtual bool SlotAllowed(string slot)
		{
			if (this.tagFilter == FilterLogic.AnyExcept)
			{
				return !this.slots.Contains(slot);
			}
			return this.tagFilter == FilterLogic.NoneExcept && this.slots.Contains(slot);
		}

		// Token: 0x0600276D RID: 10093 RVA: 0x0010FE20 File Offset: 0x0010E020
		public void RefreshJoints()
		{
			foreach (ItemMagnet.CapturedItem capturedItem in this.capturedItems)
			{
				JointDrive rotationJointDrive = default(JointDrive);
				rotationJointDrive.positionSpring = this.rotationSpring;
				rotationJointDrive.positionDamper = this.rotationDamper;
				rotationJointDrive.maximumForce = this.rotationMaxForce;
				capturedItem.joint.slerpDrive = rotationJointDrive;
				JointDrive jointDrive = default(JointDrive);
				jointDrive.positionSpring = this.positionSpring;
				jointDrive.positionDamper = this.positionDamper;
				jointDrive.maximumForce = this.positionMaxForce;
				capturedItem.joint.xDrive = jointDrive;
				capturedItem.joint.yDrive = jointDrive;
				capturedItem.joint.zDrive = jointDrive;
			}
		}

		// Token: 0x0600276E RID: 10094 RVA: 0x0010FF00 File Offset: 0x0010E100
		public void Lock()
		{
			if (!this.isLocked)
			{
				foreach (ItemMagnet.CapturedItem capturedItem in this.capturedItems)
				{
					foreach (Handle handle in capturedItem.item.handles)
					{
						handle.Release();
						handle.ReleaseAllTkHandlers();
						handle.SetTelekinesis(false);
						handle.SetTouch(false);
						capturedItem.item.allowGrip = false;
					}
					if (this.kinematicLock)
					{
						capturedItem.item.physicBody.isKinematic = true;
					}
				}
				this.isLocked = true;
			}
		}

		// Token: 0x0600276F RID: 10095 RVA: 0x0010FFE0 File Offset: 0x0010E1E0
		public void Unlock()
		{
			if (this.isLocked)
			{
				foreach (ItemMagnet.CapturedItem capturedItem in this.capturedItems)
				{
					foreach (Handle handle in capturedItem.item.handles)
					{
						handle.SetTelekinesis(true);
						handle.SetTouch(true);
					}
					capturedItem.item.allowGrip = true;
					if (this.kinematicLock)
					{
						capturedItem.item.physicBody.isKinematic = false;
					}
				}
				this.isLocked = false;
			}
		}

		// Token: 0x06002770 RID: 10096 RVA: 0x001100B0 File Offset: 0x0010E2B0
		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!base.enabled || this.isLocked)
			{
				return;
			}
			Item item = other.GetComponentInParent<Item>();
			if (!item)
			{
				return;
			}
			if (!this.ItemAllowed(item))
			{
				return;
			}
			ItemMagnet.CapturedItem existingCapturedItem = this.capturedItems.FirstOrDefault((ItemMagnet.CapturedItem c) => c.item == item);
			if (existingCapturedItem != null)
			{
				if (!existingCapturedItem.touchingColliders.Contains(other))
				{
					existingCapturedItem.touchingColliders.Add(other);
					return;
				}
			}
			else if (this.capturedItems.Count < this.maxCount)
			{
				if (this.autoUngrab && item.IsHanded())
				{
					for (int i = item.handlers.Count - 1; i >= 0; i--)
					{
						item.handlers[i].UnGrab(false);
					}
				}
				if (this.gravityMultiplier != 1f || this.massMultiplier != 1f)
				{
					foreach (CollisionHandler collisionHandler in item.collisionHandlers)
					{
						collisionHandler.SetPhysicModifier(this, new float?(this.gravityMultiplier), this.massMultiplier, -1f, -1f, this.sleepThresholdRatio, null);
					}
				}
				Vector3 orgPosition = item.transform.position;
				Quaternion orgRotation = item.transform.rotation;
				item.transform.MoveAlign(item.holderPoint, base.transform, null);
				ConfigurableJoint joint = this.rb.gameObject.AddComponent<ConfigurableJoint>();
				joint.enableCollision = this.enableCollisionWithJointRigidbody;
				joint.autoConfigureConnectedAnchor = false;
				joint.anchor = this.rb.transform.InverseTransformPoint(base.transform.position);
				joint.connectedAnchor = item.transform.InverseTransformPoint(item.holderPoint.position);
				joint.SetConnectedPhysicBody(item.physicBody);
				joint.xMotion = ConfigurableJointMotion.Free;
				joint.yMotion = ConfigurableJointMotion.Free;
				joint.zMotion = ConfigurableJointMotion.Free;
				joint.angularXMotion = ConfigurableJointMotion.Free;
				joint.angularYMotion = ConfigurableJointMotion.Free;
				joint.angularZMotion = ConfigurableJointMotion.Free;
				joint.rotationDriveMode = RotationDriveMode.Slerp;
				this.capturedItems.Add(new ItemMagnet.CapturedItem(item, joint, other));
				this.RefreshJoints();
				if (this.catchedItemIgnoreGravityPush)
				{
					item.ignoreGravityPush = true;
				}
				item.AddNonStorableModifier(this);
				item.transform.position = orgPosition;
				item.transform.rotation = orgRotation;
				UnityEvent onItemCatched = this.OnItemCatched;
				if (onItemCatched != null)
				{
					onItemCatched.Invoke();
				}
				this.OnItemCatch(item, EventTime.OnStart);
				item.InvokeMagnetCatchEvent(this, EventTime.OnStart);
			}
		}

		// Token: 0x06002771 RID: 10097 RVA: 0x001103AC File Offset: 0x0010E5AC
		protected virtual void OnItemCatch(Item item, EventTime time)
		{
			ItemMagnet.ItemEvent onItemCatchEvent = this.OnItemCatchEvent;
			if (onItemCatchEvent == null)
			{
				return;
			}
			onItemCatchEvent(item, time);
		}

		// Token: 0x06002772 RID: 10098 RVA: 0x001103C0 File Offset: 0x0010E5C0
		protected virtual void OnItemRelease(Item item, EventTime time)
		{
			ItemMagnet.ItemEvent onItemReleaseEvent = this.OnItemReleaseEvent;
			if (onItemReleaseEvent == null)
			{
				return;
			}
			onItemReleaseEvent(item, time);
		}

		// Token: 0x06002773 RID: 10099 RVA: 0x001103D4 File Offset: 0x0010E5D4
		protected virtual void Update()
		{
			if (!base.enabled || this.isLocked)
			{
				return;
			}
			int i = this.capturedItems.Count - 1;
			while (i >= 0 && this.capturedItems.Count > i)
			{
				Vector3 positionDiff = this.capturedItems[i].item.holderPoint.position - base.transform.position;
				float rotationDiff = Quaternion.Angle(this.capturedItems[i].item.holderPoint.rotation, base.transform.rotation);
				float rotationUpDiff = Vector3.Angle(base.transform.up, this.capturedItems[i].item.holderPoint.up);
				bool shouldBeStabilized = positionDiff.magnitude < this.stabilizedMaxDistance && rotationDiff < this.stabilizedMaxAngle && rotationUpDiff < this.stabilizedMaxUpAngle && this.capturedItems[i].item.physicBody.velocity.sqrMagnitude < this.stabilizedMaxVelocity * this.stabilizedMaxVelocity && this.capturedItems[i].item.physicBody.angularVelocity.sqrMagnitude < this.stabilizedMaxVelocity * this.stabilizedMaxVelocity;
				if (this.progressiveForceRadius > 0f)
				{
					JointDrive jointDrive = this.capturedItems[i].joint.xDrive;
					jointDrive.positionSpring = (1f - positionDiff.magnitude / this.progressiveForceRadius) * this.positionSpring;
					this.capturedItems[i].joint.xDrive = jointDrive;
					this.capturedItems[i].joint.yDrive = jointDrive;
					this.capturedItems[i].joint.zDrive = jointDrive;
					JointDrive rotationJointDrive = this.capturedItems[i].joint.slerpDrive;
					rotationJointDrive.positionSpring = (1f - positionDiff.magnitude / this.progressiveForceRadius) * this.rotationSpring;
					this.capturedItems[i].joint.slerpDrive = rotationJointDrive;
				}
				if (this.capturedItems[i].stabilized)
				{
					if (!shouldBeStabilized)
					{
						this.capturedItems[i].stabilized = false;
						this.capturedItems[i].item.InvokeMagnetReleaseEvent(this, EventTime.OnStart);
						UnityEvent<bool> onItemStabilized = this.OnItemStabilized;
						if (onItemStabilized != null)
						{
							onItemStabilized.Invoke(false);
						}
						this.OnItemRelease(this.capturedItems[i].item, EventTime.OnStart);
					}
				}
				else if (shouldBeStabilized)
				{
					this.capturedItems[i].stabilized = true;
					this.capturedItems[i].item.InvokeMagnetCatchEvent(this, EventTime.OnEnd);
					UnityEvent<bool> onItemStabilized2 = this.OnItemStabilized;
					if (onItemStabilized2 != null)
					{
						onItemStabilized2.Invoke(true);
					}
					this.OnItemCatch(this.capturedItems[i].item, EventTime.OnEnd);
				}
				i--;
			}
		}

		// Token: 0x06002774 RID: 10100 RVA: 0x001106E0 File Offset: 0x0010E8E0
		protected virtual void OnTriggerExit(Collider other)
		{
			if (!base.enabled || this.isLocked)
			{
				return;
			}
			Item item = other.GetComponentInParent<Item>();
			if (!item)
			{
				return;
			}
			for (int i = this.capturedItems.Count - 1; i >= 0; i--)
			{
				if (this.capturedItems[i].item == item)
				{
					if (this.capturedItems[i].touchingColliders.Contains(other))
					{
						this.capturedItems[i].touchingColliders.Remove(other);
					}
					if (this.capturedItems[i].touchingColliders.Count == 0 && (!this.releaseOnGrabOrTKOnly || item.isGripped || item.IsHanded() || item.isTelekinesisGrabbed))
					{
						this.ReleaseItem(this.capturedItems[i]);
						this.capturedItems.RemoveAt(i);
						if (this.magnetReactivateDurationOnRelease > 0f)
						{
							this.trigger.enabled = false;
							this.DelayedAction(this.magnetReactivateDurationOnRelease, delegate
							{
								this.trigger.enabled = true;
							});
						}
					}
				}
			}
		}

		/// <summary>
		/// Only work for one item
		/// </summary>
		/// <param name="target"></param>
		// Token: 0x06002775 RID: 10101 RVA: 0x00110801 File Offset: 0x0010EA01
		public void ReleaseAndMove(Transform target)
		{
			if (this.capturedItems.Count == 1)
			{
				Item item = this.capturedItems[0].item;
				this.ReleaseAllItems();
				item.physicBody.Teleport(target.position, target.rotation);
			}
		}

		// Token: 0x06002776 RID: 10102 RVA: 0x00110840 File Offset: 0x0010EA40
		public void ReleaseAllItems()
		{
			for (int i = this.capturedItems.Count - 1; i >= 0; i--)
			{
				this.ReleaseItem(this.capturedItems[i]);
			}
			this.capturedItems.Clear();
		}

		// Token: 0x06002777 RID: 10103 RVA: 0x00110884 File Offset: 0x0010EA84
		public void ReleaseItem(ItemMagnet.CapturedItem capturedItem)
		{
			UnityEngine.Object.Destroy(capturedItem.joint);
			foreach (CollisionHandler collisionHandler in capturedItem.item.collisionHandlers)
			{
				collisionHandler.RemovePhysicModifier(this);
			}
			if (this.catchedItemIgnoreGravityPush)
			{
				capturedItem.item.ignoreGravityPush = false;
			}
			capturedItem.item.RemoveNonStorableModifier(this);
			UnityEvent onItemReleased = this.OnItemReleased;
			if (onItemReleased != null)
			{
				onItemReleased.Invoke();
			}
			this.OnItemRelease(capturedItem.item, EventTime.OnEnd);
			capturedItem.item.InvokeMagnetReleaseEvent(this, EventTime.OnEnd);
		}

		// Token: 0x04002680 RID: 9856
		[Tooltip("Select if it accepts any but selected or accepts only selected.")]
		public FilterLogic tagFilter;

		// Token: 0x04002681 RID: 9857
		[Tooltip("Slots that accept the magnet.")]
		public List<string> slots;

		// Token: 0x04002684 RID: 9860
		public UnityEvent OnItemCatched;

		// Token: 0x04002685 RID: 9861
		public UnityEvent OnItemReleased;

		// Token: 0x04002686 RID: 9862
		public UnityEvent<bool> OnItemStabilized;

		// Token: 0x04002687 RID: 9863
		[Tooltip("Does it lock the item in to a kinematic state?")]
		public bool kinematicLock;

		// Token: 0x04002688 RID: 9864
		[Tooltip("Will it release the kinematic state if the item is ONLY grabbed or telekinesis grabbed.")]
		public bool releaseOnGrabOrTKOnly;

		// Token: 0x04002689 RID: 9865
		[Tooltip("Will the kinematic lock be ignored by Gravity push?")]
		public bool catchedItemIgnoreGravityPush;

		// Token: 0x0400268A RID: 9866
		[Tooltip("When enabled, it enables the collission with the joint rigidbody")]
		public bool enableCollisionWithJointRigidbody;

		// Token: 0x0400268B RID: 9867
		[Tooltip("When ticked, it will auto-ungrab the item when magnetised.")]
		public bool autoUngrab;

		// Token: 0x0400268C RID: 9868
		[Tooltip("When released, the item will have this delay before the magnet takes place")]
		public float magnetReactivateDurationOnRelease;

		// Token: 0x0400268D RID: 9869
		[Tooltip("What is the gravity multiplier of the item held by the magnet?")]
		public float gravityMultiplier;

		// Token: 0x0400268E RID: 9870
		[Tooltip("What is the mass multiplier of them item held by the magnet?")]
		public float massMultiplier = 1f;

		// Token: 0x0400268F RID: 9871
		[Tooltip("What is the rigidbody sleep threshold of the held item?")]
		public float sleepThresholdRatio;

		// Token: 0x04002690 RID: 9872
		[Tooltip("How many items can be held by the magnet?")]
		public int maxCount = 1;

		// Token: 0x04002691 RID: 9873
		[Tooltip("The radius of the progressive force movement of the item to the magnet")]
		public float progressiveForceRadius = 0.5f;

		// Token: 0x04002692 RID: 9874
		[Tooltip("The max distance before the item is stabilized.")]
		public float stabilizedMaxDistance = 0.01f;

		// Token: 0x04002693 RID: 9875
		[Tooltip("The max angle of the item when it is stabilized")]
		public float stabilizedMaxAngle = 360f;

		// Token: 0x04002694 RID: 9876
		[Tooltip("The max up angle of the item when it is stabilized.")]
		public float stabilizedMaxUpAngle = 10f;

		// Token: 0x04002695 RID: 9877
		[Tooltip("The maximum velocity of the item before it is stabilized.")]
		public float stabilizedMaxVelocity = 0.2f;

		// Token: 0x04002696 RID: 9878
		[Tooltip("The position mover spring of the magnetized item.")]
		public float positionSpring = 50f;

		// Token: 0x04002697 RID: 9879
		[Tooltip("The position mover damper of the magnetized item")]
		public float positionDamper = 5f;

		// Token: 0x04002698 RID: 9880
		[Tooltip("The maximum force the item takes to go to the magnetized area.")]
		public float positionMaxForce = 500f;

		// Token: 0x04002699 RID: 9881
		[Tooltip("The rotation spring of the magnetized item.")]
		public float rotationSpring = 50f;

		// Token: 0x0400269A RID: 9882
		[Tooltip("The rotation damper of the magnetized item")]
		public float rotationDamper = 1f;

		// Token: 0x0400269B RID: 9883
		[Tooltip("The maximum rotation force of the item")]
		public float rotationMaxForce = 300f;

		// Token: 0x0400269C RID: 9884
		[NonSerialized]
		public bool isLocked;

		// Token: 0x0400269D RID: 9885
		[NonSerialized]
		public Collider trigger;

		// Token: 0x0400269E RID: 9886
		[NonSerialized]
		public Rigidbody rb;

		// Token: 0x0400269F RID: 9887
		public List<ItemMagnet.CapturedItem> capturedItems = new List<ItemMagnet.CapturedItem>();

		// Token: 0x02000A3D RID: 2621
		// (Invoke) Token: 0x060045A4 RID: 17828
		public delegate void ItemEvent(Item item, EventTime time);

		// Token: 0x02000A3E RID: 2622
		[Serializable]
		public class CapturedItem
		{
			// Token: 0x060045A7 RID: 17831 RVA: 0x00196433 File Offset: 0x00194633
			public CapturedItem(Item item, ConfigurableJoint joint, Collider collider)
			{
				this.item = item;
				this.joint = joint;
				this.touchingColliders = new List<Collider>();
				this.touchingColliders.Add(collider);
			}

			// Token: 0x04004782 RID: 18306
			public Item item;

			// Token: 0x04004783 RID: 18307
			public ConfigurableJoint joint;

			// Token: 0x04004784 RID: 18308
			public bool stabilized;

			// Token: 0x04004785 RID: 18309
			public List<Collider> touchingColliders;
		}
	}
}
