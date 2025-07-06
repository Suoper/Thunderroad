using System;
using TMPro;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000377 RID: 887
	[ExecuteInEditMode]
	public class TMPWarper : MonoBehaviour
	{
		// Token: 0x06002A17 RID: 10775 RVA: 0x0011D9C2 File Offset: 0x0011BBC2
		private void Awake()
		{
			if (this._optionalTextHandler != null)
			{
				this._optionalTextHandler.TextChanged += this.Refresh;
			}
			this._text.ForceMeshUpdate(false, false);
			this.Refresh();
		}

		// Token: 0x06002A18 RID: 10776 RVA: 0x0011D9FC File Offset: 0x0011BBFC
		private void OnEnable()
		{
			this.Refresh();
		}

		// Token: 0x06002A19 RID: 10777 RVA: 0x0011DA04 File Offset: 0x0011BC04
		private void OnValidate()
		{
			this.Refresh();
		}

		/// <summary>
		/// Refresh the curved text.
		/// </summary>
		// Token: 0x06002A1A RID: 10778 RVA: 0x0011DA0C File Offset: 0x0011BC0C
		public void Refresh()
		{
			this.Warp(this._curveScaling);
		}

		/// <summary>
		/// Curve the target text.
		/// </summary>
		// Token: 0x06002A1B RID: 10779 RVA: 0x0011DA1C File Offset: 0x0011BC1C
		private void Warp(float curveScale = 25f)
		{
			TMP_TextInfo textInfo = this._text.textInfo;
			if (textInfo == null || textInfo.characterCount == 0)
			{
				return;
			}
			this._warpCurve.preWrapMode = WrapMode.Once;
			this._warpCurve.postWrapMode = WrapMode.Once;
			this._text.havePropertiesChanged = true;
			this._text.ForceMeshUpdate(false, false);
			float boundsMinX = this._text.bounds.min.x;
			float boundsMaxX = this._text.bounds.max.x;
			for (int i = 0; i < textInfo.characterCount; i++)
			{
				if (textInfo.characterInfo[i].isVisible)
				{
					int vIndex = textInfo.characterInfo[i].vertexIndex;
					int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
					Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
					Vector3 offsetToMidBaseline = new Vector2((vertices[vIndex].x + vertices[vIndex + 2].x) / 2f, textInfo.characterInfo[i].baseLine);
					vertices[vIndex] += -offsetToMidBaseline;
					vertices[vIndex + 1] += -offsetToMidBaseline;
					vertices[vIndex + 2] += -offsetToMidBaseline;
					vertices[vIndex + 3] += -offsetToMidBaseline;
					float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX);
					float x = x0 + 0.0001f;
					float y0 = this._warpCurve.Evaluate(x0) * curveScale;
					float y = this._warpCurve.Evaluate(x) * curveScale;
					Vector3 lhs = new Vector3(1f, 0f, 0f);
					Vector3 tangent = new Vector3(x * (boundsMaxX - boundsMinX) + boundsMinX, y) - new Vector3(offsetToMidBaseline.x, y0);
					float dot = Mathf.Acos(Vector3.Dot(lhs, tangent.normalized)) * 57.29578f;
					float angle = (Vector3.Cross(lhs, tangent).z > 0f) ? dot : (360f - dot);
					Matrix4x4 matrix = Matrix4x4.TRS(new Vector3(0f, y0, 0f), Quaternion.Euler(0f, 0f, angle), Vector3.one);
					vertices[vIndex] = matrix.MultiplyPoint3x4(vertices[vIndex]);
					vertices[vIndex + 1] = matrix.MultiplyPoint3x4(vertices[vIndex + 1]);
					vertices[vIndex + 2] = matrix.MultiplyPoint3x4(vertices[vIndex + 2]);
					vertices[vIndex + 3] = matrix.MultiplyPoint3x4(vertices[vIndex + 3]);
					vertices[vIndex] += offsetToMidBaseline;
					vertices[vIndex + 1] += offsetToMidBaseline;
					vertices[vIndex + 2] += offsetToMidBaseline;
					vertices[vIndex + 3] += offsetToMidBaseline;
					this._text.UpdateVertexData();
				}
			}
		}

		// Token: 0x040027E3 RID: 10211
		[SerializeField]
		private TMP_Text _text;

		// Token: 0x040027E4 RID: 10212
		[SerializeField]
		private UIText _optionalTextHandler;

		// Token: 0x040027E5 RID: 10213
		[SerializeField]
		private AnimationCurve _warpCurve = new AnimationCurve();

		// Token: 0x040027E6 RID: 10214
		[SerializeField]
		private float _curveScaling = 55.05f;
	}
}
