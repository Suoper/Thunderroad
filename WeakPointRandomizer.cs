using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200026E RID: 622
	public class WeakPointRandomizer : MonoBehaviour
	{
		// Token: 0x06001BD8 RID: 7128 RVA: 0x000B82F4 File Offset: 0x000B64F4
		public List<Transform> RandomizeWeakPoints(int targetCount, bool ignoreMinimumPerController = false)
		{
			from @group in this.groups
			orderby @group.priority descending
			select @group;
			List<Transform> selected = new List<Transform>();
			for (int i = 0; i < this.groups.Count; i++)
			{
				selected.AddRange(this.groups[i].GetMinRandomlyPickedPoints());
			}
			if (selected.Count < targetCount)
			{
				while (selected.Count < targetCount)
				{
					WeakPointRandomizer.Group addGroup;
					if (!this.groups.WeightedFilteredSelectInPlace((WeakPointRandomizer.Group group) => (float)group.selectedCount < group.minMaxWeakPoints.y, (WeakPointRandomizer.Group group) => (float)group.priority, out addGroup))
					{
						Debug.LogError("Can't add more weak points without violating weak point controller rules!");
						break;
					}
					selected.Add(addGroup.GetNewRandPoint());
				}
			}
			if (selected.Count > targetCount)
			{
				while (selected.Count > targetCount)
				{
					WeakPointRandomizer.Group cullGroup;
					if (!this.groups.WeightedFilteredSelectInPlace((WeakPointRandomizer.Group group) => group.selectedCount > 1, (WeakPointRandomizer.Group group) => (float)group.selectedCount, out cullGroup) && !ignoreMinimumPerController)
					{
						Debug.LogError("Couldn't reduce number of weak points without violating weak point controller rules!");
						break;
					}
					selected.Remove(cullGroup.RandRemove());
				}
			}
			for (int j = 0; j < this.groups.Count; j++)
			{
				this.groups[j].RemoveAllNonSelected();
			}
			return selected;
		}

		// Token: 0x04001AB4 RID: 6836
		public List<WeakPointRandomizer.Group> groups;

		// Token: 0x020008DA RID: 2266
		[Serializable]
		public class Group
		{
			// Token: 0x17000549 RID: 1353
			// (get) Token: 0x060041A9 RID: 16809 RVA: 0x0018BB2D File Offset: 0x00189D2D
			public int selectedCount
			{
				get
				{
					return this.selected.Count;
				}
			}

			// Token: 0x060041AA RID: 16810 RVA: 0x0018BB3C File Offset: 0x00189D3C
			private void Init()
			{
				this.allWeakPointOptions = new List<Transform>();
				this.shuffled = new List<Transform>();
				this.selected = new List<Transform>();
				foreach (object obj in this.parent)
				{
					Transform child = (Transform)obj;
					if (child.GetComponentInChildren<SimpleBreakable>(true) != null)
					{
						this.shuffled.Add(child);
					}
				}
				if ((float)this.shuffled.Count < this.minMaxWeakPoints.x)
				{
					Debug.LogError("All childs of " + this.parent.name + " without simple breakables have been ignored, which leaves it without enough for the specified minimum weakpoint count!");
				}
				this.allWeakPointOptions.AddRange(this.shuffled);
				this.shuffled.Shuffle<Transform>();
				this.initialized = true;
			}

			// Token: 0x060041AB RID: 16811 RVA: 0x0018BC28 File Offset: 0x00189E28
			public List<Transform> GetMinRandomlyPickedPoints()
			{
				if (!this.initialized)
				{
					this.Init();
				}
				int pick = Mathf.Min((int)this.minMaxWeakPoints.x, this.shuffled.Count);
				for (int i = 0; i < pick; i++)
				{
					this.selected.Add(this.shuffled[0]);
					this.shuffled.RemoveAt(0);
				}
				return this.selected;
			}

			// Token: 0x060041AC RID: 16812 RVA: 0x0018BC98 File Offset: 0x00189E98
			public Transform GetNewRandPoint()
			{
				if (!this.initialized)
				{
					this.Init();
				}
				Transform point = this.shuffled[0];
				this.selected.Add(point);
				this.shuffled.Remove(point);
				return point;
			}

			// Token: 0x060041AD RID: 16813 RVA: 0x0018BCDC File Offset: 0x00189EDC
			public Transform RandRemove()
			{
				if (!this.initialized)
				{
					this.Init();
				}
				Transform point = this.selected[UnityEngine.Random.Range(0, this.selected.Count)];
				this.shuffled.Add(point);
				this.selected.Remove(point);
				return point;
			}

			// Token: 0x060041AE RID: 16814 RVA: 0x0018BD30 File Offset: 0x00189F30
			public void RemoveAllNonSelected()
			{
				if (!this.initialized)
				{
					this.Init();
				}
				for (int i = this.allWeakPointOptions.Count - 1; i >= 0; i--)
				{
					if (!this.selected.Contains(this.allWeakPointOptions[i]))
					{
						this.allWeakPointOptions[i].gameObject.SetActive(false);
					}
				}
			}

			// Token: 0x040042E1 RID: 17121
			public Transform parent;

			// Token: 0x040042E2 RID: 17122
			public int priority;

			// Token: 0x040042E3 RID: 17123
			public Vector2 minMaxWeakPoints;

			// Token: 0x040042E4 RID: 17124
			private bool initialized;

			// Token: 0x040042E5 RID: 17125
			private List<Transform> allWeakPointOptions;

			// Token: 0x040042E6 RID: 17126
			private List<Transform> shuffled;

			// Token: 0x040042E7 RID: 17127
			private List<Transform> selected;
		}
	}
}
