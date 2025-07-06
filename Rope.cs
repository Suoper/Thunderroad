using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002EF RID: 751
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/Rope.html")]
	public class Rope : Handle
	{
		// Token: 0x060023F8 RID: 9208 RVA: 0x000F5EC8 File Offset: 0x000F40C8
		public new List<ValueDropdownItem<string>> GetAllInteractableID()
		{
			return Catalog.GetDropdownAllID<HandleData>("None");
		}

		// Token: 0x060023F9 RID: 9209 RVA: 0x000F5ED4 File Offset: 0x000F40D4
		protected override void Awake()
		{
		}

		// Token: 0x060023FA RID: 9210 RVA: 0x000F5ED6 File Offset: 0x000F40D6
		protected override void Start()
		{
			base.Start();
		}

		// Token: 0x060023FB RID: 9211 RVA: 0x000F5EE0 File Offset: 0x000F40E0
		protected new void OnEnable()
		{
			base.OnEnable();
			if (!this.isGenerated)
			{
				this.area = base.GetComponentInParent<Area>();
				if (this.area != null && !this.area.initialized)
				{
					this.area.onPlayerEnter.AddListener(new UnityAction(this.Generate));
					return;
				}
				this.Generate();
			}
		}

		// Token: 0x060023FC RID: 9212 RVA: 0x000F5F45 File Offset: 0x000F4145
		private void OnDestroy()
		{
			if (this._probeVolume != null)
			{
				this._probeVolume.UnregisterMaterials(this);
			}
		}

		// Token: 0x060023FD RID: 9213 RVA: 0x000F5F64 File Offset: 0x000F4164
		public void Generate()
		{
			if (this.isGenerated)
			{
				return;
			}
			if (this.dynamicHeight)
			{
				RaycastHit hit;
				if (!Physics.Raycast(base.transform.position, -base.transform.up, out hit, this.raycastRange, -1, QueryTriggerInteraction.Ignore))
				{
					Debug.LogError("Raycast did not hit for dynamic rope " + this.name);
					return;
				}
				GameObject ropeEnd = new GameObject("ropeEndScript");
				Vector3 ropeEndPos = new Vector3(hit.point.x, hit.point.y + this.heightFromGround, hit.point.z);
				ropeEnd.transform.position = ropeEndPos;
				ropeEnd.transform.parent = base.transform;
				this.ropeTarget = ropeEnd.transform;
			}
			this.axisLength = Vector3.Distance(this.ropeStart.position, this.ropeTarget.position);
			base.Awake();
			TubeBuilder tubeBuilder = this.ropeStart.gameObject.AddComponent<TubeBuilder>();
			tubeBuilder.radius = this.ropeRadius;
			tubeBuilder.material = this.ropeMaterial;
			tubeBuilder.target = this.ropeTarget;
			tubeBuilder.useCollider = this.ropeUseCollider;
			tubeBuilder.physicMaterial = this.ropePhysicMaterial;
			tubeBuilder.layer = this.ropeLayer;
			tubeBuilder.Generate();
			if (!string.IsNullOrEmpty(this.ropeTag))
			{
				tubeBuilder.tube.gameObject.tag = this.ropeTag;
			}
			if (AreaManager.Instance)
			{
				LightVolumeReceiver.GetVolumeFromPosition(Vector3.Lerp(this.ropeStart.position, this.ropeTarget.position, 0.5f), out this._probeVolume, this.area);
				if (this._probeVolume)
				{
					this._probeVolume.RegisterMaterials(this, new Material[]
					{
						tubeBuilder.tube.material
					});
				}
				else
				{
					Debug.LogError("Rope " + this.name + " Can't find any probe volume");
				}
			}
			this.axisLength = Vector3.Distance(this.ropeStart.position, this.ropeTarget.position);
			base.transform.position = Vector3.Lerp(this.ropeStart.position, this.ropeTarget.position, 0.5f);
			base.transform.rotation = Utils.LookRotation(this.ropeTarget.position - this.ropeStart.position, Vector3.up, Vector3.up);
			this.handOverlapColliders = new List<Collider>(tubeBuilder.GetComponentsInChildren<Collider>());
			this.isGenerated = true;
		}

		// Token: 0x060023FE RID: 9214 RVA: 0x000F61F8 File Offset: 0x000F43F8
		protected virtual void OnDrawGizmos()
		{
			if (this.ropeTarget)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(this.ropeStart.position, this.ropeTarget.position);
			}
		}

		// Token: 0x04002323 RID: 8995
		[Tooltip("Position of where the rope starts.")]
		public Transform ropeStart;

		// Token: 0x04002324 RID: 8996
		[Tooltip("Position of where the rope ends.")]
		public Transform ropeTarget;

		// Token: 0x04002325 RID: 8997
		[Tooltip("Depicts the radius of the rope mesh (Default : 0.03")]
		public float ropeRadius = 0.03f;

		// Token: 0x04002326 RID: 8998
		[Tooltip("What material the rope uses when generated.")]
		public Material ropeMaterial;

		// Token: 0x04002327 RID: 8999
		[Tooltip("Depicts if the rope has a collider")]
		public bool ropeUseCollider;

		// Token: 0x04002328 RID: 9000
		[Tooltip("Depicts what layer the rope is on (default: 17)")]
		public int ropeLayer;

		// Token: 0x04002329 RID: 9001
		[Tooltip("Depicts what physics material the rope uses.")]
		public PhysicMaterial ropePhysicMaterial;

		// Token: 0x0400232A RID: 9002
		[Tooltip("(Obsolete) Depicts what is stated on the handle when the hand is in the handle radius.")]
		public string ropeTag = "Rope";

		// Token: 0x0400232B RID: 9003
		[Header("Dynamic height")]
		[Tooltip("When enabled, the Rope Target is ignored, and instead the rope will use a raycast to depict the length of the rope.")]
		public bool dynamicHeight;

		// Token: 0x0400232C RID: 9004
		[Tooltip("The maximum range of the raycast used to determine the rope length")]
		public float raycastRange = 50f;

		// Token: 0x0400232D RID: 9005
		[Tooltip("Once the Dynamic height raycast is complete, how far from the ground do you want the rope to cut back by.")]
		public float heightFromGround = 2f;

		// Token: 0x0400232E RID: 9006
		private bool isGenerated;

		// Token: 0x0400232F RID: 9007
		private LightProbeVolume _probeVolume;

		// Token: 0x04002330 RID: 9008
		private Area area;

		// Token: 0x04002331 RID: 9009
		protected Interactable interactable;
	}
}
