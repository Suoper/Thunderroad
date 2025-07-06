using System;
using UnityEngine;

namespace ThunderRoad
{
	/// <summary>
	/// This component applies custom gravity to a collisionHandlers physic body
	/// </summary>
	// Token: 0x020002AF RID: 687
	public class Gravity : ThunderBehaviour
	{
		// Token: 0x17000207 RID: 519
		// (get) Token: 0x0600203B RID: 8251 RVA: 0x000DB108 File Offset: 0x000D9308
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate;
			}
		}

		// Token: 0x0600203C RID: 8252 RVA: 0x000DB10B File Offset: 0x000D930B
		protected internal override void ManagedFixedUpdate()
		{
			if (this.collisionHandler && this.collisionHandler.active)
			{
				this.collisionHandler.physicBody.AddForce(this.gravityMultiplier * Physics.gravity, ForceMode.Acceleration);
			}
		}

		// Token: 0x04001F44 RID: 8004
		public CollisionHandler collisionHandler;

		// Token: 0x04001F45 RID: 8005
		public float gravityMultiplier;
	}
}
