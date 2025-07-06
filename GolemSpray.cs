using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000267 RID: 615
	[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Magic spray config")]
	[Serializable]
	public class GolemSpray : GolemAbility
	{
		// Token: 0x06001BA1 RID: 7073 RVA: 0x000B714C File Offset: 0x000B534C
		public List<ValueDropdownItem<string>> GetValidSkills()
		{
			List<ValueDropdownItem<string>> result = new List<ValueDropdownItem<string>>
			{
				new ValueDropdownItem<string>("None", "")
			};
			if (!Catalog.IsJsonLoaded())
			{
				return result;
			}
			foreach (SkillData skillData in Catalog.GetDataList<SkillData>())
			{
				if (skillData is IGolemSprayable)
				{
					result.Add(new ValueDropdownItem<string>(skillData.id, skillData.id));
				}
			}
			return result;
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x06001BA2 RID: 7074 RVA: 0x000B71DC File Offset: 0x000B53DC
		// (set) Token: 0x06001BA3 RID: 7075 RVA: 0x000B71E4 File Offset: 0x000B53E4
		public List<Transform> sprayPoints { get; protected set; }

		// Token: 0x06001BA4 RID: 7076 RVA: 0x000B71F0 File Offset: 0x000B53F0
		public override bool Allow(GolemController golem)
		{
			return base.Allow(golem) && golem.lastAttackMotion != this.sprayMotion && !(golem.lastAbility is GolemSpray) && Mathf.Abs(Vector3.SignedAngle(golem.transform.forward, (golem.attackTarget.position.ToXZ() - golem.transform.position.ToXZ()).normalized, golem.transform.up)) < this.sprayAngle / 2f;
		}

		// Token: 0x06001BA5 RID: 7077 RVA: 0x000B7280 File Offset: 0x000B5480
		public override void Begin(GolemController golem)
		{
			base.Begin(golem);
			SkillData skill = Catalog.GetData<SkillData>(this.spraySkillID, true);
			if (!(skill is IGolemSprayable))
			{
				Debug.LogError("Can't use a spell which is not configured as a golem sprayable!");
				return;
			}
			this.spraySkillData = (skill as IGolemSprayable);
			this.sprayPoints = new List<Transform>();
			foreach (Transform magicPoint in golem.magicSprayPoints)
			{
				if (this.spraySources.Contains(magicPoint.name))
				{
					this.sprayPoints.Add(magicPoint);
				}
			}
			golem.PerformAttackMotion(this.sprayMotion, new Action(base.End));
		}

		// Token: 0x06001BA6 RID: 7078 RVA: 0x000B7344 File Offset: 0x000B5544
		public override void AbilityStep(int step)
		{
			base.AbilityStep(step);
			if (step == 1)
			{
				this.StartSpray();
				return;
			}
			if (step != 2)
			{
				return;
			}
			this.EndSpray();
		}

		// Token: 0x06001BA7 RID: 7079 RVA: 0x000B7363 File Offset: 0x000B5563
		public override void Interrupt()
		{
			base.Interrupt();
			this.EndSpray();
		}

		// Token: 0x06001BA8 RID: 7080 RVA: 0x000B7371 File Offset: 0x000B5571
		public override void OnEnd()
		{
			base.OnEnd();
			this.EndSpray();
		}

		// Token: 0x06001BA9 RID: 7081 RVA: 0x000B737F File Offset: 0x000B557F
		protected void StartSpray()
		{
			this.spraySkillData.GolemSprayStart(this, out this.endSpray);
		}

		// Token: 0x06001BAA RID: 7082 RVA: 0x000B7393 File Offset: 0x000B5593
		protected void EndSpray()
		{
			Action action = this.endSpray;
			if (action != null)
			{
				action();
			}
			this.endSpray = null;
		}

		// Token: 0x04001A6B RID: 6763
		public string spraySkillID;

		// Token: 0x04001A6C RID: 6764
		public List<string> spraySources = new List<string>();

		// Token: 0x04001A6D RID: 6765
		public GolemController.AttackMotion sprayMotion = GolemController.AttackMotion.Spray;

		// Token: 0x04001A6E RID: 6766
		public float sprayAngle = 90f;

		// Token: 0x04001A6F RID: 6767
		[NonSerialized]
		public IGolemSprayable spraySkillData;

		// Token: 0x04001A71 RID: 6769
		protected Action endSpray;
	}
}
