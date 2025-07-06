using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ThunderRoad
{
	// Token: 0x02000232 RID: 562
	[Serializable]
	public class GuardPose : StanceNode
	{
		// Token: 0x17000169 RID: 361
		// (get) Token: 0x060017BD RID: 6077 RVA: 0x0009E94D File Offset: 0x0009CB4D
		public override bool customID
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060017BE RID: 6078 RVA: 0x0009E950 File Offset: 0x0009CB50
		public List<ValueDropdownItem<string>> GetRiposteOptions()
		{
			List<ValueDropdownItem<string>> options = new List<ValueDropdownItem<string>>();
			MeleeStanceData meleeStanceData = this.stanceData as MeleeStanceData;
			if (meleeStanceData != null)
			{
				for (int i = 0; i < meleeStanceData.ripostes.Count; i++)
				{
					options.Add(new ValueDropdownItem<string>(meleeStanceData.ripostes[i].id, meleeStanceData.ripostes[i].id));
				}
			}
			return options;
		}

		// Token: 0x060017BF RID: 6079 RVA: 0x0009E9B6 File Offset: 0x0009CBB6
		public override StanceNode CreateNew()
		{
			return new GuardPose();
		}

		// Token: 0x060017C0 RID: 6080 RVA: 0x0009E9C0 File Offset: 0x0009CBC0
		public override T Copy<T>()
		{
			GuardPose newNode = base.Copy<T>() as GuardPose;
			newNode.riposteOptions = new List<string>();
			for (int i = 0; i < this.riposteOptions.Count; i++)
			{
				newNode.riposteOptions.Add(this.riposteOptions[i]);
			}
			return newNode as T;
		}

		// Token: 0x060017C1 RID: 6081 RVA: 0x0009EA24 File Offset: 0x0009CC24
		protected override void PopulateLists()
		{
			this.ripostes = new List<AttackMotion>();
			MeleeStanceData meleeStanceData = this.stanceData as MeleeStanceData;
			if (meleeStanceData != null)
			{
				for (int i = 0; i < meleeStanceData.ripostes.Count; i++)
				{
					if (this.riposteOptions.Contains(meleeStanceData.ripostes[i].id))
					{
						this.ripostes.Add(meleeStanceData.ripostes[i]);
					}
				}
			}
		}

		// Token: 0x060017C2 RID: 6082 RVA: 0x0009EA96 File Offset: 0x0009CC96
		public AttackMotion GetRiposte()
		{
			if (this.ripostes.IsNullOrEmpty())
			{
				return null;
			}
			return this.ripostes.WeightedSelect((AttackMotion riposte) => riposte.weight);
		}

		// Token: 0x04001712 RID: 5906
		public List<string> riposteOptions;

		// Token: 0x04001713 RID: 5907
		[NonSerialized]
		public List<AttackMotion> ripostes;
	}
}
