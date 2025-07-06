using System;
using System.Collections.Generic;
using ThunderRoad.Manikin;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x02000305 RID: 773
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Areas/LightProbeVolumeReceiver.html")]
	public class LightVolumeReceiver : ThunderBehaviour
	{
		// Token: 0x1700023D RID: 573
		// (get) Token: 0x060024ED RID: 9453 RVA: 0x000FD7D9 File Offset: 0x000FB9D9
		// (set) Token: 0x060024EE RID: 9454 RVA: 0x000FD7E1 File Offset: 0x000FB9E1
		public LightProbeVolume currentLightProbeVolume
		{
			get
			{
				return this._currentLightProbeVolume;
			}
			set
			{
				if (this._currentLightProbeVolume == value)
				{
					return;
				}
				if (this._currentLightProbeVolume != null)
				{
					this._currentLightProbeVolume.UnregisterMaterials(this);
				}
				this._currentLightProbeVolume = value;
			}
		}

		// Token: 0x1400011F RID: 287
		// (add) Token: 0x060024EF RID: 9455 RVA: 0x000FD814 File Offset: 0x000FBA14
		// (remove) Token: 0x060024F0 RID: 9456 RVA: 0x000FD84C File Offset: 0x000FBA4C
		public event LightVolumeReceiver.OnVolumeChangeDelegate onVolumeChangeEvent;

		// Token: 0x060024F1 RID: 9457 RVA: 0x000FD884 File Offset: 0x000FBA84
		private void OnDestroy()
		{
			this.ClearProbeVolumes();
			this.RemoveParentVolumeListener(false);
			if (this.item)
			{
				this.item.OnDespawnEvent -= this.OnDespawn;
				this.item.OnSnapEvent -= this.OnSnap;
				this.item.OnUnSnapEvent -= this.OnUnSnap;
			}
			if (this.creature)
			{
				this.creature.OnDespawnEvent -= this.OnDespawn;
				if (this.creature.ragdoll)
				{
					this.creature.ragdoll.rootPart.collisionHandler.OnTriggerEnterEvent -= this.OnRootRagdollPartTriggerEnter;
					this.creature.ragdoll.rootPart.collisionHandler.OnTriggerExitEvent -= this.OnRootRagdollPartTriggerExit;
				}
			}
			if (this.effect)
			{
				Effect effect = this.effect;
				effect.despawnCallback = (Effect.DespawnCallback)Delegate.Remove(effect.despawnCallback, new Effect.DespawnCallback(this.OnEffectDespawn));
			}
			this.area = null;
		}

		// Token: 0x060024F2 RID: 9458 RVA: 0x000FD9B0 File Offset: 0x000FBBB0
		private void Awake()
		{
			this.materialPropertyBlock = new MaterialPropertyBlock();
			base.TryGetComponent<Item>(out this.item);
			base.TryGetComponent<Creature>(out this.creature);
			base.TryGetComponent<Effect>(out this.effect);
			if (this.item)
			{
				this.item.OnDespawnEvent += this.OnDespawn;
				this.item.OnSnapEvent += this.OnSnap;
				this.item.OnUnSnapEvent += this.OnUnSnap;
			}
			if (this.creature)
			{
				this.creature.OnDespawnEvent += this.OnDespawn;
			}
			if (this.effect)
			{
				Effect effect = this.effect;
				effect.despawnCallback = (Effect.DespawnCallback)Delegate.Remove(effect.despawnCallback, new Effect.DespawnCallback(this.OnEffectDespawn));
				Effect effect2 = this.effect;
				effect2.despawnCallback = (Effect.DespawnCallback)Delegate.Combine(effect2.despawnCallback, new Effect.DespawnCallback(this.OnEffectDespawn));
			}
			this.parentReceiver = (base.transform.parent ? base.transform.parent.GetComponentInParent<LightVolumeReceiver>(true) : null);
			if (this.parentReceiver)
			{
				this.parentReceiver.onVolumeChangeEvent += this.OnParentVolumeChange;
			}
		}

		// Token: 0x060024F3 RID: 9459 RVA: 0x000FDB10 File Offset: 0x000FBD10
		private void Start()
		{
			if (this.initRenderersOnStart && !this.initRenderersDone)
			{
				this.InitRenderers();
			}
			if (Application.isPlaying)
			{
				if (this.method == LightVolumeReceiver.Method.SRPBatching)
				{
					for (int i = this.materialInstances.Count - 1; i >= 0; i--)
					{
						MeshRenderer meshRenderer = this.materialInstances[i].GetComponent<MeshRenderer>();
						if (meshRenderer != null && meshRenderer.lightmapIndex >= 0)
						{
							this.materialInstances.RemoveAt(i);
						}
					}
				}
				else if (this.method == LightVolumeReceiver.Method.GPUInstancing)
				{
					for (int j = this.renderers.Count - 1; j >= 0; j--)
					{
						int lightmapIndex = this.renderers[j].lightmapIndex;
					}
				}
			}
			if (this.creature && this.creature.ragdoll)
			{
				this.creature.ragdoll.rootPart.collisionHandler.OnTriggerEnterEvent += this.OnRootRagdollPartTriggerEnter;
				this.creature.ragdoll.rootPart.collisionHandler.OnTriggerExitEvent += this.OnRootRagdollPartTriggerExit;
			}
			this.StaticMeshUpdate();
		}

		// Token: 0x060024F4 RID: 9460 RVA: 0x000FDC38 File Offset: 0x000FBE38
		protected void InitRenderers()
		{
			List<Renderer> renderers = new List<Renderer>();
			base.gameObject.GetComponentsInChildren<Renderer>(true, renderers);
			this.SetRenderers(renderers, this.addMaterialInstances);
			this.initRenderersDone = true;
		}

		// Token: 0x060024F5 RID: 9461 RVA: 0x000FDC6C File Offset: 0x000FBE6C
		protected void RemoveParentVolumeListener(bool initProbeVolumeList = false)
		{
			if (this.parentReceiver)
			{
				this.parentReceiver.onVolumeChangeEvent -= this.OnParentVolumeChange;
				if (initProbeVolumeList)
				{
					this.lightProbeVolumes = new List<LightProbeVolume>(this.parentReceiver.lightProbeVolumes);
				}
				this.parentReceiver = null;
			}
		}

		// Token: 0x060024F6 RID: 9462 RVA: 0x000FDCBD File Offset: 0x000FBEBD
		private void StaticMeshUpdate()
		{
			if (this.volumeDetection != LightVolumeReceiver.VolumeDetection.StaticPerMesh)
			{
				return;
			}
			if (!this.area)
			{
				this.area = base.GetComponentInParent<Area>();
			}
			if (!this.initRenderersDone)
			{
				this.InitRenderers();
			}
			this.UpdateRenderers();
			this.staticMeshFirstRunDone = true;
		}

		// Token: 0x060024F7 RID: 9463 RVA: 0x000FDCFC File Offset: 0x000FBEFC
		protected override void ManagedOnEnable()
		{
			base.ManagedOnEnable();
			if (this.staticMeshFirstRunDone)
			{
				this.StaticMeshUpdate();
			}
		}

		// Token: 0x060024F8 RID: 9464 RVA: 0x000FDD14 File Offset: 0x000FBF14
		public void AddMaterial()
		{
			foreach (Renderer renderer in this.renderers)
			{
				List<Material> mats = new List<Material>();
				renderer.GetMaterials(mats);
				foreach (Material material in mats)
				{
					Debug.Log("Mat: " + material.name);
				}
				renderer.SetPropertyBlock(this.materialPropertyBlock);
			}
		}

		// Token: 0x060024F9 RID: 9465 RVA: 0x000FDDC8 File Offset: 0x000FBFC8
		public void SetRenderers(List<Renderer> providedRenderers, bool addMaterialInstances = true)
		{
			this.materialInstances.Clear();
			this.renderers.Clear();
			for (int i = 0; i < providedRenderers.Count; i++)
			{
				Renderer renderer = providedRenderers[i];
				if (this.method == LightVolumeReceiver.Method.SRPBatching)
				{
					MaterialInstance materialInstance;
					ManikinProperties manikinProperties;
					if (renderer.gameObject.TryGetComponent<MaterialInstance>(out materialInstance))
					{
						this.materialInstances.Add(materialInstance);
					}
					else if (addMaterialInstances && renderer.lightmapIndex < 0 && !renderer.gameObject.TryGetComponent<ManikinProperties>(out manikinProperties))
					{
						materialInstance = renderer.gameObject.AddComponent<MaterialInstance>();
						this.materialInstances.Add(materialInstance);
					}
				}
				else if (this.method == LightVolumeReceiver.Method.GPUInstancing)
				{
					this.renderers.Add(renderer);
				}
			}
		}

		// Token: 0x060024FA RID: 9466 RVA: 0x000FDE76 File Offset: 0x000FC076
		protected void OnParentVolumeChange(LightProbeVolume currentLightProbeVolume, List<LightProbeVolume> lightProbeVolumes)
		{
			if (!LightProbeVolume.Exists)
			{
				return;
			}
			this.currentLightProbeVolume = currentLightProbeVolume;
			this.lightProbeVolumes = lightProbeVolumes;
			this.UpdateRenderers();
			if (this.onVolumeChangeEvent != null)
			{
				this.onVolumeChangeEvent(currentLightProbeVolume, lightProbeVolumes);
			}
		}

		// Token: 0x060024FB RID: 9467 RVA: 0x000FDEAC File Offset: 0x000FC0AC
		protected void OnSnap(Holder holder)
		{
			if (!LightProbeVolume.Exists)
			{
				return;
			}
			if (!this.parentReceiver)
			{
				this.parentReceiver = holder.GetComponentInParent<LightVolumeReceiver>(true);
			}
			if (this.parentReceiver)
			{
				this.parentReceiver.onVolumeChangeEvent += this.OnParentVolumeChange;
				this.currentLightProbeVolume = this.parentReceiver.currentLightProbeVolume;
				this.lightProbeVolumes = this.parentReceiver.lightProbeVolumes;
				this.UpdateRenderers();
				if (this.onVolumeChangeEvent != null)
				{
					this.onVolumeChangeEvent(this.currentLightProbeVolume, this.lightProbeVolumes);
				}
			}
		}

		// Token: 0x060024FC RID: 9468 RVA: 0x000FDF46 File Offset: 0x000FC146
		protected void OnUnSnap(Holder holder)
		{
			if (!LightProbeVolume.Exists)
			{
				return;
			}
			this.RemoveParentVolumeListener(true);
		}

		// Token: 0x060024FD RID: 9469 RVA: 0x000FDF57 File Offset: 0x000FC157
		protected void OnDespawn(EventTime eventTime)
		{
			if (eventTime != EventTime.OnStart)
			{
				return;
			}
			this.ClearProbeVolumes();
			this.RestoreRenderers();
			this.RemoveParentVolumeListener(false);
			this.area = null;
		}

		// Token: 0x060024FE RID: 9470 RVA: 0x000FDF77 File Offset: 0x000FC177
		private void OnEffectDespawn(Effect despawningEffect)
		{
			this.OnDespawn(EventTime.OnStart);
		}

		// Token: 0x060024FF RID: 9471 RVA: 0x000FDF80 File Offset: 0x000FC180
		[ContextMenu("QuickSetup")]
		public void QuickSetup()
		{
			this.QuickSetup(false);
		}

		// Token: 0x06002500 RID: 9472 RVA: 0x000FDF89 File Offset: 0x000FC189
		public void QuickSetup(bool silent = false)
		{
			this.RestoreRenderers();
			this.AddMaterialInstance(silent);
			this.FindNearestVolume(silent);
			this.UpdateRenderers();
		}

		// Token: 0x06002501 RID: 9473 RVA: 0x000FDFA5 File Offset: 0x000FC1A5
		[ContextMenu("AddMaterialInstance")]
		public void AddMaterialInstance()
		{
			this.AddMaterialInstance(false);
		}

		// Token: 0x06002502 RID: 9474 RVA: 0x000FDFB0 File Offset: 0x000FC1B0
		public void AddMaterialInstance(bool silent = false)
		{
			for (int i = this.materialInstances.Count - 1; i >= 0; i--)
			{
				if (this.materialInstances[i] == null)
				{
					this.materialInstances.RemoveAt(i);
				}
			}
			Renderer[] renderersFound = base.GetComponentsInChildren<Renderer>();
			if (!silent)
			{
				Debug.Log(string.Format("AddMaterialInstance: Found {0} Renderers", renderersFound.Length));
			}
			foreach (Renderer r in renderersFound)
			{
				MaterialInstance mi = r.GetComponent<MaterialInstance>();
				if (mi == null)
				{
					Debug.Log("AddMaterialInstance: for: " + r.gameObject.name, r.gameObject);
					mi = r.gameObject.AddComponent<MaterialInstance>();
				}
				if (mi.instanceMaterials == null || mi.instanceMaterials.Length == 0)
				{
					mi.AcquireMaterials();
				}
				foreach (Material miMaterial in mi.instanceMaterials)
				{
					if (!(miMaterial == null) && !(miMaterial.shader == null) && miMaterial.shader.name == "Universal Render Pipeline/Lit")
					{
						Debug.LogError("AddMaterialInstance: You are using Lit shader which wont work with light probe volume textures", mi.gameObject);
					}
				}
				if (!this.materialInstances.Contains(mi))
				{
					this.materialInstances.Add(mi);
				}
			}
		}

		// Token: 0x06002503 RID: 9475 RVA: 0x000FE110 File Offset: 0x000FC310
		[ContextMenu("FindNearestVolume")]
		public void FindNearestVolume()
		{
			this.FindNearestVolume(false);
		}

		// Token: 0x06002504 RID: 9476 RVA: 0x000FE11C File Offset: 0x000FC31C
		public void FindNearestVolume(bool silent = false)
		{
			this.ClearProbeVolumes();
			LightProbeVolume[] array = UnityEngine.Object.FindObjectsOfType<LightProbeVolume>();
			LightProbeVolume closestVolume = null;
			float distance = float.MaxValue;
			foreach (LightProbeVolume lightVolume in array)
			{
				float dist = Vector3.Distance(lightVolume.transform.position, base.transform.position);
				if (dist < distance)
				{
					distance = dist;
					closestVolume = lightVolume;
				}
			}
			if (closestVolume != null)
			{
				this.lightProbeVolumes.Add(closestVolume);
				this.currentLightProbeVolume = closestVolume;
				if (this.onVolumeChangeEvent != null)
				{
					this.onVolumeChangeEvent(this.currentLightProbeVolume, this.lightProbeVolumes);
				}
				if (!silent)
				{
					Debug.Log("FindNearestVolume: Found LightProbeVolume: " + closestVolume.name, closestVolume);
					return;
				}
			}
			else if (!silent)
			{
				Debug.LogError("FindNearestVolume: No LightProbeVolume found");
			}
		}

		// Token: 0x06002505 RID: 9477 RVA: 0x000FE1DC File Offset: 0x000FC3DC
		[ContextMenu("UpdateRenderers")]
		public void UpdateRenderers()
		{
			if (!LightProbeVolume.Exists)
			{
				return;
			}
			if (this.volumeDetection != LightVolumeReceiver.VolumeDetection.StaticPerMesh)
			{
				if (this.volumeDetection == LightVolumeReceiver.VolumeDetection.DynamicTrigger && this.currentLightProbeVolume)
				{
					if (this.method == LightVolumeReceiver.Method.SRPBatching)
					{
						int materialInstancesCount = this.materialInstances.Count;
						for (int i = 0; i < materialInstancesCount; i++)
						{
							MaterialInstance materialInstance = this.materialInstances[i];
							if (materialInstance == null)
							{
								Debug.LogError(string.Format("LightVolumeReceiver: {0} material instance at index : {1} is null", base.gameObject.GetPathFromRoot(), i), base.gameObject);
							}
							else if (materialInstance.CachedRenderer == null)
							{
								Debug.LogError(string.Format("LightVolumeReceiver: {0} material instance CachedRenderer at index : {1} is null", base.gameObject.GetPathFromRoot(), i), base.gameObject);
							}
							else
							{
								if (materialInstance && materialInstance.CachedRenderer)
								{
									materialInstance.CachedRenderer.lightProbeUsage = LightProbeUsage.Off;
								}
								this.currentLightProbeVolume.RegisterMaterials(this, materialInstance.materials);
							}
						}
						return;
					}
					if (this.method == LightVolumeReceiver.Method.GPUInstancing)
					{
						int renderersCount = this.renderers.Count;
						for (int j = 0; j < renderersCount; j++)
						{
							LightVolumeReceiver.ApplyProbeVolume(this.renderers[j], this.materialPropertyBlock);
						}
						return;
					}
				}
				return;
			}
			this.ClearProbeVolumes();
			if (this.method == LightVolumeReceiver.Method.SRPBatching)
			{
				int materialInstancesCount2 = this.materialInstances.Count;
				for (int k = 0; k < materialInstancesCount2; k++)
				{
					MaterialInstance materialInstance2 = this.materialInstances[k];
					if (materialInstance2 && materialInstance2.CachedRenderer)
					{
						LightProbeVolume lightProbeVolume = null;
						bool flag = !(materialInstance2.CachedRenderer is ParticleSystemRenderer);
						Bounds bounds = materialInstance2.CachedRenderer.bounds;
						Vector3 position = flag ? bounds.center : materialInstance2.CachedRenderer.transform.position;
						if (flag)
						{
							LightVolumeReceiver.GetVolumeFromBoundsOrPosition(bounds, position, out lightProbeVolume, this.area);
						}
						else
						{
							LightVolumeReceiver.GetVolumeFromPosition(position, out lightProbeVolume, this.area);
						}
						if (lightProbeVolume)
						{
							materialInstance2.CachedRenderer.lightProbeUsage = LightProbeUsage.Off;
							lightProbeVolume.RegisterMaterials(this, materialInstance2.materials);
							if (!this.lightProbeVolumes.Contains(lightProbeVolume))
							{
								this.lightProbeVolumes.Add(lightProbeVolume);
							}
						}
					}
				}
				return;
			}
			if (this.method == LightVolumeReceiver.Method.GPUInstancing)
			{
				int renderersCount2 = this.renderers.Count;
				for (int l = 0; l < renderersCount2; l++)
				{
					Renderer renderer = this.renderers[l];
					if (renderer == null)
					{
						Debug.LogErrorFormat(this, "Renderer is null at " + l.ToString(), Array.Empty<object>());
					}
					if (renderer.lightmapIndex < 0)
					{
						LightProbeVolume lightProbeVolume2 = LightVolumeReceiver.ApplyProbeVolume(renderer, this.materialPropertyBlock);
						if (lightProbeVolume2 && !this.lightProbeVolumes.Contains(lightProbeVolume2))
						{
							this.lightProbeVolumes.Add(lightProbeVolume2);
						}
					}
				}
				return;
			}
		}

		// Token: 0x06002506 RID: 9478 RVA: 0x000FE4D0 File Offset: 0x000FC6D0
		public void RestoreRenderers()
		{
			if (this.method == LightVolumeReceiver.Method.SRPBatching)
			{
				using (List<MaterialInstance>.Enumerator enumerator = this.materialInstances.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						MaterialInstance materialInstance = enumerator.Current;
						if (materialInstance && materialInstance.CachedRenderer)
						{
							materialInstance.CachedRenderer.lightProbeUsage = LightProbeUsage.BlendProbes;
						}
						materialInstance.RestoreRenderer();
					}
					return;
				}
			}
			if (this.method == LightVolumeReceiver.Method.GPUInstancing)
			{
				foreach (Renderer renderer in this.renderers)
				{
					renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
					renderer.ClearMaterialPropertyBlocks();
				}
			}
		}

		// Token: 0x06002507 RID: 9479 RVA: 0x000FE59C File Offset: 0x000FC79C
		public static LightVolumeReceiver.VolumeGetStatus GetVolumeFromPosition(Vector3 position, out LightProbeVolume closestLightMapVolume, Area area = null)
		{
			List<LightProbeVolume> volumes;
			if (area == null || !LightProbeVolume.areaToVolume.TryGetValue(area, out volumes))
			{
				volumes = LightProbeVolume.list;
			}
			int volumesCount = volumes.Count;
			float distance = float.MaxValue;
			closestLightMapVolume = null;
			for (int i = 0; i < volumesCount; i++)
			{
				LightProbeVolume lightProbeVolume = volumes[i];
				if (lightProbeVolume.IsPositionInVolume(position))
				{
					closestLightMapVolume = lightProbeVolume;
					return LightVolumeReceiver.VolumeGetStatus.positionBounds;
				}
				Vector3 closestPointOnVolume = lightProbeVolume.BoxCollider.ClosestPoint(position);
				float dist = Vector3.Distance(position, closestPointOnVolume);
				if (dist <= LightVolumeReceiver.maxDistanceVolumeCheck && dist <= distance)
				{
					distance = dist;
					closestLightMapVolume = lightProbeVolume;
				}
			}
			if (!(closestLightMapVolume != null))
			{
				return LightVolumeReceiver.VolumeGetStatus.none;
			}
			return LightVolumeReceiver.VolumeGetStatus.closest;
		}

		// Token: 0x06002508 RID: 9480 RVA: 0x000FE638 File Offset: 0x000FC838
		public static LightVolumeReceiver.VolumeGetStatus GetVolumeFromBoundsOrPosition(Bounds bounds, Vector3 position, out LightProbeVolume closestLightMapVolume, Area area = null)
		{
			List<LightProbeVolume> volumes;
			if (area == null || !LightProbeVolume.areaToVolume.TryGetValue(area, out volumes))
			{
				volumes = LightProbeVolume.list;
			}
			int volumesCount = volumes.Count;
			float distance = float.MaxValue;
			closestLightMapVolume = null;
			for (int i = 0; i < volumesCount; i++)
			{
				LightProbeVolume lightProbeVolume = volumes[i];
				if (lightProbeVolume.IsInVolume(bounds, position))
				{
					closestLightMapVolume = lightProbeVolume;
					return LightVolumeReceiver.VolumeGetStatus.positionBounds;
				}
				Vector3 closestPointOnMesh = bounds.ClosestPoint(lightProbeVolume.BoxCollider.center);
				Vector3 closestPointOnVolume = lightProbeVolume.BoxCollider.ClosestPoint(closestPointOnMesh);
				float dist = Vector3.Distance(closestPointOnMesh, closestPointOnVolume);
				if (dist <= LightVolumeReceiver.maxDistanceVolumeCheck && dist <= distance)
				{
					distance = dist;
					closestLightMapVolume = lightProbeVolume;
				}
			}
			if (!(closestLightMapVolume != null))
			{
				return LightVolumeReceiver.VolumeGetStatus.none;
			}
			return LightVolumeReceiver.VolumeGetStatus.closest;
		}

		// Token: 0x06002509 RID: 9481 RVA: 0x000FE6EC File Offset: 0x000FC8EC
		public static LightProbeVolume ApplyProbeVolume(Renderer renderer, MaterialPropertyBlock materialPropertyBlock)
		{
			Vector3 position = (renderer is ParticleSystemRenderer) ? renderer.transform.position : renderer.bounds.center;
			LightProbeVolume lightProbeVolume;
			LightVolumeReceiver.GetVolumeFromPosition(position, out lightProbeVolume, null);
			if (lightProbeVolume)
			{
				renderer.lightProbeUsage = LightProbeUsage.CustomProvided;
				renderer.GetPropertyBlock(materialPropertyBlock);
				lightProbeVolume.UpdateMaterialPropertyBlock(materialPropertyBlock, position);
				renderer.SetPropertyBlock(materialPropertyBlock);
			}
			return lightProbeVolume;
		}

		// Token: 0x0600250A RID: 9482 RVA: 0x000FE74D File Offset: 0x000FC94D
		public static void DisableProbeVolume(Renderer renderer)
		{
			renderer.SetPropertyBlock(null);
			renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
		}

		// Token: 0x0600250B RID: 9483 RVA: 0x000FE75D File Offset: 0x000FC95D
		protected void OnRootRagdollPartTriggerEnter(Collider other)
		{
			if (!LightProbeVolume.Exists)
			{
				return;
			}
			if (this.creature.locomotion.enabled)
			{
				return;
			}
			this.TriggerEnter(other);
		}

		// Token: 0x0600250C RID: 9484 RVA: 0x000FE781 File Offset: 0x000FC981
		protected void OnRootRagdollPartTriggerExit(Collider other)
		{
			if (!LightProbeVolume.Exists)
			{
				return;
			}
			if (this.creature.locomotion.enabled)
			{
				return;
			}
			this.TriggerExit(other);
		}

		// Token: 0x0600250D RID: 9485 RVA: 0x000FE7A5 File Offset: 0x000FC9A5
		protected void OnTriggerEnter(Collider other)
		{
			if (!LightProbeVolume.Exists)
			{
				return;
			}
			if (this.volumeDetection == LightVolumeReceiver.VolumeDetection.StaticPerMesh)
			{
				return;
			}
			if (this.creature && !this.creature.locomotion.enabled)
			{
				return;
			}
			this.TriggerEnter(other);
		}

		// Token: 0x0600250E RID: 9486 RVA: 0x000FE7DF File Offset: 0x000FC9DF
		protected void OnTriggerExit(Collider other)
		{
			if (!LightProbeVolume.Exists)
			{
				return;
			}
			if (this.volumeDetection == LightVolumeReceiver.VolumeDetection.StaticPerMesh)
			{
				return;
			}
			if (this.creature && !this.creature.locomotion.enabled)
			{
				return;
			}
			this.TriggerExit(other);
		}

		// Token: 0x0600250F RID: 9487 RVA: 0x000FE81C File Offset: 0x000FCA1C
		public void TriggerEnter(Collider other)
		{
			if (this.volumeDetection != LightVolumeReceiver.VolumeDetection.DynamicTrigger)
			{
				return;
			}
			LightProbeVolume lightProbeVolume;
			if (other.gameObject.layer != Common.lightProbeVolumeLayer || !other.TryGetComponent<LightProbeVolume>(out lightProbeVolume))
			{
				return;
			}
			if (lightProbeVolume == this.currentLightProbeVolume)
			{
				return;
			}
			if (!this.lightProbeVolumes.Contains(lightProbeVolume))
			{
				bool added = false;
				for (int i = 0; i < this.lightProbeVolumes.Count; i++)
				{
					if (lightProbeVolume.priority <= this.lightProbeVolumes[i].priority)
					{
						this.lightProbeVolumes.Insert(i, lightProbeVolume);
						added = true;
						break;
					}
				}
				if (!added)
				{
					this.lightProbeVolumes.Add(lightProbeVolume);
				}
			}
			if (this.lightProbeVolumes[0] == this.currentLightProbeVolume)
			{
				return;
			}
			this.currentLightProbeVolume = this.lightProbeVolumes[0];
			this.UpdateRenderers();
			LightVolumeReceiver.OnVolumeChangeDelegate onVolumeChangeDelegate = this.onVolumeChangeEvent;
			if (onVolumeChangeDelegate == null)
			{
				return;
			}
			onVolumeChangeDelegate(this.currentLightProbeVolume, this.lightProbeVolumes);
		}

		// Token: 0x06002510 RID: 9488 RVA: 0x000FE90C File Offset: 0x000FCB0C
		public void TriggerExit(Collider other)
		{
			if (!LightProbeVolume.Exists)
			{
				return;
			}
			LightProbeVolume lightProbeVolume;
			if (this.volumeDetection == LightVolumeReceiver.VolumeDetection.DynamicTrigger && other.gameObject.layer == Common.lightProbeVolumeLayer && other.TryGetComponent<LightProbeVolume>(out lightProbeVolume))
			{
				if (this.lightProbeVolumes.Contains(lightProbeVolume))
				{
					this.lightProbeVolumes.Remove(lightProbeVolume);
				}
				if (this.lightProbeVolumes.Count > 0)
				{
					this.currentLightProbeVolume = this.lightProbeVolumes[0];
					this.UpdateRenderers();
				}
				else
				{
					this.ClearProbeVolumes();
				}
				LightVolumeReceiver.OnVolumeChangeDelegate onVolumeChangeDelegate = this.onVolumeChangeEvent;
				if (onVolumeChangeDelegate == null)
				{
					return;
				}
				onVolumeChangeDelegate(this.currentLightProbeVolume, this.lightProbeVolumes);
			}
		}

		// Token: 0x06002511 RID: 9489 RVA: 0x000FE9B0 File Offset: 0x000FCBB0
		public void ClearProbeVolumes()
		{
			this.currentLightProbeVolume = null;
			if (!this.lightProbeVolumes.IsNullOrEmpty())
			{
				int count = this.lightProbeVolumes.Count;
				for (int i = 0; i < count; i++)
				{
					if (this.lightProbeVolumes[i] != null)
					{
						this.lightProbeVolumes[i].UnregisterMaterials(this);
					}
				}
				this.lightProbeVolumes.Clear();
			}
		}

		// Token: 0x04002483 RID: 9347
		[Tooltip("The Method of which the Receiver uses.\n\nGPU Instancing: Uses the GPU instancing to apply the volume.\nSRPBatching: Uses SRPBatching to apply the volume.")]
		public LightVolumeReceiver.Method method = LightVolumeReceiver.Method.SRPBatching;

		// Token: 0x04002484 RID: 9348
		[Tooltip("Depicts how the 3D volume is detected.\n\nStatic Per Mesh: Detects it per mesh to apply the volume. Recommended.\nDynamic Trigger: Detects the volume dynamicaly, either through events or code.")]
		public LightVolumeReceiver.VolumeDetection volumeDetection = LightVolumeReceiver.VolumeDetection.DynamicTrigger;

		// Token: 0x04002485 RID: 9349
		[Tooltip("Initialize the light probe volumes on to the renderers on Start")]
		public bool initRenderersOnStart = true;

		// Token: 0x04002486 RID: 9350
		[Tooltip("Add a material instance on to the objects. This is recommended to allow 3D volumes to be added to materials without affecting other ones.")]
		public bool addMaterialInstances = true;

		// Token: 0x04002487 RID: 9351
		private LightProbeVolume _currentLightProbeVolume;

		// Token: 0x04002488 RID: 9352
		public List<MaterialInstance> materialInstances = new List<MaterialInstance>();

		// Token: 0x04002489 RID: 9353
		public List<LightProbeVolume> lightProbeVolumes = new List<LightProbeVolume>();

		// Token: 0x0400248A RID: 9354
		public List<Renderer> renderers = new List<Renderer>();

		// Token: 0x0400248B RID: 9355
		[NonSerialized]
		public LightVolumeReceiver parentReceiver;

		// Token: 0x0400248C RID: 9356
		[NonSerialized]
		public Creature creature;

		// Token: 0x0400248D RID: 9357
		[NonSerialized]
		public Item item;

		// Token: 0x0400248E RID: 9358
		[NonSerialized]
		public Effect effect;

		// Token: 0x0400248F RID: 9359
		[NonSerialized]
		public Area area;

		// Token: 0x04002490 RID: 9360
		protected MaterialPropertyBlock materialPropertyBlock;

		// Token: 0x04002492 RID: 9362
		protected bool initRenderersDone;

		// Token: 0x04002493 RID: 9363
		private bool staticMeshFirstRunDone;

		// Token: 0x04002494 RID: 9364
		private static float maxDistanceVolumeCheck = 10f;

		// Token: 0x02000A00 RID: 2560
		public enum Method
		{
			// Token: 0x040046AB RID: 18091
			GPUInstancing,
			// Token: 0x040046AC RID: 18092
			SRPBatching
		}

		// Token: 0x02000A01 RID: 2561
		public enum VolumeDetection
		{
			// Token: 0x040046AE RID: 18094
			StaticPerMesh,
			// Token: 0x040046AF RID: 18095
			DynamicTrigger
		}

		// Token: 0x02000A02 RID: 2562
		// (Invoke) Token: 0x06004512 RID: 17682
		public delegate void OnVolumeChangeDelegate(LightProbeVolume currentLightProbeVolume, List<LightProbeVolume> lightProbeVolumes);

		// Token: 0x02000A03 RID: 2563
		public enum VolumeGetStatus
		{
			// Token: 0x040046B1 RID: 18097
			positionBounds,
			// Token: 0x040046B2 RID: 18098
			closest,
			// Token: 0x040046B3 RID: 18099
			none
		}
	}
}
