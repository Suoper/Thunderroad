using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200039A RID: 922
	public class UIWorldMap : MonoBehaviour
	{
		// Token: 0x06002BDC RID: 11228 RVA: 0x001276D4 File Offset: 0x001258D4
		private void OnValidate()
		{
			this.background = base.GetComponentInChildren<RawImage>();
			this.background.texture = this.texture;
			int i = 0;
			foreach (object obj in base.transform)
			{
				((Transform)obj).name = i++.ToString();
			}
		}

		// Token: 0x06002BDD RID: 11229 RVA: 0x00127758 File Offset: 0x00125958
		private void OnTransformChildrenChanged()
		{
			int i = 0;
			foreach (object obj in base.transform)
			{
				((Transform)obj).name = i++.ToString();
			}
		}

		// Token: 0x06002BDE RID: 11230 RVA: 0x001277C0 File Offset: 0x001259C0
		private void Awake()
		{
			this.background.enabled = false;
		}

		// Token: 0x06002BDF RID: 11231 RVA: 0x001277D0 File Offset: 0x001259D0
		public void SetUpOverlapLocation()
		{
			this.overlapLocation = new int[base.transform.childCount][];
			Vector3 lossyScale = base.transform.lossyScale;
			Vector3 lossyScale2 = base.transform.lossyScale;
			Rect prefabRect = (this.worldMapBoard.mapCardLocationPrefab.transform as RectTransform).rect;
			int index = 0;
			foreach (object obj in base.transform)
			{
				Transform child = (Transform)obj;
				List<int> overlapIndex = new List<int>();
				Rect currentRect = new Rect(prefabRect);
				currentRect.position += new Vector2(child.localPosition.x, child.localPosition.y);
				int linkIndex = 0;
				foreach (object obj2 in base.transform)
				{
					Transform childTolink = (Transform)obj2;
					if (linkIndex == index)
					{
						linkIndex++;
					}
					else
					{
						Rect tempRect = new Rect(prefabRect);
						tempRect.position += new Vector2(childTolink.localPosition.x, childTolink.localPosition.y);
						if (currentRect.Overlaps(tempRect))
						{
							overlapIndex.Add(linkIndex);
						}
						linkIndex++;
					}
				}
				this.overlapLocation[index] = overlapIndex.ToArray();
				index++;
			}
		}

		// Token: 0x06002BE0 RID: 11232 RVA: 0x00127988 File Offset: 0x00125B88
		public void Show(bool show)
		{
			if (show)
			{
				base.gameObject.SetActive(true);
				this.worldMapBoard.mapRenderer.material.SetTexture("_BaseMap", this.texture);
				this.worldMapBoard.worldMapLabel.text = (LocalizationManager.Instance.GetLocalizedString("Levels", this.label, false) ?? this.label);
				return;
			}
			base.gameObject.SetActive(false);
		}

		// Token: 0x06002BE1 RID: 11233 RVA: 0x00127A04 File Offset: 0x00125C04
		public int AddLocation(LevelInstance levelInstance, Dictionary<string, Sprite> loadedIconTexture, int randomNearest = 1)
		{
			int mapIndexLocation;
			Transform locationTransform = this.GetNearestFreeLocation(levelInstance.mapLocationIndex, randomNearest, out mapIndexLocation);
			if (locationTransform == null)
			{
				Debug.LogError("Unable to place map location for " + levelInstance.levelDataId + ", there is no valid locations.");
				return -1;
			}
			this.CreateMapLocation(locationTransform, levelInstance, loadedIconTexture);
			return mapIndexLocation;
		}

		// Token: 0x06002BE2 RID: 11234 RVA: 0x00127A50 File Offset: 0x00125C50
		public void RemoveLocation(LevelData levelData)
		{
			this.RemoveLocation(levelData.mapLocationIndex);
		}

		// Token: 0x06002BE3 RID: 11235 RVA: 0x00127A60 File Offset: 0x00125C60
		public void RemoveLocation(int mapLocationIndex)
		{
			for (int i = this.locations.Count - 1; i >= 0; i--)
			{
				if (this.locations[i].transform.parent.GetSiblingIndex() == mapLocationIndex)
				{
					UnityEngine.Object.Destroy(this.locations[i].gameObject);
					this.locations.RemoveAt(i);
				}
			}
		}

		// Token: 0x06002BE4 RID: 11236 RVA: 0x00127AC8 File Offset: 0x00125CC8
		protected Transform GetNearestFreeLocation(int index, int randomNearest, out int outIndex)
		{
			if (index < 0 || index >= base.transform.childCount)
			{
				Debug.LogError(string.Format("Map location index {0} is out of range in worldmap {1}", index, this.label));
				outIndex = -1;
				return null;
			}
			Transform targetTransform = base.transform.GetChild(index);
			if (targetTransform == null)
			{
				Debug.LogError(string.Format("Cannot find map location index {0} in worldmap {1}", index, this.label));
				outIndex = -1;
				return null;
			}
			List<Tuple<Transform, int>> validLocations = new List<Tuple<Transform, int>>();
			if (targetTransform.GetComponentInChildren<UIWorldMapLocation>() == null && this.IsAllOverlapLocationFree(index))
			{
				if (randomNearest == 1)
				{
					outIndex = index;
					return targetTransform;
				}
				validLocations.Add(new Tuple<Transform, int>(targetTransform, index));
			}
			Vector3 targetPosition = targetTransform.position;
			int childIndex = 0;
			foreach (object obj in base.transform)
			{
				Transform child = (Transform)obj;
				if (childIndex != index && child.GetComponentInChildren<UIWorldMapLocation>() == null && this.IsAllOverlapLocationFree(childIndex))
				{
					float childDistance = Vector3.Distance(child.position, targetPosition);
					int validLocationCount = validLocations.Count;
					int insertIndex;
					for (insertIndex = 0; insertIndex < validLocationCount; insertIndex++)
					{
						float tempDistance = Vector3.Distance(validLocations[insertIndex].Item1.position, targetPosition);
						if (childDistance < tempDistance)
						{
							break;
						}
					}
					if (insertIndex < validLocationCount)
					{
						validLocations.Insert(insertIndex, new Tuple<Transform, int>(child, childIndex));
					}
					else
					{
						validLocations.Add(new Tuple<Transform, int>(child, childIndex));
					}
				}
				childIndex++;
			}
			if (validLocations.Count == 0)
			{
				outIndex = -1;
				return null;
			}
			int indexChosen = UnityEngine.Random.Range(0, Math.Min(randomNearest + 1, validLocations.Count));
			Tuple<Transform, int> chosenLocation = validLocations[indexChosen];
			outIndex = chosenLocation.Item2;
			return chosenLocation.Item1;
		}

		/// <summary>
		/// Check if other location that may overlap is free (still need to check that the location itself is free)
		/// </summary>
		/// <param name="index">the index of the location </param>
		/// <returns>true if overlapping locations are free</returns>
		// Token: 0x06002BE5 RID: 11237 RVA: 0x00127CA0 File Offset: 0x00125EA0
		protected bool IsAllOverlapLocationFree(int index)
		{
			for (int i = 0; i < this.overlapLocation[index].Length; i++)
			{
				if (base.transform.GetChild(this.overlapLocation[index][i]).GetComponentInChildren<UIWorldMapLocation>() != null)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002BE6 RID: 11238 RVA: 0x00127CE8 File Offset: 0x00125EE8
		protected void CreateMapLocation(Transform locationTransform, LevelInstance levelInstance, Dictionary<string, Sprite> loadedIconTexture)
		{
			UIWorldMapLocation location = UnityEngine.Object.Instantiate<UIWorldMapLocation>(this.worldMapBoard.mapCardLocationPrefab, locationTransform);
			location.transform.SetPositionAndRotation(locationTransform.position, locationTransform.rotation);
			location.Setup(levelInstance, this.locationsToggleGroup, loadedIconTexture);
			Pointer.GetActive();
			location.button.onPointerClick.AddListener(delegate()
			{
				this.worldMapBoard.SetLocationSelected(levelInstance);
			});
			location.button.onPointerEnter.AddListener(delegate()
			{
				this.worldMapBoard.SetLocationHover(levelInstance);
			});
			location.button.onPointerExit.AddListener(delegate()
			{
				this.worldMapBoard.ResetLocationSelected();
			});
			this.locations.Add(location);
		}

		// Token: 0x04002956 RID: 10582
		public string label;

		// Token: 0x04002957 RID: 10583
		public Texture2D texture;

		// Token: 0x04002958 RID: 10584
		public RawImage background;

		// Token: 0x04002959 RID: 10585
		public ToggleGroup locationsToggleGroup;

		// Token: 0x0400295A RID: 10586
		public bool isDefault;

		// Token: 0x0400295B RID: 10587
		[NonSerialized]
		public List<UIWorldMapLocation> locations = new List<UIWorldMapLocation>();

		// Token: 0x0400295C RID: 10588
		[NonSerialized]
		public int worldMapHash;

		// Token: 0x0400295D RID: 10589
		[NonSerialized]
		public UIWorldMapBoard worldMapBoard;

		/// <summary>
		/// link location to other location that are too close
		/// </summary>
		// Token: 0x0400295E RID: 10590
		private int[][] overlapLocation;
	}
}
