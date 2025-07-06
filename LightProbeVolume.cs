using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x02000303 RID: 771
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Areas/LightProbeVolume.html")]
	[ExecuteInEditMode]
	public class LightProbeVolume : MonoBehaviour
	{
		// Token: 0x060024D5 RID: 9429 RVA: 0x000FCD14 File Offset: 0x000FAF14
		public static void Register(LightProbeVolume lightProbeVolume)
		{
			LightProbeVolume.list.Add(lightProbeVolume);
			if (lightProbeVolume.area)
			{
				List<LightProbeVolume> volumes;
				if (!LightProbeVolume.areaToVolume.TryGetValue(lightProbeVolume.area, out volumes))
				{
					volumes = new List<LightProbeVolume>();
					LightProbeVolume.areaToVolume.Add(lightProbeVolume.area, volumes);
				}
				volumes.Add(lightProbeVolume);
			}
			else
			{
				Debug.LogError("Light Probe Volume: " + lightProbeVolume.gameObject.GetPathFromRoot() + " is not in an Area! LVRs will not work properly.");
			}
			LightProbeVolume.Exists = true;
		}

		// Token: 0x060024D6 RID: 9430 RVA: 0x000FCD94 File Offset: 0x000FAF94
		public static void Unregister(LightProbeVolume lightProbeVolume)
		{
			LightProbeVolume.list.Remove(lightProbeVolume);
			if (LightProbeVolume.list.Count == 0)
			{
				LightProbeVolume.Exists = false;
			}
			List<LightProbeVolume> volumes;
			if (lightProbeVolume.area && LightProbeVolume.areaToVolume.TryGetValue(lightProbeVolume.area, out volumes))
			{
				volumes.Remove(lightProbeVolume);
				if (volumes.Count == 0)
				{
					LightProbeVolume.areaToVolume.Remove(lightProbeVolume.area);
				}
			}
		}

		// Token: 0x1700023A RID: 570
		// (get) Token: 0x060024D7 RID: 9431 RVA: 0x000FCE01 File Offset: 0x000FB001
		public Vector3 ProbeVolumeMin
		{
			get
			{
				return this.probeVolumeMin;
			}
		}

		// Token: 0x1700023B RID: 571
		// (get) Token: 0x060024D8 RID: 9432 RVA: 0x000FCE09 File Offset: 0x000FB009
		public Vector3 ProbeVolumeSizeInverse
		{
			get
			{
				return this.probeVolumeSizeInverse;
			}
		}

		// Token: 0x1700023C RID: 572
		// (get) Token: 0x060024D9 RID: 9433 RVA: 0x000FCE11 File Offset: 0x000FB011
		// (set) Token: 0x060024DA RID: 9434 RVA: 0x000FCE19 File Offset: 0x000FB019
		public BoxCollider BoxCollider
		{
			get
			{
				return this._boxCollider;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this._boxCollider = value;
				this.hasBoxCollider = true;
				this._bounds = this._boxCollider.bounds;
			}
		}

		// Token: 0x060024DB RID: 9435 RVA: 0x000FCE44 File Offset: 0x000FB044
		public bool IsInVolume(Bounds bounds, Vector3 position)
		{
			if (!this.hasBoxCollider)
			{
				this.BoxCollider = base.GetComponent<BoxCollider>();
			}
			return this._bounds.Contains(position) || this._bounds.Intersects(bounds);
		}

		// Token: 0x060024DC RID: 9436 RVA: 0x000FCE76 File Offset: 0x000FB076
		public bool IsPositionInVolume(Vector3 position)
		{
			if (!this.hasBoxCollider)
			{
				this.BoxCollider = base.GetComponent<BoxCollider>();
			}
			return this._bounds.Contains(position);
		}

		// Token: 0x060024DD RID: 9437 RVA: 0x000FCE98 File Offset: 0x000FB098
		public void SetTexture(Texture3D SHAr, Texture3D SHAg, Texture3D SHAb, Texture3D occ)
		{
			this.SHAr = SHAr;
			this.SHAg = SHAg;
			this.SHAb = SHAb;
			this.occ = occ;
			if (this._registeredMaterials.IsNullOrEmpty())
			{
				return;
			}
			foreach (KeyValuePair<MonoBehaviour, List<Material>> item in this._registeredMaterials)
			{
				for (int i = item.Value.Count - 1; i >= 0; i--)
				{
					Material mat = item.Value[i];
					if (mat != null)
					{
						this.UpdateMaterialProperties(mat);
					}
					else
					{
						item.Value.RemoveAt(i);
						Debug.LogError(string.Concat(new string[]
						{
							"Material not alive or null in LightProbeVolume : ",
							base.gameObject.name,
							" for mono register type ",
							item.Key.GetType().Name,
							" in game object ",
							item.Key.gameObject.name
						}));
					}
				}
			}
		}

		// Token: 0x060024DE RID: 9438 RVA: 0x000FCFC0 File Offset: 0x000FB1C0
		private void Awake()
		{
			this.area = base.GetComponentInParent<Area>();
			this.BoxCollider = base.GetComponent<BoxCollider>();
		}

		// Token: 0x060024DF RID: 9439 RVA: 0x000FCFDA File Offset: 0x000FB1DA
		private void OnEnable()
		{
			LightProbeVolume.Register(this);
		}

		// Token: 0x060024E0 RID: 9440 RVA: 0x000FCFE2 File Offset: 0x000FB1E2
		private void OnDisable()
		{
			LightProbeVolume.Unregister(this);
		}

		// Token: 0x060024E1 RID: 9441 RVA: 0x000FCFEA File Offset: 0x000FB1EA
		private void OnDestroy()
		{
			LightProbeVolume.Unregister(this);
			this.SetTexture(null, null, null, null);
			this.ClearAllRegisteredMaterials();
		}

		// Token: 0x060024E2 RID: 9442 RVA: 0x000FD004 File Offset: 0x000FB204
		private void ClearAllRegisteredMaterials()
		{
			if (this._registeredMaterials.IsNullOrEmpty())
			{
				return;
			}
			foreach (KeyValuePair<MonoBehaviour, List<Material>> item in this._registeredMaterials)
			{
				for (int i = item.Value.Count - 1; i >= 0; i--)
				{
					Material mat = item.Value[i];
					if (mat != null)
					{
						mat.SetFloat(LightProbeVolume.UseProbeVolume, 0f);
						mat.SetFloat(LightProbeVolume.UseProbeVolumeLux, 0f);
						mat.DisableKeyword("_PROBEVOLUME_ON");
						mat.SetTexture(LightProbeVolume.ProbeVolumeShR, null);
						mat.SetTexture(LightProbeVolume.ProbeVolumeShG, null);
						mat.SetTexture(LightProbeVolume.ProbeVolumeShB, null);
						mat.SetTexture(LightProbeVolume.ProbeVolumeOcc, null);
					}
					item.Value.RemoveAt(i);
				}
			}
			this._registeredMaterials.Clear();
		}

		// Token: 0x060024E3 RID: 9443 RVA: 0x000FD110 File Offset: 0x000FB310
		public void RegisterMaterials(MonoBehaviour receiver, Material[] materials)
		{
			if (receiver == null)
			{
				return;
			}
			if (materials.IsNullOrEmpty())
			{
				return;
			}
			if (this._registeredMaterials == null)
			{
				this._registeredMaterials = new Dictionary<MonoBehaviour, List<Material>>();
			}
			List<Material> previousMaterials;
			if (!this._registeredMaterials.TryGetValue(receiver, out previousMaterials))
			{
				previousMaterials = new List<Material>();
				this._registeredMaterials.Add(receiver, previousMaterials);
			}
			for (int i = 0; i < materials.Length; i++)
			{
				previousMaterials.Add(materials[i]);
			}
			for (int j = 0; j < materials.Length; j++)
			{
				this.UpdateMaterialProperties(materials[j]);
			}
		}

		// Token: 0x060024E4 RID: 9444 RVA: 0x000FD194 File Offset: 0x000FB394
		public void UnregisterMaterials(MonoBehaviour receiver)
		{
			if (this._registeredMaterials.IsNullOrEmpty())
			{
				return;
			}
			if (receiver == null)
			{
				return;
			}
			List<Material> materials;
			if (this._registeredMaterials.Remove(receiver, out materials))
			{
				for (int i = materials.Count - 1; i >= 0; i--)
				{
					Material mat = materials[i];
					if (mat != null)
					{
						mat.SetTexture(LightProbeVolume.ProbeVolumeAmbient, null);
						mat.SetTexture(LightProbeVolume.ProbeVolumeShR, null);
						mat.SetTexture(LightProbeVolume.ProbeVolumeShG, null);
						mat.SetTexture(LightProbeVolume.ProbeVolumeShB, null);
						mat.SetTexture(LightProbeVolume.ProbeVolumeOcc, null);
					}
					else
					{
						materials.RemoveAt(i);
						Debug.LogError(string.Concat(new string[]
						{
							"Material not alive or null in LightProbeVolume : ",
							base.gameObject.name,
							" for mono register type ",
							receiver.GetType().Name,
							" in game object ",
							receiver.gameObject.name
						}));
					}
				}
			}
		}

		// Token: 0x060024E5 RID: 9445 RVA: 0x000FD290 File Offset: 0x000FB490
		public void UpdateMaterialProperties(Material material)
		{
			if (material != null)
			{
				material.SetFloat(LightProbeVolume.UseProbeVolume, 1f);
				material.SetFloat(LightProbeVolume.UseProbeVolumeLux, 1f);
				material.EnableKeyword("_PROBEVOLUME_ON");
				material.SetMatrix(LightProbeVolume.ProbeWorldToTexture, base.transform.worldToLocalMatrix);
				material.SetVector(LightProbeVolume.VolumeMin, this.probeVolumeMin);
				material.SetVector(LightProbeVolume.ProbeVolumeSizeInv, this.probeVolumeSizeInverse);
				material.SetTexture(LightProbeVolume.ProbeVolumeAmbient, this.Ambient);
				material.SetTexture(LightProbeVolume.ProbeVolumeShR, this.SHAr);
				material.SetTexture(LightProbeVolume.ProbeVolumeShG, this.SHAg);
				material.SetTexture(LightProbeVolume.ProbeVolumeShB, this.SHAb);
				if (this.useOcclusion)
				{
					material.SetTexture(LightProbeVolume.ProbeVolumeOcc, this.occ);
					return;
				}
			}
			else
			{
				Debug.LogError("Material null");
			}
		}

		// Token: 0x060024E6 RID: 9446 RVA: 0x000FD37E File Offset: 0x000FB57E
		public void UpdateMaterialPropertiesMatrixOnly(Material material)
		{
			if (material != null)
			{
				material.SetMatrix(LightProbeVolume.ProbeWorldToTexture, base.transform.worldToLocalMatrix);
			}
		}

		// Token: 0x060024E7 RID: 9447 RVA: 0x000FD3A0 File Offset: 0x000FB5A0
		public void UpdateMaterialPropertyBlock(MaterialPropertyBlock block, Vector3 worldPosition)
		{
			if (this.SHAr != null && this.SHAg != null && this.SHAb != null)
			{
				Vector3 a = base.transform.worldToLocalMatrix.MultiplyPoint(worldPosition);
				Vector3 inverse = this.probeVolumeSizeInverse;
				Vector3 texCoord = a - this.probeVolumeMin;
				texCoord.x *= inverse.x;
				texCoord.y *= inverse.y;
				texCoord.z *= inverse.z;
				block.SetVector(LightProbeVolume.UnitySHAr, this.SHAr.GetPixelBilinear(texCoord.x, texCoord.y, texCoord.z));
				block.SetVector(LightProbeVolume.UnitySHAg, this.SHAg.GetPixelBilinear(texCoord.x, texCoord.y, texCoord.z));
				block.SetVector(LightProbeVolume.UnitySHAb, this.SHAb.GetPixelBilinear(texCoord.x, texCoord.y, texCoord.z));
				if (this.useOcclusion && this.occ != null)
				{
					block.SetVector(LightProbeVolume.UnityProbesOcclusion, this.occ.GetPixelBilinear(texCoord.x, texCoord.y, texCoord.z));
				}
			}
		}

		// Token: 0x060024E8 RID: 9448 RVA: 0x000FD504 File Offset: 0x000FB704
		public static Vector4[] GetShaderSHL1CoeffsFromNormalizedSH(SphericalHarmonicsL2 probe)
		{
			Vector4[] coeff = new Vector4[3];
			for (int i = 0; i < 3; i++)
			{
				coeff[i].x = probe[i, 3];
				coeff[i].y = probe[i, 1];
				coeff[i].z = probe[i, 2];
				coeff[i].w = probe[i, 0] - probe[i, 6];
			}
			return coeff;
		}

		// Token: 0x060024E9 RID: 9449 RVA: 0x000FD584 File Offset: 0x000FB784
		public static Texture3D DiscardTopMipmap(Texture3D texture)
		{
			if (texture == null)
			{
				Debug.LogError("Texture is null!");
				return null;
			}
			if (texture.mipmapCount <= 1)
			{
				Debug.LogError("Texture has no mip maps!");
				return null;
			}
			Vector3Int dimension = new Vector3Int(texture.width / 2, texture.height / 2, texture.depth / 2);
			Texture3D resizedTexture = new Texture3D(dimension.x, dimension.y, dimension.z, texture.format, texture.mipmapCount > 1);
			resizedTexture.filterMode = texture.filterMode;
			resizedTexture.wrapMode = texture.wrapMode;
			for (int x = 0; x < dimension.x; x++)
			{
				for (int y = 0; y < dimension.y; y++)
				{
					for (int z = 0; z < dimension.z; z++)
					{
						Vector3 uvw = new Vector3((float)x / (float)dimension.x, (float)y / (float)dimension.y, (float)z / (float)dimension.z);
						Color pixel = texture.GetPixelBilinear(uvw.x, uvw.y, uvw.z, 1);
						resizedTexture.SetPixel(x, y, z, pixel, 0);
					}
				}
			}
			resizedTexture.Apply();
			return resizedTexture;
		}

		// Token: 0x04002462 RID: 9314
		public static List<LightProbeVolume> list = new List<LightProbeVolume>();

		// Token: 0x04002463 RID: 9315
		public static Dictionary<Area, List<LightProbeVolume>> areaToVolume = new Dictionary<Area, List<LightProbeVolume>>();

		/// <summary>
		/// Is there at least one LightProbeVolume in the scene?
		/// </summary>
		// Token: 0x04002464 RID: 9316
		public static bool Exists = false;

		// Token: 0x04002465 RID: 9317
		[Tooltip("The lower the priority, the more likely the volume will be used by items. If an item is inside two volumes, it will select the one with the lowest priority.")]
		public int priority = 1;

		// Token: 0x04002466 RID: 9318
		[Tooltip("Size of the Light Volume box.")]
		public Vector3 size = Vector3.one;

		// Token: 0x04002467 RID: 9319
		[Tooltip("Enables Light Probe occlusion (used for light occlusion e.g. directional light in an interior space.)")]
		public bool useOcclusion = true;

		// Token: 0x04002468 RID: 9320
		[SerializeField]
		[HideInInspector]
		private Vector3 probeVolumeMin;

		// Token: 0x04002469 RID: 9321
		[SerializeField]
		[HideInInspector]
		public Vector3 probeVolumeSizeInverse;

		// Token: 0x0400246A RID: 9322
		[Tooltip("Ambient 3D Texture of the Light Probe Volume")]
		public Texture3D Ambient;

		// Token: 0x0400246B RID: 9323
		[Tooltip("The Red Channel 3D Texture of the Light Probe Volume")]
		public Texture3D SHAr;

		// Token: 0x0400246C RID: 9324
		[Tooltip("The Green Channel 3D Texture of the Light Probe Volume")]
		public Texture3D SHAg;

		// Token: 0x0400246D RID: 9325
		[Tooltip("The Blue Channel 3D Texture of the Light Probe Volume")]
		public Texture3D SHAb;

		// Token: 0x0400246E RID: 9326
		[Tooltip("The Occlusion Channel 3D Texture of the Light Probe Volume")]
		public Texture3D occ;

		// Token: 0x0400246F RID: 9327
		private static readonly int UseProbeVolume = Shader.PropertyToID("_UseProbeVolume");

		// Token: 0x04002470 RID: 9328
		private static readonly int UseProbeVolumeLux = Shader.PropertyToID("_PROBEVOLUME");

		// Token: 0x04002471 RID: 9329
		private static readonly int ProbeWorldToTexture = Shader.PropertyToID("_ProbeWorldToTexture");

		// Token: 0x04002472 RID: 9330
		private static readonly int VolumeMin = Shader.PropertyToID("_ProbeVolumeMin");

		// Token: 0x04002473 RID: 9331
		private static readonly int ProbeVolumeSizeInv = Shader.PropertyToID("_ProbeVolumeSizeInv");

		// Token: 0x04002474 RID: 9332
		private static readonly int ProbeVolumeAmbient = Shader.PropertyToID("_ProbeVolumeAmbient");

		// Token: 0x04002475 RID: 9333
		private static readonly int ProbeVolumeShR = Shader.PropertyToID("_ProbeVolumeShR");

		// Token: 0x04002476 RID: 9334
		private static readonly int ProbeVolumeShG = Shader.PropertyToID("_ProbeVolumeShG");

		// Token: 0x04002477 RID: 9335
		private static readonly int ProbeVolumeShB = Shader.PropertyToID("_ProbeVolumeShB");

		// Token: 0x04002478 RID: 9336
		private static readonly int ProbeVolumeOcc = Shader.PropertyToID("_ProbeVolumeOcc");

		// Token: 0x04002479 RID: 9337
		private static readonly int UnitySHAr = Shader.PropertyToID("unity_SHAr");

		// Token: 0x0400247A RID: 9338
		private static readonly int UnitySHAg = Shader.PropertyToID("unity_SHAg");

		// Token: 0x0400247B RID: 9339
		private static readonly int UnitySHAb = Shader.PropertyToID("unity_SHAb");

		// Token: 0x0400247C RID: 9340
		private static readonly int UnityProbesOcclusion = Shader.PropertyToID("unity_ProbesOcclusion");

		// Token: 0x0400247D RID: 9341
		private BoxCollider _boxCollider;

		// Token: 0x0400247E RID: 9342
		private Bounds _bounds;

		// Token: 0x0400247F RID: 9343
		private bool hasBoxCollider;

		// Token: 0x04002480 RID: 9344
		[NonSerialized]
		public Area area;

		/// <summary>
		/// Used from the custom editor to change the gizmos.
		/// </summary>
		// Token: 0x04002481 RID: 9345
		[NonSerialized]
		public bool editingSizeThroughEditor;

		// Token: 0x04002482 RID: 9346
		private Dictionary<MonoBehaviour, List<Material>> _registeredMaterials;
	}
}
