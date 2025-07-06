using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002F5 RID: 757
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/ToggleObjects.html")]
	public class ToggleObjects : MonoBehaviour
	{
		// Token: 0x06002420 RID: 9248 RVA: 0x000F6EA4 File Offset: 0x000F50A4
		public void Toggle()
		{
			foreach (Renderer renderer in this.renderers)
			{
				if (renderer)
				{
					renderer.enabled = !renderer.enabled;
				}
			}
			foreach (Behaviour behaviour in this.behaviours)
			{
				if (behaviour)
				{
					behaviour.enabled = !behaviour.enabled;
				}
			}
			foreach (GameObject gameObject in this.gameObjects)
			{
				if (gameObject)
				{
					gameObject.SetActive(!gameObject.activeSelf);
				}
			}
		}

		// Token: 0x06002421 RID: 9249 RVA: 0x000F6FB0 File Offset: 0x000F51B0
		public void Toggle(bool enabled)
		{
			foreach (Renderer renderer in this.renderers)
			{
				if (renderer)
				{
					renderer.enabled = enabled;
				}
			}
			foreach (Behaviour behaviour in this.behaviours)
			{
				if (behaviour)
				{
					behaviour.enabled = enabled;
				}
			}
			foreach (GameObject gameObject in this.gameObjects)
			{
				if (gameObject)
				{
					gameObject.SetActive(enabled);
				}
			}
		}

		// Token: 0x06002422 RID: 9250 RVA: 0x000F70A4 File Offset: 0x000F52A4
		public void GetChildRenderers()
		{
			this.GetChildRenderers(base.gameObject);
		}

		// Token: 0x06002423 RID: 9251 RVA: 0x000F70B4 File Offset: 0x000F52B4
		protected void GetChildRenderers(GameObject gameObject)
		{
			this.renderers.Clear();
			foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
			{
				if (!renderer.hideFlags.HasFlag(HideFlags.DontSaveInEditor) && renderer.enabled)
				{
					this.renderers.Add(renderer);
				}
			}
		}

		// Token: 0x06002424 RID: 9252 RVA: 0x000F7114 File Offset: 0x000F5314
		protected void CopyGameObjectsToRenderers()
		{
			foreach (GameObject gameObject in this.gameObjects)
			{
				foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
				{
					if (!renderer.hideFlags.HasFlag(HideFlags.DontSaveInEditor) && renderer.enabled)
					{
						this.renderers.Add(renderer);
					}
				}
			}
		}

		// Token: 0x04002374 RID: 9076
		[Tooltip("List of renderers that you want to toggle on/off")]
		public List<Renderer> renderers = new List<Renderer>();

		// Token: 0x04002375 RID: 9077
		[Tooltip("List of behaviours that you want to toggle on/off")]
		public List<Behaviour> behaviours = new List<Behaviour>();

		// Token: 0x04002376 RID: 9078
		[Tooltip("List of gameObjects that you want to toggle on/off")]
		public List<GameObject> gameObjects = new List<GameObject>();

		// Token: 0x04002377 RID: 9079
		[Tooltip("Update the renderers/gameobjects to LOD1 when you export to android.")]
		public bool updateOnAndroidExport = true;
	}
}
