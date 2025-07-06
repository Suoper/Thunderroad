using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

namespace ThunderRoad
{
	// Token: 0x020002E3 RID: 739
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/Mirror.html")]
	public class Mirror : ThunderBehaviour
	{
		// Token: 0x1400011B RID: 283
		// (add) Token: 0x0600238A RID: 9098 RVA: 0x000F2C9C File Offset: 0x000F0E9C
		// (remove) Token: 0x0600238B RID: 9099 RVA: 0x000F2CD4 File Offset: 0x000F0ED4
		public event Mirror.OnArmourEditModeChanged OnArmourEditModeChangedEvent;

		// Token: 0x1400011C RID: 284
		// (add) Token: 0x0600238C RID: 9100 RVA: 0x000F2D0C File Offset: 0x000F0F0C
		// (remove) Token: 0x0600238D RID: 9101 RVA: 0x000F2D44 File Offset: 0x000F0F44
		public event Mirror.OnRenderStateChanged OnRenderStateChangedEvent;

		// Token: 0x0600238E RID: 9102 RVA: 0x000F2D79 File Offset: 0x000F0F79
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.Refresh();
		}

		// Token: 0x0600238F RID: 9103 RVA: 0x000F2D90 File Offset: 0x000F0F90
		public void SetActive(bool active)
		{
			if (active)
			{
				RenderPipelineManager.beginCameraRendering -= this.ExecuteBeforeCameraRender;
				RenderPipelineManager.beginCameraRendering += this.ExecuteBeforeCameraRender;
			}
			else
			{
				RenderPipelineManager.beginCameraRendering -= this.ExecuteBeforeCameraRender;
			}
			this.active = active;
		}

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x06002390 RID: 9104 RVA: 0x000F2DDC File Offset: 0x000F0FDC
		// (set) Token: 0x06002391 RID: 9105 RVA: 0x000F2DE4 File Offset: 0x000F0FE4
		public bool playerHeadVisible { get; protected set; }

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x06002392 RID: 9106 RVA: 0x000F2DED File Offset: 0x000F0FED
		// (set) Token: 0x06002393 RID: 9107 RVA: 0x000F2DF5 File Offset: 0x000F0FF5
		public Transform playerReflectionHead { get; protected set; }

		// Token: 0x17000230 RID: 560
		// (get) Token: 0x06002394 RID: 9108 RVA: 0x000F2DFE File Offset: 0x000F0FFE
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06002395 RID: 9109 RVA: 0x000F2E01 File Offset: 0x000F1001
		protected override void ManagedOnEnable()
		{
			Equipment.onAnyCreaturePartChanged = (Equipment.OnCreaturePartChanged)Delegate.Combine(Equipment.onAnyCreaturePartChanged, new Equipment.OnCreaturePartChanged(this.OnAnyCreaturePartChanged));
		}

		// Token: 0x06002396 RID: 9110 RVA: 0x000F2E24 File Offset: 0x000F1024
		protected override void ManagedOnDisable()
		{
			RenderPipelineManager.beginCameraRendering -= this.ExecuteBeforeCameraRender;
			Equipment.onAnyCreaturePartChanged = (Equipment.OnCreaturePartChanged)Delegate.Remove(Equipment.onAnyCreaturePartChanged, new Equipment.OnCreaturePartChanged(this.OnAnyCreaturePartChanged));
			this.isRendering = false;
			this.xrDisplays.Clear();
			if (this.leftEyeRenderTexture && this.leftEyeRenderTexture.IsCreated())
			{
				this.leftEyeRenderTexture.Release();
				UnityEngine.Object.Destroy(this.leftEyeRenderTexture);
			}
			if (this.rightEyeRenderTexture && this.rightEyeRenderTexture.IsCreated())
			{
				this.rightEyeRenderTexture.Release();
				UnityEngine.Object.Destroy(this.rightEyeRenderTexture);
			}
		}

