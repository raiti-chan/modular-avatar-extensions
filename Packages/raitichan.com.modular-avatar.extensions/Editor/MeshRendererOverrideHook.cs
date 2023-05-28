using System.Linq;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor {
	public class MeshRendererOverrideHook {
		public void OnProcessAvatar(GameObject avatarGameObject) {
			this.TraversObject(avatarGameObject);
		}

		private void TraversObject(GameObject currentObject, MAExMeshRendererOverride parentOverride = null) {
			MAExMeshRendererOverride currentOverride = currentObject.GetComponent<MAExMeshRendererOverride>();
			MAExMeshRendererOverrideBlocker currentBlocker = currentObject.GetComponent<MAExMeshRendererOverrideBlocker>();

			if (currentBlocker != null) {
				parentOverride = null;
			}

			if (currentOverride != null) {
				parentOverride = currentOverride;
			}

			if (parentOverride != null) {
				MeshRenderer meshRenderer = currentObject.GetComponent<MeshRenderer>();
				if (meshRenderer != null) {
					meshRenderer.probeAnchor = parentOverride.AnchorOverride;
				}

				SkinnedMeshRenderer skinnedMeshRenderer = currentObject.GetComponent<SkinnedMeshRenderer>();
				if (skinnedMeshRenderer != null) {
					skinnedMeshRenderer.probeAnchor = parentOverride.AnchorOverride;
					skinnedMeshRenderer.rootBone = parentOverride.RootBone;
					skinnedMeshRenderer.localBounds = parentOverride.Bounds;
				}
			}

			foreach (Transform child in currentObject.transform.Cast<Transform>()) {
				this.TraversObject(child.gameObject, parentOverride);
			}
		}
	}
}