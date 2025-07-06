using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000360 RID: 864
	public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		// Token: 0x06002889 RID: 10377 RVA: 0x00114CC4 File Offset: 0x00112EC4
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			base.Clear();
			int i = 0;
			while (i < this.keyData.Count && i < this.valueData.Count)
			{
				base[this.keyData[i]] = this.valueData[i];
				i++;
			}
		}

		// Token: 0x0600288A RID: 10378 RVA: 0x00114D1C File Offset: 0x00112F1C
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			this.keyData.Clear();
			this.valueData.Clear();
			foreach (KeyValuePair<TKey, TValue> item in this)
			{
				this.keyData.Add(item.Key);
				this.valueData.Add(item.Value);
			}
		}

		// Token: 0x04002721 RID: 10017
		[SerializeField]
		[HideInInspector]
		private List<TKey> keyData = new List<TKey>();

		// Token: 0x04002722 RID: 10018
		[SerializeField]
		[HideInInspector]
		private List<TValue> valueData = new List<TValue>();
	}
}
