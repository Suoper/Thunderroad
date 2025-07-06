using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200033B RID: 827
	public static class Extensions
	{
		// Token: 0x06002661 RID: 9825 RVA: 0x0010A784 File Offset: 0x00108984
		public static void PlayFade(this AudioSource audioSource, ref Coroutine coroutine, MonoBehaviour coroutineOwner, float delay, bool oneShot = false, AudioClip audioClip = null)
		{
			if (coroutine != null)
			{
				coroutineOwner.StopCoroutine(coroutine);
			}
			if (oneShot)
			{
				audioSource.PlayOneShot(audioClip ? audioClip : audioSource.clip);
			}
			else
			{
				audioSource.Play();
			}
			float currentVolume = audioSource.volume;
			coroutine = coroutineOwner.ProgressiveAction(delay, delegate(float t)
			{
				audioSource.volume = Mathf.Lerp(currentVolume, 1f, t);
			});
		}

		// Token: 0x06002662 RID: 9826 RVA: 0x0010A804 File Offset: 0x00108A04
		public static void StopFade(this AudioSource audioSource, ref Coroutine coroutine, MonoBehaviour coroutineOwner, float delay)
		{
			if (coroutine != null)
			{
				coroutineOwner.StopCoroutine(coroutine);
			}
			float currentVolume = audioSource.volume;
			coroutine = coroutineOwner.ProgressiveAction(delay, delegate(float t)
			{
				audioSource.volume = Mathf.Lerp(currentVolume, 0f, t);
				if (t == 1f)
				{
					audioSource.Stop();
				}
			});
		}

		// Token: 0x06002663 RID: 9827 RVA: 0x0010A850 File Offset: 0x00108A50
		public static Coroutine DelayedAction(this MonoBehaviour monoBehaviour, float delay, Action delayedAction)
		{
			return monoBehaviour.StartCoroutine(Extensions.DelayedActionCoroutine(delay, delayedAction));
		}

		// Token: 0x06002664 RID: 9828 RVA: 0x0010A85F File Offset: 0x00108A5F
		private static IEnumerator DelayedActionCoroutine(float delay, Action delayedAction)
		{
			yield return new WaitForSeconds(delay);
			delayedAction();
			yield break;
		}

		// Token: 0x06002665 RID: 9829 RVA: 0x0010A875 File Offset: 0x00108A75
		public static Coroutine WaitConditionAction(this MonoBehaviour monoBehaviour, Func<bool> condition, Action delayedAction)
		{
			return monoBehaviour.StartCoroutine(Extensions.WaitConditionActionCoroutine(condition, delayedAction));
		}

		// Token: 0x06002666 RID: 9830 RVA: 0x0010A884 File Offset: 0x00108A84
		private static IEnumerator WaitConditionActionCoroutine(Func<bool> condition, Action delayedAction)
		{
			while (!condition())
			{
				yield return null;
			}
			delayedAction();
			yield break;
		}

		// Token: 0x06002667 RID: 9831 RVA: 0x0010A89A File Offset: 0x00108A9A
		public static Coroutine ProgressiveAction(this MonoBehaviour monoBehaviour, float delay, Action<float> progressiveAction)
		{
			return monoBehaviour.StartCoroutine(Extensions.ProgressiveActionCoroutine(delay, progressiveAction));
		}

		// Token: 0x06002668 RID: 9832 RVA: 0x0010A8A9 File Offset: 0x00108AA9
		private static IEnumerator ProgressiveActionCoroutine(float delay, Action<float> progressiveAction)
		{
			float time = 0f;
			while (time < delay)
			{
				progressiveAction(time / delay);
				time += Time.deltaTime;
				yield return null;
			}
			progressiveAction(1f);
			yield break;
		}

		// Token: 0x06002669 RID: 9833 RVA: 0x0010A8C0 File Offset: 0x00108AC0
		public static string GetPathFrom(this Transform transform, Transform root)
		{
			if (transform == root)
			{
				return null;
			}
			string path = "/" + transform.name;
			while (transform.parent != null && !(transform.parent == root))
			{
				transform = transform.parent;
				path = "/" + transform.name + path;
			}
			return path.Remove(0, 1);
		}

		// Token: 0x0600266A RID: 9834 RVA: 0x0010A92B File Offset: 0x00108B2B
		public static bool TryGetCustomAttribute<T>(this MemberInfo memberInfo, out T attribute) where T : Attribute
		{
			attribute = memberInfo.GetCustomAttribute<T>();
			return attribute != null;
		}

		// Token: 0x0600266B RID: 9835 RVA: 0x0010A947 File Offset: 0x00108B47
		public static bool TryGetCount(this ICollection collection, out int count)
		{
			count = 0;
			if (collection == null)
			{
				return false;
			}
			count = collection.Count;
			return true;
		}

		// Token: 0x0600266C RID: 9836 RVA: 0x0010A95A File Offset: 0x00108B5A
		public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component) where T : Component
		{
			component = gameObject.GetComponentInChildren<T>();
			return component;
		}

		// Token: 0x0600266D RID: 9837 RVA: 0x0010A978 File Offset: 0x00108B78
		public static bool TryGetOrAddComponentInChildren<T>(this GameObject gameObject, out T component) where T : Component
		{
			component = gameObject.GetComponentInChildren<T>();
			if (component == null)
			{
				GameObject holder = new GameObject();
				holder.transform.SetParent(gameObject.transform);
				component = holder.AddComponent<T>();
			}
			return component;
		}

		// Token: 0x0600266E RID: 9838 RVA: 0x0010A9D8 File Offset: 0x00108BD8
		public static T GetOrAddComponent<T>(this GameObject self) where T : Component
		{
			T component = self.GetComponent<T>();
			if (component == null)
			{
				component = self.AddComponent<T>();
			}
			return component;
		}

		// Token: 0x0600266F RID: 9839 RVA: 0x0010AA02 File Offset: 0x00108C02
		public static bool TryGetOrAddComponent<T>(this GameObject gameObject, out T component) where T : Component
		{
			if (!gameObject.TryGetComponent<T>(out component))
			{
				component = gameObject.AddComponent<T>();
			}
			return component;
		}

		// Token: 0x06002670 RID: 9840 RVA: 0x0010AA29 File Offset: 0x00108C29
		public static PhysicBody GetPhysicBodyInParent(this Component component)
		{
			return component.gameObject.GetPhysicBodyInParent();
		}

		// Token: 0x06002671 RID: 9841 RVA: 0x0010AA38 File Offset: 0x00108C38
		public static PhysicBody GetPhysicBodyInParent(this GameObject gameObject)
		{
			PhysicBody physicBody = gameObject.GetPhysicBody();
			if (physicBody != null)
			{
				return physicBody;
			}
			Transform t = gameObject.transform;
			while (t.parent != null)
			{
				physicBody = t.parent.gameObject.GetPhysicBody();
				if (physicBody != null)
				{
					return physicBody;
				}
				t = t.parent.transform;
			}
			return null;
		}

		// Token: 0x06002672 RID: 9842 RVA: 0x0010AA91 File Offset: 0x00108C91
		public static PhysicBody[] GetPhysicBodiesInChildren(this Component component, bool includeInactive = false)
		{
			return component.gameObject.GetPhysicBodiesInChildren(includeInactive);
		}

		// Token: 0x06002673 RID: 9843 RVA: 0x0010AAA0 File Offset: 0x00108CA0
		public static PhysicBody[] GetPhysicBodiesInChildren(this GameObject gameObject, bool includeInactive = false)
		{
			Rigidbody[] rbs = gameObject.GetComponentsInChildren<Rigidbody>(includeInactive);
			int rbsLength = rbs.Length;
			ArticulationBody[] abs = gameObject.GetComponentsInChildren<ArticulationBody>(includeInactive);
			PhysicBody[] pbs = new PhysicBody[rbsLength + abs.Length];
			for (int i = 0; i < pbs.Length; i++)
			{
				if (i < rbsLength)
				{
					pbs[i] = rbs[i].GetPhysicBody();
				}
				else
				{
					pbs[i] = abs[i - rbsLength].GetPhysicBody();
				}
			}
			return pbs;
		}

		// Token: 0x06002674 RID: 9844 RVA: 0x0010AB02 File Offset: 0x00108D02
		public static bool IsPhysicBody(this RaycastHit raycastHit, PhysicBody physicBody)
		{
			return physicBody.rigidBody == raycastHit.rigidbody;
		}

		// Token: 0x06002675 RID: 9845 RVA: 0x0010AB16 File Offset: 0x00108D16
		public static bool TryGetPhysicBody(this Collider collider, out PhysicBody physicBody)
		{
			if (collider.attachedRigidbody)
			{
				physicBody = new PhysicBody(collider.attachedRigidbody);
				return true;
			}
			physicBody = null;
			return false;
		}

		// Token: 0x06002676 RID: 9846 RVA: 0x0010AB38 File Offset: 0x00108D38
		public static PhysicBody AsPhysicBody(this Rigidbody rb)
		{
			return new PhysicBody(rb);
		}

		// Token: 0x06002677 RID: 9847 RVA: 0x0010AB40 File Offset: 0x00108D40
		public static PhysicBody GetPhysicBody(this RaycastHit hit)
		{
			if (hit.rigidbody != null)
			{
				return hit.rigidbody.AsPhysicBody();
			}
			return hit.transform.GetPhysicBody();
		}

		// Token: 0x06002678 RID: 9848 RVA: 0x0010AB6C File Offset: 0x00108D6C
		public static PhysicBody GetPhysicBody(this Component component)
		{
			Rigidbody rb = component as Rigidbody;
			PhysicBody result;
			if (rb == null)
			{
				Collider col = component as Collider;
				if (col != null)
				{
					if (col.attachedRigidbody != null)
					{
						return col.attachedRigidbody.AsPhysicBody();
					}
				}
				result = component.gameObject.GetPhysicBody();
			}
			else
			{
				result = new PhysicBody(rb);
			}
			return result;
		}

		// Token: 0x06002679 RID: 9849 RVA: 0x0010ABC4 File Offset: 0x00108DC4
		public static PhysicBody GetPhysicBody(this GameObject gameObject)
		{
			Rigidbody rigidBody;
			if (!gameObject.TryGetComponent<Rigidbody>(out rigidBody))
			{
				return null;
			}
			return new PhysicBody(rigidBody);
		}

		// Token: 0x0600267A RID: 9850 RVA: 0x0010ABE3 File Offset: 0x00108DE3
		public static void SetConnectedPhysicBody(this Joint joint, PhysicBody physicBody)
		{
			joint.connectedBody = physicBody.rigidBody;
		}

		// Token: 0x0600267B RID: 9851 RVA: 0x0010ABF1 File Offset: 0x00108DF1
		public static PhysicBody GetConnectedPhysicBody(this Joint joint)
		{
			if (joint.connectedArticulationBody)
			{
				return joint.connectedArticulationBody.gameObject.GetPhysicBody();
			}
			return joint.connectedBody.gameObject.GetPhysicBody();
		}

		// Token: 0x0600267C RID: 9852 RVA: 0x0010AC24 File Offset: 0x00108E24
		public static bool CalculateBodyLaunchVector(this PhysicBody physicbody, Vector3 target, out Vector3 launchVector, float speed = -1f, float gravityMultiplier = 1f)
		{
			Vector3 toTarget = target - physicbody.gameObject.transform.position;
			if (speed < 0f)
			{
				speed = physicbody.velocity.magnitude;
			}
			return Utils.CalculateProjectileLaunchVector(toTarget, speed, out launchVector, gravityMultiplier);
		}

		// Token: 0x0600267D RID: 9853 RVA: 0x0010AC68 File Offset: 0x00108E68
		public static U FindOrAddList<T, U, V>(this Dictionary<T, U> dict, T key) where U : ICollection<V>, new()
		{
			if (dict == null)
			{
				dict = new Dictionary<T, U>();
			}
			U found;
			if (!dict.TryGetValue(key, out found))
			{
				found = Activator.CreateInstance<U>();
				dict[key] = found;
			}
			return found;
		}

		// Token: 0x0600267E RID: 9854 RVA: 0x0010AC9C File Offset: 0x00108E9C
		public static void AddToKeyedList<T, U, V>(this Dictionary<T, U> dict, T key, V element) where U : ICollection<V>, new()
		{
			U u = dict.FindOrAddList(key);
			u.Add(element);
		}

		// Token: 0x0600267F RID: 9855 RVA: 0x0010ACC0 File Offset: 0x00108EC0
		public static void RemoveFromKeyedList<T, U, V>(this Dictionary<T, U> dict, T key, V element) where U : ICollection<V>, new()
		{
			U u = dict.FindOrAddList(key);
			u.Remove(element);
		}

		/// <summary>
		/// This is a O(1) way of removing things from a list by moving them to the end
		/// </summary>
		/// <param name="list"></param>
		/// <param name="index"></param>
		/// <param name="listCount"></param>
		/// <typeparam name="T"></typeparam>
		// Token: 0x06002680 RID: 9856 RVA: 0x0010ACE4 File Offset: 0x00108EE4
		public static void RemoveAtIgnoreOrder<T>(this IList<T> list, int index, int listCount)
		{
			int last = listCount - 1;
			if (last != index)
			{
				list[index] = list[last];
			}
			list.RemoveAt(last);
		}

		/// <summary>
		/// This is a O(1) way of removing things from a list by moving them to the end
		/// </summary>
		/// <param name="list"></param>
		/// <param name="index"></param>
		/// <typeparam name="T"></typeparam>
		// Token: 0x06002681 RID: 9857 RVA: 0x0010AD0E File Offset: 0x00108F0E
		public static void RemoveAtIgnoreOrder<T>(this IList<T> list, int index)
		{
			list.RemoveAtIgnoreOrder(index, list.Count);
		}

		// Token: 0x06002682 RID: 9858 RVA: 0x0010AD20 File Offset: 0x00108F20
		public static List<T> Shuffle<T>(this List<T> list)
		{
			System.Random _random = new System.Random();
			int i = list.Count;
			for (int j = 0; j < i; j++)
			{
				int r = j + (int)(_random.NextDouble() * (double)(i - j));
				T obj = list[r];
				list[r] = list[j];
				list[j] = obj;
			}
			return list;
		}

		// Token: 0x06002683 RID: 9859 RVA: 0x0010AD78 File Offset: 0x00108F78
		public static T[] Shuffle<T>(this T[] array)
		{
			System.Random _random = new System.Random();
			int i = array.Length;
			for (int j = 0; j < i; j++)
			{
				int r = j + (int)(_random.NextDouble() * (double)(i - j));
				T obj = array[r];
				array[r] = array[j];
				array[j] = obj;
			}
			return array;
		}

		// Token: 0x06002684 RID: 9860 RVA: 0x0010ADCC File Offset: 0x00108FCC
		public static void BlackAndWhiteSort<T>(this List<T> list, int count, Func<T, bool> validCheck, Action<T> validAction, Action<T> invalidAction)
		{
			int num;
			list.BlackAndWhiteSort(count, validCheck, validAction, invalidAction, out num);
		}

		// Token: 0x06002685 RID: 9861 RVA: 0x0010ADE8 File Offset: 0x00108FE8
		public static void BlackAndWhiteSort<T>(this List<T> list, int count, Func<T, bool> validCheck, Action<T> validAction, Action<T> invalidAction, out int validNumber)
		{
			int currentIndex = 0;
			int validIndex = 0;
			int invalidIndex = count - 1;
			while (currentIndex <= invalidIndex)
			{
				T item = list[currentIndex];
				int moveIndex;
				if (validCheck(item))
				{
					if (validAction != null)
					{
						validAction(item);
					}
					moveIndex = validIndex;
					validIndex++;
				}
				else
				{
					if (invalidAction != null)
					{
						invalidAction(item);
					}
					moveIndex = invalidIndex;
					invalidIndex--;
				}
				if (currentIndex != moveIndex)
				{
					T move = list[moveIndex];
					list[moveIndex] = list[currentIndex];
					list[currentIndex] = move;
				}
				else
				{
					currentIndex++;
				}
			}
			validNumber = validIndex;
		}

		// Token: 0x06002686 RID: 9862 RVA: 0x0010AE6C File Offset: 0x0010906C
		public static void BlackAndWhiteSort<T>(this T[] array, int count, Func<T, bool> validCheck, Action<T> validAction, Action<T> invalidAction)
		{
			int num;
			array.BlackAndWhiteSort(count, validCheck, validAction, invalidAction, out num);
		}

		// Token: 0x06002687 RID: 9863 RVA: 0x0010AE88 File Offset: 0x00109088
		public static void BlackAndWhiteSort<T>(this T[] array, int count, Func<T, bool> validCheck, Action<T> validAction, Action<T> invalidAction, out int validNumber)
		{
			int currentIndex = 0;
			int validIndex = 0;
			int invalidIndex = count - 1;
			while (currentIndex <= invalidIndex)
			{
				T item = array[currentIndex];
				int moveIndex;
				if (validCheck(item))
				{
					if (validAction != null)
					{
						validAction(item);
					}
					moveIndex = validIndex;
					validIndex++;
				}
				else
				{
					if (invalidAction != null)
					{
						invalidAction(item);
					}
					moveIndex = invalidIndex;
					invalidIndex--;
				}
				if (currentIndex != moveIndex)
				{
					T move = array[moveIndex];
					array[moveIndex] = array[currentIndex];
					array[currentIndex] = move;
				}
				else
				{
					currentIndex++;
				}
			}
			validNumber = validIndex;
		}

		// Token: 0x06002688 RID: 9864 RVA: 0x0010AF0A File Offset: 0x0010910A
		public static bool RandomFilteredSelectInPlace<T>(this IEnumerable<T> enumerable, Func<T, bool> condition, out T result)
		{
			return enumerable.WeightedFilteredSelectInPlace(condition, (T element) => 1f, out result);
		}

		// Token: 0x06002689 RID: 9865 RVA: 0x0010AF34 File Offset: 0x00109134
		public static bool WeightedFilteredSelectInPlace<T>(this IEnumerable<T> enumerable, Func<T, bool> condition, Func<T, float> weight, out T result)
		{
			bool found = false;
			result = default(T);
			float totalWeight = 0f;
			foreach (T element in enumerable)
			{
				if (condition(element))
				{
					float elementWeight = weight(element);
					if (elementWeight > 0f && UnityEngine.Random.Range(0f, totalWeight + elementWeight) >= totalWeight)
					{
						result = element;
						found = true;
					}
					totalWeight += elementWeight;
				}
			}
			return found;
		}

		// Token: 0x0600268A RID: 9866 RVA: 0x0010AFC0 File Offset: 0x001091C0
		public static T WeightedSelect<T>(this IEnumerable<T> enumerable, Func<T, float> weight)
		{
			T result = default(T);
			float totalWeight = 0f;
			foreach (T element in enumerable)
			{
				float elementWeight = weight(element);
				if (elementWeight > 0f && UnityEngine.Random.Range(0f, totalWeight + elementWeight) >= totalWeight)
				{
					result = element;
				}
				totalWeight += elementWeight;
			}
			return result;
		}

		// Token: 0x0600268B RID: 9867 RVA: 0x0010B03C File Offset: 0x0010923C
		public static byte[] Serialize<T>(this T source)
		{
			JsonSerializer ser = Catalog.GetJsonNetSerializer();
			byte[] result;
			using (MemoryStream s = new MemoryStream())
			{
				using (StreamWriter writer = new StreamWriter(s, Encoding.UTF8, 4096))
				{
					using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
					{
						ser.Serialize(jsonWriter, source);
						jsonWriter.Flush();
					}
				}
				result = s.ToArray();
			}
			return result;
		}

		// Token: 0x0600268C RID: 9868 RVA: 0x0010B0D4 File Offset: 0x001092D4
		public static T Deserialize<T>(this byte[] source)
		{
			JsonSerializer ser = Catalog.GetJsonNetSerializer();
			T result;
			using (MemoryStream s = new MemoryStream(source))
			{
				using (StreamReader reader = new StreamReader(s))
				{
					using (JsonTextReader jsonReader = new JsonTextReader(reader))
					{
						T t = ser.Deserialize<T>(jsonReader);
						s.Dispose();
						result = t;
					}
				}
			}
			return result;
		}

		// Token: 0x0600268D RID: 9869 RVA: 0x0010B158 File Offset: 0x00109358
		public static Task<T> CloneJsonAsync<T>(this T source)
		{
			Extensions.<CloneJsonAsync>d__44<T> <CloneJsonAsync>d__;
			<CloneJsonAsync>d__.<>t__builder = AsyncTaskMethodBuilder<T>.Create();
			<CloneJsonAsync>d__.source = source;
			<CloneJsonAsync>d__.<>1__state = -1;
			<CloneJsonAsync>d__.<>t__builder.Start<Extensions.<CloneJsonAsync>d__44<T>>(ref <CloneJsonAsync>d__);
			return <CloneJsonAsync>d__.<>t__builder.Task;
		}

		// Token: 0x0600268E RID: 9870 RVA: 0x0010B19C File Offset: 0x0010939C
		public static T CloneJson<T>(this T source)
		{
			JsonSerializer ser = Catalog.GetJsonNetSerializer();
			T result;
			using (MemoryStream s = new MemoryStream())
			{
				using (StreamWriter writer = new StreamWriter(s, Encoding.UTF8, 4096, true))
				{
					using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
					{
						ser.Serialize(jsonWriter, source);
						jsonWriter.Flush();
					}
				}
				s.Seek(0L, SeekOrigin.Begin);
				using (StreamReader reader = new StreamReader(s))
				{
					using (JsonTextReader jsonReader = new JsonTextReader(reader))
					{
						T t = ser.Deserialize<T>(jsonReader);
						s.Dispose();
						result = t;
					}
				}
			}
			return result;
		}

		// Token: 0x0600268F RID: 9871 RVA: 0x0010B288 File Offset: 0x00109488
		public static IEnumerator AsIEnumerator(this Task task)
		{
			while (!task.IsCompleted)
			{
				yield return null;
			}
			if (task.IsFaulted)
			{
				throw task.Exception;
			}
			yield break;
		}

		/// <summary>
		/// Forces iteration through an IEnumerator, useful to make coroutines synchronous
		/// </summary>
		/// <param name="enumerator"></param>
		// Token: 0x06002690 RID: 9872 RVA: 0x0010B298 File Offset: 0x00109498
		public static void AsSynchronous(this IEnumerator enumerator)
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				IEnumerator next = obj as IEnumerator;
				if (next != null)
				{
					next.AsSynchronous();
				}
			}
		}

		// Token: 0x06002691 RID: 9873 RVA: 0x0010B2C4 File Offset: 0x001094C4
		public static bool IsApproximately(this float a, float b)
		{
			return Mathf.Approximately(a, b);
		}

		// Token: 0x06002692 RID: 9874 RVA: 0x0010B2CD File Offset: 0x001094CD
		public static bool IsNormalized(this float f)
		{
			return f >= 0f && f <= 1f;
		}

		// Token: 0x06002693 RID: 9875 RVA: 0x0010B2E4 File Offset: 0x001094E4
		public static bool IsInfiniteOrNaN(this float f)
		{
			return float.IsNaN(f) || float.IsInfinity(f) || float.IsNegativeInfinity(f);
		}

		// Token: 0x06002694 RID: 9876 RVA: 0x0010B300 File Offset: 0x00109500
		public static float Sum(this float[] floats)
		{
			float result = 0f;
			for (int i = 0; i < floats.Length; i++)
			{
				result += floats[i];
			}
			return result;
		}

		// Token: 0x06002695 RID: 9877 RVA: 0x0010B328 File Offset: 0x00109528
		public static float DistanceSqr(this Vector3 a, Vector3 b)
		{
			return Extensions.GetDistanceSqr(a, b);
		}

		/// <summary>
		/// Returns the Squared Distance between two vectors
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>Squared Distance</returns>
		// Token: 0x06002696 RID: 9878 RVA: 0x0010B334 File Offset: 0x00109534
		public static float GetDistanceSqr(Vector3 a, Vector3 b)
		{
			float num4 = a.x - b.x;
			float num2 = a.y - b.y;
			float num3 = a.z - b.z;
			return num4 * num4 + num2 * num2 + num3 * num3;
		}

		// Token: 0x06002697 RID: 9879 RVA: 0x0010B374 File Offset: 0x00109574
		public static bool ValueBetween(this Vector2Int vector, int value)
		{
			return value >= vector.x && value <= vector.y;
		}

		// Token: 0x06002698 RID: 9880 RVA: 0x0010B38F File Offset: 0x0010958F
		public static bool ValueBetween(this Vector2 vector, float value)
		{
			return value >= vector.x && value <= vector.y;
		}

		// Token: 0x06002699 RID: 9881 RVA: 0x0010B3A8 File Offset: 0x001095A8
		public static bool PointInRadius(this Vector3 vectorA, Vector3 vectorB, float radius)
		{
			return vectorA.DistanceSqr(vectorB) < radius * radius;
		}

		// Token: 0x0600269A RID: 9882 RVA: 0x0010B3BC File Offset: 0x001095BC
		public static bool PointInRadius(this Vector3 vectorA, Vector3 vectorB, float radius, out float radiusDistanceRatio)
		{
			float sqrMagnitude = vectorA.DistanceSqr(vectorB);
			if (sqrMagnitude < radius * radius)
			{
				radiusDistanceRatio = 1f - sqrMagnitude / (radius * radius);
				return true;
			}
			radiusDistanceRatio = 0f;
			return false;
		}

		// Token: 0x0600269B RID: 9883 RVA: 0x0010B3F0 File Offset: 0x001095F0
		[return: TupleElementNames(new string[]
		{
			"inDirection",
			"notInDirection"
		})]
		public static ValueTuple<Vector3, Vector3> ProjectAndSubtract(this Vector3 vector, Vector3 normal)
		{
			ValueTuple<Vector3, Vector3> result = new ValueTuple<Vector3, Vector3>(Vector3.Project(vector, normal), Vector3.zero);
			result.Item2 = vector - result.Item1;
			return result;
		}

		// Token: 0x0600269C RID: 9884 RVA: 0x0010B424 File Offset: 0x00109624
		public static Vector3 GetClosestPoint(this Vector3 origin, params Vector3[] checkPoints)
		{
			Vector3 closest = checkPoints[0];
			for (int i = 1; i < checkPoints.Length; i++)
			{
				if ((checkPoints[i] - origin).sqrMagnitude < (closest - origin).sqrMagnitude)
				{
					closest = checkPoints[i];
				}
			}
			return closest;
		}

		// Token: 0x0600269D RID: 9885 RVA: 0x0010B478 File Offset: 0x00109678
		public static T GetClosestObject<T>(this Vector3 origin, params T[] objects) where T : Component
		{
			T closest = objects[0];
			for (int i = 1; i < objects.Length; i++)
			{
				if ((objects[i].transform.position - origin).sqrMagnitude < (closest.transform.position - origin).sqrMagnitude)
				{
					closest = objects[i];
				}
			}
			return closest;
		}

		// Token: 0x0600269E RID: 9886 RVA: 0x0010B4E8 File Offset: 0x001096E8
		public static Vector3 GetFurthestPoint(this Vector3 origin, params Vector3[] checkPoints)
		{
			Vector3 furthest = checkPoints[0];
			for (int i = 1; i < checkPoints.Length; i++)
			{
				if ((checkPoints[i] - origin).sqrMagnitude > (furthest - origin).sqrMagnitude)
				{
					furthest = checkPoints[i];
				}
			}
			return furthest;
		}

		// Token: 0x0600269F RID: 9887 RVA: 0x0010B53C File Offset: 0x0010973C
		public static T GetFurthestObject<T>(this Vector3 origin, params T[] objects) where T : Component
		{
			T furthest = objects[0];
			for (int i = 1; i < objects.Length; i++)
			{
				if ((objects[i].transform.position - origin).sqrMagnitude > (furthest.transform.position - origin).sqrMagnitude)
				{
					furthest = objects[i];
				}
			}
			return furthest;
		}

		// Token: 0x060026A0 RID: 9888 RVA: 0x0010B5AC File Offset: 0x001097AC
		public static bool HasFlagNoGC(this EffectModule.PlatformFilter flags, EffectModule.PlatformFilter value)
		{
			return (flags & value) > (EffectModule.PlatformFilter)0;
		}

		// Token: 0x060026A1 RID: 9889 RVA: 0x0010B5B4 File Offset: 0x001097B4
		public static bool HasFlagNoGC(this ItemData.Storage flags, ItemData.Storage value)
		{
			return (flags & value) > (ItemData.Storage)0;
		}

		// Token: 0x060026A2 RID: 9890 RVA: 0x0010B5BC File Offset: 0x001097BC
		public static bool HasFlagNoGC(this HingeDrive.InputType flags, HingeDrive.InputType value)
		{
			return (flags & value) > HingeDrive.InputType.None;
		}

		// Token: 0x060026A3 RID: 9891 RVA: 0x0010B5C4 File Offset: 0x001097C4
		public static bool HasFlagNoGC(this ManagedLoops flags, ManagedLoops value)
		{
			return (flags & value) > (ManagedLoops)0;
		}

		// Token: 0x060026A4 RID: 9892 RVA: 0x0010B5CC File Offset: 0x001097CC
		public static bool HasFlagNoGC(this RagdollPart.Type flags, RagdollPart.Type value)
		{
			return (flags & value) > (RagdollPart.Type)0;
		}

		// Token: 0x060026A5 RID: 9893 RVA: 0x0010B5D4 File Offset: 0x001097D4
		public static bool HasFlagNoGC(this Damager.PenetrationConditions flags, Damager.PenetrationConditions value)
		{
			return (flags & value) > Damager.PenetrationConditions.None;
		}

		// Token: 0x060026A6 RID: 9894 RVA: 0x0010B5DC File Offset: 0x001097DC
		public static bool HasFlagNoGC(this DamagerData.PenetrationTempModifier flags, DamagerData.PenetrationTempModifier value)
		{
			return (flags & value) > (DamagerData.PenetrationTempModifier)0;
		}

		// Token: 0x060026A7 RID: 9895 RVA: 0x0010B5E4 File Offset: 0x001097E4
		public static bool HasFlagNoGC(this DamageModifierData.Modifier.TierFilter flags, DamageModifierData.Modifier.TierFilter value)
		{
			return (flags & value) > (DamageModifierData.Modifier.TierFilter)0;
		}

		// Token: 0x060026A8 RID: 9896 RVA: 0x0010B5EC File Offset: 0x001097EC
		public static bool HasFlagNoGC(this ColliderGroupData.Modifier.TierFilter flags, ColliderGroupData.Modifier.TierFilter value)
		{
			return (flags & value) > (ColliderGroupData.Modifier.TierFilter)0;
		}

		// Token: 0x060026A9 RID: 9897 RVA: 0x0010B5F4 File Offset: 0x001097F4
		public static bool HasFlagNoGC(this EffectModuleReveal.TypeFilter flags, EffectModuleReveal.TypeFilter value)
		{
			return (flags & value) > (EffectModuleReveal.TypeFilter)0;
		}

		// Token: 0x060026AA RID: 9898 RVA: 0x0010B5FC File Offset: 0x001097FC
		public static bool HasFlagNoGC(this EffectModule.DamageTypeFilter flags, EffectModule.DamageTypeFilter value)
		{
			return (flags & value) > (EffectModule.DamageTypeFilter)0;
		}

		// Token: 0x060026AB RID: 9899 RVA: 0x0010B604 File Offset: 0x00109804
		public static bool HasFlagNoGC(this EffectModule.PenetrationFilter flags, EffectModule.PenetrationFilter value)
		{
			return (flags & value) > (EffectModule.PenetrationFilter)0;
		}

		// Token: 0x060026AC RID: 9900 RVA: 0x0010B60C File Offset: 0x0010980C
		public static bool HasFlagNoGC(this EffectModule.DamagerFilter flags, EffectModule.DamagerFilter value)
		{
			return (flags & value) > (EffectModule.DamagerFilter)0;
		}

		// Token: 0x060026AD RID: 9901 RVA: 0x0010B614 File Offset: 0x00109814
		public static bool HasFlagNoGC(this HandleRagdollData.ForceLiftCondition flags, HandleRagdollData.ForceLiftCondition value)
		{
			return (flags & value) > (HandleRagdollData.ForceLiftCondition)0;
		}

		// Token: 0x060026AE RID: 9902 RVA: 0x0010B61C File Offset: 0x0010981C
		public static bool HasFlagNoGC(this Golem.Tier flags, Golem.Tier value)
		{
			return (flags & value) > Golem.Tier.Any;
		}

		// Token: 0x060026AF RID: 9903 RVA: 0x0010B624 File Offset: 0x00109824
		public static bool HasFlagNoGC(this BuildSettings.ContentFlag flags, BuildSettings.ContentFlag value)
		{
			return (flags & value) > BuildSettings.ContentFlag.None;
		}

		// Token: 0x060026B0 RID: 9904 RVA: 0x0010B62C File Offset: 0x0010982C
		public static bool CheckContentActive(this BuildSettings.ContentFlag flags, BuildSettings.ContentFlag content)
		{
			return flags != BuildSettings.ContentFlag.None && (content == BuildSettings.ContentFlag.None || (flags & content) == content);
		}

		// Token: 0x060026B1 RID: 9905 RVA: 0x0010B63E File Offset: 0x0010983E
		public static BuildSettings.ContentFlag AsRealFlag(this BuildSettings.SingleContentFlag flag)
		{
			return (BuildSettings.ContentFlag)(1 << (int)flag);
		}

		// Token: 0x060026B2 RID: 9906 RVA: 0x0010B648 File Offset: 0x00109848
		public static Collider Clone(this Collider collider, GameObject gameObject)
		{
			if (collider is SphereCollider)
			{
				(collider as SphereCollider).Clone(gameObject);
			}
			else if (collider is CapsuleCollider)
			{
				(collider as CapsuleCollider).Clone(gameObject);
			}
			else if (collider is BoxCollider)
			{
				(collider as BoxCollider).Clone(gameObject);
			}
			else if (collider is MeshCollider)
			{
				(collider as MeshCollider).Clone(gameObject);
			}
			return null;
		}

		// Token: 0x060026B3 RID: 9907 RVA: 0x0010B6B0 File Offset: 0x001098B0
		public static SphereCollider Clone(this SphereCollider collider, GameObject gameObject)
		{
			SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
			sphereCollider.center = collider.center;
			sphereCollider.radius = collider.radius;
			sphereCollider.sharedMaterial = collider.sharedMaterial;
			sphereCollider.isTrigger = collider.isTrigger;
			return sphereCollider;
		}

		// Token: 0x060026B4 RID: 9908 RVA: 0x0010B6E8 File Offset: 0x001098E8
		public static CapsuleCollider Clone(this CapsuleCollider collider, GameObject gameObject)
		{
			CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
			capsuleCollider.center = collider.center;
			capsuleCollider.radius = collider.radius;
			capsuleCollider.height = collider.height;
			capsuleCollider.sharedMaterial = collider.sharedMaterial;
			capsuleCollider.isTrigger = collider.isTrigger;
			return capsuleCollider;
		}

		// Token: 0x060026B5 RID: 9909 RVA: 0x0010B737 File Offset: 0x00109937
		public static BoxCollider Clone(this BoxCollider collider, GameObject gameObject)
		{
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.center = collider.center;
			boxCollider.size = collider.size;
			boxCollider.sharedMaterial = collider.sharedMaterial;
			boxCollider.isTrigger = collider.isTrigger;
			return boxCollider;
		}

		// Token: 0x060026B6 RID: 9910 RVA: 0x0010B770 File Offset: 0x00109970
		public static MeshCollider Clone(this MeshCollider collider, GameObject gameObject)
		{
			MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
			meshCollider.convex = collider.convex;
			meshCollider.cookingOptions = collider.cookingOptions;
			meshCollider.sharedMesh = collider.sharedMesh;
			meshCollider.sharedMaterial = collider.sharedMaterial;
			meshCollider.isTrigger = collider.isTrigger;
			return meshCollider;
		}

		// Token: 0x060026B7 RID: 9911 RVA: 0x0010B7C0 File Offset: 0x001099C0
		public static float GetScaledRadius(this SphereCollider sphere)
		{
			return sphere.radius * Mathf.Max(new float[]
			{
				sphere.transform.lossyScale.x,
				sphere.transform.lossyScale.y,
				sphere.transform.lossyScale.z
			});
		}

		// Token: 0x060026B8 RID: 9912 RVA: 0x0010B818 File Offset: 0x00109A18
		public static void ScaledWorldInfo(this CapsuleCollider capsule, out Vector3 pointA, out Vector3 pointB, out float height, out float radius)
		{
			pointA = (pointB = capsule.transform.TransformPoint(capsule.center));
			capsule.GetScaledHeightRadius(out height, out radius);
			float halfHeightMinusRadius = height / 2f - radius;
			if (halfHeightMinusRadius <= 0f)
			{
				return;
			}
			Vector3 capsuleAxis = capsule.transform.TransformDirection(capsule.AxisVector());
			pointA = capsule.transform.TransformPoint(capsule.center) + capsuleAxis * halfHeightMinusRadius;
			pointB = capsule.transform.TransformPoint(capsule.center) + capsuleAxis * -halfHeightMinusRadius;
		}

		// Token: 0x060026B9 RID: 9913 RVA: 0x0010B8C0 File Offset: 0x00109AC0
		public static void FocusPoints(this CapsuleCollider capsule, out Vector3 pointA, out Vector3 pointB)
		{
			float num;
			float num2;
			capsule.ScaledWorldInfo(out pointA, out pointB, out num, out num2);
		}

		// Token: 0x060026BA RID: 9914 RVA: 0x0010B8DC File Offset: 0x00109ADC
		public static Vector3 AxisVector(this CapsuleCollider capsule)
		{
			return new Vector3((capsule.direction == 0) ? 1f : 0f, (capsule.direction == 1) ? 1f : 0f, (capsule.direction == 2) ? 1f : 0f);
		}

		// Token: 0x060026BB RID: 9915 RVA: 0x0010B92C File Offset: 0x00109B2C
		public static float GetScaledRadius(this CapsuleCollider capsule)
		{
			float num;
			float radius;
			capsule.GetScaledHeightRadius(out num, out radius);
			return radius;
		}

		// Token: 0x060026BC RID: 9916 RVA: 0x0010B944 File Offset: 0x00109B44
		public static float GetScaledHeight(this CapsuleCollider capsule)
		{
			float height;
			float num;
			capsule.GetScaledHeightRadius(out height, out num);
			return height;
		}

		// Token: 0x060026BD RID: 9917 RVA: 0x0010B95C File Offset: 0x00109B5C
		public static void GetScaledHeightRadius(this CapsuleCollider capsule, out float height, out float radius)
		{
			float axisScale = 1f;
			float radiusScale = 1f;
			switch (capsule.direction)
			{
			case 0:
				axisScale = capsule.transform.lossyScale.x;
				radiusScale = Mathf.Max(capsule.transform.lossyScale.y, capsule.transform.lossyScale.z);
				break;
			case 1:
				axisScale = capsule.transform.lossyScale.y;
				radiusScale = Mathf.Max(capsule.transform.lossyScale.x, capsule.transform.lossyScale.z);
				break;
			case 2:
				axisScale = capsule.transform.lossyScale.z;
				radiusScale = Mathf.Max(capsule.transform.lossyScale.x, capsule.transform.lossyScale.y);
				break;
			}
			radius = capsule.radius * radiusScale;
			height = capsule.height * axisScale;
		}

		// Token: 0x060026BE RID: 9918 RVA: 0x0010BA50 File Offset: 0x00109C50
		public static float GetFirstValue(this AnimationCurve animationCurve)
		{
			if (animationCurve.length != 0)
			{
				return animationCurve[0].value;
			}
			return 0f;
		}

		// Token: 0x060026BF RID: 9919 RVA: 0x0010BA7C File Offset: 0x00109C7C
		public static float GetLastValue(this AnimationCurve animationCurve)
		{
			if (animationCurve.length != 0)
			{
				return animationCurve[animationCurve.length - 1].value;
			}
			return 0f;
		}

		// Token: 0x060026C0 RID: 9920 RVA: 0x0010BAB0 File Offset: 0x00109CB0
		public static float GetFirstTime(this AnimationCurve animationCurve)
		{
			if (animationCurve.length != 0)
			{
				return animationCurve[0].time;
			}
			return 0f;
		}

		// Token: 0x060026C1 RID: 9921 RVA: 0x0010BADC File Offset: 0x00109CDC
		public static float GetLastTime(this AnimationCurve animationCurve)
		{
			if (animationCurve.length != 0)
			{
				return animationCurve[animationCurve.length - 1].time;
			}
			return 0f;
		}

		// Token: 0x060026C2 RID: 9922 RVA: 0x0010BB10 File Offset: 0x00109D10
		public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
		{
			transform.localScale = Vector3.one;
			transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
		}

		// Token: 0x060026C3 RID: 9923 RVA: 0x0010BB6C File Offset: 0x00109D6C
		public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
		{
			float multiplier = 1f;
			for (int i = 0; i < decimalPlaces; i++)
			{
				multiplier *= 10f;
			}
			return new Vector3(Mathf.Round(vector3.x * multiplier) / multiplier, Mathf.Round(vector3.y * multiplier) / multiplier, Mathf.Round(vector3.z * multiplier) / multiplier);
		}

		// Token: 0x060026C4 RID: 9924 RVA: 0x0010BBC8 File Offset: 0x00109DC8
		public static float SignedAngleFromDirection(this Vector3 fromdir, Vector3 todir, Vector3 referenceup)
		{
			Vector3 lhs = Vector3.Cross(fromdir, todir);
			float angle = Vector3.Angle(fromdir, todir);
			if (Vector3.Dot(lhs, referenceup) > 0f)
			{
				return angle;
			}
			return -angle;
		}

		// Token: 0x060026C5 RID: 9925 RVA: 0x0010BBF5 File Offset: 0x00109DF5
		public static Vector3 ToXZ(this Vector3 fromdir)
		{
			fromdir.y = 0f;
			return fromdir;
		}

		// Token: 0x060026C6 RID: 9926 RVA: 0x0010BC04 File Offset: 0x00109E04
		public static Vector3 ToYZ(this Vector3 fromdir)
		{
			fromdir.x = 0f;
			return fromdir;
		}

		// Token: 0x060026C7 RID: 9927 RVA: 0x0010BC13 File Offset: 0x00109E13
		public static Vector3 ClampMagnitude(this Vector3 vector, float minMagnitude, float maxMagnitude)
		{
			if (vector.sqrMagnitude < minMagnitude * minMagnitude)
			{
				return vector.normalized * minMagnitude;
			}
			if (vector.sqrMagnitude > maxMagnitude * maxMagnitude)
			{
				return vector.normalized * maxMagnitude;
			}
			return vector;
		}

		// Token: 0x060026C8 RID: 9928 RVA: 0x0010BC4C File Offset: 0x00109E4C
		public static Transform FindOrAddTransform(this Transform parent, string name, Vector3 position, Quaternion? rotation = null, Vector3? scale = null)
		{
			Transform returnable = parent.Find(name);
			if (!returnable)
			{
				returnable = new GameObject(name).transform;
				returnable.parent = parent;
				returnable.position = position;
				returnable.rotation = ((rotation != null) ? rotation.Value : Quaternion.identity);
				returnable.localScale = ((scale != null) ? scale.Value : Vector3.one);
			}
			return returnable;
		}

		// Token: 0x060026C9 RID: 9929 RVA: 0x0010BCBE File Offset: 0x00109EBE
		public static Quaternion To(this Quaternion from, Quaternion to)
		{
			return to * Quaternion.Inverse(from);
		}

		/// <summary>
		/// Sets a joint's targetRotation to match a given local rotation.
		/// The joint transform's local rotation must be cached on Start and passed into this method.
		/// </summary>
		// Token: 0x060026CA RID: 9930 RVA: 0x0010BCCC File Offset: 0x00109ECC
		public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion startLocalRotation)
		{
			if (joint.configuredInWorldSpace)
			{
				Debug.LogError("SetTargetRotationLocal should not be used with joints that are configured in world space. For world space joints, use SetTargetRotation.", joint);
			}
			Extensions.SetTargetRotationInternal(joint, targetLocalRotation, startLocalRotation, Space.Self);
		}

		/// <summary>
		/// Sets a joint's targetRotation to match a given world rotation.
		/// The joint transform's world rotation must be cached on Start and passed into this method.
		/// </summary>
		// Token: 0x060026CB RID: 9931 RVA: 0x0010BCEA File Offset: 0x00109EEA
		public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion startWorldRotation)
		{
			if (!joint.configuredInWorldSpace)
			{
				Debug.LogError("SetTargetRotation must be used with joints that are configured in world space. For local space joints, use SetTargetRotationLocal.", joint);
			}
			Extensions.SetTargetRotationInternal(joint, targetWorldRotation, startWorldRotation, Space.World);
		}

		// Token: 0x060026CC RID: 9932 RVA: 0x0010BD08 File Offset: 0x00109F08
		private static void SetTargetRotationInternal(ConfigurableJoint joint, Quaternion targetRotation, Quaternion startRotation, Space space)
		{
			Vector3 right = joint.axis;
			Vector3 normalized = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
			Vector3 up = Vector3.Cross(normalized, right).normalized;
			Quaternion worldToJointSpace = Quaternion.LookRotation(normalized, up);
			Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);
			if (space == Space.World)
			{
				resultRotation *= startRotation * Quaternion.Inverse(targetRotation);
			}
			else
			{
				resultRotation *= Quaternion.Inverse(targetRotation) * startRotation;
			}
			resultRotation *= worldToJointSpace;
			joint.targetRotation = resultRotation;
		}

		// Token: 0x060026CD RID: 9933 RVA: 0x0010BD90 File Offset: 0x00109F90
		public static void SetAnimatorDefault(this Animator animator, bool keepState = true)
		{
			foreach (AnimatorControllerParameter param in animator.parameters)
			{
				if (!animator.IsParameterControlledByCurve(param.nameHash))
				{
					AnimatorControllerParameterType type = param.type;
					switch (type)
					{
					case AnimatorControllerParameterType.Float:
						animator.SetFloat(param.nameHash, param.defaultFloat);
						break;
					case (AnimatorControllerParameterType)2:
						break;
					case AnimatorControllerParameterType.Int:
						animator.SetInteger(param.nameHash, param.defaultInt);
						break;
					case AnimatorControllerParameterType.Bool:
						animator.SetBool(param.nameHash, param.defaultBool);
						break;
					default:
						if (type == AnimatorControllerParameterType.Trigger)
						{
							animator.ResetTrigger(param.nameHash);
						}
						break;
					}
				}
			}
			animator.Update(0f);
			animator.keepAnimatorStateOnDisable = keepState;
		}

		// Token: 0x060026CE RID: 9934 RVA: 0x0010BE48 File Offset: 0x0010A048
		public static bool IsStatic(this MemberInfo member)
		{
			if (member == null)
			{
				return false;
			}
			FieldInfo fieldInfo = member as FieldInfo;
			if (fieldInfo != null)
			{
				return fieldInfo.IsStatic;
			}
			PropertyInfo propertyInfo = member as PropertyInfo;
			if (propertyInfo == null)
			{
				MethodBase methodBase = member as MethodBase;
				if (methodBase != null)
				{
					return methodBase.IsStatic;
				}
				EventInfo eventInfo = member as EventInfo;
				if (eventInfo != null)
				{
					return eventInfo.GetRaiseMethod(true).IsStatic;
				}
				Type type = member as Type;
				if (type == null)
				{
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Unable to determine IsStatic for member {0}.{1}MemberType was {2} but only fields, properties, methods, events and types are supported.", member.DeclaringType.FullName, member.Name, member.GetType().FullName));
				}
				return type.IsSealed && type.IsAbstract;
			}
			else
			{
				if (propertyInfo.CanRead)
				{
					return propertyInfo.GetGetMethod(true).IsStatic;
				}
				return propertyInfo.GetSetMethod(true).IsStatic;
			}
		}

		// Token: 0x060026CF RID: 9935 RVA: 0x0010BF10 File Offset: 0x0010A110
		public static bool IsSameOrSubclassOf(this Type type, Type baseType)
		{
			return type == baseType || type.IsSubclassOf(baseType);
		}
	}
}
