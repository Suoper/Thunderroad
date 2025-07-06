using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000282 RID: 642
	[RequireComponent(typeof(RagdollPart))]
	public class WeakPointController : MonoBehaviour
	{
		// Token: 0x06001E47 RID: 7751 RVA: 0x000CDEEC File Offset: 0x000CC0EC
		public static void SelectWeakPoints(Ragdoll ragdoll, int targetCount, Action<List<Item>> callback, bool ignoreMinimumPerController = false)
		{
			WeakPointController[] allControllers = ragdoll.GetComponentsInChildren<WeakPointController>();
			from wpc in allControllers
			orderby wpc.priority descending
			select wpc;
			List<Transform> selected = new List<Transform>();
			foreach (WeakPointController controller in allControllers)
			{
				selected.AddRange(controller.GetMinRandomlyPickedPoints());
			}
			if (selected.Count < targetCount)
			{
				while (selected.Count < targetCount)
				{
					WeakPointController addController;
					if (!allControllers.WeightedFilteredSelectInPlace((WeakPointController wpc) => wpc.selected.Count < wpc.minMaxActive.y, (WeakPointController wpc) => (float)wpc.priority, out addController))
					{
						Debug.LogError("Can't add more weak points without violating weak point controller rules!");
						break;
					}
					selected.Add(addController.GetNewRandPoint());
				}
			}
			if (ignoreMinimumPerController && selected.Count > targetCount)
			{
				while (selected.Count > targetCount)
				{
					WeakPointController cullController;
					if (!allControllers.WeightedFilteredSelectInPlace((WeakPointController wpc) => wpc.selected.Count > 1, (WeakPointController wpc) => (float)wpc.selected.Count, out cullController))
					{
						Debug.LogError("Couldn't reduce number of weak points without violating weak point controller rules!");
						break;
					}
					selected.Remove(cullController.RandRemove());
				}
			}
			List<Item> breakableWeakPoints = new List<Item>();
			foreach (Transform transform in selected)
			{
				Item item = transform.GetComponentInChildren<Item>();
				if (item == null || item.breakable == null)
				{
					Debug.LogError("Something went wrong, a weak point is not configured with a breakable item!");
				}
				else
				{
					breakableWeakPoints.Add(item);
				}
			}
			for (int j = 0; j < allControllers.Length; j++)
			{
				allControllers[j].RemoveAllNonSelected();
			}
			if (callback != null)
			{
				callback(breakableWeakPoints);
			}
		}

		// Token: 0x06001E48 RID: 7752 RVA: 0x000CE0D4 File Offset: 0x000CC2D4
		public List<Transform> GetMinRandomlyPickedPoints()
		{
			if (this.shuffled.IsNullOrEmpty())
			{
				this.Init();
			}
			for (int i = 0; i < Mathf.Min(this.minMaxActive.x, this.shuffled.Count); i++)
			{
				this.selected.Add(this.shuffled[0]);
				this.shuffled.RemoveAt(0);
			}
			return this.selected;
		}

		// Token: 0x06001E49 RID: 7753 RVA: 0x000CE144 File Offset: 0x000CC344
		public Transform GetNewRandPoint()
		{
			Transform point = this.shuffled[0];
			this.selected.Add(point);
			this.shuffled.Remove(point);
			return point;
		}

		// Token: 0x06001E4A RID: 7754 RVA: 0x000CE178 File Offset: 0x000CC378
		public Transform RandRemove()
		{
			Transform point = this.selected[UnityEngine.Random.Range(0, this.selected.Count)];
			this.shuffled.Add(point);
			this.selected.Remove(point);
			return point;
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x000CE1BC File Offset: 0x000CC3BC
		public void RemoveAllNonSelected()
		{
			for (int i = this.weakPoints.Count - 1; i >= 0; i--)
			{
				if (!this.selected.Contains(this.weakPoints[i]))
				{
					Item[] componentsInChildren = this.weakPoints[i].GetComponentsInChildren<Item>();
					for (int j = 0; j < componentsInChildren.Length; j++)
					{
						componentsInChildren[j].Despawn();
					}
					UnityEngine.Object.Destroy(this.weakPoints[i].gameObject);
				}
			}
		}

		// Token: 0x06001E4C RID: 7756 RVA: 0x000CE238 File Offset: 0x000CC438
		private void Start()
		{
			this.Init();
		}

		// Token: 0x06001E4D RID: 7757 RVA: 0x000CE240 File Offset: 0x000CC440
		private void Init()
		{
			this.selected = new List<Transform>();
			this.shuffled = new List<Transform>();
			this.shuffled.AddRange(this.weakPoints);
			this.shuffled.Shuffle<Transform>();
			for (int i = this.shuffled.Count - 1; i >= 0; i--)
			{
				Item item = this.shuffled[i].GetComponentInChildren<Item>();
				if (item == null || !(item.breakable != null))
				{
					this.shuffled.RemoveAt(i);
				}
			}
			if (this.shuffled.Count < this.minMaxActive.x)
			{
				Debug.LogError(base.name + ": Non-breakable weak points have been discarded! There are not enough weak points to meet this controller's minimum!");
			}
		}

		// Token: 0x06001E4E RID: 7758 RVA: 0x000CE2F4 File Offset: 0x000CC4F4
		public void AddAllChildBreakableItemsAsWeakPoints(int includeParents = 0)
		{
			Breakable[] componentsInChildren = base.GetComponentsInChildren<Breakable>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				Transform weakPoint = componentsInChildren[j].transform;
				if (includeParents > 0)
				{
					for (int i = 0; i < includeParents; i++)
					{
						weakPoint = weakPoint.parent;
					}
				}
				this.weakPoints.Add(weakPoint);
			}
		}

		// Token: 0x06001E4F RID: 7759 RVA: 0x000CE344 File Offset: 0x000CC544
		public void AttachWeakPoints()
		{
			Rigidbody rb = base.GetComponent<Rigidbody>();
			if (rb == null)
			{
				Debug.LogError("No RB on controller!");
				return;
			}
			Item[] componentsInChildren = base.GetComponentsInChildren<Item>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.AddComponent<FixedJoint>().connectedBody = rb;
			}
		}

		// Token: 0x04001CB9 RID: 7353
		public int priority;

		// Token: 0x04001CBA RID: 7354
		public List<Transform> weakPoints;

		// Token: 0x04001CBB RID: 7355
		public Vector2Int minMaxActive = new Vector2Int(1, 1);

		// Token: 0x04001CBC RID: 7356
		[NonSerialized]
		public int added;

		// Token: 0x04001CBD RID: 7357
		public List<Transform> selected;

		// Token: 0x04001CBE RID: 7358
		private List<Transform> shuffled;
	}
}
