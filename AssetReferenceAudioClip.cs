using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ThunderRoad
{
	// Token: 0x02000331 RID: 817
	[Serializable]
	public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
	{
		/// <summary>
		/// Constructs a new reference to a AudioClip.
		/// </summary>
		/// <param name="guid">The object guid.</param>
		// Token: 0x060025E2 RID: 9698 RVA: 0x0010493F File Offset: 0x00102B3F
		public AssetReferenceAudioClip(string guid) : base(guid)
		{
		}
	}
}