		// Token: 0x06002397 RID: 9111 RVA: 0x000F2ED4 File Offset: 0x000F10D4
		private void OnAnyCreaturePartChanged(EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				this.creaturePartUpdating = true;
				return;
			}
			if (eventTime == EventTime.OnEnd)
			{
				this.creaturePartUpdating = false;
			}
		}

		// Token: 0x06002398 RID: 9112 RVA: 0x000F2EEC File Offset: 0x000F10EC
		private void Awake()
		{
			if (!XRSettings.enabled)
			{
				base.enabled = false;
				return;
			}
			Mirror.local = this;
			if (this.meshToHide == null)
			{
				this.meshToHide = Array.Empty<MeshRenderer>();
			}
			else
			{
				List<MeshRenderer> hiddenMeshes = new List<MeshRenderer>();
				foreach (MeshRenderer hiddenMesh in this.meshToHide)
				{
					if (!(hiddenMesh == null))
					{
						hiddenMeshes.Add(hiddenMesh);
					}
				}
				this.meshToHide = hiddenMeshes.ToArray();
			}
			this.quality = Catalog.gameData.platformParameters.mirrorQuality;
			this.mirrorMesh.material = ThunderRoadSettings.current.mirrorMaterial;
			this.reflectionCamera = new GameObject("ReflectionCamera").AddComponent<Camera>();
			this.reflectionCamera.transform.SetParent(base.transform, true);
			this.reflectionCamera.enabled = false;
			this.universalAdditionalCameraData = this.reflectionCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
			this.Refresh();
		}

		// Token: 0x06002399 RID: 9113 RVA: 0x000F2FDC File Offset: 0x000F11DC
		protected internal override void ManagedUpdate()
		{
			this.playerHeadVisible = false;
			if (!this.active || !this.mirrorMesh.isVisible)
			{
				return;
			}
			if (Player.currentCreature == null)
			{
				return;
			}
			Vector3? scale;
			if (this.playerReflectionHead == null)
			{
				Vector3 vector;
				switch (this.reflectionDirection)
				{
				case Mirror.ReflectionDirection.Up:
					vector = Vector3.up;
					break;
				case Mirror.ReflectionDirection.Down:
					vector = Vector3.down;
					break;
				case Mirror.ReflectionDirection.Forward:
					vector = Vector3.forward;
					break;
				case Mirror.ReflectionDirection.Back:
					vector = Vector3.back;
					break;
				case Mirror.ReflectionDirection.Left:
					vector = Vector3.left;
					break;
				case Mirror.ReflectionDirection.Right:
					vector = Vector3.right;
					break;
				default:
					vector = this.reflectionLocalDirection;
					break;
				}
				this.reflectionLocalDirection = vector;
				this.reflectionWorldDirection = base.transform.TransformDirection(this.reflectionLocalDirection);
				this.reflectionPlane = new Plane(this.reflectionWorldDirection, base.transform.position);
				Transform transform = base.transform;
				string name = "PlayerHeadReflection";
				Vector3 zero = Vector3.zero;
				Quaternion? rotation = null;
				scale = null;
				this.playerReflectionHead = transform.FindOrAddTransform(name, zero, rotation, scale);
			}
			Vector3 reflectionPos;
			if (!this.IsObjectVisibleFromViewer(Player.local.head.transform, Player.local.head.transform, out reflectionPos, out scale))
			{
				return;
			}
			this.playerReflectionHead.position = reflectionPos;
			this.playerHeadVisible = true;
		}

		// Token: 0x0600239A RID: 9114 RVA: 0x000F312C File Offset: 0x000F132C
		public bool IsObjectVisibleFromViewer(Transform mirroredObject, Transform viewer, out Vector3 insideReflectionPos, out Vector3? mirrorPlanePosition)
		{
			Vector3 oPosition = mirroredObject.position;
			Vector3 pointOnPlane = this.reflectionPlane.ClosestPointOnPlane(oPosition);
			Vector3 objectToPlane = oPosition - pointOnPlane;
			insideReflectionPos = pointOnPlane - objectToPlane;
			Vector3 viewerPosition = viewer.position;
			Ray viewerToReflection = new Ray(viewerPosition, (insideReflectionPos - viewerPosition).normalized);
			float dist;
			if (this.reflectionPlane.Raycast(viewerToReflection, out dist))
			{
				float widthHalf = this.widthAndHeight.x / 2f;
				float heightHalf = this.widthAndHeight.y / 2f;
				mirrorPlanePosition = new Vector3?(viewerToReflection.GetPoint(dist));
				Vector3 localCorrected = Quaternion.FromToRotation(base.transform.forward, this.reflectionWorldDirection) * base.transform.InverseTransformPoint(mirrorPlanePosition.Value);
				if (localCorrected.x > -widthHalf && localCorrected.x < widthHalf && localCorrected.y > -heightHalf && localCorrected.y < heightHalf)
				{
					return true;
				}
			}
			mirrorPlanePosition = null;
			return false;
		}

		// Token: 0x0600239B RID: 9115 RVA: 0x000F3240 File Offset: 0x000F1440
		[ContextMenu("Refresh")]
		public void Refresh()
		{
			Vector3 vector;
			switch (this.reflectionDirection)
			{
			case Mirror.ReflectionDirection.Up:
				vector = Vector3.up;
				break;
			case Mirror.ReflectionDirection.Down:
				vector = Vector3.down;
				break;
			case Mirror.ReflectionDirection.Forward:
				vector = Vector3.forward;
				break;
			case Mirror.ReflectionDirection.Back:
				vector = Vector3.back;
				break;
			case Mirror.ReflectionDirection.Left:
				vector = Vector3.left;
				break;
			case Mirror.ReflectionDirection.Right:
				vector = Vector3.right;
				break;
			default:
				vector = this.reflectionLocalDirection;
				break;
			}
			this.reflectionLocalDirection = vector;
			this.reflectionWorldDirection = base.transform.TransformDirection(this.reflectionLocalDirection);
			this.reflectionPlane = new Plane(this.reflectionWorldDirection, base.transform.position);
			this.playerReflectionHead = base.transform.FindOrAddTransform("PlayerHeadReflection", Vector3.zero, null, null);
			if (Application.isPlaying)
			{
				this.xrDisplays.Clear();
				SubsystemManager.GetInstances<XRDisplaySubsystem>(this.xrDisplays);
				if (this.leftEyeRenderTexture != null && this.leftEyeRenderTexture.IsCreated())
				{
					this.leftEyeRenderTexture.Release();
					UnityEngine.Object.Destroy(this.leftEyeRenderTexture);
				}
				if (this.rightEyeRenderTexture != null && this.rightEyeRenderTexture.IsCreated())
				{
					this.rightEyeRenderTexture.Release();
					UnityEngine.Object.Destroy(this.rightEyeRenderTexture);
				}
				this.leftEyeRenderTexture = new RenderTexture((int)(this.quality * (float)XRSettings.eyeTextureWidth), (int)(this.quality * (float)XRSettings.eyeTextureHeight), 24);
				this.rightEyeRenderTexture = new RenderTexture((int)(this.quality * (float)XRSettings.eyeTextureWidth), (int)(this.quality * (float)XRSettings.eyeTextureHeight), 24);
				this.leftEyeRenderTexture.name = "mirror_leftEye";
				this.rightEyeRenderTexture.name = "mirror_rightEye";
				this.leftEyeRenderTexture.filterMode = (this.rightEyeRenderTexture.filterMode = this.filterMode);
				this.leftEyeRenderTexture.antiAliasing = (this.rightEyeRenderTexture.antiAliasing = this.antiAliasing);
				this.leftEyeRenderTexture.hideFlags = (this.rightEyeRenderTexture.hideFlags = HideFlags.HideAndDontSave);
				this.leftEyeRenderTexture.autoGenerateMips = (this.rightEyeRenderTexture.autoGenerateMips = false);
				this.leftEyeRenderTexture.wrapMode = (this.rightEyeRenderTexture.wrapMode = TextureWrapMode.Clamp);
				if (this.leftEyeRenderTexture.width > 0 && this.leftEyeRenderTexture.height > 0)
				{
					this.leftEyeRenderTexture.Create();
				}
				if (this.rightEyeRenderTexture.width > 0 && this.rightEyeRenderTexture.height > 0)
				{
					this.rightEyeRenderTexture.Create();
				}
				if (this.reflectionCamera)
				{
					this.reflectionCamera.clearFlags = this.clearFlags;
					this.reflectionCamera.backgroundColor = this.backgroundColor;
					this.reflectionCamera.cullingMask = this.cullingMask;
					this.reflectionCamera.useOcclusionCulling = this.useOcclusionCulling;
				}
				if (this.universalAdditionalCameraData)
				{
					this.universalAdditionalCameraData.renderShadows = this.shadow;
					this.universalAdditionalCameraData.SetRenderer(this.rendererIndex);
				}
				if (this.mirrorMesh.sharedMaterial)
				{
					if (this.reflectionWithoutGI)
					{
						this.mirrorMesh.sharedMaterial.EnableKeyword("_FULLMIRROR");
					}
					else
					{
						this.mirrorMesh.sharedMaterial.DisableKeyword("_FULLMIRROR");
					}
					this.mirrorMesh.sharedMaterial.SetFloat(Mirror.ReflectionIntensity, this.Intensity);
				}
			}
		}

		/// <summary>
		/// Toggle the armour edit mode.
		/// This is used in the Lever event for the bench.
		/// </summary>
		// Token: 0x0600239C RID: 9116 RVA: 0x000F35D4 File Offset: 0x000F17D4
		public void SetEditMode(bool state)
		{
			this.allowArmourEditing = state;
			if (Player.currentCreature != null && Player.currentCreature.equipment != null)
			{
				Player.currentCreature.equipment.SetWearablesState(state);
				Mirror.OnArmourEditModeChanged onArmourEditModeChangedEvent = this.OnArmourEditModeChangedEvent;
				if (onArmourEditModeChangedEvent != null)
				{
					onArmourEditModeChangedEvent(state);
				}
			}
			if (!state)
			{
				Mirror.ClearRenderQueue();
			}
		}

		// Token: 0x0600239D RID: 9117 RVA: 0x000F3631 File Offset: 0x000F1831
		public void ExecuteBeforeCameraRender(ScriptableRenderContext context, Camera camera)
		{
			if (camera != this.reflectionCamera && camera.cameraType != CameraType.SceneView && camera.cameraType != CameraType.Preview)
			{
				this.RenderCam(context, camera);
			}
		}

		// Token: 0x0600239E RID: 9118 RVA: 0x000F365C File Offset: 0x000F185C
		public void RenderCam(ScriptableRenderContext context, Camera camera)
		{
			if (!this.active || !this.mirrorMesh.isVisible || (this.stopDuringCreaturePartUpdate && this.creaturePartUpdating))
			{
				this.isRendering = false;
				if (!this.hasInvokedEvent)
				{
					this.hasInvokedEvent = true;
					Mirror.OnRenderStateChanged onRenderStateChangedEvent = this.OnRenderStateChangedEvent;
					if (onRenderStateChangedEvent == null)
					{
						return;
					}
					onRenderStateChangedEvent(this.isRendering);
				}
				return;
			}
			this.isRendering = true;
			if (this.hasInvokedEvent)
			{
				this.hasInvokedEvent = false;
				Mirror.OnRenderStateChanged onRenderStateChangedEvent2 = this.OnRenderStateChangedEvent;
				if (onRenderStateChangedEvent2 != null)
				{
					onRenderStateChangedEvent2(this.isRendering);
				}
			}
			this.reflectionWorldDirection = base.transform.TransformDirection(this.reflectionLocalDirection);
			Vector3 normal = this.reflectionWorldDirection;
			float d = -Vector3.Dot(normal, base.transform.position);
			Matrix4x4 reflectionMatrix = Mirror.CalculateReflectionMatrix(new Vector4(normal.x, normal.y, normal.z, d));
			GL.invertCulling = true;
			bool orgFog = RenderSettings.fog;
			float orgFogEndDistance = RenderSettings.fogEndDistance;
			if (this.enableFog)
			{
				RenderSettings.fogEndDistance = 99999f;
			}
			else
			{
				RenderSettings.fog = false;
			}
			for (int i = 0; i < this.meshToHide.Length; i++)
			{
				this.meshToHide[i].enabled = false;
			}
			if (this.showWearableHighlight)
			{
				for (int j = this.renderObjects.Count - 1; j >= 0; j--)
				{
					if (this.renderObjects[j] == null)
					{
						this.renderObjects.RemoveAt(j);
					}
					else
					{
						this.renderObjects[j].materials = new Material[]
						{
							this.renderObjects[j].material,
							ThunderRoadSettings.current.mirrorOutlineMaterial
						};
					}
				}
			}
			if (camera.stereoTargetEye == StereoTargetEyeMask.None)
			{
				this.RenderCam(reflectionMatrix, context, camera);
			}
			else
			{
				this.RenderEye(Mirror.Side.Left, reflectionMatrix, context, camera);
				this.RenderEye(Mirror.Side.Right, reflectionMatrix, context, camera);
			}
			if (this.showWearableHighlight)
			{
				for (int k = 0; k < this.renderObjects.Count; k++)
				{
					this.renderObjects[k].materials = new Material[]
					{
						this.renderObjects[k].material
					};
				}
			}
			if (this.enableFog)
			{
				RenderSettings.fogEndDistance = orgFogEndDistance;
			}
			else
			{
				RenderSettings.fog = orgFog;
			}
			GL.invertCulling = false;
			for (int l = 0; l < this.meshToHide.Length; l++)
			{
				this.meshToHide[l].enabled = true;
			}
		}

		// Token: 0x0600239F RID: 9119 RVA: 0x000F38C8 File Offset: 0x000F1AC8
		public static void ClearRenderQueue()
		{
			if (Mirror.local == null)
			{
				return;
			}
			for (int i = 0; i < Mirror.local.renderObjects.Count; i++)
			{
				Mirror.RemoveFromRenderQueue(new Renderer[]
				{
					Mirror.local.renderObjects[i]
				});
			}
		}

		// Token: 0x060023A0 RID: 9120 RVA: 0x000F391C File Offset: 0x000F1B1C
		public static void QueueRenderObjects(params Renderer[] renderers)
		{
			if (Mirror.local == null || renderers.IsNullOrEmpty())
			{
				return;
			}
			foreach (Renderer renderer in renderers)
			{
				if (!(renderer == null) && !Mirror.local.renderObjects.Contains(renderer))
				{
					Mirror.local.renderObjects.Add(renderer);
				}
			}
		}

		// Token: 0x060023A1 RID: 9121 RVA: 0x000F3980 File Offset: 0x000F1B80
		public static void RemoveFromRenderQueue(params Renderer[] renderers)
		{
			if (Mirror.local == null || renderers.IsNullOrEmpty())
			{
				return;
			}
			for (int i = Mirror.local.renderObjects.Count - 1; i >= 0; i--)
			{
				if (Mirror.local.renderObjects[i] == null)
				{
					Mirror.local.renderObjects.RemoveAt(i);
				}
				else
				{
					for (int x = 0; x < renderers.Length; x++)
					{
						if (Mirror.local.renderObjects[i] == renderers[x])
						{
							Mirror.local.renderObjects[i].materials = new Material[]
							{
								Mirror.local.renderObjects[i].material
							};
							Mirror.local.renderObjects.RemoveAt(i);
							break;
						}
					}
				}
			}
		}

		// Token: 0x060023A2 RID: 9122 RVA: 0x000F3A5C File Offset: 0x000F1C5C
		private void RenderEye(Mirror.Side side, Matrix4x4 reflectionMatrix, ScriptableRenderContext context, Camera camera)
		{
			if (this.xrDisplays.Count <= 0)
			{
				return;
			}
			Matrix4x4 view = camera.worldToCameraMatrix;
			Matrix4x4 proj = camera.projectionMatrix;
			foreach (XRDisplaySubsystem display in this.xrDisplays)
			{
				if (display.running && display.GetRenderPassCount() > 0)
				{
					XRDisplaySubsystem.XRRenderPass renderPass;
					display.GetRenderPass(0, out renderPass);
					XRDisplaySubsystem.XRRenderParameter renderParameter;
					renderPass.GetRenderParameter(camera, (int)side, out renderParameter);
					view = renderParameter.view;
					proj = renderParameter.projection;
				}
			}
			this.reflectionCamera.worldToCameraMatrix = view * reflectionMatrix;
			this.reflectionCamera.projectionMatrix = proj;
			Vector3 cpos = this.reflectionCamera.worldToCameraMatrix.MultiplyPoint(base.transform.position + this.reflectionWorldDirection * 0.001f);
			Vector3 cnormal = this.reflectionCamera.worldToCameraMatrix.MultiplyVector(this.reflectionWorldDirection).normalized;
			Vector4 clipPlane = new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
			this.reflectionCamera.projectionMatrix = this.reflectionCamera.CalculateObliqueMatrix(clipPlane);
			this.reflectionCamera.targetTexture = ((side == Mirror.Side.Left) ? this.leftEyeRenderTexture : this.rightEyeRenderTexture);
			float IPD = Vector3.Distance(Mirror.GetEyePosition(Mirror.Side.Left), Mirror.GetEyePosition(Mirror.Side.Right)) * camera.transform.lossyScale.x;
			this.reflectionCamera.transform.position = this.GetMirroredPos(base.transform.position, this.reflectionWorldDirection, camera.transform.position);
			if (side == Mirror.Side.Left)
			{
				this.reflectionCamera.transform.position -= camera.transform.right * IPD / 2f;
			}
			if (side == Mirror.Side.Right)
			{
				this.reflectionCamera.transform.position += camera.transform.right * IPD / 2f;
			}
			this.reflectionCamera.transform.rotation = Quaternion.LookRotation(this.reflectionWorldDirection.normalized);
			UniversalRenderPipeline.RenderSingleCamera(context, this.reflectionCamera);
			this.mirrorMesh.sharedMaterial.SetTexture((side == Mirror.Side.Left) ? Mirror.LeftEye : Mirror.RightEye, (side == Mirror.Side.Left) ? this.leftEyeRenderTexture : this.rightEyeRenderTexture);
		}

		// Token: 0x060023A3 RID: 9123 RVA: 0x000F3CFC File Offset: 0x000F1EFC
		private void RenderCam(Matrix4x4 reflectionMatrix, ScriptableRenderContext context, Camera camera)
		{
			Matrix4x4 view = camera.worldToCameraMatrix;
			Matrix4x4 proj = camera.projectionMatrix;
			this.reflectionCamera.worldToCameraMatrix = view * reflectionMatrix;
			this.reflectionCamera.projectionMatrix = proj;
			Vector3 cpos = this.reflectionCamera.worldToCameraMatrix.MultiplyPoint(base.transform.position + this.reflectionWorldDirection * 0.001f);
			Vector3 cnormal = this.reflectionCamera.worldToCameraMatrix.MultiplyVector(this.reflectionWorldDirection).normalized;
			Vector4 clipPlane = new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
			this.reflectionCamera.projectionMatrix = this.reflectionCamera.CalculateObliqueMatrix(clipPlane);
			this.reflectionCamera.targetTexture = this.leftEyeRenderTexture;
			this.reflectionCamera.transform.position = this.GetMirroredPos(base.transform.position, this.reflectionWorldDirection, camera.transform.position);
			this.reflectionCamera.transform.rotation = Quaternion.LookRotation(this.reflectionWorldDirection.normalized);
			UniversalRenderPipeline.RenderSingleCamera(context, this.reflectionCamera);
			this.mirrorMesh.sharedMaterial.SetTexture(Mirror.LeftEye, this.leftEyeRenderTexture);
		}

		// Token: 0x060023A4 RID: 9124 RVA: 0x000F3E54 File Offset: 0x000F2054
		public Vector3 GetMirroredPos(Vector3 mirrorPos, Vector3 mirrorNormal, Vector3 inputPos)
		{
			Vector3 v2 = Vector3.Reflect(mirrorPos - inputPos, mirrorNormal);
			return mirrorPos - v2;
		}

		// Token: 0x060023A5 RID: 9125 RVA: 0x000F3E78 File Offset: 0x000F2078
		public static Vector3 GetEyePosition(Mirror.Side eye)
		{
			if (!XRSettings.enabled)
			{
				return Camera.main.transform.position;
			}
			InputDevice device = InputDevices.GetDeviceAtXRNode((eye == Mirror.Side.Left) ? XRNode.LeftEye : XRNode.RightEye);
			Vector3 posLeft;
			if (device.isValid && device.TryGetFeatureValue((eye == Mirror.Side.Left) ? CommonUsages.leftEyePosition : CommonUsages.rightEyePosition, out posLeft))
			{
				return posLeft;
			}
			return default(Vector3);
		}

		// Token: 0x060023A6 RID: 9126 RVA: 0x000F3ED8 File Offset: 0x000F20D8
		private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
		{
			Matrix4x4 reflectionMatrix = Matrix4x4.zero;
			reflectionMatrix.m00 = 1f - 2f * plane[0] * plane[0];
			reflectionMatrix.m01 = -2f * plane[0] * plane[1];
			reflectionMatrix.m02 = -2f * plane[0] * plane[2];
			reflectionMatrix.m03 = -2f * plane[3] * plane[0];
			reflectionMatrix.m10 = -2f * plane[1] * plane[0];
			reflectionMatrix.m11 = 1f - 2f * plane[1] * plane[1];
			reflectionMatrix.m12 = -2f * plane[1] * plane[2];
			reflectionMatrix.m13 = -2f * plane[3] * plane[1];
			reflectionMatrix.m20 = -2f * plane[2] * plane[0];
			reflectionMatrix.m21 = -2f * plane[2] * plane[1];
			reflectionMatrix.m22 = 1f - 2f * plane[2] * plane[2];
			reflectionMatrix.m23 = -2f * plane[3] * plane[2];
			reflectionMatrix.m30 = 0f;
			reflectionMatrix.m31 = 0f;
			reflectionMatrix.m32 = 0f;
			reflectionMatrix.m33 = 1f;
			return reflectionMatrix;
		}

		// Token: 0x060023A7 RID: 9127 RVA: 0x000F4098 File Offset: 0x000F2298
		protected virtual void OnDrawGizmosSelected()
		{
			Mirror.DrawGizmoArrow(base.transform.position, this.reflectionWorldDirection * 0.5f, Color.blue, 0.25f, 20f);
			Vector2 localXlowHigh = new Vector2(-this.widthAndHeight.x / 2f, this.widthAndHeight.x / 2f);
			Vector2 localYlowHigh = new Vector2(-this.widthAndHeight.y / 2f, this.widthAndHeight.y / 2f);
			Gizmos.color = Color.white;
			for (int i = 0; i < 4; i++)
			{
				Vector3 reflectionDir = base.transform.TransformDirection(this.reflectionWorldDirection);
				Vector3 offsetStart = Quaternion.FromToRotation(base.transform.forward, reflectionDir) * new Vector3((i < 2) ? localXlowHigh.x : localXlowHigh.y, (i == 1 || i == 2) ? localYlowHigh.x : localYlowHigh.y, 0f);
				int j = (i + 1) % 4;
				Vector3 offsetEnd = Quaternion.FromToRotation(base.transform.forward, reflectionDir) * new Vector3((j < 2) ? localXlowHigh.x : localXlowHigh.y, (j == 1 || j == 2) ? localYlowHigh.x : localYlowHigh.y, 0f);
				Gizmos.DrawLine(base.transform.position + offsetStart, base.transform.position + offsetEnd);
			}
		}

		// Token: 0x060023A8 RID: 9128 RVA: 0x000F4220 File Offset: 0x000F2420
		public static void DrawGizmoArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20f)
		{
			Gizmos.color = color;
			Gizmos.DrawRay(pos, direction);
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}

		// Token: 0x0400228B RID: 8843
		public static Mirror local;

		// Token: 0x0400228C RID: 8844
		[Tooltip("When enabled, occlusion culling is enabled when the player is in the mirror")]
		public bool useOcclusionCulling;

		// Token: 0x0400228D RID: 8845
		[Tooltip("When enabled, player can change their clothing when in front of the mirror.")]
		public bool allowArmourEditing;

		// Token: 0x0400228E RID: 8846
		[Space]
		[Tooltip("Depicts the direction the reflection is pointing for OcclusionCulling to take effect")]
		public Mirror.ReflectionDirection reflectionDirection;

		// Token: 0x0400228F RID: 8847
		[Tooltip("Depicts the width/height of the mirror.")]
		public Vector2 widthAndHeight = Vector2.one;

		// Token: 0x04002290 RID: 8848
		[Range(0f, 1f)]
		[Tooltip("Depicts the quality of the mirror reflection")]
		public float quality = 1f;

		// Token: 0x04002291 RID: 8849
		[Range(0f, 1f)]
		[Tooltip("Adjusts the intensity of the grain/dirt on the mirror(?)")]
		public float Intensity = 1f;

		// Token: 0x04002292 RID: 8850
		[Tooltip("When enabled, the reflection will reflect everything without any global illumination on the scene")]
		public bool reflectionWithoutGI = true;

		// Token: 0x04002293 RID: 8851
		[Tooltip("Adjusts the mirror reflection's anti-aliasing")]
		public int antiAliasing = 4;

		// Token: 0x04002294 RID: 8852
		[Tooltip("Adjusts the anisotropic filtering of the mirror reflection")]
		public FilterMode filterMode = FilterMode.Bilinear;

		// Token: 0x04002295 RID: 8853
		[Tooltip("Depicts flags that the mirror will avoid rendering in its' reflection.")]
		public CameraClearFlags clearFlags = CameraClearFlags.Skybox;

		// Token: 0x04002296 RID: 8854
		public int rendererIndex = 2;

		// Token: 0x04002297 RID: 8855
		[Tooltip("When enabled, the mirror reflection will render shadows")]
		public bool shadow = true;

		// Token: 0x04002298 RID: 8856
		[Tooltip("When enabled, the mirror reflection will render fog.")]
		public bool enableFog = true;

		// Token: 0x04002299 RID: 8857
		[Tooltip("When enabled, and if armor/clothing editing is allowed, highlighting over the armor will make a white outline in the mirror to depict what clothing is changed.")]
		public bool showWearableHighlight = true;

		// Token: 0x0400229A RID: 8858
		public Color backgroundColor = Color.black;

		// Token: 0x0400229B RID: 8859
		[Tooltip("Depicts what layers are culled from reflection rendering")]
		public LayerMask cullingMask = -1;

		// Token: 0x0400229C RID: 8860
		[Tooltip("Depict what mesh the mirror reflects from")]
		public MeshRenderer mirrorMesh;

		// Token: 0x0400229D RID: 8861
		[Tooltip("A list of specific renderers to hide from the mirror reflection")]
		public MeshRenderer[] meshToHide;

		// Token: 0x0400229E RID: 8862
		private Vector3 reflectionLocalDirection;

		// Token: 0x0400229F RID: 8863
		private Vector3 reflectionWorldDirection;

		// Token: 0x040022A0 RID: 8864
		private bool hasInvokedEvent;

		// Token: 0x040022A3 RID: 8867
		[NonSerialized]
		public bool isRendering;

		// Token: 0x040022A4 RID: 8868
		protected internal bool active = true;

		// Token: 0x040022A5 RID: 8869
		public bool stopDuringCreaturePartUpdate = true;

		// Token: 0x040022A6 RID: 8870
		protected bool creaturePartUpdating;

		// Token: 0x040022A9 RID: 8873
		private Camera reflectionCamera;

		// Token: 0x040022AA RID: 8874
		private RenderTexture leftEyeRenderTexture;

		// Token: 0x040022AB RID: 8875
		private RenderTexture rightEyeRenderTexture;

		// Token: 0x040022AC RID: 8876
		private UniversalAdditionalCameraData universalAdditionalCameraData;

		// Token: 0x040022AD RID: 8877
		private List<XRDisplaySubsystem> xrDisplays = new List<XRDisplaySubsystem>();

		// Token: 0x040022AE RID: 8878
		private List<Renderer> renderObjects = new List<Renderer>();

		// Token: 0x040022AF RID: 8879
		private Plane reflectionPlane;

		// Token: 0x040022B0 RID: 8880
		private static readonly int ReflectionIntensity = Shader.PropertyToID("_ReflectionIntensity");

		// Token: 0x040022B1 RID: 8881
		private static readonly int LeftEye = Shader.PropertyToID("_LeftEye");

		// Token: 0x040022B2 RID: 8882
		private static readonly int RightEye = Shader.PropertyToID("_RightEye");

		// Token: 0x020009C9 RID: 2505
		// (Invoke) Token: 0x0600447A RID: 17530
		public delegate void OnArmourEditModeChanged(bool state);

		// Token: 0x020009CA RID: 2506
		// (Invoke) Token: 0x0600447E RID: 17534
		public delegate void OnRenderStateChanged(bool state);

		// Token: 0x020009CB RID: 2507
		public enum Side
		{
			// Token: 0x040045E5 RID: 17893
			Left,
			// Token: 0x040045E6 RID: 17894
			Right
		}

		// Token: 0x020009CC RID: 2508
		public enum ReflectionDirection
		{
			// Token: 0x040045E8 RID: 17896
			Up,
			// Token: 0x040045E9 RID: 17897
			Down,
			// Token: 0x040045EA RID: 17898
			Forward,
			// Token: 0x040045EB RID: 17899
			Back,
			// Token: 0x040045EC RID: 17900
			Left,
			// Token: 0x040045ED RID: 17901
			Right
		}
	}
}
