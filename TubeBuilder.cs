using System;
using System.Collections;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002F6 RID: 758
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Misc/TubeBuilder.html")]
	public class TubeBuilder : MonoBehaviour
	{
		// Token: 0x06002426 RID: 9254 RVA: 0x000F71D4 File Offset: 0x000F53D4
		protected virtual void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (this.preGenerate)
			{
				this.Generate();
				return;
			}
			this.ClearTube();
		}

		// Token: 0x06002427 RID: 9255 RVA: 0x000F71FC File Offset: 0x000F53FC
		public virtual void ClearTube()
		{
			Transform tubeTransform = base.transform.Find("Tube");
			if (tubeTransform)
			{
				if (Application.isEditor)
				{
					base.StartCoroutine(this.DestroyCoroutine(tubeTransform.gameObject));
					return;
				}
				UnityEngine.Object.Destroy(tubeTransform.gameObject);
			}
		}

		// Token: 0x06002428 RID: 9256 RVA: 0x000F7248 File Offset: 0x000F5448
		private IEnumerator DestroyCoroutine(UnityEngine.Object obj)
		{
			yield return Yielders.EndOfFrame;
			UnityEngine.Object.DestroyImmediate(obj);
			yield break;
		}

		// Token: 0x06002429 RID: 9257 RVA: 0x000F7257 File Offset: 0x000F5457
		public virtual void Awake()
		{
			this.Generate();
		}

		// Token: 0x0600242A RID: 9258 RVA: 0x000F7260 File Offset: 0x000F5460
		public virtual void Generate()
		{
			if (!this.target || !this.material)
			{
				return;
			}
			this.ClearTube();
			this.tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder).GetComponent<MeshRenderer>();
			this.tube.transform.SetParent(base.transform);
			this.tube.name = "Tube";
			this.tube.material = this.material;
			this.tube.gameObject.layer = this.layer;
			this.materialInstance = this.tube.material;
			Collider collider = this.tube.GetComponent<Collider>();
			if (this.useCollider)
			{
				collider.material = this.physicMaterial;
			}
			else
			{
				base.StartCoroutine(this.DestroyCoroutine(collider));
			}
			this.UpdateTube();
		}

		// Token: 0x0600242B RID: 9259 RVA: 0x000F7333 File Offset: 0x000F5533
		protected virtual void Update()
		{
			if (this.continuousUpdate)
			{
				this.UpdateTube();
			}
		}

		// Token: 0x0600242C RID: 9260 RVA: 0x000F7344 File Offset: 0x000F5544
		protected virtual void UpdateTube()
		{
			if (!this.target)
			{
				return;
			}
			this.tube.transform.position = Vector3.Lerp(base.transform.position, this.target.transform.position, 0.5f);
			this.tube.transform.rotation = Quaternion.FromToRotation(this.tube.transform.TransformDirection(Vector3.up), this.target.position - base.transform.position) * this.tube.transform.rotation;
			float distance = Vector3.Distance(base.transform.position, this.target.position);
			this.tube.transform.localScale = new Vector3(this.radius, distance / 2f, this.radius);
			this.materialInstance.SetVector("_BaseMap_ST", new Vector4(1f, distance * this.tilingOffset, 0f, 0f));
		}

		// Token: 0x0600242D RID: 9261 RVA: 0x000F745E File Offset: 0x000F565E
		protected virtual void OnDrawGizmos()
		{
			if (!this.preGenerate && this.target)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(base.transform.position, this.target.position);
			}
		}

		// Token: 0x04002378 RID: 9080
		public Transform target;

		// Token: 0x04002379 RID: 9081
		public float radius = 0.015f;

		// Token: 0x0400237A RID: 9082
		public float tilingOffset = 10f;

		// Token: 0x0400237B RID: 9083
		public Material material;

		// Token: 0x0400237C RID: 9084
		public int layer;

		// Token: 0x0400237D RID: 9085
		public bool useCollider;

		// Token: 0x0400237E RID: 9086
		public PhysicMaterial physicMaterial;

		// Token: 0x0400237F RID: 9087
		public bool preGenerate;

		// Token: 0x04002380 RID: 9088
		public bool continuousUpdate;

		// Token: 0x04002381 RID: 9089
		[NonSerialized]
		public MeshRenderer tube;

		// Token: 0x04002382 RID: 9090
		private Material materialInstance;
	}
}
