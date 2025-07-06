using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000334 RID: 820
	public static class Common
	{
		/// <summary>
		/// File size references.
		/// </summary>
		// Token: 0x1700024B RID: 587
		// (get) Token: 0x060025E5 RID: 9701 RVA: 0x00104959 File Offset: 0x00102B59
		private static string[] SizeReferences { get; } = new string[]
		{
			"B",
			"KB",
			"MB",
			"GB",
			"TB",
			"PB",
			"EB"
		};

		// Token: 0x1700024C RID: 588
		// (get) Token: 0x060025E6 RID: 9702 RVA: 0x00104960 File Offset: 0x00102B60
		// (set) Token: 0x060025E7 RID: 9703 RVA: 0x00104980 File Offset: 0x00102B80
		public static int lightProbeVolumeLayer
		{
			get
			{
				if (Common._lightProbeVolumeLayer <= 0)
				{
					return Common._lightProbeVolumeLayer = LayerMask.NameToLayer("LightProbeVolume");
				}
				return Common._lightProbeVolumeLayer;
			}
			private set
			{
				Common._lightProbeVolumeLayer = value;
			}
		}

		// Token: 0x1700024D RID: 589
		// (get) Token: 0x060025E8 RID: 9704 RVA: 0x00104988 File Offset: 0x00102B88
		// (set) Token: 0x060025E9 RID: 9705 RVA: 0x001049A8 File Offset: 0x00102BA8
		public static int zoneLayer
		{
			get
			{
				if (Common._zoneLayer <= 0)
				{
					return Common._zoneLayer = LayerMask.NameToLayer("Zone");
				}
				return Common._zoneLayer;
			}
			private set
			{
				Common._zoneLayer = value;
			}
		}

		/// <summary>
		/// Play an audio container at the target point, with mixer support and db.
		/// </summary>
		// Token: 0x060025EA RID: 9706 RVA: 0x001049B0 File Offset: 0x00102BB0
		public static void PlayClipAtPoint(this AudioContainer container, Vector3 point, float volumeDB, AudioMixerName mixer = AudioMixerName.Master)
		{
			container.PickAudioClip().PlayClipAtPoint(point, volumeDB, mixer);
		}

		/// <summary>
		/// Play an audio clip at the target point, with mixer support and db.
		/// </summary>
		// Token: 0x060025EB RID: 9707 RVA: 0x001049C0 File Offset: 0x00102BC0
		public static void PlayClipAtPoint(this AudioClip clip, Vector3 point, float volumeDB, AudioMixerName mixer = AudioMixerName.Master)
		{
			AudioMixerGroup mixerGroup = ThunderRoadSettings.GetAudioMixerGroup(mixer);
			AudioSource source = new GameObject("AudioContainer-PlayAtPoint").AddComponent<AudioSource>();
			source.transform.position = point;
			source.outputAudioMixerGroup = mixerGroup;
			source.spatialBlend = 0f;
			source.volume = EffectAudio.DecibelToLinear(volumeDB);
			source.clip = clip;
			source.Play();
			UnityEngine.Object.Destroy(source.gameObject, source.clip.length);
		}

		// Token: 0x060025EC RID: 9708 RVA: 0x00104A34 File Offset: 0x00102C34
		public static bool IsDirectoryWritable(string dirPath)
		{
			bool result;
			try
			{
				if (!Directory.Exists(dirPath))
				{
					result = false;
				}
				else
				{
					using (File.Create(Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
					{
					}
					result = true;
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		/// <summary>
		/// Is the target gameobject visible to the input camera?
		/// </summary>
		// Token: 0x060025ED RID: 9709 RVA: 0x00104A98 File Offset: 0x00102C98
		public static bool IsVisibleToCamera(this Transform obj, Camera viewer)
		{
			if (obj == null || viewer == null)
			{
				return false;
			}
			GeometryUtility.CalculateFrustumPlanes(viewer, Common.planes);
			return GeometryUtility.TestPlanesAABB(Common.planes, new Bounds(obj.position, obj.localScale));
		}

		/// <summary>
		/// Wrap an enumerator in a try/catch block to catch exceptions they may throw.
		/// </summary>
		// Token: 0x060025EE RID: 9710 RVA: 0x00104AD4 File Offset: 0x00102CD4
		public static IEnumerator WrapSafely(this IEnumerator enumerator, Action<Exception> errorThrown = null)
		{
			for (;;)
			{
				object current;
				try
				{
					if (!enumerator.MoveNext())
					{
						yield break;
					}
					current = enumerator.Current;
				}
				catch (Exception error)
				{
					if (errorThrown != null)
					{
						errorThrown(error);
					}
					Debug.LogError(error);
					yield break;
				}
				yield return current;
			}
			yield break;
		}

		/// <summary>
		/// Try get the child directory or return the original path.
		/// </summary>
		// Token: 0x060025EF RID: 9711 RVA: 0x00104AEC File Offset: 0x00102CEC
		public static string TryGetChildDirectory(this string directory)
		{
			if (!Directory.Exists(directory))
			{
				return directory;
			}
			string[] children = Directory.GetDirectories(directory);
			if (!children.IsNullOrEmpty())
			{
				return children[0];
			}
			return directory;
		}

		/// <summary>
		/// Get the total directory size and return it as a formatted string.
		/// </summary>
		// Token: 0x060025F0 RID: 9712 RVA: 0x00104B18 File Offset: 0x00102D18
		public static string FormatSizeFromDirectory(this string directory, string format = "0")
		{
			if (!Directory.Exists(directory))
			{
				return string.Empty;
			}
			FileInfo[] files = new DirectoryInfo(directory).GetFiles("*.*", SearchOption.AllDirectories);
			long size = 0L;
			for (int i = 0; i < files.Length; i++)
			{
				size += files[i].Length;
			}
			return size.FormatSizeFromBytes(format);
		}

		/// <summary>
		/// Convert the input bytes to a readable size..
		/// </summary>
		// Token: 0x060025F1 RID: 9713 RVA: 0x00104B68 File Offset: 0x00102D68
		public static string FormatSizeFromBytes(this int byteCount)
		{
			return ((long)byteCount).FormatSizeFromBytes("0");
		}

		/// <summary>
		/// Convert the input bytes to a readable size..
		/// </summary>
		// Token: 0x060025F2 RID: 9714 RVA: 0x00104B78 File Offset: 0x00102D78
		public static string FormatSizeFromBytes(this long byteCount, string format = "0")
		{
			if (byteCount == 0L)
			{
				return "0" + Common.SizeReferences[0];
			}
			long num2 = Math.Abs(byteCount);
			int place = Convert.ToInt32(Math.Floor(Math.Log((double)num2, 1024.0)));
			double num = Math.Round((double)num2 / Math.Pow(1024.0, (double)place), 2);
			return Math.Abs((double)Math.Sign(byteCount) * num).ToString(format) + Common.SizeReferences[place];
		}

		/// <summary>
		/// Get the total free space left on the main drive.
		/// </summary>
		/// <returns>Available, out is total drive size</returns>
		// Token: 0x060025F3 RID: 9715 RVA: 0x00104BF8 File Offset: 0x00102DF8
		public static long GetTotalFreeSpace(out long total)
		{
			if (Application.platform != RuntimePlatform.Android)
			{
				foreach (DriveInfo drive in DriveInfo.GetDrives())
				{
					if (drive.IsReady && Application.dataPath.StartsWith(drive.Name.Replace("\\", "/")))
					{
						total = drive.TotalSize;
						return Math.Abs(drive.TotalSize - drive.AvailableFreeSpace);
					}
				}
				total = 0L;
				return -1L;
			}
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.os.StatFs", new object[]
			{
				Application.persistentDataPath
			});
			long blockSize = androidJavaObject.Call<long>("getBlockSizeLong", Array.Empty<object>());
			long totalBlocks = androidJavaObject.Call<long>("getBlockCountLong", Array.Empty<object>());
			long num = androidJavaObject.Call<long>("getAvailableBlocksLong", Array.Empty<object>());
			total = totalBlocks * blockSize;
			return num * blockSize;
		}

		/// <summary>
		/// Create a sprite from the target texture.
		/// </summary>
		// Token: 0x060025F4 RID: 9716 RVA: 0x00104CC9 File Offset: 0x00102EC9
		public static Sprite CreateSprite(this Texture2D texture)
		{
			return Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height), Vector2.one * 0.5f, 1f, 0U, SpriteMeshType.FullRect);
		}

		/// <summary>
		/// Load a Texture2D from raw bytes.
		/// </summary>
		// Token: 0x060025F5 RID: 9717 RVA: 0x00104D04 File Offset: 0x00102F04
		public static Texture2D LoadTexture(this byte[] rawData)
		{
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.LoadImage(rawData);
			texture2D.Apply();
			return texture2D;
		}

		// Token: 0x060025F6 RID: 9718 RVA: 0x00104D1C File Offset: 0x00102F1C
		public static bool ActiveInPrefabHierarchy(this GameObject gameObject)
		{
			GameObject go = gameObject;
			while (go.activeSelf)
			{
				if (!go.transform.parent)
				{
					return true;
				}
				go = go.transform.parent.gameObject;
			}
			return false;
		}

		// Token: 0x060025F7 RID: 9719 RVA: 0x00104D5C File Offset: 0x00102F5C
		public static void CacheLayers()
		{
			Common.layers = new int[Enum.GetValues(typeof(LayerName)).Length];
			foreach (object obj in Enum.GetValues(typeof(LayerName)))
			{
				LayerName layerName = (LayerName)obj;
				if (layerName != LayerName.None)
				{
					Common.layers[(int)layerName] = LayerMask.NameToLayer(layerName.ToString());
				}
			}
		}

		// Token: 0x060025F8 RID: 9720 RVA: 0x00104DF4 File Offset: 0x00102FF4
		public static LayerName GetLayerName(int layer)
		{
			LayerName name;
			if (!Enum.TryParse<LayerName>(LayerMask.LayerToName(layer), out name))
			{
				return LayerName.None;
			}
			return name;
		}

		// Token: 0x060025F9 RID: 9721 RVA: 0x00104E13 File Offset: 0x00103013
		public static int GetLayer(LayerName layerName)
		{
			if (Common.layers == null)
			{
				return LayerMask.NameToLayer(layerName.ToString());
			}
			return Common.layers[(int)layerName];
		}

		// Token: 0x060025FA RID: 9722 RVA: 0x00104E38 File Offset: 0x00103038
		public static int MakeLayerMask(params LayerName[] layers)
		{
			int mask = 0;
			for (int i = 0; i < layers.Length; i++)
			{
				mask |= 1 << Common.GetLayer(layers[i]);
			}
			return mask;
		}

		// Token: 0x060025FB RID: 9723 RVA: 0x00104E66 File Offset: 0x00103066
		public static bool Contains(this LayerMask mask, int layer)
		{
			return mask == (mask | 1 << layer);
		}

		// Token: 0x060025FC RID: 9724 RVA: 0x00104E7D File Offset: 0x0010307D
		public static int GetMaskAddLayer(int mask, int layer)
		{
			return mask | ~(1 << layer);
		}

		// Token: 0x060025FD RID: 9725 RVA: 0x00104E88 File Offset: 0x00103088
		public static int GetMaskRemoveLayer(int mask, int layer)
		{
			return mask & ~(1 << layer);
		}

		// Token: 0x060025FE RID: 9726 RVA: 0x00104E94 File Offset: 0x00103094
		public static QualityLevel GetQualityLevel(bool ignoreCache = false)
		{
			if (Common.qualityLevelCached && !ignoreCache)
			{
				return Common.currentQualityLevel;
			}
			QualityLevel platform;
			if (Enum.TryParse<QualityLevel>(QualitySettings.names[QualitySettings.GetQualityLevel()], out platform))
			{
				Common.qualityLevelCached = Application.isPlaying;
				Common.currentQualityLevel = platform;
				return platform;
			}
			Debug.LogError("Quality Settings names don't match platform enum!");
			return QualityLevel.Windows;
		}

		// Token: 0x1700024E RID: 590
		// (get) Token: 0x060025FF RID: 9727 RVA: 0x00104EE2 File Offset: 0x001030E2
		public static bool IsAndroid
		{
			get
			{
				return Common.GetQualityLevel(false) == QualityLevel.Android;
			}
		}

		// Token: 0x1700024F RID: 591
		// (get) Token: 0x06002600 RID: 9728 RVA: 0x00104EED File Offset: 0x001030ED
		public static bool IsWindows
		{
			get
			{
				return Common.GetQualityLevel(false) == QualityLevel.Windows;
			}
		}

		// Token: 0x06002601 RID: 9729 RVA: 0x00104EF8 File Offset: 0x001030F8
		public static int GetRandomWeightedIndex(float[] weights)
		{
			if (weights == null || weights.Length == 0)
			{
				return -1;
			}
			float t = 0f;
			for (int i = 0; i < weights.Length; i++)
			{
				float w = weights[i];
				if (float.IsPositiveInfinity(w))
				{
					return i;
				}
				if (w >= 0f && !float.IsNaN(w))
				{
					t += weights[i];
				}
			}
			float r = UnityEngine.Random.value;
			float s = 0f;
			for (int i = 0; i < weights.Length; i++)
			{
				float w = weights[i];
				if (!float.IsNaN(w) && w > 0f)
				{
					s += w / t;
					if (s >= r)
					{
						return i;
					}
				}
			}
			return -1;
		}

		// Token: 0x06002602 RID: 9730 RVA: 0x00104F88 File Offset: 0x00103188
		public static void DrawText(GUISkin guiSkin, string text, Vector3 position, Color? color = null, int fontSize = 0, float yOffset = 0f)
		{
			GUISkin skin = GUI.skin;
			if (guiSkin == null)
			{
				Debug.LogWarning("editor warning: guiSkin parameter is null");
			}
			else
			{
				GUI.skin = guiSkin;
			}
			GUIContent textContent = new GUIContent(text);
			GUIStyle style = (guiSkin != null) ? new GUIStyle(guiSkin.GetStyle("Label")) : new GUIStyle();
			if (color != null)
			{
				style.normal.textColor = color.Value;
			}
			if (fontSize > 0)
			{
				style.fontSize = fontSize;
			}
			Vector2 textSize = style.CalcSize(textContent);
			Vector3 screenPoint = Camera.current.WorldToScreenPoint(position);
			if (screenPoint.z > 0f)
			{
				Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f + yOffset, screenPoint.z));
			}
			GUI.skin = skin;
		}

		// Token: 0x06002603 RID: 9731 RVA: 0x0010506C File Offset: 0x0010326C
		public static T GetClosest<T>(List<T> components, Vector3 position, bool prioritizeShortestPath) where T : Component
		{
			if (prioritizeShortestPath)
			{
				float shortestPathLength = float.PositiveInfinity;
				Component shortestPathComponent = null;
				foreach (T t in components)
				{
					Component component = t;
					Common.navMeshPath.ClearCorners();
					if (NavMesh.CalculatePath(position, component.transform.position, -1, Common.navMeshPath))
					{
						float pathLength = Common.GetPathLength(Common.navMeshPath);
						if (pathLength < shortestPathLength)
						{
							shortestPathLength = pathLength;
							shortestPathComponent = component;
						}
					}
				}
				if (shortestPathComponent)
				{
					return shortestPathComponent as T;
				}
			}
			float closestDistanceSqr = float.PositiveInfinity;
			Component closestComponent = null;
			foreach (T t2 in components)
			{
				Component component2 = t2;
				float dSqrToTarget = (component2.transform.position - position).sqrMagnitude;
				if (dSqrToTarget < closestDistanceSqr)
				{
					closestDistanceSqr = dSqrToTarget;
					closestComponent = component2;
				}
			}
			return closestComponent as T;
		}

		// Token: 0x06002604 RID: 9732 RVA: 0x00105190 File Offset: 0x00103390
		public static Transform GetClosest(List<Transform> transforms, Vector3 position, bool prioritizeShortestPath)
		{
			if (prioritizeShortestPath)
			{
				float shortestPathLength = float.PositiveInfinity;
				Transform shortestPathTransform = null;
				foreach (Transform transform in transforms)
				{
					Common.navMeshPath.ClearCorners();
					if (NavMesh.CalculatePath(position, transform.transform.position, -1, Common.navMeshPath))
					{
						float pathLength = Common.GetPathLength(Common.navMeshPath);
						if (pathLength < shortestPathLength)
						{
							shortestPathLength = pathLength;
							shortestPathTransform = transform;
						}
					}
				}
				if (shortestPathTransform)
				{
					return shortestPathTransform;
				}
			}
			float closestDistanceSqr = float.PositiveInfinity;
			Transform closestTransform = null;
			foreach (Transform transform2 in transforms)
			{
				float dSqrToTarget = (transform2.transform.position - position).sqrMagnitude;
				if (dSqrToTarget < closestDistanceSqr)
				{
					closestDistanceSqr = dSqrToTarget;
					closestTransform = transform2;
				}
			}
			return closestTransform;
		}

		// Token: 0x06002605 RID: 9733 RVA: 0x00105294 File Offset: 0x00103494
		public static float GetPathLength(NavMeshPath path)
		{
			float lng = 0f;
			Vector3[] corners = path.corners;
			if (path.status != NavMeshPathStatus.PathInvalid && corners.Length > 1)
			{
				for (int i = 1; i < corners.Length; i++)
				{
					lng += Vector3.Distance(corners[i - 1], corners[i]);
				}
			}
			return lng;
		}

		// Token: 0x06002606 RID: 9734 RVA: 0x001052E4 File Offset: 0x001034E4
		public static Component CloneComponent(Component source, GameObject destination, bool copyProperties = false)
		{
			Component destinationComponent = destination.AddComponent(source.GetType());
			if (copyProperties)
			{
				foreach (PropertyInfo property in source.GetType().GetProperties())
				{
					if (property.CanWrite)
					{
						property.SetValue(destinationComponent, property.GetValue(source, null), null);
					}
				}
			}
			foreach (FieldInfo field in source.GetType().GetFields())
			{
				field.SetValue(destinationComponent, field.GetValue(source));
			}
			return destinationComponent;
		}

		// Token: 0x06002607 RID: 9735 RVA: 0x0010536C File Offset: 0x0010356C
		public static string GetPathFromRoot(this GameObject gameObject)
		{
			string path = "/" + gameObject.name;
			while (gameObject.transform.parent != null)
			{
				gameObject = gameObject.transform.parent.gameObject;
				path = "/" + gameObject.name + path;
			}
			return path;
		}

		// Token: 0x06002608 RID: 9736 RVA: 0x001053C4 File Offset: 0x001035C4
		public static Vector3 GetRowPosition(Transform transform, int index, float rowCount, float rowSpace)
		{
			return transform.position + transform.right * (rowSpace * ((float)index % rowCount)) + transform.forward * (rowSpace * (float)Mathf.FloorToInt((float)index / rowCount));
		}

		// Token: 0x06002609 RID: 9737 RVA: 0x00105400 File Offset: 0x00103600
		public static bool InPrefabScene(this Component component)
		{
			return string.IsNullOrEmpty(component.gameObject.scene.path);
		}

		// Token: 0x0600260A RID: 9738 RVA: 0x0010542C File Offset: 0x0010362C
		public static int GetIndexByName(this Dropdown dropDown, string name)
		{
			if (dropDown == null)
			{
				return -1;
			}
			if (string.IsNullOrEmpty(name))
			{
				return -1;
			}
			List<Dropdown.OptionData> list = dropDown.options;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].text.Equals(name))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600260B RID: 9739 RVA: 0x00105480 File Offset: 0x00103680
		public static void SetLayerRecursively(this GameObject obj, int layer)
		{
			obj.layer = layer;
			foreach (object obj2 in obj.transform)
			{
				((Transform)obj2).gameObject.SetLayerRecursively(layer);
			}
		}

		// Token: 0x0600260C RID: 9740 RVA: 0x001054E4 File Offset: 0x001036E4
		public static string GetStringBetween(this string text, string start, string end)
		{
			int pFrom = text.IndexOf(start) + start.Length;
			int pTo = text.LastIndexOf(end);
			return text.Substring(pFrom, pTo - pFrom);
		}

		// Token: 0x0600260D RID: 9741 RVA: 0x00105512 File Offset: 0x00103712
		public static void SetParentOrigin(this Transform transform, Transform parent)
		{
			transform.SetParent(parent, false);
			transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			transform.localScale = Vector3.one;
		}

		// Token: 0x0600260E RID: 9742 RVA: 0x00105538 File Offset: 0x00103738
		public static void SetParentOrigin(this Transform transform, Transform parent, Vector3? localPosition = null, Quaternion? localRotation = null, Vector3? localScale = null)
		{
			transform.parent = parent;
			transform.localPosition = ((localPosition == null) ? Vector3.zero : localPosition.Value);
			transform.localRotation = ((localRotation == null) ? Quaternion.identity : localRotation.Value);
			transform.localScale = ((localScale == null) ? Vector3.one : localScale.Value);
		}

		// Token: 0x0600260F RID: 9743 RVA: 0x001055A3 File Offset: 0x001037A3
		public static void MoveAlign(this Transform transform, Transform child, Transform target, Transform parent = null)
		{
			transform.MoveAlign(child, target.position, target.rotation, parent);
		}

		// Token: 0x06002610 RID: 9744 RVA: 0x001055BC File Offset: 0x001037BC
		public static void MoveAlign(this Transform transform, Transform child, Vector3 targetPosition, Quaternion targetRotation, Transform parent = null)
		{
			Quaternion deltaRotation = targetRotation * Quaternion.Inverse(child.rotation);
			transform.transform.rotation = deltaRotation * transform.transform.rotation;
			Vector3 displacement = targetPosition - child.position;
			transform.transform.position += displacement;
			if (parent)
			{
				transform.transform.SetParent(parent, true);
			}
		}

		// Token: 0x06002611 RID: 9745 RVA: 0x00105632 File Offset: 0x00103832
		public static void RotateAroundPivot(this Transform transform, Vector3 pivot, Quaternion rotation)
		{
			transform.position = transform.position.RotateAroundPivot(pivot, rotation);
			transform.rotation = rotation * transform.rotation;
		}

		// Token: 0x06002612 RID: 9746 RVA: 0x00105659 File Offset: 0x00103859
		public static Vector3 RotateAroundPivot(this Vector3 start, Vector3 pivot, Quaternion rotation)
		{
			return rotation * (start - pivot) + pivot;
		}

		// Token: 0x06002613 RID: 9747 RVA: 0x00105670 File Offset: 0x00103870
		public static void LocalQuaternionRotation(this Transform transform, Quaternion change, Transform parent = null)
		{
			if (parent == null)
			{
				parent = transform.parent;
			}
			Quaternion rotation = change * parent.InverseTransformRotation(transform.rotation);
			transform.rotation = parent.TransformRotation(rotation);
		}

		// Token: 0x06002614 RID: 9748 RVA: 0x001056AE File Offset: 0x001038AE
		public static void LocalEulerRotation(this Transform transform, Vector3 change, Transform parent = null)
		{
			transform.LocalQuaternionRotation(Quaternion.Euler(change), parent);
		}

		// Token: 0x06002615 RID: 9749 RVA: 0x001056C0 File Offset: 0x001038C0
		public static Vector3 InverseTransformPoint(Vector3 transforPos, Quaternion transformRotation, Vector3 transformScale, Vector3 pos)
		{
			return Matrix4x4.TRS(transforPos, transformRotation, transformScale).inverse.MultiplyPoint3x4(pos);
		}

		// Token: 0x06002616 RID: 9750 RVA: 0x001056E6 File Offset: 0x001038E6
		public static void SetPositionLocalPseudoParent(this Transform transform, Transform pseudoParent, Vector3 localPosition)
		{
			transform.position = pseudoParent.TransformPoint(localPosition);
		}

		// Token: 0x06002617 RID: 9751 RVA: 0x001056F5 File Offset: 0x001038F5
		public static void SetRotationLocalPseudoParent(this Transform transform, Transform pseudoParent, Quaternion localRotation)
		{
			transform.rotation = pseudoParent.TransformRotation(localRotation);
		}

		// Token: 0x06002618 RID: 9752 RVA: 0x00105704 File Offset: 0x00103904
		public static void SetEulersLocalPseudoParent(this Transform transform, Transform pseudoParent, Vector3 localEulers)
		{
			transform.eulerAngles = pseudoParent.TransformRotation(Quaternion.Euler(localEulers)).eulerAngles;
		}

		/// <summary>
		/// Transforms rotation from world space to local space.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		// Token: 0x06002619 RID: 9753 RVA: 0x0010572B File Offset: 0x0010392B
		public static Quaternion InverseTransformRotation(this Transform transform, Quaternion rotation)
		{
			return Quaternion.Inverse(transform.rotation) * rotation;
		}

		/// <summary>
		/// Transforms rotation from local space to world space.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="localRotation"></param>
		/// <returns></returns>
		// Token: 0x0600261A RID: 9754 RVA: 0x0010573E File Offset: 0x0010393E
		public static Quaternion TransformRotation(this Transform transform, Quaternion localRotation)
		{
			return transform.rotation * localRotation;
		}

		// Token: 0x0600261B RID: 9755 RVA: 0x0010574C File Offset: 0x0010394C
		public static void MirrorChilds(this Transform transform, Vector3 mirrorAxis)
		{
			foreach (Transform child in transform.GetComponentsInChildren<Transform>())
			{
				if (!(child == transform))
				{
					Transform orgParent = child.parent;
					Transform mirror = new GameObject("Mirror").transform;
					mirror.SetParent(orgParent, false);
					mirror.localPosition = Vector3.zero;
					mirror.localRotation = Quaternion.identity;
					mirror.localScale = Vector3.one;
					child.SetParent(mirror, true);
					mirror.localScale = Vector3.Scale(mirrorAxis, transform.localScale);
					child.SetParent(orgParent, true);
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(mirror.gameObject);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(mirror.gameObject);
					}
					child.localScale = Vector3.Scale(mirrorAxis, transform.localScale);
				}
			}
		}

		// Token: 0x0600261C RID: 9756 RVA: 0x00105820 File Offset: 0x00103A20
		public static void MirrorRelativeToParent(this Transform transform, Vector3 mirrorAxis)
		{
			Transform root = new GameObject("MirrorRoot").transform;
			root.SetParent(transform.parent);
			root.localPosition = Vector3.zero;
			root.localRotation = Quaternion.identity;
			root.localScale = Vector3.one;
			transform.SetParent(root, true);
			root.MirrorChilds(mirrorAxis);
			transform.SetParent(root.parent, true);
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(root.gameObject);
				return;
			}
			UnityEngine.Object.DestroyImmediate(root.gameObject);
		}

		// Token: 0x0600261D RID: 9757 RVA: 0x001058A4 File Offset: 0x00103AA4
		public static bool TryGetHigherLodMeshFilter(MeshFilter lod0MeshFilter, LOD[] lods, out MeshFilter meshFilter)
		{
			for (int lodIndex = 0; lodIndex < lods.Length; lodIndex++)
			{
				for (int i = 0; i < lods[lodIndex].renderers.Length; i++)
				{
					if (lods[lodIndex].renderers[i])
					{
						MeshFilter currentMeshFilter = lods[lodIndex].renderers[i].GetComponent<MeshFilter>();
						if (currentMeshFilter && currentMeshFilter.sharedMesh && !(currentMeshFilter.sharedMesh == lod0MeshFilter.sharedMesh))
						{
							meshFilter = currentMeshFilter;
							if (Common.StripLODStringPart(lod0MeshFilter.sharedMesh.name) == Common.StripLODStringPart(meshFilter.sharedMesh.name))
							{
								return true;
							}
						}
					}
				}
			}
			meshFilter = null;
			return false;
		}

		// Token: 0x0600261E RID: 9758 RVA: 0x00105968 File Offset: 0x00103B68
		public static string StripLODStringPart(string text)
		{
			return text.ToLower().Replace("_lod0", "").Replace("_lod1", "").Replace("_lod2", "").Replace("_lod3", "").Replace("_lod4", "").Replace("_lod5", "").Replace("_lod6", "");
		}

		// Token: 0x0600261F RID: 9759 RVA: 0x001059E4 File Offset: 0x00103BE4
		public static Color32 HueColourValue(HueColorName color)
		{
			return (Color32)Common.hueColourValues[color];
		}

		// Token: 0x06002620 RID: 9760 RVA: 0x001059FC File Offset: 0x00103BFC
		public static void DrawGizmoArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20f, bool dim3 = false)
		{
			Gizmos.color = color;
			Gizmos.DrawRay(pos, direction);
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
			Vector3 forward = Quaternion.LookRotation(direction) * Quaternion.Euler(180f + arrowHeadAngle, 0f, 0f) * new Vector3(0f, 0f, 1f);
			Vector3 backward = Quaternion.LookRotation(direction) * Quaternion.Euler(180f - arrowHeadAngle, 0f, 0f) * new Vector3(0f, 0f, 1f);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
			if (dim3)
			{
				Gizmos.DrawRay(pos + direction, forward * arrowHeadLength);
				Gizmos.DrawRay(pos + direction, backward * arrowHeadLength);
			}
		}

		// Token: 0x06002621 RID: 9761 RVA: 0x00105B56 File Offset: 0x00103D56
		public static void DrawGizmoCapsule(Vector3 pos, Vector3 direction, float length, float radius, Color color = default(Color))
		{
			Common.DrawGizmoCapsule(pos, Quaternion.LookRotation(direction), length, radius, color);
		}

		// Token: 0x06002622 RID: 9762 RVA: 0x00105B68 File Offset: 0x00103D68
		public static void DrawGizmoCapsule(Vector3 pos, Quaternion direction, float length, float radius, Color color = default(Color))
		{
		}

		// Token: 0x06002623 RID: 9763 RVA: 0x00105B6C File Offset: 0x00103D6C
		public static void DrawGizmoRectangle(Vector3 center, Vector3 up, Vector3 right, float height, float width)
		{
			Common.<>c__DisplayClass76_0 CS$<>8__locals1;
			CS$<>8__locals1.center = center;
			CS$<>8__locals1.height = height;
			CS$<>8__locals1.up = up;
			CS$<>8__locals1.width = width;
			CS$<>8__locals1.right = right;
			for (int i = 0; i < 4; i++)
			{
				int nextPoint = i + 1;
				if (nextPoint > 3)
				{
					nextPoint = 0;
				}
				Gizmos.DrawLine(Common.<DrawGizmoRectangle>g__GetPoint|76_0(i, ref CS$<>8__locals1), Common.<DrawGizmoRectangle>g__GetPoint|76_0(nextPoint, ref CS$<>8__locals1));
			}
		}

		// Token: 0x06002624 RID: 9764 RVA: 0x00105BCD File Offset: 0x00103DCD
		private static float _copysign(float sizeval, float signval)
		{
			if (Mathf.Sign(signval) != 1f)
			{
				return -Mathf.Abs(sizeval);
			}
			return Mathf.Abs(sizeval);
		}

		// Token: 0x06002625 RID: 9765 RVA: 0x00105BEC File Offset: 0x00103DEC
		public static Quaternion GetRotation(this Matrix4x4 matrix)
		{
			Quaternion q = default(Quaternion);
			q.w = Mathf.Sqrt(Mathf.Max(0f, 1f + matrix.m00 + matrix.m11 + matrix.m22)) / 2f;
			q.x = Mathf.Sqrt(Mathf.Max(0f, 1f + matrix.m00 - matrix.m11 - matrix.m22)) / 2f;
			q.y = Mathf.Sqrt(Mathf.Max(0f, 1f - matrix.m00 + matrix.m11 - matrix.m22)) / 2f;
			q.z = Mathf.Sqrt(Mathf.Max(0f, 1f - matrix.m00 - matrix.m11 + matrix.m22)) / 2f;
			q.x = Common._copysign(q.x, matrix.m21 - matrix.m12);
			q.y = Common._copysign(q.y, matrix.m02 - matrix.m20);
			q.z = Common._copysign(q.z, matrix.m10 - matrix.m01);
			return q;
		}

		// Token: 0x06002626 RID: 9766 RVA: 0x00105D38 File Offset: 0x00103F38
		public static Vector3 GetPosition(this Matrix4x4 matrix)
		{
			float m = matrix.m03;
			float y = matrix.m13;
			float z = matrix.m23;
			return new Vector3(m, y, z);
		}

		// Token: 0x06002627 RID: 9767 RVA: 0x00105D60 File Offset: 0x00103F60
		public static Vector3 GetScale(this Matrix4x4 m)
		{
			float x = Mathf.Sqrt(m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02);
			float y = Mathf.Sqrt(m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12);
			float z = Mathf.Sqrt(m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22);
			return new Vector3(x, y, z);
		}

		// Token: 0x06002629 RID: 9769 RVA: 0x0010600C File Offset: 0x0010420C
		[CompilerGenerated]
		internal static Vector3 <DrawGizmoRectangle>g__GetPoint|76_0(int i, ref Common.<>c__DisplayClass76_0 A_1)
		{
			bool negativeHeight = i == 0 || i == 1;
			bool negativeWidth = i == 0 || i == 3;
			return A_1.center + (negativeHeight ? (-A_1.height) : A_1.height) * 0.5f * A_1.up + (negativeWidth ? (-A_1.width) : A_1.width) * 0.5f * A_1.right;
		}

		// Token: 0x040025F4 RID: 9716
		private static int _lightProbeVolumeLayer;

		// Token: 0x040025F5 RID: 9717
		private static Plane[] planes = new Plane[6];

		// Token: 0x040025F6 RID: 9718
		private static int _zoneLayer;

		// Token: 0x040025F7 RID: 9719
		public static int[] layers;

		// Token: 0x040025F8 RID: 9720
		private static bool qualityLevelCached;

		// Token: 0x040025F9 RID: 9721
		private static QualityLevel currentQualityLevel;

		// Token: 0x040025FA RID: 9722
		private static NavMeshPath navMeshPath = new NavMeshPath();

		// Token: 0x040025FB RID: 9723
		private static Hashtable hueColourValues = new Hashtable
		{
			{
				HueColorName.Lime,
				new Color32(166, 254, 0, byte.MaxValue)
			},
			{
				HueColorName.Green,
				new Color32(0, 254, 111, byte.MaxValue)
			},
			{
				HueColorName.Aqua,
				new Color32(0, 201, 254, byte.MaxValue)
			},
			{
				HueColorName.Blue,
				new Color32(0, 122, 254, byte.MaxValue)
			},
			{
				HueColorName.Navy,
				new Color32(60, 0, 254, byte.MaxValue)
			},
			{
				HueColorName.Purple,
				new Color32(143, 0, 254, byte.MaxValue)
			},
			{
				HueColorName.Pink,
				new Color32(232, 0, 254, byte.MaxValue)
			},
			{
				HueColorName.Red,
				new Color32(254, 9, 0, byte.MaxValue)
			},
			{
				HueColorName.Orange,
				new Color32(254, 161, 0, byte.MaxValue)
			},
			{
				HueColorName.Yellow,
				new Color32(254, 224, 0, byte.MaxValue)
			},
			{
				HueColorName.White,
				new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)
			}
		};
	}
}
