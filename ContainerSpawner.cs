using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002BD RID: 701
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/ContainerSpawner.html")]
	public class ContainerSpawner : MonoBehaviour
	{
		// Token: 0x060021F6 RID: 8694 RVA: 0x000EA340 File Offset: 0x000E8540
		protected void Start()
		{
			if (this.spawnOnStart)
			{
				this.Spawn(null);
			}
		}

		/// <summary>
		/// Spawn this container and return all objects that were spawned.
		/// </summary>
		/// <param name="overrideSpawn">Allows each spawned item to be spawned in another way than the default.</param>
		// Token: 0x060021F7 RID: 8695 RVA: 0x000EA354 File Offset: 0x000E8554
		public void Spawn(Action<ItemContent, int> overrideSpawn = null)
		{
			this.hasSpawnedOnce = true;
			ContainerData containerData = Catalog.GetData<ContainerData>(this.containerId, true);
			if (containerData == null)
			{
				Debug.LogWarning("Container '" + this.containerId + "' not found on : " + base.gameObject.GetPathFromRoot());
				return;
			}
			List<ItemContent> contents = containerData.GetContents().GetContentsOfType(true, Array.Empty<Func<ItemContent, bool>>());
			if (contents.IsNullOrEmpty())
			{
				Debug.LogWarning("Container '" + this.containerId + "' contains no data.");
				return;
			}
			if (this.spawnAll && contents.Count > 1)
			{
				ItemContent[] massObjects = this.SelectRandomObjects(contents, this.allowDuplicates);
				ValueTuple<int, bool>[] pointTracker = new ValueTuple<int, bool>[this.spawnPoints.Count];
				for (int i = 0; i < this.spawnPoints.Count; i++)
				{
					pointTracker[i] = new ValueTuple<int, bool>(i, true);
				}
				List<ValueTuple<int, bool>> pointCache = new List<ValueTuple<int, bool>>();
				for (int j = 0; j < massObjects.Length; j++)
				{
					ValueTuple<int, bool> selectedPoint;
					if (!this.GetRandomSpawnPoint(pointCache, ref pointTracker, out selectedPoint))
					{
						return;
					}
					if (overrideSpawn != null)
					{
						overrideSpawn(massObjects[j], selectedPoint.Item1);
					}
					else
					{
						massObjects[j].Spawn(new Action<Item, int>(this.OnItemSpawn), selectedPoint.Item1, this.spawnOwner, true);
					}
				}
				return;
			}
			if (overrideSpawn != null)
			{
				overrideSpawn(contents[UnityEngine.Random.Range(0, contents.Count)], UnityEngine.Random.Range(0, this.spawnPoints.Count));
				return;
			}
			contents[UnityEngine.Random.Range(0, contents.Count)].Spawn(new Action<Item, int>(this.OnItemSpawn), UnityEngine.Random.Range(0, this.spawnPoints.Count), this.spawnOwner, true);
		}

		/// <summary>
		/// Spawn this container and return all objects that were spawned.
		/// </summary>
		/// <param name="callback">callback with item and point index.</param>
		// Token: 0x060021F8 RID: 8696 RVA: 0x000EA508 File Offset: 0x000E8708
		public void Spawn(Action<Item, int> callback)
		{
			Action<Item, int> spawnCallback = new Action<Item, int>(this.OnItemSpawn);
			if (callback != null)
			{
				spawnCallback = (Action<Item, int>)Delegate.Combine(spawnCallback, callback);
			}
			this.hasSpawnedOnce = true;
			ContainerData containerData = Catalog.GetData<ContainerData>(this.containerId, true);
			if (containerData == null)
			{
				Debug.LogWarning("Container '" + this.containerId + "' not found");
				return;
			}
			List<ItemContent> contents = containerData.GetContents().GetContentsOfType(true, Array.Empty<Func<ItemContent, bool>>());
			if (contents.IsNullOrEmpty())
			{
				Debug.LogWarning("Container '" + this.containerId + "' contains no data.");
				return;
			}
			if (this.spawnAll && contents.Count > 1)
			{
				ItemContent[] massObjects = this.SelectRandomObjects(contents, this.allowDuplicates);
				ValueTuple<int, bool>[] pointTracker = new ValueTuple<int, bool>[this.spawnPoints.Count];
				for (int i = 0; i < this.spawnPoints.Count; i++)
				{
					pointTracker[i] = new ValueTuple<int, bool>(i, true);
				}
				List<ValueTuple<int, bool>> pointCache = new List<ValueTuple<int, bool>>();
				for (int j = 0; j < massObjects.Length; j++)
				{
					ValueTuple<int, bool> selectedPoint;
					if (!this.GetRandomSpawnPoint(pointCache, ref pointTracker, out selectedPoint))
					{
						return;
					}
					massObjects[j].Spawn(spawnCallback, selectedPoint.Item1, this.spawnOwner, true);
				}
				return;
			}
			contents[UnityEngine.Random.Range(0, contents.Count)].Spawn(spawnCallback, UnityEngine.Random.Range(0, this.spawnPoints.Count), this.spawnOwner, true);
		}

		/// <summary>
		/// Get a random spawn point that isn't taken.
		/// </summary>
		// Token: 0x060021F9 RID: 8697 RVA: 0x000EA674 File Offset: 0x000E8874
		private bool GetRandomSpawnPoint([TupleElementNames(new string[]
		{
			"index",
			"free"
		})] List<ValueTuple<int, bool>> tmp, [TupleElementNames(new string[]
		{
			"index",
			"free"
		})] ref ValueTuple<int, bool>[] points, [TupleElementNames(new string[]
		{
			"index",
			"free"
		})] out ValueTuple<int, bool> result)
		{
			tmp.Clear();
			for (int i = 0; i < points.Length; i++)
			{
				if (points[i].Item2)
				{
					tmp.Add(points[i]);
				}
			}
			ValueTuple<int, bool> point = (tmp.Count == 0) ? new ValueTuple<int, bool>(-1, false) : tmp[UnityEngine.Random.Range(0, tmp.Count)];
			for (int j = 0; j < points.Length; j++)
			{
				if (points[j].Item1 == point.Item1)
				{
					points[j].Item2 = false;
					break;
				}
			}
			result = point;
			return point.Item1 != -1;
		}

		/// <summary>
		/// Returns an array of content filled with randomly selected objects.
		///
		/// If recursive objects aren't allowed the list may be shorter than the capacity if the collection is less than the spawn points.
		/// </summary>
		/// <param name="allowDuplicates">Can the same object spawn multiple times?</param>
		// Token: 0x060021FA RID: 8698 RVA: 0x000EA720 File Offset: 0x000E8920
		private ItemContent[] SelectRandomObjects(List<ItemContent> masterList, bool allowDuplicates = true)
		{
			ItemContent[] content = new ItemContent[masterList.Count];
			int contentSize = 0;
			for (int i = 0; i < content.Length; i++)
			{
				ItemContent selected = allowDuplicates ? masterList[UnityEngine.Random.Range(0, masterList.Count)] : this.GetNoneRecursive<ItemContent>(masterList, content, masterList[UnityEngine.Random.Range(0, masterList.Count)]);
				if (selected != null)
				{
					content[i] = selected;
					contentSize++;
				}
			}
			if (contentSize != content.Length)
			{
				Array.Resize<ItemContent>(ref content, contentSize);
			}
			return content;
		}

		/// <summary>
		/// Tries to return an object that is unique to the input list.
		/// </summary>
		/// <param name="masterList">All collections.</param>
		/// <param name="content">Current collections.</param>
		/// <param name="selected">Currently Selected Collection.</param>
		// Token: 0x060021FB RID: 8699 RVA: 0x000EA798 File Offset: 0x000E8998
		private T GetNoneRecursive<T>(List<T> masterList, T[] content, T selected) where T : new()
		{
			bool tryAgain = false;
			int attempts = 3;
			while (attempts > 0)
			{
				foreach (T data in content)
				{
					if (EqualityComparer<T>.Default.Equals(selected, data))
					{
						tryAgain = true;
						attempts--;
						break;
					}
				}
				if (!tryAgain)
				{
					break;
				}
				if (attempts <= 0)
				{
					return default(T);
				}
				selected = masterList[UnityEngine.Random.Range(0, masterList.Count)];
				tryAgain = false;
			}
			return selected;
		}

		/// <summary>
		/// Invoked when an item has been spawned.
		/// </summary>
		// Token: 0x060021FC RID: 8700 RVA: 0x000EA808 File Offset: 0x000E8A08
		private void OnItemSpawn(Item item, int spawnIndex)
		{
			if (item == null)
			{
				return;
			}
			item.DisallowDespawn = this.disallowDespawn;
			Transform spawnPoint = this.spawnPoints[spawnIndex];
			Holder holder = spawnPoint.GetComponent<Holder>();
			if (holder != null)
			{
				holder.Snap(item, false);
				return;
			}
			if (this.holderIsPivot)
			{
				Vector3 pivot = spawnPoint.position + (spawnPoint.position - item.spawnPoint.position);
				item.transform.position = pivot;
			}
			else
			{
				item.transform.position = spawnPoint.position;
			}
			item.transform.rotation = spawnPoint.rotation;
		}

		/// <summary>
		/// If the spawner isn't registering the files this will quickly allow the artist to do a one-click fix.
		/// </summary>
		// Token: 0x060021FD RID: 8701 RVA: 0x000EA8A5 File Offset: 0x000E8AA5
		private void ReloadJson()
		{
			Catalog.EditorLoadAllJson(true, false, true);
		}

		// Token: 0x060021FE RID: 8702 RVA: 0x000EA8AF File Offset: 0x000E8AAF
		private List<ValueDropdownItem<string>> GetAllContainerId()
		{
			List<ValueDropdownItem<string>> list = new List<ValueDropdownItem<string>>();
			list.AddRange(Catalog.GetDropdownAllID<ContainerData>("None"));
			list.AddRange(Catalog.GetDropdownAllID<LootTableBase>("None"));
			return list;
		}

		// Token: 0x040020DE RID: 8414
		public string containerId;

		// Token: 0x040020DF RID: 8415
		public bool pooled;

		// Token: 0x040020E0 RID: 8416
		public bool spawnAll;

		// Token: 0x040020E1 RID: 8417
		public bool allowDuplicates = true;

		// Token: 0x040020E2 RID: 8418
		public bool spawnOnStart = true;

		// Token: 0x040020E3 RID: 8419
		public bool disallowDespawn;

		// Token: 0x040020E4 RID: 8420
		public bool holderIsPivot;

		// Token: 0x040020E5 RID: 8421
		public List<Transform> spawnPoints = new List<Transform>();

		// Token: 0x040020E6 RID: 8422
		public Item.Owner spawnOwner;

		// Token: 0x040020E7 RID: 8423
		public bool hasSpawnedOnce;
	}
}
