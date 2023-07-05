using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[DisallowMultipleComponent]
	[AddComponentMenu("Modular Avatar/MAEx Object Preset")]
	public class MAExObjectPreset : MAExAnimatorGeneratorModuleBase<MAExObjectPreset> {
		public string parameterName;
		public int defaultValue;
		public bool isInternal;
		public bool saved;

		public List<Preset> presets;

		public IEnumerable<GameObject> GetAllReferencedGameObjects() {
			HashSet<GameObject> alreadyReturned = new HashSet<GameObject>();
			foreach (Preset preset in this.presets) {
				foreach (GameObject showObject in preset.showObjects
					         .Where(showObject => showObject != null && !alreadyReturned.Contains(showObject))) {
					alreadyReturned.Add(showObject);
					yield return showObject;
				}

				foreach (GameObject toggleObject in preset.toggleSets.SelectMany(toggleSet =>
					         toggleSet.showObjects.Where(toggleObject => toggleObject != null && !alreadyReturned.Contains(toggleObject)))) {
					alreadyReturned.Add(toggleObject);
					yield return toggleObject;
				}
			}
		}

		public IEnumerable<SkinnedMeshRenderer> GetAllReferencedSkinnedMeshRenderer() {
			HashSet<SkinnedMeshRenderer> alreadyReturned = new HashSet<SkinnedMeshRenderer>();
			foreach (Preset preset in this.presets) {
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in preset.blendShapes
					         .Select(blendShape => blendShape.skinnedMeshRenderer)
					         .Where(skinnedMeshRenderer => skinnedMeshRenderer != null && !alreadyReturned.Contains(skinnedMeshRenderer))) {
					alreadyReturned.Add(skinnedMeshRenderer);
					yield return skinnedMeshRenderer;
				}

				foreach (SkinnedMeshRenderer skinnedMeshRenderer in preset.toggleSets
					         .SelectMany(toggleSet => toggleSet.blendShapes)
					         .Select(blendShapes => blendShapes.skinnedMeshRenderer)
					         .Where(skinnedMeshRenderer => skinnedMeshRenderer != null && !alreadyReturned.Contains(skinnedMeshRenderer))) {
					alreadyReturned.Add(skinnedMeshRenderer);
					yield return skinnedMeshRenderer;
				}
			}
		}

		public IEnumerable<string> GetAllUsedBlendShapeName(SkinnedMeshRenderer skinnedMeshRenderer) {
			HashSet<string> alreadyReturned = new HashSet<string>();
			foreach (Preset preset in this.presets) {
				foreach (string key in preset.blendShapes
					         .Where(blendShape => blendShape.skinnedMeshRenderer == skinnedMeshRenderer)
					         .SelectMany(blendShape => blendShape.weights)
					         .Select(weight => weight.key)
					         .Where(key => !alreadyReturned.Contains(key))) {
					alreadyReturned.Add(key);
					yield return key;
				}

				foreach (string key in preset.toggleSets
					         .SelectMany(toggleSet => toggleSet.blendShapes)
					         .Where(blendShape => blendShape.skinnedMeshRenderer == skinnedMeshRenderer)
					         .SelectMany(blendShape => blendShape.weights)
					         .Select(weight => weight.key)
					         .Where(key => !alreadyReturned.Contains(key))) {
					alreadyReturned.Add(key);
					yield return key;
				}
			}
		}

		public IEnumerable<Renderer> GetAllReferencedRenderers() {
			HashSet<Renderer> alreadyReturned = new HashSet<Renderer>();
			foreach (Preset preset in this.presets) {
				foreach (Renderer target in preset.materialReplaces
					         .Select(replace => replace.renderer)
					         .Where(target => target != null && !alreadyReturned.Contains(target))) {
					alreadyReturned.Add(target);
					yield return target;
				}

				foreach (Renderer target in preset.toggleSets
					         .SelectMany(toggleSet => toggleSet.materialReplaces)
					         .Select(replace => replace.renderer)
					         .Where(target => target != null && !alreadyReturned.Contains(target))) {
					alreadyReturned.Add(target);
					yield return target;
				}
			}
		}

		public IEnumerable<int> GetAllUsedMaterialIndex(Renderer target) {
			HashSet<int> alreadyReturned = new HashSet<int>();
			foreach (Preset preset in this.presets) {
				foreach (MaterialReplace materialReplace in preset.materialReplaces.Where(replace => replace.renderer == target)) {
					for (int i = 0; i < materialReplace.materials.Count; i++) {
						if (materialReplace.materials[i] == null) continue;
						if (alreadyReturned.Contains(i)) continue;
						alreadyReturned.Add(i);
						yield return i;
					}
				}

				foreach (MaterialReplace materialReplace in preset.toggleSets.SelectMany(toggleSet => toggleSet.materialReplaces).Where(replace => replace.renderer == target)) {
					for (int i = 0; i < materialReplace.materials.Count; i++) {
						if (materialReplace.materials[i] == null) continue;
						if (alreadyReturned.Contains(i)) continue;
						alreadyReturned.Add(i);
						yield return i;
					}
				}
			}
		}

		public int GetSyncedToggleParameterUseCount() {
			return this.presets.Select(preset => preset.toggleSets.Count).Max();
		}

		[Serializable]
		public class Preset {
			public string displayName;
			public Texture2D menuIcon;

			public List<GameObject> showObjects;
			public List<BlendShape> blendShapes;
			public List<MaterialReplace> materialReplaces;

			public List<ToggleSet> toggleSets;

			public IEnumerable<GameObject> GetAllReferencedToggleGameObject() {
				HashSet<GameObject> alreadyReturned = new HashSet<GameObject>();
				foreach (GameObject toggleObject in toggleSets.SelectMany(toggleSet => toggleSet.showObjects.Where(toggleObject => toggleObject != null && !alreadyReturned.Contains(toggleObject)))) {
					alreadyReturned.Add(toggleObject);
					yield return toggleObject;
				}
			}
		}

		[Serializable]
		public class ToggleSet {
			public string displayName;
			public Texture2D menuIcon;

			public bool defaultValue;
			public bool saved;
			public bool preview;

			public List<GameObject> showObjects;
			public List<BlendShape> blendShapes;
			public List<MaterialReplace> materialReplaces;
		}

		[Serializable]
		public class BlendShape {
			public SkinnedMeshRenderer skinnedMeshRenderer;
			public List<BlendShapeWeight> weights;
		}

		[Serializable]
		public struct BlendShapeWeight {
			public string key;
			public float value;
		}

		[Serializable]
		public class MaterialReplace {
			public Renderer renderer;
			public List<Material> materials;
		}
	}
}