using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200030F RID: 783
	public class ArenaPillar : MonoBehaviour
	{
		// Token: 0x1700023E RID: 574
		// (get) Token: 0x06002532 RID: 9522 RVA: 0x000FF0A4 File Offset: 0x000FD2A4
		public bool IsPillarActive
		{
			get
			{
				return this._pillArctive;
			}
		}

		// Token: 0x06002533 RID: 9523 RVA: 0x000FF0AC File Offset: 0x000FD2AC
		private void OnDestroy()
		{
			this.DespawnItem();
		}

		// Token: 0x06002534 RID: 9524 RVA: 0x000FF0B4 File Offset: 0x000FD2B4
		public void HidePillar()
		{
			if (!this._pillArctive)
			{
				return;
			}
			this.rewardPillarSound.Play();
			for (int i = 0; i < this.rewardPillarAnimators.Count; i++)
			{
				this.rewardPillarAnimators[i].SetBool(ArenaPillar.GoOut, false);
			}
			this.rewardPillarFX.SetActive(false);
			this._pillArctive = false;
		}

		// Token: 0x06002535 RID: 9525 RVA: 0x000FF118 File Offset: 0x000FD318
		public void ShowPillar()
		{
			if (this._pillArctive)
			{
				return;
			}
			this.rewardPillarSound.Play();
			this.rewardPillarFX.SetActive(true);
			for (int i = 0; i < this.rewardPillarAnimators.Count; i++)
			{
				this.rewardPillarAnimators[i].SetBool(ArenaPillar.GoOut, true);
			}
			this._pillArctive = true;
		}

		// Token: 0x06002536 RID: 9526 RVA: 0x000FF179 File Offset: 0x000FD379
		public void OnPillarUp()
		{
			if (this.pillarUpEvent != null)
			{
				this.pillarUpEvent();
			}
		}

		// Token: 0x06002537 RID: 9527 RVA: 0x000FF18E File Offset: 0x000FD38E
		public void OnPillarDown()
		{
			if (this.pillarDownEvent != null)
			{
				this.pillarDownEvent();
			}
		}

		// Token: 0x06002538 RID: 9528 RVA: 0x000FF1A4 File Offset: 0x000FD3A4
		public void SpawnItem(ItemData itemData, Action<Item> onItemSpawn)
		{
			if (itemData != null)
			{
				this._itemSpawnCallback = onItemSpawn;
				itemData.SpawnAsync(new Action<Item>(this.OnItemSpawn), null, null, null, true, null, Item.Owner.None);
				return;
			}
			Debug.LogError("ItemData is null. ArenaPillar cannot spawn item.");
		}

		// Token: 0x06002539 RID: 9529 RVA: 0x000FF1F0 File Offset: 0x000FD3F0
		private void OnItemSpawn(Item item)
		{
			this._item = item;
			item.transform.position = this.spawnPoint.position;
			item.transform.MoveAlign(item.GetDefaultHolderPoint().anchor, this.spawnPoint, null);
			item.DisallowDespawn = true;
			if (this._linkItemRigidbody == null)
			{
				this.spawnPoint.gameObject.TryGetOrAddComponent(out this._linkItemRigidbody);
			}
			this._linkItemRigidbody.isKinematic = true;
			if (this._linkItemConfigurableJoint == null)
			{
				this.spawnPoint.gameObject.TryGetOrAddComponent(out this._linkItemConfigurableJoint);
			}
			this._linkItemConfigurableJoint.SetConnectedPhysicBody(item.physicBody);
			this._linkItemConfigurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
			JointDrive joint = this._linkItemConfigurableJoint.xDrive;
			joint.positionSpring = this.drivePositionSpring;
			joint.positionDamper = this.drivePositionDamper;
			this._linkItemConfigurableJoint.xDrive = joint;
			this._linkItemConfigurableJoint.yDrive = joint;
			this._linkItemConfigurableJoint.zDrive = joint;
			joint = this._linkItemConfigurableJoint.slerpDrive;
			joint.positionSpring = this.slerpPositionSpring;
			joint.positionDamper = this.slerpPositionDamper;
			this._linkItemConfigurableJoint.slerpDrive = joint;
			Transform im = item.transform.Find("Whoosh");
			Transform pp = item.transform.Find("Handle");
			this._linkItemConfigurableJoint.targetRotation = Quaternion.identity;
			if (im != null && pp != null && im.position.y < pp.position.y)
			{
				this._linkItemConfigurableJoint.targetRotation = new Quaternion(0f, 180f, 0f, 1f);
			}
			this._item.OnGrabEvent += this.OnGrabItem;
			this._item.OnTelekinesisGrabEvent += this.OnTkItem;
			if (this._itemSpawnCallback != null)
			{
				this._itemSpawnCallback(item);
				this._itemSpawnCallback = null;
			}
		}

		// Token: 0x0600253A RID: 9530 RVA: 0x000FF3F6 File Offset: 0x000FD5F6
		private void OnTkItem(Handle arg1, SpellTelekinesis arg2)
		{
			this.OnGrabItem(null, null);
		}

		// Token: 0x0600253B RID: 9531 RVA: 0x000FF400 File Offset: 0x000FD600
		private void OnGrabItem(Handle handle, RagdollHand ragdollHand)
		{
			this.ReleaseItemFromLink();
		}

		// Token: 0x0600253C RID: 9532 RVA: 0x000FF408 File Offset: 0x000FD608
		public void ReleaseItemFromLink()
		{
			if (this._item != null)
			{
				this._item.OnGrabEvent -= this.OnGrabItem;
				this._item.OnTelekinesisGrabEvent -= this.OnTkItem;
				this._item = null;
				if (this._linkItemConfigurableJoint != null)
				{
					this._linkItemConfigurableJoint.connectedBody = null;
				}
			}
		}

		// Token: 0x0600253D RID: 9533 RVA: 0x000FF474 File Offset: 0x000FD674
		public void DespawnItem()
		{
			if (this._item != null)
			{
				this._item.OnGrabEvent -= this.OnGrabItem;
				this._item.OnTelekinesisGrabEvent -= this.OnTkItem;
				this._item.Despawn();
				this._item = null;
				if (this._linkItemConfigurableJoint != null)
				{
					this._linkItemConfigurableJoint.connectedBody = null;
				}
			}
		}

		// Token: 0x0600253E RID: 9534 RVA: 0x000FF4E9 File Offset: 0x000FD6E9
		public void TpPlayer(float duration, Action onPlayerTp)
		{
			if (duration > 0f)
			{
				base.StartCoroutine(this.TpPlayerCoroutine(duration, onPlayerTp));
				return;
			}
			Player.local.Teleport(this.tpPosition, false, true);
			if (onPlayerTp != null)
			{
				onPlayerTp();
			}
		}

		// Token: 0x0600253F RID: 9535 RVA: 0x000FF51E File Offset: 0x000FD71E
		private IEnumerator TpPlayerCoroutine(float duration, Action onPlayerTp)
		{
			float fadeInDuration = duration / 2f;
			CameraEffects.DoFadeEffect(true, fadeInDuration);
			for (float timer = 0f; timer < fadeInDuration; timer += Time.unscaledDeltaTime)
			{
				yield return null;
			}
			Player.local.Teleport(this.tpPosition, false, true);
			CameraEffects.DoFadeEffect(false, fadeInDuration);
			for (float timer = 0f; timer < fadeInDuration; timer += Time.unscaledDeltaTime)
			{
				yield return null;
			}
			if (onPlayerTp != null)
			{
				onPlayerTp();
			}
			yield break;
		}

		// Token: 0x040024B4 RID: 9396
		public List<Animator> rewardPillarAnimators;

		// Token: 0x040024B5 RID: 9397
		public AudioSource rewardPillarSound;

		// Token: 0x040024B6 RID: 9398
		public GameObject rewardPillarFX;

		// Token: 0x040024B7 RID: 9399
		public Transform spawnPoint;

		// Token: 0x040024B8 RID: 9400
		public Transform tpPosition;

		// Token: 0x040024B9 RID: 9401
		public float drivePositionSpring = 100f;

		// Token: 0x040024BA RID: 9402
		public float drivePositionDamper = 2f;

		// Token: 0x040024BB RID: 9403
		public float slerpPositionSpring = 200f;

		// Token: 0x040024BC RID: 9404
		public float slerpPositionDamper;

		// Token: 0x040024BD RID: 9405
		private bool _pillArctive;

		// Token: 0x040024BE RID: 9406
		private static readonly int GoOut = Animator.StringToHash("GoOut");

		// Token: 0x040024BF RID: 9407
		private ConfigurableJoint _linkItemConfigurableJoint;

		// Token: 0x040024C0 RID: 9408
		private Rigidbody _linkItemRigidbody;

		// Token: 0x040024C1 RID: 9409
		private Item _item;

		// Token: 0x040024C2 RID: 9410
		public Action pillarUpEvent;

		// Token: 0x040024C3 RID: 9411
		public Action pillarDownEvent;

		// Token: 0x040024C4 RID: 9412
		private Action<Item> _itemSpawnCallback;
	}
}
