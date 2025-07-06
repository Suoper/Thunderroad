using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002FE RID: 766
	public class Clouds : MonoBehaviour
	{
		// Token: 0x17000238 RID: 568
		// (get) Token: 0x0600249E RID: 9374 RVA: 0x000FB49E File Offset: 0x000F969E
		public static Clouds instance
		{
			get
			{
				return Clouds._instance;
			}
		}

		// Token: 0x0600249F RID: 9375 RVA: 0x000FB4A5 File Offset: 0x000F96A5
		private void Awake()
		{
			if (!Clouds._instance)
			{
				Clouds._instance = this;
				return;
			}
			if (Clouds._instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x17000239 RID: 569
		// (get) Token: 0x060024A0 RID: 9376 RVA: 0x000FB4D2 File Offset: 0x000F96D2
		public MeshRenderer meshRenderer
		{
			get
			{
				if (!this._meshRenderer)
				{
					this._meshRenderer = base.GetComponent<MeshRenderer>();
				}
				return this._meshRenderer;
			}
		}

		// Token: 0x04002434 RID: 9268
		protected static Clouds _instance;

		// Token: 0x04002435 RID: 9269
		protected MeshRenderer _meshRenderer;
	}
}
