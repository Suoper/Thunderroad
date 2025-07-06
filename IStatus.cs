using System;

namespace ThunderRoad
{
	// Token: 0x0200023D RID: 573
	public interface IStatus
	{
		// Token: 0x06001820 RID: 6176
		void OnCatalogRefresh();

		// Token: 0x06001821 RID: 6177
		void Spawn(StatusData data, ThunderEntity entity);

		/// <summary>
		/// Called when the effect is first applied.
		/// </summary>
		// Token: 0x06001822 RID: 6178
		void Apply();

		/// <summary>
		/// Called when the effect is first applied.
		/// Not called on subsequent Apply() calls from ReapplyOnValueChange.
		/// </summary>
		// Token: 0x06001823 RID: 6179
		void FirstApply();

		// Token: 0x06001824 RID: 6180
		void PlayEffect();

		/// <summary>
		/// Called when the status parameter value has changed, if it has one.
		/// You may want to re-apply or change the effect on your entity.
		/// </summary>
		// Token: 0x06001825 RID: 6181
		void OnValueChange();

		/// <summary>
		/// Called once per entity per effect per FixedUpdate.
		/// </summary>
		// Token: 0x06001826 RID: 6182
		void FixedUpdate();

		/// <summary>
		/// Called once per entity per effect per frame.
		/// </summary>
		// Token: 0x06001827 RID: 6183
		void Update();

		/// <summary>
		/// Called when it's time to remove the effect. May be called on value change if ReapplyOnValueChange is true.
		/// </summary>
		// Token: 0x06001828 RID: 6184
		void Remove();

		/// <summary>
		/// Called when the effect is removed. Only called when the effect is fully removed, not just being reapplied.
		/// </summary>
		// Token: 0x06001829 RID: 6185
		void FullRemove();

		// Token: 0x0600182A RID: 6186
		bool CheckExpired();

		/// <summary>
		/// Called to check for any expiring handlers.
		/// </summary>
		// Token: 0x0600182B RID: 6187
		bool Refresh();

		/// <summary>
		/// Called to despawn the status and release its pooled objects
		/// </summary>
		// Token: 0x0600182C RID: 6188
		void Despawn();

		/// <summary>
		/// Does the status effect have any handlers?
		/// </summary>
		// Token: 0x0600182D RID: 6189
		bool HasHandlers();

		/// <summary>
		/// Remove a handler from this effect.
		/// Does not refresh the effect, use <c>ThunderEntity.Remove()</c> instead.
		/// </summary>
		// Token: 0x0600182E RID: 6190
		bool RemoveHandler(object handler);

		/// <summary>
		/// Remove all handlers from this effect.
		/// Does not remove the effect, use <c>ThunderEntity.Clear()</c> instead.
		/// </summary>
		// Token: 0x0600182F RID: 6191
		void ClearHandlers();

		// Token: 0x06001830 RID: 6192
		void Transfer(ThunderEntity other);

		// Token: 0x06001831 RID: 6193
		bool AddHandler(object handler, float duration = float.PositiveInfinity, object parameter = null, bool playEffect = true);
	}
}
