using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000193 RID: 403
	public class EffectModuleLight : EffectModule
	{
		// Token: 0x06001357 RID: 4951 RVA: 0x0008893C File Offset: 0x00086B3C
		public override bool Spawn(EffectData effectData, Vector3 position, Quaternion rotation, out Effect effect, float intensity = 0f, float speed = 0f, Transform parent = null, CollisionInstance collisionInstance = null, bool pooled = true, ColliderGroup colliderGroup = null)
		{
			if (!base.Spawn(effectData, position, rotation, out effect, intensity, speed, parent, collisionInstance, pooled, colliderGroup))
			{
				return false;
			}
			if (!Catalog.gameData.platformParameters.enableEffectLight)
			{
				return false;
			}
			EffectLight effectToSpawn = null;
			if (pooled)
			{
				foreach (EffectLight effectPooled in EffectModuleLight.pool)
				{
					if (!effectPooled.isOutOfPool)
					{
						effectToSpawn = effectPooled;
						break;
					}
				}
				if (!effectToSpawn)
				{
					float olderLifeTime = 0f;
					foreach (EffectLight effectPooled2 in EffectModuleLight.pool)
					{
						float lifeTime = Time.time - effectPooled2.playTime;
						if (lifeTime > olderLifeTime)
						{
							olderLifeTime = lifeTime;
							effectToSpawn = effectPooled2;
						}
					}
				}
			}
			if (pooled && effectToSpawn)
			{
				if (effectToSpawn.gameObject.activeSelf)
				{
					effectToSpawn.Despawn();
				}
				effect = this.Configure(effectToSpawn);
			}
			else
			{
				GameObject obj = new GameObject("Light_" + effectData.id);
				effect = this.Configure(obj.AddComponent<EffectLight>());
			}
			if (parent)
			{
				effect.transform.SetParent(parent, false);
			}
			else if (Level.current)
			{
				effect.transform.SetParent(Level.current.transform, false);
			}
			effect.transform.SetPositionAndRotation(position, rotation * this.localRotation);
			effect.isOutOfPool = true;
			effect.SetIntensity(intensity, false);
			effect.SetSpeed(speed, false);
			EffectLight effectLight = effect as EffectLight;
			if (effectLight != null)
			{
				Light light = effectLight.pointLight;
				if (light != null)
				{
					Light light2 = light;
					Texture cookie2;
					if (colliderGroup != null)
					{
						Texture cookie = colliderGroup.cookie;
						if (cookie != null)
						{
							cookie2 = cookie;
							goto IL_1C5;
						}
					}
					cookie2 = null;
					IL_1C5:
					light2.cookie = cookie2;
				}
			}
			return true;
		}

		// Token: 0x06001358 RID: 4952 RVA: 0x00088B30 File Offset: 0x00086D30
		public EffectLight Configure(EffectLight light)
		{
			light.gameObject.SetActive(true);
			light.intensityCurve = this.intensityCurve;
			light.intensitySmoothFactor = this.intensitySmoothFactor;
			light.rangeCurve = this.rangeCurve;
			light.rangeSmoothFactor = this.rangeSmoothFactor;
			this.colorCurve = Utils.CreateGradient(this.colorStart, this.colorEnd);
			light.colorCurve = this.colorCurve;
			light.colorSmoothFactor = this.colorSmoothFactor;
			light.flickerIntensityCurve = this.flickerIntensityCurve;
			light.flickerRateCurve = this.flickerRateCurve;
			light.loopFadeDelay = this.loopFadeDelay;
			return light;
		}

		// Token: 0x06001359 RID: 4953 RVA: 0x00088BCD File Offset: 0x00086DCD
		public static IEnumerator GeneratePool()
		{
			yield return EffectModuleLight.ClearPool();
			if (!Catalog.gameData.platformParameters.enableEffectLight)
			{
				yield break;
			}
			for (int i = 0; i < Catalog.gameData.platformParameters.poolingLightCount; i++)
			{
				GameObject gameObject = new GameObject("Light" + i.ToString());
				gameObject.transform.SetParent(EffectModuleLight.poolRoot);
				EffectLight effectController = gameObject.AddComponent<EffectLight>();
				effectController.isPooled = true;
				gameObject.SetActive(false);
				EffectModuleLight.pool.Add(effectController);
			}
			yield break;
		}

		// Token: 0x0600135A RID: 4954 RVA: 0x00088BD5 File Offset: 0x00086DD5
		public static IEnumerator DespawnAllOutOfPool()
		{
			EffectModuleLight.poolRoot = GameManager.poolTransform.Find("Light");
			if (!EffectModuleLight.poolRoot)
			{
				EffectModuleLight.poolRoot = new GameObject("Light").transform;
				EffectModuleLight.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			foreach (EffectLight effect in EffectModuleLight.pool)
			{
				if (effect.isOutOfPool)
				{
					effect.Despawn();
				}
			}
			yield return null;
			yield break;
		}

		// Token: 0x0600135B RID: 4955 RVA: 0x00088BDD File Offset: 0x00086DDD
		public static IEnumerator ClearPool()
		{
			EffectModuleLight.poolRoot = GameManager.poolTransform.Find("Light");
			if (!EffectModuleLight.poolRoot)
			{
				EffectModuleLight.poolRoot = new GameObject("Light").transform;
				EffectModuleLight.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			int num;
			for (int i = EffectModuleLight.pool.Count - 1; i >= 0; i = num - 1)
			{
				EffectModuleLight.pool[i].isPooled = false;
				UnityEngine.Object.Destroy(EffectModuleLight.pool[i].gameObject);
				yield return null;
				num = i;
			}
			EffectModuleLight.pool.Clear();
			yield break;
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x00088BE8 File Offset: 0x00086DE8
		public static void Despawn(EffectLight effect)
		{
			if (effect.isPooled && EffectModuleLight.poolRoot)
			{
				effect.transform.SetParent(EffectModuleLight.poolRoot);
				effect.transform.localPosition = Vector3.zero;
				effect.transform.localRotation = Quaternion.identity;
				effect.transform.localScale = Vector3.one;
				effect.isOutOfPool = false;
				effect.gameObject.SetActive(false);
				return;
			}
			UnityEngine.Object.Destroy(effect.gameObject);
		}

		// Token: 0x040011F6 RID: 4598
		public Quaternion localRotation;

		// Token: 0x040011F7 RID: 4599
		public new AnimationCurve intensityCurve = AnimationCurve.Constant(0f, 1f, 1f);

		// Token: 0x040011F8 RID: 4600
		public float intensitySmoothFactor;

		// Token: 0x040011F9 RID: 4601
		public AnimationCurve rangeCurve = AnimationCurve.Constant(0f, 1f, 1f);

		// Token: 0x040011FA RID: 4602
		public float rangeSmoothFactor;

		// Token: 0x040011FB RID: 4603
		public Color colorStart;

		// Token: 0x040011FC RID: 4604
		public Color colorEnd;

		// Token: 0x040011FD RID: 4605
		[NonSerialized]
		protected Gradient colorCurve;

		// Token: 0x040011FE RID: 4606
		public float colorSmoothFactor;

		// Token: 0x040011FF RID: 4607
		public float loopFadeDelay;

		// Token: 0x04001200 RID: 4608
		public AnimationCurve flickerIntensityCurve = AnimationCurve.Constant(0f, 1f, 1f);

		// Token: 0x04001201 RID: 4609
		public AnimationCurve flickerRateCurve = AnimationCurve.Constant(0f, 1f, 0f);

		// Token: 0x04001202 RID: 4610
		public static List<EffectLight> pool = new List<EffectLight>();

		// Token: 0x04001203 RID: 4611
		public static Transform poolRoot;
	}
}
