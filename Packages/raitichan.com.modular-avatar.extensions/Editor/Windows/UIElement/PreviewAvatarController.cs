using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class PreviewAvatarController {
		private readonly VRCAvatarDescriptor _originAvatar;
		private readonly VRCAvatarDescriptor _previewAvatar;

		private readonly Dictionary<GameObject, GameObject> _originToPreviewObjectDictionary = new Dictionary<GameObject, GameObject>();
		private readonly Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer> _originToPreviewSkinnedMeshRendererDictionary = new Dictionary<SkinnedMeshRenderer, SkinnedMeshRenderer>();
		private readonly Dictionary<Renderer, Renderer> _originToPreviewRendererDictionary = new Dictionary<Renderer, Renderer>();

		public PreviewAvatarController(VRCAvatarDescriptor originAvatar, VRCAvatarDescriptor previewAvatar) {
			this._originAvatar = originAvatar;
			this._previewAvatar = previewAvatar;
		}

		public void ResetPreviewObject() {
			foreach (KeyValuePair<GameObject, GameObject> gameObject in this._originToPreviewObjectDictionary) {
				gameObject.Value.SetActive(gameObject.Key.activeSelf);
			}

			this._originToPreviewObjectDictionary.Clear();

			foreach (KeyValuePair<SkinnedMeshRenderer, SkinnedMeshRenderer> skinnedMeshRenderer in this._originToPreviewSkinnedMeshRendererDictionary) {
				int blendShapeCount = skinnedMeshRenderer.Key.sharedMesh.blendShapeCount;
				for (int i = 0; i < blendShapeCount; i++) {
					skinnedMeshRenderer.Value.SetBlendShapeWeight(i, skinnedMeshRenderer.Key.GetBlendShapeWeight(i));
				}
			}

			this._originToPreviewSkinnedMeshRendererDictionary.Clear();

			foreach (KeyValuePair<Renderer, Renderer> renderer in this._originToPreviewRendererDictionary) {
				int materialCount = renderer.Key.sharedMaterials.Length;
				Material[] materials = new Material[materialCount];
				for (int i = 0; i < materialCount; i++) {
					materials[i] = renderer.Key.sharedMaterials[i];
				}

				renderer.Value.sharedMaterials = materials;
			}

			this._originToPreviewRendererDictionary.Clear();
		}

		public void SetActive(GameObject originObject, bool enable) {
			if (originObject == null) throw new NullReferenceException(nameof(originObject));
			if (!this._originToPreviewObjectDictionary.TryGetValue(originObject, out GameObject previewObject)) {
				string objectPath = originObject.GetPathInAvatar();
				previewObject = this._previewAvatar.transform.Find(objectPath).gameObject;
				this._originToPreviewObjectDictionary[originObject] = previewObject;
			}

			previewObject.SetActive(enable);
		}

		public void ResetActive(GameObject originObject) {
			if (originObject == null) throw new NullReferenceException(nameof(originObject));
			if (!this._originToPreviewObjectDictionary.TryGetValue(originObject, out GameObject previewObject)) return;
			previewObject.SetActive(originObject.activeSelf);
		}

		public void SetBlendShapeWeight(SkinnedMeshRenderer originRenderer, string blendShapeName, float weight) {
			if (originRenderer == null) throw new NullReferenceException(nameof(originRenderer));
			if (!this._originToPreviewSkinnedMeshRendererDictionary.TryGetValue(originRenderer, out SkinnedMeshRenderer previewRenderer)) {
				string objectPath = originRenderer.GetPathInAvatar();
				previewRenderer = this._previewAvatar.transform.Find(objectPath).GetComponent<SkinnedMeshRenderer>();
				this._originToPreviewSkinnedMeshRendererDictionary[originRenderer] = previewRenderer;
			}

			int blendShapeIndex = previewRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
			if (blendShapeIndex < 0 || previewRenderer.sharedMesh.blendShapeCount <= blendShapeIndex) return;
			previewRenderer.SetBlendShapeWeight(blendShapeIndex, weight);
		}

		public void ResetBlendShapeWeight(SkinnedMeshRenderer originRenderer, string blendShapeName) {
			if (originRenderer == null) throw new NullReferenceException(nameof(originRenderer));
			if (!this._originToPreviewSkinnedMeshRendererDictionary.TryGetValue(originRenderer, out SkinnedMeshRenderer previewRenderer)) return;
			int blendShapeIndex = previewRenderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
			if (blendShapeIndex < 0 || previewRenderer.sharedMesh.blendShapeCount <= blendShapeIndex) return;
			previewRenderer.SetBlendShapeWeight(blendShapeIndex, originRenderer.GetBlendShapeWeight(blendShapeIndex));
		}

		public void SetMaterial(Renderer originRenderer, int materialIndex, Material material) {
			if (originRenderer == null) throw new NullReferenceException(nameof(originRenderer));
			if (!this._originToPreviewRendererDictionary.TryGetValue(originRenderer, out Renderer previewRenderer)) {
				string objectPath = originRenderer.GetPathInAvatar();
				previewRenderer = this._previewAvatar.transform.Find(objectPath).GetComponent<Renderer>();
				this._originToPreviewRendererDictionary[originRenderer] = previewRenderer;
			}

			int materialCount = previewRenderer.sharedMaterials.Length;
			Material[] materials = new Material[materialCount];
			for (int i = 0; i < materialCount; i++) {
				materials[i] = previewRenderer.sharedMaterials[i];
			}
			materials[materialIndex] = material;
			previewRenderer.sharedMaterials = materials;
		}

		public void ResetMaterial(Renderer originRenderer, int materialIndex) {
			if (originRenderer == null) throw new NullReferenceException(nameof(originRenderer));
			if (!this._originToPreviewRendererDictionary.TryGetValue(originRenderer, out Renderer previewRenderer)) return;
			int materialCount = previewRenderer.sharedMaterials.Length;
			Material[] materials = new Material[materialCount];
			for (int i = 0; i < materialCount; i++) {
				materials[i] = previewRenderer.sharedMaterials[i];
			}
			materials[materialIndex] = originRenderer.sharedMaterials[materialIndex];
			previewRenderer.sharedMaterials = materials;
		}
	}
}