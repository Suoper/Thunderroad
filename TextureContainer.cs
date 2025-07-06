using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002A4 RID: 676
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/TextureContainer")]
	[CreateAssetMenu(menuName = "ThunderRoad/Textures/Texture Container")]
	public class TextureContainer : ScriptableObject
	{
		// Token: 0x06001F87 RID: 8071 RVA: 0x000D67D4 File Offset: 0x000D49D4
		public Texture GetRandomTexture()
		{
			if (this.textures.Count == 0)
			{
				return null;
			}
			if (this.textures.Count == 1)
			{
				return this.textures[0];
			}
			int index = UnityEngine.Random.Range(0, this.textures.Count - 1);
			return this.PickTexture(index);
		}

		// Token: 0x06001F88 RID: 8072 RVA: 0x000D6826 File Offset: 0x000D4A26
		public Texture PickTexture(int index)
		{
			return this.textures[index];
		}

		// Token: 0x04001EB8 RID: 7864
		public List<Texture> textures;
	}
}
