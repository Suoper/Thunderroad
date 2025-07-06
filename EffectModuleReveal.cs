using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.Reveal;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000196 RID: 406
	public class EffectModuleReveal : EffectModule
	{
		// Token: 0x0600138E RID: 5006 RVA: 0x0008B098 File Offset: 0x00089298
		public override IEnumerator RefreshCoroutine(EffectData effectData, bool editorLoad = false)
		{
			if (editorLoad)
			{
				this.textureContainer = base.EditorLoad<TextureContainer>(this.maskTextureContainerAddress);
			}
			else
			{
				if (this.textureContainer)
				{
					Catalog.ReleaseAsset<TextureContainer>(this.textureContainer);
				}
				yield return Catalog.LoadAssetCoroutine<TextureContainer>(this.maskTextureContainerAddress, delegate(TextureContainer value)
				{
					this.textureContainer = value;
				}, effectData.id + " (EffectModuleReveal)");
			}
			yield break;
		}

		// Token: 0x0600138F RID: 5007 RVA: 0x0008B0B5 File Offset: 0x000892B5
		public static IEnumerator GeneratePool()
		{
			yield return EffectModuleReveal.ClearPool();
			if (!Catalog.gameData.platformParameters.enableEffectReveal)
			{
				yield break;
			}
			for (int i = 0; i < Catalog.gameData.platformParameters.poolingRevealCount; i++)
			{
				GameObject gameObject = new GameObject("Reveal" + i.ToString());
				gameObject.transform.SetParent(EffectModuleReveal.poolRoot);
				EffectReveal effect = gameObject.AddComponent<EffectReveal>();
				effect.isPooled = true;
				gameObject.SetActive(false);
				EffectModuleReveal.pool.Add(effect);
			}
			yield break;
		}

		// Token: 0x06001390 RID: 5008 RVA: 0x0008B0BD File Offset: 0x000892BD
		public static IEnumerator DespawnAllOutOfPool()
		{
			EffectModuleReveal.poolRoot = GameManager.poolTransform.Find("Reveals");
			if (!EffectModuleReveal.poolRoot)
			{
				EffectModuleReveal.poolRoot = new GameObject("Reveals").transform;
				EffectModuleReveal.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			foreach (EffectReveal effect in EffectModuleReveal.pool)
			{
				if (effect.isOutOfPool)
				{
					effect.Despawn();
				}
			}
			yield return null;
			yield break;
		}

		// Token: 0x06001391 RID: 5009 RVA: 0x0008B0C5 File Offset: 0x000892C5
		public static IEnumerator ClearPool()
		{
			EffectModuleReveal.poolRoot = GameManager.poolTransform.Find("Reveals");
			if (!EffectModuleReveal.poolRoot)
			{
				EffectModuleReveal.poolRoot = new GameObject("Reveals").transform;
				EffectModuleReveal.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			int num;
			for (int i = EffectModuleReveal.pool.Count - 1; i >= 0; i = num - 1)
			{
				EffectModuleReveal.pool[i].isPooled = false;
				UnityEngine.Object.Destroy(EffectModuleReveal.pool[i].gameObject);
				yield return null;
				num = i;
			}
			EffectModuleReveal.pool.Clear();
			yield break;
		}

		// Token: 0x06001392 RID: 5010 RVA: 0x0008B0D0 File Offset: 0x000892D0
		public override bool Spawn(EffectData effectData, Vector3 position, Quaternion rotation, out Effect effect, float intensity = 0f, float speed = 0f, Transform parent = null, CollisionInstance collisionInstance = null, bool pooled = true, ColliderGroup colliderGroup = null)
		{
			if (!base.Spawn(effectData, position, rotation, out effect, intensity, speed, parent, collisionInstance, pooled, colliderGroup))
			{
				return false;
			}
			if (!Catalog.gameData.platformParameters.enableEffectReveal)
			{
				return false;
			}
			CollisionHandler targetCollisionHandler = (collisionInstance != null && collisionInstance.targetColliderGroup) ? collisionInstance.targetColliderGroup.collisionHandler : null;
			if (this.applyOn == EffectReveal.Direction.Source)
			{
				targetCollisionHandler = ((collisionInstance != null && collisionInstance.sourceColliderGroup) ? collisionInstance.sourceColliderGroup.collisionHandler : null);
			}
			if (targetCollisionHandler)
			{
				if (targetCollisionHandler.item)
				{
					if (!this.allowItem || !GameManager.options.enableItemReveal)
					{
						effect = null;
						return false;
					}
				}
				else
				{
					if (!targetCollisionHandler.isRagdollPart)
					{
						effect = null;
						return false;
					}
					if (!this.allowRagdollPart || !GameManager.options.enableCharacterReveal)
					{
						effect = null;
						return false;
					}
				}
				EffectReveal effectToSpawn = null;
				if (pooled)
				{
					foreach (EffectReveal effectPooled in EffectModuleReveal.pool)
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
						foreach (EffectReveal effectPooled2 in EffectModuleReveal.pool)
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
				if (effectToSpawn)
				{
					if (effectToSpawn.gameObject.activeSelf)
					{
						effectToSpawn.Despawn();
					}
					effect = this.Configure(effectToSpawn, effectData, collisionInstance, targetCollisionHandler);
				}
				else
				{
					GameObject obj = new GameObject("reveal");
					effect = this.Configure(obj.AddComponent<EffectReveal>(), effectData, collisionInstance, targetCollisionHandler);
				}
				if (parent)
				{
					effect.transform.SetParent(parent, false);
				}
				else if (Level.current)
				{
					effect.transform.SetParent(Level.current.transform, false);
				}
				effect.transform.SetPositionAndRotation(position, rotation);
				effect.isOutOfPool = true;
				effect.SetIntensity(intensity, false);
				effect.SetSpeed(speed, false);
				LightVolumeReceiver lightVolumeReceiver = effect.lightVolumeReceiver;
				if (lightVolumeReceiver != null)
				{
					lightVolumeReceiver.UpdateRenderers();
				}
				return true;
			}
			effect = null;
			return false;
		}

		// Token: 0x06001393 RID: 5011 RVA: 0x0008B33C File Offset: 0x0008953C
		public static void Despawn(EffectReveal effect)
		{
			if (effect.isPooled && EffectModuleReveal.poolRoot)
			{
				effect.transform.SetParent(EffectModuleReveal.poolRoot);
				effect.transform.localPosition = Vector3.zero;
				effect.transform.localRotation = Quaternion.identity;
				effect.transform.localScale = Vector3.one;
				effect.isOutOfPool = false;
				effect.gameObject.SetActive(false);
				return;
			}
			UnityEngine.Object.Destroy(effect.gameObject);
		}

		// Token: 0x06001394 RID: 5012 RVA: 0x0008B3BC File Offset: 0x000895BC
		public static void DespawnAllPooled()
		{
			foreach (EffectReveal poolObject in EffectModuleReveal.pool)
			{
				if (poolObject.gameObject.activeSelf)
				{
					poolObject.Despawn();
				}
			}
		}

		// Token: 0x06001395 RID: 5013 RVA: 0x0008B41C File Offset: 0x0008961C
		public EffectReveal Configure(EffectReveal effect, EffectData effectData, CollisionInstance collisionInstance, CollisionHandler targetCollisionHandler)
		{
			if (effect.lightVolumeReceiver == null)
			{
				effect.lightVolumeReceiver = effect.GetComponent<LightVolumeReceiver>();
			}
			effect.gameObject.SetActive(true);
			if (this.step == Effect.Step.Custom)
			{
				effect.stepCustomHashId = this.stepCustomIdHash;
			}
			if (effect.revealMaterialControllers == null)
			{
				effect.revealMaterialControllers = new List<RevealMaterialController>();
			}
			effect.revealMaterialControllers.Clear();
			if (targetCollisionHandler.isRagdollPart)
			{
				int renderersCount = targetCollisionHandler.ragdollPart.renderers.Count;
				for (int i = 0; i < renderersCount; i++)
				{
					Creature.RendererData rendererInfo = targetCollisionHandler.ragdollPart.renderers[i];
					if (rendererInfo.revealDecal && ((rendererInfo.revealDecal.type == RevealDecal.Type.Default && this.typeFilter.HasFlagNoGC(EffectModuleReveal.TypeFilter.Default)) || (rendererInfo.revealDecal.type == RevealDecal.Type.Body && this.typeFilter.HasFlagNoGC(EffectModuleReveal.TypeFilter.Body)) || (rendererInfo.revealDecal.type == RevealDecal.Type.Outfit && this.typeFilter.HasFlagNoGC(EffectModuleReveal.TypeFilter.Outfit))))
					{
						effect.revealMaterialControllers.Add(rendererInfo.revealDecal.revealMaterialController);
						RevealMaterialController controller;
						if (rendererInfo.splitRenderer && rendererInfo.splitRenderer.TryGetComponent<RevealMaterialController>(out controller))
						{
							effect.revealMaterialControllers.Add(controller);
						}
					}
				}
			}
			else if (targetCollisionHandler.isItem)
			{
				int revealDecalsCount = targetCollisionHandler.item.revealDecals.Count;
				for (int j = 0; j < revealDecalsCount; j++)
				{
					RevealDecal revealDecal = targetCollisionHandler.item.revealDecals[j];
					if ((revealDecal.type == RevealDecal.Type.Default && this.typeFilter.HasFlagNoGC(EffectModuleReveal.TypeFilter.Default)) || (revealDecal.type == RevealDecal.Type.Body && this.typeFilter.HasFlagNoGC(EffectModuleReveal.TypeFilter.Body)) || (revealDecal.type == RevealDecal.Type.Outfit && this.typeFilter.HasFlagNoGC(EffectModuleReveal.TypeFilter.Outfit)))
					{
						effect.revealMaterialControllers.Add(revealDecal.revealMaterialController);
					}
				}
			}
			effect.revealData = this.revealData;
			effect.collisionHandler = targetCollisionHandler;
			effect.applyOn = this.applyOn;
			effect.depth = this.depth;
			if (collisionInstance != null && collisionInstance.damageStruct.penetration != DamageStruct.Penetration.None)
			{
				effect.minSize = this.penetrationSize;
				effect.maxSize = this.penetrationSize;
				effect.minChannelMultiplier = this.penetrationChannelMultiplier;
				effect.maxChannelMultiplier = this.penetrationChannelMultiplier;
			}
			else
			{
				effect.minSize = this.minSize;
				effect.maxSize = this.maxSize;
				effect.minChannelMultiplier = this.minChannelMultiplier;
				effect.maxChannelMultiplier = this.maxChannelMultiplier;
			}
			if (this.textureContainer == null)
			{
				Debug.LogError("EffectReveal mask texture container is null for textureContainer " + this.maskTextureContainerAddress + ". Is the address correct?");
			}
			effect.maskTexture = this.textureContainer.GetRandomTexture();
			if (effect.maskTexture == null)
			{
				Debug.LogError("EffectReveal mask texture is null for textureContainer " + this.maskTextureContainerAddress + ". Is the container setup with textures?");
			}
			effect.offsetDistance = this.offsetDistance;
			return effect;
		}

		// Token: 0x04001248 RID: 4680
		public string maskTextureContainerAddress;

		// Token: 0x04001249 RID: 4681
		[NonSerialized]
		public TextureContainer textureContainer;

		// Token: 0x0400124A RID: 4682
		public EffectReveal.Direction applyOn = EffectReveal.Direction.Target;

		// Token: 0x0400124B RID: 4683
		public EffectModuleReveal.TypeFilter typeFilter = EffectModuleReveal.TypeFilter.Default | EffectModuleReveal.TypeFilter.Body | EffectModuleReveal.TypeFilter.Outfit;

		// Token: 0x0400124C RID: 4684
		public float depth = 1.2f;

		// Token: 0x0400124D RID: 4685
		public float minSize = 0.05f;

		// Token: 0x0400124E RID: 4686
		public float maxSize = 0.1f;

		// Token: 0x0400124F RID: 4687
		public float penetrationSize = 0.1f;

		// Token: 0x04001250 RID: 4688
		public Vector4 minChannelMultiplier = Vector4.one;

		// Token: 0x04001251 RID: 4689
		public Vector4 maxChannelMultiplier = Vector4.one;

		// Token: 0x04001252 RID: 4690
		public Vector4 penetrationChannelMultiplier = Vector4.one;

		// Token: 0x04001253 RID: 4691
		public float offsetDistance = 0.05f;

		// Token: 0x04001254 RID: 4692
		public bool allowItem;

		// Token: 0x04001255 RID: 4693
		public bool allowRagdollPart;

		// Token: 0x04001256 RID: 4694
		public RevealData[] revealData;

		// Token: 0x04001257 RID: 4695
		public static List<EffectReveal> pool = new List<EffectReveal>();

		// Token: 0x04001258 RID: 4696
		public static Transform poolRoot;

		// Token: 0x020007A5 RID: 1957
		[Flags]
		public enum TypeFilter
		{
			// Token: 0x04003E9E RID: 16030
			Default = 1,
			// Token: 0x04003E9F RID: 16031
			Body = 2,
			// Token: 0x04003EA0 RID: 16032
			Outfit = 4
		}
	}
}
