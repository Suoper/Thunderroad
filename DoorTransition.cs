using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002C3 RID: 707
	public class DoorTransition : MonoBehaviour
	{
		// Token: 0x06002264 RID: 8804 RVA: 0x000ED460 File Offset: 0x000EB660
		private void Start()
		{
			List<Collider> doorColliders = new List<Collider>();
			base.GetComponents<Collider>(doorColliders);
			foreach (Collider col in doorColliders)
			{
				foreach (Collider col2 in this.ignoredColliders)
				{
					Physics.IgnoreCollision(col, col2);
				}
			}
			this.initialPos = this.doorJoint.transform.position;
			this.initialYRotation = this.doorJoint.transform.rotation.y;
			if (!this.doorJoint)
			{
				this.doorJoint = base.GetComponent<HingeJoint>();
			}
			if (this.doorJoint)
			{
				this.maxAngle = this.doorJoint.limits.max * this.jointSensitivity;
			}
			else
			{
				Debug.LogError("DoorJoint not found !");
			}
			this.destinationData = Catalog.GetData<LevelData>(this.mapDestination, true);
			if (this.destinationData == null && !string.IsNullOrEmpty(this.mapDestination))
			{
				Debug.LogError("Destination " + this.mapDestination + " not found.");
			}
			if (this.destinationData == null && this.teleportDestination == null)
			{
				Debug.LogError("No destination set for door " + base.gameObject.name + ".");
			}
		}

		// Token: 0x06002265 RID: 8805 RVA: 0x000ED5F4 File Offset: 0x000EB7F4
		private void LateUpdate()
		{
			if (!this.isDoorOpen && this.doorJoint.angle >= this.maxAngle)
			{
				base.StartCoroutine(this.OnDoorOpen());
			}
		}

		// Token: 0x06002266 RID: 8806 RVA: 0x000ED61E File Offset: 0x000EB81E
		private IEnumerator OnDoorOpen()
		{
			this.isDoorOpen = true;
			CameraEffects.DoFadeEffect(true, this.fadeoutTimer);
			yield return Yielders.ForSeconds(this.fadeoutTimer);
			if (this.destinationData != null)
			{
				LevelManager.LoadLevel(this.destinationData, Level.current.mode, null, LoadingCamera.State.Enabled);
			}
			else if (this.teleportDestination != null)
			{
				yield return Yielders.ForSeconds(this.fadeoutTimer);
				Player.local.gameObject.transform.position = this.teleportDestination.transform.position;
				CameraEffects.DoFadeEffect(false, this.fadeoutTimer);
			}
			else
			{
				Debug.LogError("No destination for door " + base.gameObject.name + ".");
			}
			this.doorJoint.GetComponent<Rigidbody>().isKinematic = true;
			this.doorJoint.GetComponent<Rigidbody>().velocity = Vector3.zero;
			this.isDoorOpen = false;
			this.doorJoint.transform.position = this.initialPos;
			this.doorJoint.transform.eulerAngles = new Vector3(this.doorJoint.transform.rotation.x, this.initialYRotation, this.doorJoint.transform.rotation.z);
			this.doorJoint.GetComponent<Rigidbody>().isKinematic = false;
			yield break;
		}

		// Token: 0x04002178 RID: 8568
		public HingeJoint doorJoint;

		// Token: 0x04002179 RID: 8569
		public float jointSensitivity = 0.9f;

		// Token: 0x0400217A RID: 8570
		public float fadeoutTimer = 0.5f;

		// Token: 0x0400217B RID: 8571
		public string mapDestination;

		// Token: 0x0400217C RID: 8572
		public Transform teleportDestination;

		// Token: 0x0400217D RID: 8573
		public List<Collider> ignoredColliders;

		// Token: 0x0400217E RID: 8574
		private float maxAngle;

		// Token: 0x0400217F RID: 8575
		private bool isDoorOpen;

		// Token: 0x04002180 RID: 8576
		private LevelData destinationData;

		// Token: 0x04002181 RID: 8577
		private Vector3 initialPos;

		// Token: 0x04002182 RID: 8578
		private float initialYRotation;
	}
}
