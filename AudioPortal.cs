using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002B9 RID: 697
	[RequireComponent(typeof(BoxCollider))]
	[AddComponentMenu("ThunderRoad/Levels/Audio Portal")]
	public class AudioPortal : MonoBehaviour
	{
		// Token: 0x060021D0 RID: 8656 RVA: 0x000E9024 File Offset: 0x000E7224
		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				base.gameObject.layer = Common.GetLayer(LayerName.Zone);
			}
			this.boxCollider = base.GetComponent<BoxCollider>();
			this.boxCollider.isTrigger = true;
			this.boxCollider.size = new Vector3(this.size.x, this.size.y, this.depthForward + this.depthBackward);
			this.boxCollider.center = new Vector3(0f, 0f, (this.depthForward - this.depthBackward) / 2f);
			this.boxCollider.hideFlags = HideFlags.HideInInspector;
			if (this.size.x <= 0f)
			{
				this.size = new Vector2(1f, this.size.y);
			}
			if (this.size.y <= 0f)
			{
				this.size = new Vector2(this.size.x, 1f);
			}
			if (this.depthForward <= 0f)
			{
				this.depthForward = 1f;
			}
			if (this.depthBackward <= 0f)
			{
				this.depthBackward = 1f;
			}
		}

		// Token: 0x060021D1 RID: 8657 RVA: 0x000E9158 File Offset: 0x000E7358
		private void Awake()
		{
			this.playerlayer = Common.GetLayer(LayerName.PlayerLocomotion);
			base.gameObject.layer = Common.GetLayer(LayerName.Zone);
		}

		// Token: 0x060021D2 RID: 8658 RVA: 0x000E9179 File Offset: 0x000E7379
		private void OnDisable()
		{
			this.playerCollider = null;
			this.playerDetected = false;
		}

		// Token: 0x060021D3 RID: 8659 RVA: 0x000E918C File Offset: 0x000E738C
		private void OnTriggerEnter(Collider other)
		{
			if (!this.playerDetected && other.gameObject.layer == this.playerlayer)
			{
				this.playerHead = other.GetComponentInParent<PlayerHead>();
				if (this.playerHead)
				{
					this.playerCollider = other;
					this.playerDetected = true;
					base.StopAllCoroutines();
					base.StartCoroutine(this.UpdateBlend());
				}
			}
		}

		// Token: 0x060021D4 RID: 8660 RVA: 0x000E91EE File Offset: 0x000E73EE
		private void OnTriggerExit(Collider other)
		{
			if (this.playerDetected && other == this.playerCollider)
			{
				this.playerDetected = false;
			}
		}

		// Token: 0x060021D5 RID: 8661 RVA: 0x000E920D File Offset: 0x000E740D
		private IEnumerator UpdateBlend()
		{
			while (this.playerDetected)
			{
				Vector3 localPosition = base.transform.InverseTransformPoint(this.playerHead.transform.position);
				this.forwardValue = Mathf.InverseLerp(-this.depthBackward, 0f, localPosition.z);
				this.backwardValue = Mathf.InverseLerp(this.depthForward, 0f, localPosition.z);
				this.forwardArea.UpdateIntensity(this.forwardValue, (this.forwardValue == 1f) ? 1f : this.GetDoorOpenValue(), false);
				this.backwardArea.UpdateIntensity(this.backwardValue, (this.backwardValue == 1f) ? 1f : this.GetDoorOpenValue(), false);
				yield return null;
			}
			if (base.transform.InverseTransformPoint(this.playerHead.transform.position).z >= 0f)
			{
				this.forwardValue = 1f;
				this.backwardValue = 0f;
			}
			else
			{
				this.forwardValue = 0f;
				this.backwardValue = 1f;
			}
			while (!this.forwardArea.IsTargetVolumeReached() && !this.backwardArea.IsTargetVolumeReached())
			{
				this.forwardArea.UpdateIntensity(this.forwardValue, (this.forwardValue == 1f) ? 1f : this.GetDoorOpenValue(), false);
				this.backwardArea.UpdateIntensity(this.backwardValue, (this.backwardValue == 1f) ? 1f : this.GetDoorOpenValue(), false);
				yield return null;
			}
			yield break;
		}

		// Token: 0x060021D6 RID: 8662 RVA: 0x000E921C File Offset: 0x000E741C
		private float GetDoorOpenValue()
		{
			if (this.doorReferences.Count > 0)
			{
				float largerAngle = 0f;
				foreach (AudioPortal.DoorReference doorReference in this.doorReferences)
				{
					float angle = Vector3.Angle(doorReference.frame.forward, doorReference.door.forward);
					if (angle > largerAngle)
					{
						largerAngle = angle;
					}
				}
				return largerAngle / this.doorMaxAngle;
			}
			return 1f;
		}

		// Token: 0x060021D7 RID: 8663 RVA: 0x000E92AC File Offset: 0x000E74AC
		private void OnDrawGizmos()
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = new Color(0.5f, 0.5f, 0f, 0.25f);
			Gizmos.DrawCube(Vector3.zero, new Vector3(this.size.x, this.size.y, 0f));
			Gizmos.DrawWireCube(this.boxCollider.center, this.boxCollider.size);
		}

		// Token: 0x040020B3 RID: 8371
		[Header("General")]
		public BoxCollider boxCollider;

		// Token: 0x040020B4 RID: 8372
		public Vector2 size = Vector2.one;

		// Token: 0x040020B5 RID: 8373
		public float depthForward = 1f;

		// Token: 0x040020B6 RID: 8374
		public float depthBackward = 1f;

		// Token: 0x040020B7 RID: 8375
		[Header("Door Occlusion")]
		public List<AudioPortal.DoorReference> doorReferences;

		// Token: 0x040020B8 RID: 8376
		public float doorMaxAngle = 90f;

		// Token: 0x040020B9 RID: 8377
		[Header("Areas")]
		public AudioArea backwardArea;

		// Token: 0x040020BA RID: 8378
		public AudioArea forwardArea;

		// Token: 0x040020BB RID: 8379
		[Header("Debug")]
		[NonSerialized]
		public bool playerDetected;

		// Token: 0x040020BC RID: 8380
		[NonSerialized]
		public float forwardValue;

		// Token: 0x040020BD RID: 8381
		[NonSerialized]
		public float backwardValue;

		// Token: 0x040020BE RID: 8382
		protected PlayerHead playerHead;

		// Token: 0x040020BF RID: 8383
		protected Collider playerCollider;

		// Token: 0x040020C0 RID: 8384
		protected int playerlayer;

		// Token: 0x02000985 RID: 2437
		[Serializable]
		public class DoorReference
		{
			// Token: 0x040044D7 RID: 17623
			public Transform frame;

			// Token: 0x040044D8 RID: 17624
			public Transform door;
		}

		// Token: 0x02000986 RID: 2438
		[Serializable]
		public class AudioSourceBlend
		{
			// Token: 0x060043D9 RID: 17369 RVA: 0x0018FEA4 File Offset: 0x0018E0A4
			public void ResetcutoffFrequencyCurve()
			{
				this.cutoffFrequencyCurve = new AnimationCurve(new Keyframe[]
				{
					new Keyframe(0f, 100f),
					new Keyframe(1f, 22000f, 30000f, 30000f)
				});
			}

			// Token: 0x040044D9 RID: 17625
			public AudioPortal.Area area;

			// Token: 0x040044DA RID: 17626
			public AudioSource audioSource;

			// Token: 0x040044DB RID: 17627
			public Vector2 minMaxVolume = new Vector2(0f, 1f);

			// Token: 0x040044DC RID: 17628
			public AudioLowPassFilter audioLowPassFilter;

			// Token: 0x040044DD RID: 17629
			public AnimationCurve cutoffFrequencyCurve;

			// Token: 0x040044DE RID: 17630
			[NonSerialized]
			public bool targetReached;
		}

		// Token: 0x02000987 RID: 2439
		public enum Area
		{
			// Token: 0x040044E0 RID: 17632
			Forward,
			// Token: 0x040044E1 RID: 17633
			Backward
		}
	}
}
