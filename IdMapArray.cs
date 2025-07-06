using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000270 RID: 624
	[PreferBinarySerialization]
	public class IdMapArray : ScriptableObject
	{
		// Token: 0x06001BEE RID: 7150 RVA: 0x000BAB48 File Offset: 0x000B8D48
		public int GetIdAtUV(float ux, float uy)
		{
			if (this.nibbleArray == null || this.nibbleArray.Length == 0)
			{
				Debug.LogError("ID map array is null.");
				return -1;
			}
			int x = (int)(ux * (float)this.width);
			int i = (int)(uy * (float)this.height) * this.width + x;
			return (int)this.nibbleArray[i];
		}

		// Token: 0x06001BEF RID: 7151 RVA: 0x000BABA4 File Offset: 0x000B8DA4
		public void ConvertArrayToTexture()
		{
			if (this.nibbleArray == null || this.nibbleArray.Length == 0)
			{
				Debug.LogError("ID map array is null.");
				return;
			}
			Catalog.EditorLoadAllJson(false, false, true);
			Texture2D texture = new Texture2D(this.width, this.height);
			for (int y = 0; y < this.height; y++)
			{
				for (int x = 0; x < this.width; x++)
				{
					try
					{
						int i = y * this.width + x;
						byte id = this.nibbleArray[i];
						Color color = Catalog.gameData.GetIDMapColor((int)id);
						texture.SetPixel(x, y, color);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error setting pixel at {0}, {1}: {2}", x, y, e.Message));
					}
				}
			}
			texture.Apply();
			this.debugTexture = texture;
			this.estimatedIDMapSize = this.originalWidth * this.originalHeight * 3;
			this.estimatedIDMapArraySize = this.nibbleArray.Length / 2;
		}

		// Token: 0x06001BF0 RID: 7152 RVA: 0x000BACAC File Offset: 0x000B8EAC
		private void OnValidate()
		{
			this.debugTexture = null;
		}

		// Token: 0x04001AC4 RID: 6852
		[Tooltip("The path to the ID map texture.")]
		public string idMapPath;

		// Token: 0x04001AC5 RID: 6853
		[Tooltip("The factor to scale the ID map down by.")]
		public int scale = 2;

		// Token: 0x04001AC6 RID: 6854
		public NibbleArray nibbleArray;

		// Token: 0x04001AC7 RID: 6855
		public int originalWidth;

		// Token: 0x04001AC8 RID: 6856
		public int originalHeight;

		// Token: 0x04001AC9 RID: 6857
		public int width;

		// Token: 0x04001ACA RID: 6858
		public int height;

		// Token: 0x04001ACB RID: 6859
		private int estimatedIDMapSize;

		// Token: 0x04001ACC RID: 6860
		private int estimatedIDMapArraySize;

		// Token: 0x04001ACD RID: 6861
		public Texture2D debugTexture;
	}
}
