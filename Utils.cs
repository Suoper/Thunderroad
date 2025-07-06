using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ThunderRoad
{
	// Token: 0x02000361 RID: 865
	public static class Utils
	{
		// Token: 0x0600288C RID: 10380 RVA: 0x00114DC0 File Offset: 0x00112FC0
		public static Color UnHDR(this Color color)
		{
			float largest = (color.r > color.g) ? ((color.r > color.g) ? color.r : color.g) : ((color.b > color.g) ? color.b : color.g);
			if (largest <= 1f)
			{
				return color;
			}
			return new Color(color.r / largest, color.g / largest, color.b / largest, color.a);
		}

		// Token: 0x0600288D RID: 10381 RVA: 0x00114E43 File Offset: 0x00113043
		public static float Clamp(this float input, float low, float high)
		{
			return Mathf.Clamp(input, low, high);
		}

		// Token: 0x0600288E RID: 10382 RVA: 0x00114E4D File Offset: 0x0011304D
		public static float Remap(this float input, float inLow, float inHigh, float outLow, float outHigh)
		{
			return (input - inLow) / (inHigh - inLow) * (outHigh - outLow) + outLow;
		}

		// Token: 0x0600288F RID: 10383 RVA: 0x00114E5D File Offset: 0x0011305D
		public static float RemapClamp(this float input, float inLow, float inHigh, float outLow, float outHigh)
		{
			return Mathf.Clamp((input - inLow) / (inHigh - inLow) * (outHigh - outLow) + outLow, outLow, outHigh);
		}

		// Token: 0x06002890 RID: 10384 RVA: 0x00114E75 File Offset: 0x00113075
		public static float Remap01(this float input, float inLow, float inHigh)
		{
			return (input - inLow) / (inHigh - inLow);
		}

		// Token: 0x06002891 RID: 10385 RVA: 0x00114E7E File Offset: 0x0011307E
		public static float RemapClamp01(this float input, float inLow, float inHigh)
		{
			return Mathf.Clamp((input - inLow) / (inHigh - inLow), 0f, 1f);
		}

		// Token: 0x06002892 RID: 10386 RVA: 0x00114E98 File Offset: 0x00113098
		public static Side Other(this Side side)
		{
			Side result;
			if (side != Side.Right)
			{
				if (side != Side.Left)
				{
					throw new ArgumentOutOfRangeException(string.Format("Side out of range: {0}", side));
				}
				result = Side.Right;
			}
			else
			{
				result = Side.Left;
			}
			return result;
		}

		// Token: 0x06002893 RID: 10387 RVA: 0x00114ECB File Offset: 0x001130CB
		public static Vector3 WithX(this Vector3 vector, float x)
		{
			return new Vector3(x, vector.y, vector.z);
		}

		// Token: 0x06002894 RID: 10388 RVA: 0x00114EDF File Offset: 0x001130DF
		public static Vector3 WithY(this Vector3 vector, float y)
		{
			return new Vector3(vector.x, y, vector.z);
		}

		// Token: 0x06002895 RID: 10389 RVA: 0x00114EF3 File Offset: 0x001130F3
		public static Vector3 WithZ(this Vector3 vector, float z)
		{
			return new Vector3(vector.x, vector.y, z);
		}

		/// <summary>
		/// Shuffle the first <c>length</c> elements of an array using the Fisher-Yates shuffle
		/// </summary>
		/// <param name="array">Input array</param>
		/// <param name="length">Number of elements to shuffle</param>
		/// <returns>A reference to the same, now-shuffled array</returns>
		// Token: 0x06002896 RID: 10390 RVA: 0x00114F08 File Offset: 0x00113108
		public static T[] Shuffle<T>(this T[] array, int length = -1)
		{
			System.Random random = new System.Random();
			int i = (length < 0) ? array.Length : length;
			for (int j = 0; j < i; j++)
			{
				int r = j + (int)(random.NextDouble() * (double)(i - j));
				int num = r;
				int num2 = j;
				T t = array[j];
				T t2 = array[r];
				array[num] = t;
				array[num2] = t2;
			}
			return array;
		}

		// Token: 0x06002897 RID: 10391 RVA: 0x00114F74 File Offset: 0x00113174
		public static T RandomChoice<T>(this List<T> list)
		{
			if (list != null && list.Count > 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			return default(T);
		}

		/// <summary>
		/// Returns the smallest radius or axis of a collider
		/// </summary>
		/// <param name="collider"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		// Token: 0x06002898 RID: 10392 RVA: 0x00114FAC File Offset: 0x001131AC
		public static bool TryGetSmallestRadius(Collider collider, out float radius)
		{
			SphereCollider sphereCollider = collider as SphereCollider;
			if (sphereCollider != null)
			{
				radius = sphereCollider.radius;
				return true;
			}
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			if (capsuleCollider != null)
			{
				float capRadius = capsuleCollider.radius;
				float height = capsuleCollider.height;
				radius = Mathf.Min(capRadius, height / 2f);
				return true;
			}
			BoxCollider boxCollider = collider as BoxCollider;
			if (boxCollider == null)
			{
				radius = 0f;
				return false;
			}
			Vector3 size = boxCollider.size;
			radius = Mathf.Min(new float[]
			{
				size.x,
				size.y,
				size.z
			}) / 2f;
			return true;
		}

		// Token: 0x06002899 RID: 10393 RVA: 0x00115048 File Offset: 0x00113248
		public static bool TryGetScriptableRendererData(int universalRendererDataIndex, out ScriptableRendererData scriptableRendererData)
		{
			UniversalRenderPipelineAsset pipeline = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
			FieldInfo field = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
			ScriptableRendererData[] scriptableRendererDatas = (ScriptableRendererData[])((field != null) ? field.GetValue(pipeline) : null);
			if (scriptableRendererDatas != null && universalRendererDataIndex < scriptableRendererDatas.Length)
			{
				scriptableRendererData = scriptableRendererDatas[universalRendererDataIndex];
				return true;
			}
			scriptableRendererData = null;
			return false;
		}

		// Token: 0x0600289A RID: 10394 RVA: 0x0011509C File Offset: 0x0011329C
		public static bool TryGetFeature(string name, ScriptableRendererData scriptableRendererData, out ScriptableRendererFeature feature)
		{
			feature = null;
			if (scriptableRendererData == null)
			{
				return false;
			}
			foreach (ScriptableRendererFeature rendererFeature in scriptableRendererData.rendererFeatures)
			{
				if (rendererFeature.name.Equals(name))
				{
					feature = rendererFeature;
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600289B RID: 10395 RVA: 0x00115110 File Offset: 0x00113310
		public static T GetOrAddComponent<T>(this Component component) where T : Component
		{
			T t;
			if (component == null)
			{
				t = default(T);
			}
			else
			{
				GameObject gameObject = component.gameObject;
				t = ((gameObject != null) ? gameObject.GetComponent<T>() : default(T));
			}
			T result;
			if ((result = t) == null)
			{
				result = component.gameObject.AddComponent<T>();
			}
			return result;
		}

		/// <summary>
		/// Is the target collection null or empty?
		/// </summary>
		// Token: 0x0600289C RID: 10396 RVA: 0x00115159 File Offset: 0x00113359
		public static bool IsNullOrEmpty(this ICollection collection)
		{
			return collection == null || collection.Count == 0;
		}

		// Token: 0x0600289D RID: 10397 RVA: 0x00115169 File Offset: 0x00113369
		public static bool CountCheck(this ICollection collection, Func<int, bool> check)
		{
			return collection != null && check(collection.Count);
		}

		// Token: 0x0600289E RID: 10398 RVA: 0x0011517C File Offset: 0x0011337C
		public static bool CountEquals(this ICollection collection, int count)
		{
			return collection.CountCheck((int c) => c == count);
		}

		/// <summary>
		/// Is the target array null or empty?
		/// </summary>
		// Token: 0x0600289F RID: 10399 RVA: 0x001151A8 File Offset: 0x001133A8
		public static bool IsNullOrEmpty(this Array array)
		{
			return array == null || array.Length == 0;
		}

		// Token: 0x060028A0 RID: 10400 RVA: 0x001151B8 File Offset: 0x001133B8
		public static bool CountCheck(this Array array, Func<int, bool> check)
		{
			return array != null && check(array.Length);
		}

		// Token: 0x060028A1 RID: 10401 RVA: 0x001151CC File Offset: 0x001133CC
		public static bool CountEquals(this Array array, int count)
		{
			return array.CountCheck((int c) => c == count);
		}

		// Token: 0x060028A2 RID: 10402 RVA: 0x001151F8 File Offset: 0x001133F8
		public static bool IsNullAndEmpty(this AnimationCurve animationCurve)
		{
			return animationCurve != null && animationCurve.keys.Length == 0;
		}

		// Token: 0x060028A3 RID: 10403 RVA: 0x0011520C File Offset: 0x0011340C
		public static void SetAndApplyGamma(this Texture2D tex, float gamma = 2.2f)
		{
			float invVal = 1f / gamma;
			Color[] colors = tex.GetPixels();
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = new Color(Mathf.Pow(colors[i].r, invVal), Mathf.Pow(colors[i].g, invVal), Mathf.Pow(colors[i].b, invVal), colors[i].a);
			}
			tex.SetPixels(colors);
			tex.Apply();
		}

		// Token: 0x060028A4 RID: 10404 RVA: 0x00115291 File Offset: 0x00113491
		public static string ToDetailedString(this Vector3 v)
		{
			return string.Format("<{0}, {1}, {2}>", v.x, v.y, v.z);
		}

		// Token: 0x060028A5 RID: 10405 RVA: 0x001152BE File Offset: 0x001134BE
		public static float RoundDownToNearest(float value, float roundto)
		{
			if (roundto == 0f)
			{
				return value;
			}
			return Mathf.Floor(value / roundto) * roundto;
		}

		// Token: 0x060028A6 RID: 10406 RVA: 0x001152D4 File Offset: 0x001134D4
		public static Vector2 GetAngles(Transform reference, Vector3 targetPosition)
		{
			Vector3 targetDirection = targetPosition - reference.position;
			targetDirection = reference.InverseTransformDirection(targetDirection);
			float x = Mathf.Atan2(targetDirection.x, targetDirection.z) * 57.29578f;
			float angleOnY = Mathf.Atan2(targetDirection.y, targetDirection.z) * 57.29578f;
			return new Vector2(x, angleOnY);
		}

		// Token: 0x060028A7 RID: 10407 RVA: 0x0011532B File Offset: 0x0011352B
		public static Quaternion LookRotation(Vector3 direction, Vector3 localDirection, Vector3 upward)
		{
			return Quaternion.LookRotation(direction, upward) * Quaternion.FromToRotation(Vector3.forward, -localDirection);
		}

		// Token: 0x060028A8 RID: 10408 RVA: 0x00115349 File Offset: 0x00113549
		public static Quaternion QuaternionFromTo(Quaternion from, Quaternion to)
		{
			return from.To(to);
		}

		// Token: 0x060028A9 RID: 10409 RVA: 0x00115354 File Offset: 0x00113554
		public static Quaternion QuaternionSlerpTowards(Quaternion current, Quaternion target, float maxAngleDelta)
		{
			float totalAngleDelta = Quaternion.Angle(current, target);
			if (totalAngleDelta.IsApproximately(0f))
			{
				return target;
			}
			return Quaternion.Slerp(current, target, Mathf.Clamp01(maxAngleDelta / totalAngleDelta));
		}

		// Token: 0x060028AA RID: 10410 RVA: 0x00115388 File Offset: 0x00113588
		public static Quaternion QuaternionSmoothSlerp(Quaternion current, Quaternion target, ref float angularVel, float smoothTime, float maxSpeed = float.PositiveInfinity, float deltaTime = 0f)
		{
			if (deltaTime == 0f)
			{
				deltaTime = Time.deltaTime;
			}
			float angleDelta = Quaternion.Angle(current, target);
			if (angleDelta.IsApproximately(0f))
			{
				return target;
			}
			float angleStep = Mathf.SmoothDampAngle(angleDelta, 0f, ref angularVel, smoothTime, maxSpeed, deltaTime);
			return Quaternion.Slerp(current, target, 1f - angleStep / angleDelta);
		}

		// Token: 0x060028AB RID: 10411 RVA: 0x001153E0 File Offset: 0x001135E0
		public static Vector3 Vector3SlerpTowards(Vector3 current, Vector3 target, float maxAngleDelta)
		{
			float totalAngleDelta = Vector3.Angle(current, target);
			if (totalAngleDelta.IsApproximately(0f))
			{
				return target;
			}
			return Vector3.Slerp(current, target, Mathf.Clamp01(maxAngleDelta / totalAngleDelta));
		}

		// Token: 0x060028AC RID: 10412 RVA: 0x00115414 File Offset: 0x00113614
		public static Vector3 Vector3SmoothSlerp(Vector3 current, Vector3 target, ref float angularVel, float smoothTime, float maxSpeed = float.PositiveInfinity, float deltaTime = 0f)
		{
			if (deltaTime == 0f)
			{
				deltaTime = Time.deltaTime;
			}
			float angleDelta = Vector3.Angle(current, target);
			if (angleDelta.IsApproximately(0f))
			{
				return target;
			}
			float angleStep = Mathf.SmoothDampAngle(angleDelta, 0f, ref angularVel, smoothTime, maxSpeed, deltaTime);
			return Vector3.Slerp(current, target, 1f - angleStep / angleDelta);
		}

		// Token: 0x060028AD RID: 10413 RVA: 0x0011546A File Offset: 0x0011366A
		public static float FlatAngleAroundAxis(Vector3 from, Vector3 to, Vector3 axis)
		{
			return Vector3.SignedAngle(Vector3.ProjectOnPlane(from, axis), Vector3.ProjectOnPlane(to, axis), axis);
		}

		// Token: 0x060028AE RID: 10414 RVA: 0x00115480 File Offset: 0x00113680
		public static Vector3 ClampMagnitude(Vector3 v, float max, float min)
		{
			double sm = (double)v.sqrMagnitude;
			if (sm > (double)max * (double)max)
			{
				return v.normalized * max;
			}
			if (sm < (double)min * (double)min)
			{
				return v.normalized * min;
			}
			return v;
		}

		// Token: 0x060028AF RID: 10415 RVA: 0x001154C4 File Offset: 0x001136C4
		public static Vector3 GetLinearVelocityAtPoint(Rigidbody rigidbody, Vector3 position, Vector3 velocity, Vector3 angularVelocity)
		{
			Vector3 worldCenterOfMass = rigidbody.transform.TransformPoint(rigidbody.centerOfMass);
			return Vector3.Cross(angularVelocity, position - worldCenterOfMass) + velocity;
		}

		// Token: 0x060028B0 RID: 10416 RVA: 0x001154F6 File Offset: 0x001136F6
		public static Color ThreeColorLerp(Color min, Color mid, Color max, float t)
		{
			return Utils.ThreePointLerp(min, mid, max, t);
		}

		// Token: 0x060028B1 RID: 10417 RVA: 0x00115515 File Offset: 0x00113715
		public static Vector4 ThreePointLerp(Vector4 min, Vector4 mid, Vector4 max, float t)
		{
			return Utils.MultiPointLerp(t, new Vector4[]
			{
				min,
				mid,
				max
			});
		}

		// Token: 0x060028B2 RID: 10418 RVA: 0x0011553C File Offset: 0x0011373C
		public static Vector4 MultiPointLerp(float t, params Vector4[] orderedPoints)
		{
			t = Mathf.Clamp01(t);
			int zones = orderedPoints.Length - 1;
			float step = 1f / (float)zones;
			int lower = Mathf.FloorToInt((float)zones * t);
			int upper = Mathf.CeilToInt((float)zones * t);
			return Vector4.Lerp(orderedPoints[lower], orderedPoints[upper], Mathf.InverseLerp(step * (float)lower, step * (float)upper, t));
		}

		// Token: 0x060028B3 RID: 10419 RVA: 0x00115598 File Offset: 0x00113798
		public static string GetTransformPath(Transform root, Transform transform)
		{
			string path = transform.name;
			while (transform.parent != root)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}
			return path;
		}

		// Token: 0x060028B4 RID: 10420 RVA: 0x001155D8 File Offset: 0x001137D8
		public static bool CalculateProjectileLaunchVector(Vector3 toTarget, float speed, out Vector3 launchVector, float gravityMultiplier = 1f)
		{
			launchVector = Vector3.zero;
			Vector3 gravity = Physics.gravity * gravityMultiplier;
			float gSquared = gravity.sqrMagnitude;
			float b = speed * speed + Vector3.Dot(toTarget, gravity);
			float discriminant = b * b - gSquared * toTarget.sqrMagnitude;
			if (discriminant >= 0f)
			{
				float discRoot = Mathf.Sqrt(discriminant);
				float time = Mathf.Sqrt((b - discRoot) * 2f / gSquared);
				launchVector = toTarget / time - gravity * time / 2f;
				return true;
			}
			return false;
		}

		// Token: 0x060028B5 RID: 10421 RVA: 0x0011566C File Offset: 0x0011386C
		public static AxisDirection GetAxisDirection(Vector2 axis, float deadzone = 0f)
		{
			if (axis.magnitude > deadzone)
			{
				float axisAngle = Utils.GetAxisAngle(axis);
				if (axisAngle > 315f || axisAngle <= 45f)
				{
					return AxisDirection.Up;
				}
				if (axisAngle > 45f && axisAngle <= 135f)
				{
					return AxisDirection.Right;
				}
				if (axisAngle > 135f && axisAngle <= 225f)
				{
					return AxisDirection.Down;
				}
				if (axisAngle > 225f && axisAngle <= 315f)
				{
					return AxisDirection.Left;
				}
			}
			return AxisDirection.None;
		}

		// Token: 0x060028B6 RID: 10422 RVA: 0x001156D4 File Offset: 0x001138D4
		public static float GetAxisAngle(Vector2 axis)
		{
			float angle = Mathf.Atan2(axis.y, axis.x) * 57.29578f;
			angle = 90f - angle;
			if (angle < 0f)
			{
				angle += 360f;
			}
			return angle;
		}

		// Token: 0x060028B7 RID: 10423 RVA: 0x00115712 File Offset: 0x00113912
		public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
		{
			float startVolume = audioSource.volume;
			while (audioSource.volume > 0f)
			{
				audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
				yield return null;
			}
			audioSource.Stop();
			audioSource.volume = startVolume;
			yield break;
		}

		// Token: 0x060028B8 RID: 10424 RVA: 0x00115728 File Offset: 0x00113928
		public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
		{
			float startVolume = 0.2f;
			audioSource.volume = 0f;
			audioSource.Play();
			while (audioSource.volume < 1f)
			{
				audioSource.volume += startVolume * Time.deltaTime / FadeTime;
				yield return null;
			}
			audioSource.volume = 1f;
			yield break;
		}

		// Token: 0x060028B9 RID: 10425 RVA: 0x0011573E File Offset: 0x0011393E
		public static bool RadiusTouching(Vector3 pointA, float radiusA, Vector3 pointB, float radiusB)
		{
			return pointA.PointInRadius(pointB, radiusA + radiusB);
		}

		// Token: 0x060028BA RID: 10426 RVA: 0x0011574C File Offset: 0x0011394C
		public static bool ClosestPointsOnTwoLines(Vector3 lineStart1, Vector3 lineEnd1, Vector3 lineStart2, Vector3 lineEnd2, out Vector3 closestPointLine1, out Vector3 closestPointLine2)
		{
			Vector3 lineVec = (lineEnd1 - lineStart1).normalized;
			Vector3 lineVec2 = (lineEnd2 - lineStart2).normalized;
			closestPointLine1 = Vector3.zero;
			closestPointLine2 = Vector3.zero;
			float a = Vector3.Dot(lineVec, lineVec);
			float b = Vector3.Dot(lineVec, lineVec2);
			float e = Vector3.Dot(lineVec2, lineVec2);
			float d = a * e - b * b;
			if (d != 0f)
			{
				Vector3 r = lineStart1 - lineStart2;
				float c = Vector3.Dot(lineVec, r);
				float f = Vector3.Dot(lineVec2, r);
				float s = (b * f - c * e) / d;
				float t = (a * f - c * b) / d;
				closestPointLine1 = lineStart1 + lineVec * Mathf.Clamp(s, 0f, Vector3.Distance(lineStart1, lineEnd1));
				closestPointLine2 = lineStart2 + lineVec2 * Mathf.Clamp(t, 0f, Vector3.Distance(lineStart2, lineEnd2));
				Vector3 closest = Utils.ClosestPointOnLine(lineStart2, lineEnd2, closestPointLine1);
				Vector3 closest2 = Utils.ClosestPointOnLine(lineStart1, lineEnd1, closestPointLine2);
				if ((closestPointLine1 - closest).sqrMagnitude < (closestPointLine2 - closest2).sqrMagnitude)
				{
					closestPointLine2 = closest;
				}
				else
				{
					closestPointLine1 = closest2;
				}
				return true;
			}
			Vector3 lineCenter = Vector3.Lerp(lineStart1, lineEnd1, 0.5f);
			Vector3 centerLocalPos2 = Matrix4x4.TRS(Vector3.Lerp(lineStart2, lineEnd2, 0.5f), Quaternion.LookRotation(lineVec2), Vector3.one).inverse.MultiplyPoint3x4(lineCenter);
			float lenght2 = Vector3.Distance(lineStart2, lineEnd2);
			closestPointLine1 = lineCenter;
			closestPointLine2 = Utils.ClosestPointOnLine(lineStart2, lineEnd2, lineCenter);
			if (centerLocalPos2.z > lenght2 * 0.5f)
			{
				closestPointLine1 = Utils.ClosestPointOnLine(lineStart1, lineEnd1, lineEnd2);
				closestPointLine2 = lineEnd2;
			}
			else if (centerLocalPos2.z < -lenght2 * 0.5f)
			{
				closestPointLine1 = Utils.ClosestPointOnLine(lineStart1, lineEnd1, lineStart2);
				closestPointLine2 = lineStart2;
			}
			return false;
		}

		// Token: 0x060028BB RID: 10427 RVA: 0x00115964 File Offset: 0x00113B64
		public static Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
		{
			Vector3 line = lineEnd - lineStart;
			float len = line.magnitude;
			line.Normalize();
			float d = Vector3.Dot(point - lineStart, line);
			d = Mathf.Clamp(d, 0f, len);
			return lineStart + line * d;
		}

		// Token: 0x060028BC RID: 10428 RVA: 0x001159B0 File Offset: 0x00113BB0
		public static Vector3 MiddleOfPoints(params Vector3[] points)
		{
			if (points.IsNullOrEmpty())
			{
				return Vector3.zero;
			}
			Vector3 result = Vector3.zero;
			foreach (Vector3 point in points)
			{
				result += point;
			}
			return result / (float)points.Length;
		}

		// Token: 0x060028BD RID: 10429 RVA: 0x001159FB File Offset: 0x00113BFB
		public static float AngleOfThreePoints(Vector3 reference, Vector3 endPointA, Vector3 endPointB)
		{
			return Vector3.Angle(endPointA - reference, endPointB - reference);
		}

		// Token: 0x060028BE RID: 10430 RVA: 0x00115A10 File Offset: 0x00113C10
		public static float InverseLerpVector3(Vector3 a, Vector3 b, Vector3 value)
		{
			Vector3 AB = b - a;
			return Vector3.Dot(value - a, AB) / Vector3.Dot(AB, AB);
		}

		// Token: 0x060028BF RID: 10431 RVA: 0x00115A3C File Offset: 0x00113C3C
		public static bool IsInside(this Vector3 point, BoxCollider boxCollider)
		{
			point = boxCollider.transform.InverseTransformPoint(point) - boxCollider.center;
			Vector3 size = boxCollider.size;
			float l_HalfX = size.x * 0.5f;
			float l_HalfY = size.y * 0.5f;
			float l_HalfZ = size.z * 0.5f;
			return point.x < l_HalfX && point.x > -l_HalfX && point.y < l_HalfY && point.y > -l_HalfY && point.z < l_HalfZ && point.z > -l_HalfZ;
		}

		// Token: 0x060028C0 RID: 10432 RVA: 0x00115AC9 File Offset: 0x00113CC9
		public static bool IsInside(this Vector3 point, SphereCollider sphereCollider)
		{
			return Vector3.Distance(sphereCollider.transform.TransformPoint(sphereCollider.center), point) < sphereCollider.radius;
		}

		// Token: 0x060028C1 RID: 10433 RVA: 0x00115AEC File Offset: 0x00113CEC
		public static bool IsInside(this Vector3 point, CapsuleCollider capsuleCollider)
		{
			Transform colliderTransform = capsuleCollider.transform;
			float halfHeight = Mathf.Clamp(capsuleCollider.height * 0.5f - capsuleCollider.radius, 0f, float.PositiveInfinity);
			if (halfHeight > 0f)
			{
				Vector3 lineHeight = new Vector3((capsuleCollider.direction == 0) ? halfHeight : 0f, (capsuleCollider.direction == 1) ? halfHeight : 0f, (capsuleCollider.direction == 2) ? halfHeight : 0f);
				Vector3 colLineStart = colliderTransform.TransformPoint(capsuleCollider.center - lineHeight);
				Vector3 colLineEnd = colliderTransform.TransformPoint(capsuleCollider.center + lineHeight);
				if (Vector3.Distance(Utils.ClosestPointOnLine(colLineStart, colLineEnd, point), point) < capsuleCollider.radius)
				{
					return true;
				}
			}
			else if (Vector3.Distance(colliderTransform.TransformPoint(capsuleCollider.center), point) < capsuleCollider.radius)
			{
				return true;
			}
			return false;
		}

		// Token: 0x060028C2 RID: 10434 RVA: 0x00115BC4 File Offset: 0x00113DC4
		public static bool InsideCollider(Collider collider, Vector3 point)
		{
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			if (capsuleCollider == null)
			{
				SphereCollider sphereCollider = collider as SphereCollider;
				if (sphereCollider == null)
				{
					BoxCollider boxCollider = collider as BoxCollider;
					if (boxCollider == null)
					{
						if (!(collider is MeshCollider) && !(collider is TerrainCollider) && !(collider is WheelCollider) && !(collider is CharacterController))
						{
						}
					}
					else if (point.IsInside(boxCollider))
					{
						return true;
					}
				}
				else if (point.IsInside(sphereCollider))
				{
					return true;
				}
			}
			else if (point.IsInside(capsuleCollider))
			{
				return true;
			}
			return false;
		}

		// Token: 0x060028C3 RID: 10435 RVA: 0x00115C34 File Offset: 0x00113E34
		public static Vector3 ClosestPointOnSurface(Collider collider, Vector3 point, out bool isInside)
		{
			isInside = false;
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			if (capsuleCollider)
			{
				float demiHeight = Mathf.Clamp(capsuleCollider.height * 0.5f - capsuleCollider.radius, 0f, float.PositiveInfinity);
				if (demiHeight > 0f)
				{
					Vector3 lineHeight = new Vector3((capsuleCollider.direction == 0) ? demiHeight : 0f, (capsuleCollider.direction == 1) ? demiHeight : 0f, (capsuleCollider.direction == 2) ? demiHeight : 0f);
					Vector3 vector = collider.transform.TransformPoint(capsuleCollider.center - lineHeight);
					Vector3 colLineEnd = collider.transform.TransformPoint(capsuleCollider.center + lineHeight);
					Debug.DrawLine(vector, colLineEnd, Color.white);
					Vector3 closestCenterOnLine = Utils.ClosestPointOnLine(vector, colLineEnd, point);
					if (Vector3.Distance(closestCenterOnLine, point) < capsuleCollider.radius)
					{
						isInside = true;
					}
					return closestCenterOnLine + (point - closestCenterOnLine).normalized * capsuleCollider.radius;
				}
				Vector3 center = collider.transform.TransformPoint(capsuleCollider.center);
				if (Vector3.Distance(center, point) < capsuleCollider.radius)
				{
					isInside = true;
				}
				return center + (point - center).normalized * capsuleCollider.radius;
			}
			else
			{
				SphereCollider sphereCollider = collider as SphereCollider;
				if (sphereCollider)
				{
					Vector3 center2 = collider.transform.TransformPoint(sphereCollider.center);
					if (Vector3.Distance(center2, point) < capsuleCollider.radius)
					{
						isInside = true;
					}
					return center2 + (point - center2).normalized * sphereCollider.radius;
				}
				Debug.LogError("This collider type is not supported");
				return Vector3.zero;
			}
		}

		// Token: 0x060028C4 RID: 10436 RVA: 0x00115DF0 File Offset: 0x00113FF0
		public static void ClosestPointOnSurface(Collider collider, Vector3 lineStart, Vector3 lineStop, out Vector3 colliderPoint, out Vector3 linePoint, out bool isInside)
		{
			isInside = false;
			CapsuleCollider capsuleCollider = collider as CapsuleCollider;
			if (capsuleCollider)
			{
				float demiHeight = Mathf.Clamp(capsuleCollider.height * 0.5f - capsuleCollider.radius, 0f, float.PositiveInfinity);
				if (demiHeight > 0f)
				{
					Vector3 lineHeight = new Vector3((capsuleCollider.direction == 0) ? demiHeight : 0f, (capsuleCollider.direction == 1) ? demiHeight : 0f, (capsuleCollider.direction == 2) ? demiHeight : 0f);
					Vector3 colLineStart = collider.transform.TransformPoint(capsuleCollider.center - lineHeight);
					Vector3 colLineEnd = collider.transform.TransformPoint(capsuleCollider.center + lineHeight);
					Debug.DrawLine(colLineStart, colLineEnd, Color.white);
					Utils.ClosestPointsOnTwoLines(lineStart, lineStop, colLineStart, colLineEnd, out linePoint, out colliderPoint);
					if (Vector3.Distance(colliderPoint, linePoint) < capsuleCollider.radius)
					{
						isInside = true;
					}
					colliderPoint += (linePoint - colliderPoint).normalized * capsuleCollider.radius;
					return;
				}
				Vector3 center = collider.transform.TransformPoint(capsuleCollider.center);
				linePoint = Utils.ClosestPointOnLine(lineStart, lineStop, center);
				if (Vector3.Distance(linePoint, center) < capsuleCollider.radius)
				{
					isInside = true;
				}
				colliderPoint = center + (linePoint - center).normalized * capsuleCollider.radius;
				return;
			}
			else
			{
				SphereCollider sphereCollider = collider as SphereCollider;
				if (sphereCollider)
				{
					Vector3 center2 = collider.transform.TransformPoint(sphereCollider.center);
					linePoint = Utils.ClosestPointOnLine(lineStart, lineStop, center2);
					if (Vector3.Distance(linePoint, center2) < sphereCollider.radius)
					{
						isInside = true;
					}
					colliderPoint = center2 + (linePoint - center2).normalized * sphereCollider.radius;
					return;
				}
				Debug.LogError("This collider type is not supported");
				colliderPoint = Vector3.zero;
				linePoint = Vector3.zero;
				return;
			}
		}

		// Token: 0x060028C5 RID: 10437 RVA: 0x0011602C File Offset: 0x0011422C
		public static Bounds GetCombinedBoundingBox(Transform root)
		{
			if (root == null)
			{
				throw new ArgumentException("The supplied transform was null");
			}
			Collider[] componentsInChildren = root.GetComponentsInChildren<Collider>();
			if (componentsInChildren.Length == 0)
			{
				throw new ArgumentException("The supplied transform " + ((root != null) ? root.name : null) + " does not have any children with colliders");
			}
			Bounds totalBBox = componentsInChildren[0].bounds;
			foreach (Collider collider in componentsInChildren)
			{
				totalBBox.Encapsulate(collider.bounds);
			}
			return totalBBox;
		}

		// Token: 0x060028C6 RID: 10438 RVA: 0x001160A4 File Offset: 0x001142A4
		public static string AddSpacesToSentence(string text, bool preserveAcronyms)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}
			StringBuilder newText = new StringBuilder(text.Length * 2);
			newText.Append(text[0]);
			for (int i = 1; i < text.Length; i++)
			{
				if (char.IsUpper(text[i]) && ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) || (preserveAcronyms && char.IsUpper(text[i - 1]) && i < text.Length - 1 && !char.IsUpper(text[i + 1]))))
				{
					newText.Append(' ');
				}
				newText.Append(text[i]);
			}
			return newText.ToString();
		}

		// Token: 0x060028C7 RID: 10439 RVA: 0x00116164 File Offset: 0x00114364
		public static Color Rainbow(float progress)
		{
			float num = Mathf.Abs(progress % 1f) * 6f;
			int ascending = (int)(num % 1f * 255f);
			int descending = 255 - ascending;
			switch ((int)num)
			{
			case 0:
				return Utils.FromArgb(255, 255, ascending, 0);
			case 1:
				return Utils.FromArgb(255, descending, 255, 0);
			case 2:
				return Utils.FromArgb(255, 0, 255, ascending);
			case 3:
				return Utils.FromArgb(255, 0, descending, 255);
			case 4:
				return Utils.FromArgb(255, ascending, 0, 255);
			default:
				return Utils.FromArgb(255, 255, 0, descending);
			}
		}

		// Token: 0x060028C8 RID: 10440 RVA: 0x00116224 File Offset: 0x00114424
		public static Color ColorFromHSV(double hue, double saturation, double value)
		{
			int hi = Convert.ToInt32(Math.Floor(hue / 60.0)) % 6;
			double f = hue / 60.0 - Math.Floor(hue / 60.0);
			value *= 255.0;
			int v = Convert.ToInt32(value);
			int p = Convert.ToInt32(value * (1.0 - saturation));
			int q = Convert.ToInt32(value * (1.0 - f * saturation));
			int t = Convert.ToInt32(value * (1.0 - (1.0 - f) * saturation));
			if (hi == 0)
			{
				return Utils.FromArgb(255, v, t, p);
			}
			if (hi == 1)
			{
				return Utils.FromArgb(255, q, v, p);
			}
			if (hi == 2)
			{
				return Utils.FromArgb(255, p, v, t);
			}
			if (hi == 3)
			{
				return Utils.FromArgb(255, p, q, v);
			}
			if (hi == 4)
			{
				return Utils.FromArgb(255, t, p, v);
			}
			return Utils.FromArgb(255, v, p, q);
		}

		// Token: 0x060028C9 RID: 10441 RVA: 0x00116330 File Offset: 0x00114530
		public static Color FromArgb(int alpha, int red, int green, int blue)
		{
			float fa = (float)alpha / 255f;
			float r = (float)red / 255f;
			float fg = (float)green / 255f;
			float fb = (float)blue / 255f;
			return new Color(r, fg, fb, fa);
		}

		// Token: 0x060028CA RID: 10442 RVA: 0x00116368 File Offset: 0x00114568
		public static Vector3 ClosestDirection(Vector3 direction, Cardinal cardinal)
		{
			float maxDot = float.NegativeInfinity;
			Vector3 ret = Vector3.zero;
			foreach (Vector3 dir in (cardinal == Cardinal.XYZ) ? Utils.cardinalXYZ : ((cardinal == Cardinal.XZ) ? Utils.cardinalXZ : ((cardinal == Cardinal.X) ? Utils.cardinalX : Utils.cardinalZ)))
			{
				float t = Vector3.Dot(direction, dir);
				if (t > maxDot)
				{
					ret = dir;
					maxDot = t;
				}
			}
			return ret;
		}

		// Token: 0x060028CB RID: 10443 RVA: 0x001163D4 File Offset: 0x001145D4
		public static void IgnoreLayerCollision(int layer, LayerMask layerMask)
		{
			for (int layerValue = 0; layerValue < 32; layerValue++)
			{
				if (LayerMask.LayerToName(layerValue) != "")
				{
					Physics.IgnoreLayerCollision(layer, layerValue, false);
					if (layerMask == (layerMask | 1 << layerValue))
					{
						Physics.IgnoreLayerCollision(layer, layerValue, true);
					}
				}
			}
		}

		// Token: 0x060028CC RID: 10444 RVA: 0x00116425 File Offset: 0x00114625
		public static float PercentageBetween(float start, float current, float end)
		{
			return (current - start) / (end - start);
		}

		// Token: 0x060028CD RID: 10445 RVA: 0x0011642E File Offset: 0x0011462E
		public static float CalculateRatio(float input, float inputMin, float inputMax, float outputMin, float outputMax)
		{
			if (input > inputMax)
			{
				input = inputMax;
			}
			if (input < inputMin)
			{
				input = inputMin;
			}
			return (input - inputMin) / (inputMax - inputMin) * (outputMax - outputMin) + outputMin;
		}

		// Token: 0x060028CE RID: 10446 RVA: 0x0011644C File Offset: 0x0011464C
		public static Vector3 PreventNaN(this Vector3 vector)
		{
			if (float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z))
			{
				return Vector3.zero;
			}
			return vector;
		}

		// Token: 0x060028CF RID: 10447 RVA: 0x0011647C File Offset: 0x0011467C
		public static int GetLayerMask(params LayerName[] layers)
		{
			string[] layerNames = new string[layers.Length];
			for (int i = 0; i < layers.Length; i++)
			{
				layerNames[i] = layers[i].ToString();
			}
			return LayerMask.GetMask(layerNames);
		}

		// Token: 0x060028D0 RID: 10448 RVA: 0x001164BB File Offset: 0x001146BB
		public static LayerMask GetLayer(LayerType layerType)
		{
			return LayerMask.NameToLayer(layerType.ToString());
		}

		// Token: 0x060028D1 RID: 10449 RVA: 0x001164D4 File Offset: 0x001146D4
		public static void SetPositiveLocalScale(Transform transform)
		{
			transform.transform.localScale = new Vector3(Mathf.Abs(transform.transform.localScale.x), Mathf.Abs(transform.transform.localScale.y), Mathf.Abs(transform.transform.localScale.z));
		}

		// Token: 0x060028D2 RID: 10450 RVA: 0x00116530 File Offset: 0x00114730
		public static Component GetClosest(Vector3 position, Component[] others)
		{
			if (others.Length == 1)
			{
				return others[0];
			}
			Component bestTarget = null;
			float closestDistanceSqr = float.PositiveInfinity;
			int count = others.Length;
			for (int i = 0; i < count; i++)
			{
				Component potentialTarget = others[i];
				if (Utils.TryGetCloserTarget(position, potentialTarget.transform.position, ref closestDistanceSqr))
				{
					bestTarget = potentialTarget;
				}
			}
			return bestTarget;
		}

		/// <summary>
		/// This will return true if the distanceSqr between position and targetPosition is less than closestDistanceSqr and will update closestDistanceSqr with the new distanceSqr
		/// </summary>
		/// <param name="position"></param>
		/// <param name="targetPosition"></param>
		/// <param name="closestDistanceSqr"></param>
		/// <returns></returns>
		// Token: 0x060028D3 RID: 10451 RVA: 0x00116580 File Offset: 0x00114780
		public static bool TryGetCloserTarget(Vector3 position, Vector3 targetPosition, ref float closestDistanceSqr)
		{
			float dSqrToTarget = (targetPosition - position).sqrMagnitude;
			if (dSqrToTarget < closestDistanceSqr)
			{
				closestDistanceSqr = dSqrToTarget;
				return true;
			}
			return false;
		}

		// Token: 0x060028D4 RID: 10452 RVA: 0x001165A8 File Offset: 0x001147A8
		public static Collider GetClosest(Vector3 position, Collider[] others, out Vector3 closestColliderPoint, int arrayLength = 0)
		{
			closestColliderPoint = Vector3.zero;
			if (others.Length == 1)
			{
				closestColliderPoint = others[0].ClosestPoint(position);
				return others[0];
			}
			Collider closestCollider = null;
			float closestDistanceSqr = float.PositiveInfinity;
			for (int i = 0; i < ((arrayLength < 1) ? others.Length : arrayLength); i++)
			{
				if (!(others[i] is MeshCollider) || (others[i] as MeshCollider).convex)
				{
					Vector3 closestPoint = others[i].ClosestPoint(position);
					if (Utils.TryGetCloserTarget(position, closestPoint, ref closestDistanceSqr))
					{
						closestColliderPoint = closestPoint;
						closestCollider = others[i];
					}
				}
			}
			return closestCollider;
		}

		// Token: 0x060028D5 RID: 10453 RVA: 0x00116630 File Offset: 0x00114830
		public static MonoBehaviour GetClosest(Transform source, List<MonoBehaviour> others)
		{
			return Utils.GetClosest(source, others.ToArray());
		}

		// Token: 0x060028D6 RID: 10454 RVA: 0x0011663E File Offset: 0x0011483E
		public static MonoBehaviour GetClosest(Vector3 position, List<MonoBehaviour> others)
		{
			return Utils.GetClosest(position, others.ToArray());
		}

		// Token: 0x060028D7 RID: 10455 RVA: 0x0011664C File Offset: 0x0011484C
		public static MonoBehaviour GetClosest(Transform source, MonoBehaviour[] others)
		{
			return Utils.GetClosest(source.position, others) as MonoBehaviour;
		}

		// Token: 0x060028D8 RID: 10456 RVA: 0x0011666C File Offset: 0x0011486C
		public static MonoBehaviour GetClosest(Vector3 position, MonoBehaviour[] others)
		{
			return Utils.GetClosest(position, others) as MonoBehaviour;
		}

		// Token: 0x060028D9 RID: 10457 RVA: 0x00116688 File Offset: 0x00114888
		public static List<Transform> GetClosestList(Transform source, List<Transform> others)
		{
			return (from o in others
			orderby (o.position - source.position).sqrMagnitude
			select o).ToList<Transform>();
		}

		// Token: 0x060028DA RID: 10458 RVA: 0x001166B9 File Offset: 0x001148B9
		public static Vector3 RoundVector3(Vector3 vector3, int dp = 10)
		{
			return new Vector3(Mathf.Round(vector3.x * (float)dp) / (float)dp, Mathf.Round(vector3.y * (float)dp) / (float)dp, Mathf.Round(vector3.z * (float)dp) / (float)dp);
		}

		// Token: 0x060028DB RID: 10459 RVA: 0x001166F3 File Offset: 0x001148F3
		[return: TupleElementNames(new string[]
		{
			"source",
			"element"
		})]
		public static IEnumerable<ValueTuple<int, T>> CombinedEnumerable<T>(params IEnumerable<T>[] enumerables)
		{
			int num;
			for (int e = 0; e < enumerables.Length; e = num + 1)
			{
				foreach (T element in enumerables[e])
				{
					yield return new ValueTuple<int, T>(e, element);
				}
				IEnumerator<T> enumerator = null;
				num = e;
			}
			yield break;
			yield break;
		}

		// Token: 0x060028DC RID: 10460 RVA: 0x00116704 File Offset: 0x00114904
		public static void MoveToFrontHead(Transform transf, Transform head, float distance, Transform finger = null)
		{
			if (finger)
			{
				Vector3 viewDirection = finger.position - head.position;
				transf.position = finger.position + viewDirection.normalized * distance;
				transf.rotation = Quaternion.LookRotation(viewDirection);
				return;
			}
			transf.position = head.position + head.forward.normalized * distance;
			transf.rotation = Quaternion.LookRotation(head.forward);
		}

		// Token: 0x060028DD RID: 10461 RVA: 0x0011678C File Offset: 0x0011498C
		public static CharacterJoint CloneCharacterJoint(CharacterJoint source, GameObject destination, Rigidbody newRigidbody = null)
		{
			CharacterJoint characterJoint = destination.AddComponent<CharacterJoint>();
			characterJoint.connectedBody = (newRigidbody ? newRigidbody : source.connectedBody);
			characterJoint.anchor = source.anchor;
			characterJoint.axis = source.axis;
			characterJoint.autoConfigureConnectedAnchor = source.autoConfigureConnectedAnchor;
			characterJoint.connectedAnchor = source.connectedAnchor;
			characterJoint.swingAxis = source.swingAxis;
			characterJoint.twistLimitSpring = source.twistLimitSpring;
			characterJoint.lowTwistLimit = source.lowTwistLimit;
			characterJoint.highTwistLimit = source.highTwistLimit;
			characterJoint.swingLimitSpring = source.swingLimitSpring;
			characterJoint.swing1Limit = source.swing1Limit;
			characterJoint.swing2Limit = source.swing2Limit;
			characterJoint.enableProjection = source.enableProjection;
			characterJoint.projectionDistance = source.projectionDistance;
			characterJoint.projectionAngle = source.projectionAngle;
			characterJoint.breakForce = source.breakForce;
			characterJoint.breakTorque = source.breakTorque;
			characterJoint.enableCollision = source.enableCollision;
			characterJoint.enablePreprocessing = source.enablePreprocessing;
			characterJoint.massScale = source.massScale;
			characterJoint.connectedMassScale = source.connectedMassScale;
			return characterJoint;
		}

		// Token: 0x060028DE RID: 10462 RVA: 0x001168A8 File Offset: 0x00114AA8
		public static Component CopyComponent(Component original, GameObject destination)
		{
			Type type = original.GetType();
			Component copy = destination.AddComponent(type);
			foreach (FieldInfo field in type.GetFields())
			{
				field.SetValue(copy, field.GetValue(original));
			}
			return copy;
		}

		// Token: 0x060028DF RID: 10463 RVA: 0x001168F0 File Offset: 0x00114AF0
		public static void CopyComponentFieldsValues(Component sourceComponent, Component destinationComponent, bool publicOnly, bool readOnlyAttribute)
		{
			foreach (KeyValuePair<FieldInfo, object> fieldValue in Utils.GetComponentFieldsValues(sourceComponent))
			{
				if ((!publicOnly || fieldValue.Key.IsPublic) && (!readOnlyAttribute || !Attribute.IsDefined(fieldValue.Key, typeof(ReadOnlyAttribute))))
				{
					fieldValue.Key.SetValue(destinationComponent, fieldValue.Value);
				}
			}
		}

		// Token: 0x060028E0 RID: 10464 RVA: 0x0011697C File Offset: 0x00114B7C
		public static Dictionary<FieldInfo, object> GetComponentFieldsValues(Component component)
		{
			Dictionary<FieldInfo, object> fieldsValues = new Dictionary<FieldInfo, object>();
			foreach (FieldInfo field in component.GetType().GetFields())
			{
				fieldsValues.Add(field, field.GetValue(component));
			}
			return fieldsValues;
		}

		// Token: 0x060028E1 RID: 10465 RVA: 0x001169BC File Offset: 0x00114BBC
		public static void SetComponentFieldsValues(Component component, Dictionary<FieldInfo, object> fieldsValues)
		{
			foreach (KeyValuePair<FieldInfo, object> fieldValue in fieldsValues)
			{
				fieldValue.Key.SetValue(component, fieldValue.Value);
			}
		}

		// Token: 0x060028E2 RID: 10466 RVA: 0x00116A18 File Offset: 0x00114C18
		public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
		{
			return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).MultiplyPoint3x4(position);
		}

		// Token: 0x060028E3 RID: 10467 RVA: 0x00116A44 File Offset: 0x00114C44
		public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
		{
			return Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse.MultiplyPoint3x4(position);
		}

		// Token: 0x060028E4 RID: 10468 RVA: 0x00116A78 File Offset: 0x00114C78
		public static AnimationCurve Curve(params float[] values)
		{
			AnimationCurve curve = new AnimationCurve();
			int i = 0;
			foreach (float value in values)
			{
				curve.AddKey((float)i / ((float)values.Length - 1f), value);
				i++;
			}
			return curve;
		}

		// Token: 0x060028E5 RID: 10469 RVA: 0x00116AC0 File Offset: 0x00114CC0
		public static AnimationCurve Multiply(this AnimationCurve curve, float multiplier)
		{
			AnimationCurve newCurve = new AnimationCurve();
			foreach (Keyframe value in curve.keys)
			{
				newCurve.AddKey(new Keyframe(value.time, value.value * multiplier, value.inTangent, value.outTangent, value.inWeight, value.outWeight));
			}
			return newCurve;
		}

		// Token: 0x060028E6 RID: 10470 RVA: 0x00116B2C File Offset: 0x00114D2C
		public static Gradient CreateGradient(Color start, Color end)
		{
			Gradient gradient = new Gradient();
			gradient.SetKeys(new GradientColorKey[]
			{
				new GradientColorKey(start, 0f),
				new GradientColorKey(end, 1f)
			}, new GradientAlphaKey[]
			{
				new GradientAlphaKey(start.a, 0f),
				new GradientAlphaKey(end.a, 1f)
			});
			return gradient;
		}

		// Token: 0x060028E7 RID: 10471 RVA: 0x00116BA4 File Offset: 0x00114DA4
		public static string SplitCamelCase(string input)
		{
			return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
		}

		// Token: 0x060028E8 RID: 10472 RVA: 0x00116BBC File Offset: 0x00114DBC
		public static string UnicodeToInternationalCharacters(string str)
		{
			MatchCollection mc = Regex.Matches(str, "([\\w]+)|(\\\\u([\\w]{4}))");
			if (mc.Count <= 0)
			{
				return str;
			}
			StringBuilder sb = new StringBuilder();
			foreach (object obj in mc)
			{
				string v = ((Match)obj).Value;
				if (v.StartsWith("\\"))
				{
					string text = v.Substring(2);
					byte[] codes = new byte[2];
					int code = Convert.ToInt32(text.Substring(0, 2), 16);
					int code2 = Convert.ToInt32(text.Substring(2), 16);
					codes[0] = (byte)code2;
					codes[1] = (byte)code;
					sb.Append(Encoding.Unicode.GetString(codes));
				}
				else
				{
					sb.Append(v);
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Convert audio mixer volume decibels in the scale [-80,0] to [0-100]%
		/// </summary>
		/// <param name="decibels">Decibels = [-80,0]</param>
		/// <returns>[0,100] percentage value</returns>
		// Token: 0x060028E9 RID: 10473 RVA: 0x00116C9C File Offset: 0x00114E9C
		public static int DecibelsToPercentage(float decibels)
		{
			if (decibels <= -80f)
			{
				return 0;
			}
			return (int)(Mathf.Pow(10f, decibels / 20f) * 100f);
		}

		/// <summary>
		/// Convert audio mixer volume slider value in the scale [0-100]% to [-80,0] decibels
		/// </summary>
		/// <param name="percentage">Percentage = [0,100]</param>
		/// <returns>[-80,0] decibel value</returns>
		// Token: 0x060028EA RID: 10474 RVA: 0x00116CC0 File Offset: 0x00114EC0
		public static float PercentageToDecibels(float percentage)
		{
			if (percentage <= 0f)
			{
				return -80f;
			}
			return 20f * Mathf.Log10(percentage / 100f);
		}

		// Token: 0x060028EB RID: 10475 RVA: 0x00116CE4 File Offset: 0x00114EE4
		public static Task<bool> CopyDirectoryAsync(string sourceDir, string destinationDir, bool recursive)
		{
			Utils.<CopyDirectoryAsync>d__100 <CopyDirectoryAsync>d__;
			<CopyDirectoryAsync>d__.<>t__builder = AsyncTaskMethodBuilder<bool>.Create();
			<CopyDirectoryAsync>d__.sourceDir = sourceDir;
			<CopyDirectoryAsync>d__.destinationDir = destinationDir;
			<CopyDirectoryAsync>d__.recursive = recursive;
			<CopyDirectoryAsync>d__.<>1__state = -1;
			<CopyDirectoryAsync>d__.<>t__builder.Start<Utils.<CopyDirectoryAsync>d__100>(ref <CopyDirectoryAsync>d__);
			return <CopyDirectoryAsync>d__.<>t__builder.Task;
		}

		/// <summary>
		/// Clone a directory.
		/// </summary>
		// Token: 0x060028EC RID: 10476 RVA: 0x00116D38 File Offset: 0x00114F38
		public static bool CopyDirectory(string sourceDir, string destinationDir, bool recursive)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDir);
			if (!dir.Exists)
			{
				return false;
			}
			DirectoryInfo[] dirs = dir.GetDirectories();
			Directory.CreateDirectory(destinationDir);
			foreach (FileInfo file in dir.GetFiles())
			{
				string targetFilePath = Path.Combine(destinationDir, file.Name);
				file.CopyTo(targetFilePath, true);
			}
			if (recursive)
			{
				foreach (DirectoryInfo subDir in dirs)
				{
					string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
					Utils.CopyDirectory(subDir.FullName, newDestinationDir, true);
				}
			}
			return true;
		}

		/// <summary>
		/// Returns the flat distance (ignoring the y axs) between two positions. 
		/// </summary>
		// Token: 0x060028ED RID: 10477 RVA: 0x00116DD4 File Offset: 0x00114FD4
		public static float FlatDistance(Vector3 a, Vector3 b)
		{
			Vector3 a2 = a.ToXZ();
			Vector3 flatB = b.ToXZ();
			return (a2 - flatB).sqrMagnitude;
		}

		// Token: 0x060028EE RID: 10478 RVA: 0x00116DFC File Offset: 0x00114FFC
		public static float[] PredictFutureDistances(Vector3 objectA, Vector3 velocityA, Vector3 objectB, Vector3 velocityB, params float[] deltaTimes)
		{
			float[] distances = new float[deltaTimes.Length];
			Utils.PredictFutureDistancesNonAlloc(objectA, velocityA, objectB, velocityB, distances, deltaTimes);
			return distances;
		}

		// Token: 0x060028EF RID: 10479 RVA: 0x00116E20 File Offset: 0x00115020
		public static void PredictFutureDistancesNonAlloc(Vector3 objectA, Vector3 velocityA, Vector3 objectB, Vector3 velocityB, float[] resultDistances, params float[] deltaTimes)
		{
			for (int i = 0; i < deltaTimes.Length; i++)
			{
				float time = deltaTimes[i];
				resultDistances[i] = (Vector3.LerpUnclamped(objectB, objectB + velocityB, time) - Vector3.LerpUnclamped(objectA, objectA + velocityA, time)).magnitude;
			}
		}

		// Token: 0x060028F0 RID: 10480 RVA: 0x00116E70 File Offset: 0x00115070
		public static float[] PredictFutureDistancesScaled(Transform objectA, Vector3 velocityA, Vector3 objectB, Vector3 velocityB, params float[] deltaTimes)
		{
			float[] distances = new float[deltaTimes.Length];
			Utils.PredictFutureDistancesScaledNonAlloc(objectA, velocityA, objectB, velocityB, distances, deltaTimes);
			return distances;
		}

		// Token: 0x060028F1 RID: 10481 RVA: 0x00116E94 File Offset: 0x00115094
		public static void PredictFutureDistancesScaledNonAlloc(Transform objectA, Vector3 velocityA, Vector3 objectB, Vector3 velocityB, float[] resultDistances, params float[] deltaTimes)
		{
			for (int i = 0; i < deltaTimes.Length; i++)
			{
				float time = deltaTimes[i];
				resultDistances[i] = (objectA.InverseTransformPoint(Vector3.LerpUnclamped(objectB, objectB + velocityB, time)) - objectA.InverseTransformPoint(Vector3.LerpUnclamped(objectA.position, objectA.position + velocityA, time))).magnitude;
			}
		}

		/// <summary>
		/// Returns true if this string is null, empty, or contains only whitespace.
		/// </summary>
		/// <param name="str">The string to check.</param>
		/// <returns>True if this string is null, empty, or contains only whitespace. Otherwise, it returns false.</returns>
		// Token: 0x060028F2 RID: 10482 RVA: 0x00116EF8 File Offset: 0x001150F8
		public static bool IsNullOrEmptyOrWhitespace(this string str)
		{
			return string.IsNullOrEmpty(str) || str.All(new Func<char, bool>(char.IsWhiteSpace));
		}

		/// <summary>
		/// Remove the tab characters from a string
		/// </summary>
		/// <returns>The same text with the tab characters replaced by empty space characters.</returns>
		// Token: 0x060028F3 RID: 10483 RVA: 0x00116F18 File Offset: 0x00115118
		public static string RemoveTabs(string text)
		{
			char tab = '\t';
			return text.Replace(tab, ' ');
		}

		// Token: 0x060028F4 RID: 10484 RVA: 0x00116F34 File Offset: 0x00115134
		public static bool AddressableResourceExists<T>(object key)
		{
			using (IEnumerator<IResourceLocator> enumerator = Addressables.ResourceLocators.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IList<IResourceLocation> list;
					if (enumerator.Current.Locate(key, typeof(T), out list))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060028F5 RID: 10485 RVA: 0x00116F94 File Offset: 0x00115194
		public static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			return Mathf.Pow(1f - t, 3f) * p0 + 3f * Mathf.Pow(1f - t, 2f) * t * p1 + 3f * (1f - t) * t * t * p2 + t * t * t * p3;
		}

		// Token: 0x060028F6 RID: 10486 RVA: 0x00117011 File Offset: 0x00115211
		public static IEnumerator LoopOver(Action<float> action, float duration, Action finish = null, float step = 0f, bool realtime = false)
		{
			return Utils.LoopRoutine(action, duration, finish, step, realtime);
		}

		// Token: 0x060028F7 RID: 10487 RVA: 0x0011701E File Offset: 0x0011521E
		public static Coroutine LoopOver(this MonoBehaviour gameObject, Action<float> action, float duration, Action finish = null, float step = 0f, bool realtime = false)
		{
			if (gameObject == null)
			{
				return null;
			}
			return gameObject.StartCoroutine(Utils.LoopRoutine(action, duration, finish, step, realtime));
		}

		// Token: 0x060028F8 RID: 10488 RVA: 0x00117037 File Offset: 0x00115237
		public static Coroutine LoopOver(this MonoBehaviour gameObject, Action<float, object> action, object parameter, float duration, Action finish = null, float step = 0f, bool realtime = false)
		{
			if (gameObject == null)
			{
				return null;
			}
			return gameObject.StartCoroutine(Utils.LoopRoutine(action, duration, parameter, finish, step, realtime));
		}

		// Token: 0x060028F9 RID: 10489 RVA: 0x00117052 File Offset: 0x00115252
		public static Coroutine RunAfter(this MonoBehaviour gameObject, IEnumerator enumerator, float delay, bool realtime = false)
		{
			if (gameObject == null)
			{
				return null;
			}
			return gameObject.StartCoroutine(Utils.RunAfterRoutine(enumerator, delay, realtime));
		}

		// Token: 0x060028FA RID: 10490 RVA: 0x00117067 File Offset: 0x00115267
		public static Coroutine RunAfter(this MonoBehaviour gameObject, Action action, float delay, bool realtime = false)
		{
			if (gameObject == null)
			{
				return null;
			}
			return gameObject.StartCoroutine(Utils.RunAfterRoutine(action, delay, realtime));
		}

		// Token: 0x060028FB RID: 10491 RVA: 0x0011707C File Offset: 0x0011527C
		public static IEnumerator LoopRoutine(Action<float> action, float duration, Action finish, float step, bool realtime)
		{
			float startTime = realtime ? Time.unscaledTime : Time.time;
			for (;;)
			{
				float value = (realtime ? Time.unscaledTime : Time.time) - startTime;
				if (value >= duration)
				{
					break;
				}
				if (action != null)
				{
					action(value / duration);
				}
				yield return (step == 0f) ? 0 : new WaitForSeconds(step);
			}
			if (action != null)
			{
				action(1f);
			}
			if (finish != null)
			{
				finish();
			}
			yield break;
		}

		// Token: 0x060028FC RID: 10492 RVA: 0x001170A8 File Offset: 0x001152A8
		public static IEnumerator LoopRoutine(Action<float, object> action, float duration, object parameter, Action finish, float step, bool realtime)
		{
			float startTime = realtime ? Time.unscaledTime : Time.time;
			for (;;)
			{
				float value = (realtime ? Time.unscaledTime : Time.time) - startTime;
				if (value >= duration)
				{
					break;
				}
				try
				{
					if (action != null)
					{
						action(value / duration, parameter);
					}
				}
				catch (NullReferenceException exception)
				{
					Debug.LogException(exception);
				}
				yield return (step == 0f) ? 0 : new WaitForSeconds(step);
			}
			try
			{
				if (action != null)
				{
					action(1f, parameter);
				}
				if (finish != null)
				{
					finish();
				}
				yield break;
			}
			catch (NullReferenceException exception2)
			{
				Debug.LogException(exception2);
				yield break;
			}
			yield break;
		}

		// Token: 0x060028FD RID: 10493 RVA: 0x001170DC File Offset: 0x001152DC
		public static IEnumerator RunAfterRoutine(IEnumerator enumerator, float delay, bool realtime = false)
		{
			yield return realtime ? new WaitForSecondsRealtime(delay) : new WaitForSeconds(delay);
			yield return enumerator;
			yield break;
		}

		// Token: 0x060028FE RID: 10494 RVA: 0x001170F9 File Offset: 0x001152F9
		public static IEnumerator RunAfterRoutine(Action action, float delay, bool realtime = false)
		{
			yield return realtime ? new WaitForSecondsRealtime(delay) : new WaitForSeconds(delay);
			if (action != null)
			{
				action();
			}
			yield break;
		}

		// Token: 0x04002723 RID: 10019
		private static Vector3[] cardinalXYZ = new Vector3[]
		{
			Vector3.left,
			Vector3.right,
			Vector3.forward,
			Vector3.back,
			Vector3.up,
			Vector3.down
		};

		// Token: 0x04002724 RID: 10020
		private static Vector3[] cardinalXZ = new Vector3[]
		{
			Vector3.left,
			Vector3.right,
			Vector3.forward,
			Vector3.back
		};

		// Token: 0x04002725 RID: 10021
		private static Vector3[] cardinalX = new Vector3[]
		{
			Vector3.left,
			Vector3.right
		};

		// Token: 0x04002726 RID: 10022
		private static Vector3[] cardinalZ = new Vector3[]
		{
			Vector3.forward,
			Vector3.back
		};

		// Token: 0x02000A4E RID: 2638
		public class CurveBuilder
		{
			// Token: 0x060045BC RID: 17852 RVA: 0x001968CB File Offset: 0x00194ACB
			public CurveBuilder()
			{
				this.keys = new List<Keyframe>();
			}

			// Token: 0x060045BD RID: 17853 RVA: 0x001968DE File Offset: 0x00194ADE
			public Utils.CurveBuilder WithKey(float time, float value)
			{
				this.keys.Add(new Keyframe(time, value));
				return this;
			}

			// Token: 0x060045BE RID: 17854 RVA: 0x001968F3 File Offset: 0x00194AF3
			public Utils.CurveBuilder WithKey(float time, float value, float inTangent, float outTangent)
			{
				this.keys.Add(new Keyframe(time, value, inTangent, outTangent));
				return this;
			}

			// Token: 0x060045BF RID: 17855 RVA: 0x0019690B File Offset: 0x00194B0B
			public AnimationCurve Build()
			{
				return new AnimationCurve(this.keys.ToArray());
			}

			// Token: 0x040047BE RID: 18366
			public List<Keyframe> keys;
		}
	}
}
