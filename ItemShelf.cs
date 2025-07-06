using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002D5 RID: 725
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/ItemShelf.html")]
	public class ItemShelf : ThunderBehaviour
	{
		// Token: 0x06002305 RID: 8965 RVA: 0x000F0439 File Offset: 0x000EE639
		private void Awake()
		{
			this.GetAllSpots();
		}

		// Token: 0x06002306 RID: 8966 RVA: 0x000F0441 File Offset: 0x000EE641
		private void OnValidate()
		{
			this.GetAllSpots();
		}

		// Token: 0x06002307 RID: 8967 RVA: 0x000F044C File Offset: 0x000EE64C
		private void GetAllSpots()
		{
			this.shelfSpots = new List<Transform>();
			foreach (object obj in base.transform)
			{
				Transform child = (Transform)obj;
				this.shelfSpots.Add(child);
			}
			this.shelfSpots = (from s in this.shelfSpots
			orderby Vector3.Distance(base.transform.position, s.position)
			select s).ToList<Transform>();
			this.previousChildCount = this.shelfSpots.Count;
		}

		// Token: 0x06002308 RID: 8968 RVA: 0x000F04E8 File Offset: 0x000EE6E8
		private void OnDrawGizmosSelected()
		{
			this.DrawGizmos();
		}

		// Token: 0x06002309 RID: 8969 RVA: 0x000F04F0 File Offset: 0x000EE6F0
		private void OnDrawGizmos()
		{
		}

		// Token: 0x0600230A RID: 8970 RVA: 0x000F04F4 File Offset: 0x000EE6F4
		private void DrawGizmos()
		{
			foreach (Transform position in this.shelfSpots)
			{
				Gizmos.matrix = Matrix4x4.TRS(position.position, position.rotation, Vector3.one);
				Gizmos.DrawWireCube(Vector3.zero, this.shelfSpotSize);
			}
			Gizmos.matrix = Matrix4x4.identity;
		}

		// Token: 0x04002203 RID: 8707
		public List<Transform> shelfSpots;

		// Token: 0x04002204 RID: 8708
		public Vector3 shelfSpotSize;

		// Token: 0x04002205 RID: 8709
		public bool itemMustFitInBounds = true;

		// Token: 0x04002206 RID: 8710
		public bool displayAtRandomSpots;

		// Token: 0x04002207 RID: 8711
		private int previousChildCount;
	}
}
