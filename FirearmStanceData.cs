using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200022B RID: 555
	public class FirearmStanceData : StanceData
	{
		// Token: 0x06001775 RID: 6005 RVA: 0x0009D6EC File Offset: 0x0009B8EC
		protected void UpdateIDs()
		{
			this.idlePose.id = this.rootName + "IdlePose";
			this.aimPose.id = this.rootName + "AimPose";
			this.fireAction.id = this.rootName + "ShootAnimation";
			this.reloadAction.id = this.rootName + "ReloadAnimation";
		}

		// Token: 0x06001776 RID: 6006 RVA: 0x0009D768 File Offset: 0x0009B968
		protected override StanceData MakeFilteredClone(ItemModuleAI mainHand, ItemModuleAI offHand, int difficulty = 0)
		{
			FirearmStanceData firearmStanceData = (FirearmStanceData)base.MakeFilteredClone(mainHand, offHand, difficulty);
			firearmStanceData.idlePose = this.idlePose.Copy<FirearmPose>();
			firearmStanceData.aimPose = this.aimPose.Copy<FirearmPose>();
			firearmStanceData.fireAction = this.fireAction.Copy<FirearmAction>();
			firearmStanceData.reloadAction = this.reloadAction.Copy<FirearmAction>();
			return firearmStanceData;
		}

		// Token: 0x06001777 RID: 6007 RVA: 0x0009D7C7 File Offset: 0x0009B9C7
		public override IEnumerable<StanceNode> AllStanceNodes()
		{
			yield return this.idlePose;
			yield return this.aimPose;
			yield return this.fireAction;
			yield return this.reloadAction;
			yield break;
		}

		// Token: 0x06001778 RID: 6008 RVA: 0x0009D7D7 File Offset: 0x0009B9D7
		public override IdlePose GetRandomIdle()
		{
			return this.idlePose;
		}

		// Token: 0x06001779 RID: 6009 RVA: 0x0009D7DF File Offset: 0x0009B9DF
		public override IdlePose GetIdleByID(string id)
		{
			if (!(id == this.idlePose.id))
			{
				return null;
			}
			return this.idlePose;
		}

		// Token: 0x0600177A RID: 6010 RVA: 0x0009D7FC File Offset: 0x0009B9FC
		protected override StanceData CreateNew()
		{
			return new FirearmStanceData();
		}

		// Token: 0x040016E4 RID: 5860
		public string rootName;

		// Token: 0x040016E5 RID: 5861
		[Header("Idle")]
		public FirearmPose idlePose = new FirearmPose();

		// Token: 0x040016E6 RID: 5862
		[Header("Aim")]
		public FirearmPose aimPose = new FirearmPose();

		// Token: 0x040016E7 RID: 5863
		[Header("Fire")]
		public FirearmAction fireAction = new FirearmAction();

		// Token: 0x040016E8 RID: 5864
		[Header("Reload")]
		public FirearmAction reloadAction = new FirearmAction();
	}
}
