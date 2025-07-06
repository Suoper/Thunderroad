using System;
using UnityEngine.AddressableAssets;

namespace ThunderRoad
{
	// Token: 0x02000332 RID: 818
	[Serializable]
	public class AssetReferenceAudioContainer : AssetReferenceT<AudioContainer>
	{
		/// <summary>
		/// Constructs a new reference to a AudioContainer.
		/// </summary>
		/// <param name="guid">The object guid.</param>
		// Token: 0x060025E3 RID: 9699 RVA: 0x00104948 File Offset: 0x00102B48
		public AssetReferenceAudioContainer(string guid) : base(guid)
		{
		}
	}
}
