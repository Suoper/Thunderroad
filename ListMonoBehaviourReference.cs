using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000346 RID: 838
	[Serializable]
	public class ListMonoBehaviourReference<I> where I : IMonoBehaviourReference
	{
		// Token: 0x17000255 RID: 597
		// (get) Token: 0x06002734 RID: 10036 RVA: 0x0010F069 File Offset: 0x0010D269
		public int Count
		{
			get
			{
				if (this._monoBehaviours != null)
				{
					return this._monoBehaviours.Count;
				}
				return 0;
			}
		}

		// Token: 0x06002735 RID: 10037 RVA: 0x0010F080 File Offset: 0x0010D280
		public bool TryGetAtIndex(int index, out I result)
		{
			result = default(I);
			if (this._monoBehaviours == null)
			{
				return false;
			}
			if (index < 0)
			{
				return false;
			}
			if (index >= this._monoBehaviours.Count)
			{
				return false;
			}
			MonoBehaviour monoBehaviour = this._monoBehaviours[index];
			if (monoBehaviour is I)
			{
				I iMonoBehaviourReference = monoBehaviour as I;
				result = iMonoBehaviourReference;
				return true;
			}
			return false;
		}

		// Token: 0x06002736 RID: 10038 RVA: 0x0010F0DF File Offset: 0x0010D2DF
		public void Add(I toAdd)
		{
			if (this._monoBehaviours == null)
			{
				this._monoBehaviours = new List<MonoBehaviour>();
			}
			this._monoBehaviours.Add(toAdd.GetMonoBehaviourReference());
		}

		// Token: 0x06002737 RID: 10039 RVA: 0x0010F10C File Offset: 0x0010D30C
		public void RemoveAt(int index)
		{
			if (this._monoBehaviours == null)
			{
				return;
			}
			if (index < 0)
			{
				return;
			}
			if (index >= this._monoBehaviours.Count)
			{
				return;
			}
			this._monoBehaviours.RemoveAt(index);
		}

		// Token: 0x06002738 RID: 10040 RVA: 0x0010F137 File Offset: 0x0010D337
		public void Clear()
		{
			this._monoBehaviours.Clear();
		}

		// Token: 0x0400265B RID: 9819
		[SerializeField]
		private List<MonoBehaviour> _monoBehaviours;
	}
}
