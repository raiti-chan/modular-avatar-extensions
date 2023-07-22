using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[DisallowMultipleComponent]
	[AddComponentMenu("Modular Avatar/MAEx Object Preset")]
	public class MAExObjectPreset : MAExAnimatorGeneratorModuleBase<MAExObjectPreset>, ISerializationCallbackReceiver {
		[SerializeField] private int _dataVersion;
		public string parameterName;
		public int defaultValue;
		public bool isInternal;
		public bool saved;

		public List<Preset> presets;

		/// <summary>
		/// デフォルトで表示しないオブジェクトを取得
		/// プリセットで表示設定されているオブジェクト
		/// トグルで表示設定されているオブジェクト(ただしトグルの属するプリセット上で非表示に設定されている場合を除く)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<GameObject> GetDefaultHideObjects() {
			HashSet<GameObject> alreadyReturned = new HashSet<GameObject>();

			foreach (Preset preset in this.presets) {
				Dictionary<GameObject, bool> isEnableInPreset = new Dictionary<GameObject, bool>();
				foreach (EnableObject enableObject in preset.enableObjects.Where(obj => obj.gameObject != null && !alreadyReturned.Contains(obj.gameObject))) {
					// alreadyReturnedに含まれていれば、既にHideObjectなので考慮する必要が無い
					isEnableInPreset[enableObject.gameObject] = enableObject.enable;
					if (!enableObject.enable) continue;
					alreadyReturned.Add(enableObject.gameObject);
					yield return enableObject.gameObject;
				}

				foreach (EnableObject enableObject in preset.toggleSets.SelectMany(toggleSet => toggleSet.enableObjects)
					         .Where(obj => obj.enable && obj.gameObject != null && !alreadyReturned.Contains(obj.gameObject))) {
					// ToggleSet内の表示設定オブジェクトのみを抽出
					if (isEnableInPreset.TryGetValue(enableObject.gameObject, out bool isEnable)) {
						// プリセットで非表示に設定されている場合は無視
						if (!isEnable) continue;
					}

					alreadyReturned.Add(enableObject.gameObject);
					yield return enableObject.gameObject;
				}
			}
		}
		
		public IEnumerable<GameObject> GetDefaultShowObjects() {
			HashSet<GameObject> defaultShowObjects = new HashSet<GameObject>(this.GetDefaultHideObjects());
			HashSet<GameObject> alreadyReturned = new HashSet<GameObject>();
			foreach (Preset preset in this.presets) {
				foreach (EnableObject enableObject in preset.enableObjects.Where(obj => !alreadyReturned.Contains(obj.gameObject) && !defaultShowObjects.Contains(obj.gameObject))) {
					alreadyReturned.Add(enableObject.gameObject);
					yield return enableObject.gameObject;
				}

				foreach (EnableObject enableObject in preset.toggleSets.SelectMany(toggleSet => toggleSet.enableObjects)
					         .Where(obj => !alreadyReturned.Contains(obj.gameObject) && !defaultShowObjects.Contains(obj.gameObject))) {
					alreadyReturned.Add(enableObject.gameObject);
					yield return enableObject.gameObject;
				}
			}
		}

		private bool IsDefaultHideObject(GameObject target) {
			foreach (Preset preset in this.presets) {
				bool containsInPreset = false;
				foreach (EnableObject enableObject in preset.enableObjects
					         .Where(obj => obj.gameObject == target)) {
					if (enableObject.enable) return true;
					containsInPreset = true;
					break;
				}
				if (containsInPreset) continue;
				
				if (preset.toggleSets.SelectMany(toggleSet => toggleSet.enableObjects)
				    .Where(obj => obj.gameObject == target)
				    .Any(enableObject => enableObject.enable)) {
					return true;
				}
			}

			return false;
		}

		public bool IsDefaultShowObject(GameObject target) {
			return !this.IsDefaultHideObject(target);
		}

		public bool IsContainsObject(GameObject target) {
			foreach (Preset preset in this.presets) {
				if (preset.enableObjects.Any(enableObject => enableObject.gameObject == target)) {
					return true;
				}

				if (preset.toggleSets.SelectMany(toggleSet => toggleSet.enableObjects).Any(enableObject => enableObject.gameObject == target)) {
					return true;
				}
			}

			return false;
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

		public void OnBeforeSerialize() { }

		[Obsolete("Is Internal Method")]
		public void OnAfterDeserialize() {
			if (this._dataVersion == 1) return;
			foreach (Preset preset in this.presets) {
				preset.OnAfterDeserialize();
			}

			this._dataVersion = 1;
		}

		[Serializable]
		public class Preset {
			public string displayName;
			public Texture2D menuIcon;

			public List<EnableObject> enableObjects;
			public List<BlendShape> blendShapes;
			public List<MaterialReplace> materialReplaces;

			public List<ToggleSet> toggleSets;
			
			[SerializeField] private List<GameObject> showObjects;

			public EnableObject this[GameObject target] => this.enableObjects.FirstOrDefault(obj => obj.gameObject == target);

			public IEnumerable<GameObject> GetHideObjects() {
				HashSet<GameObject> isShowSetInToggle = new HashSet<GameObject>(this.toggleSets.SelectMany(toggleSet => toggleSet.enableObjects)
					.Where(obj => !obj.enable && obj.gameObject != null)
					.Select(obj => obj.gameObject));
				
				HashSet<GameObject> alreadyReturned = new HashSet<GameObject>();
				foreach (EnableObject enableObject in this.enableObjects
					         .Where(obj => !obj.enable && obj.gameObject != null && !alreadyReturned.Contains(obj.gameObject))) {
					// トグルで非表示設定がある場合無視
					if (isShowSetInToggle.Contains(enableObject.gameObject)) continue;
					// プリセットで非表示設定
					alreadyReturned.Add(enableObject.gameObject);
					yield return enableObject.gameObject;
				}
			}

			public IEnumerable<GameObject> GetShowObjects() {
				HashSet<GameObject> isShowSetInToggle = new HashSet<GameObject>(this.toggleSets.SelectMany(toggleSet => toggleSet.enableObjects)
					.Where(obj => obj.enable && obj.gameObject != null)
					.Select(obj => obj.gameObject));

				HashSet<GameObject> alreadyReturned = new HashSet<GameObject>();
				foreach (EnableObject enableObject in this.enableObjects
					         .Where(obj => obj.enable && obj.gameObject != null && !alreadyReturned.Contains(obj.gameObject))) {
					// トグルで表示設定がある場合無視
					if (isShowSetInToggle.Contains(enableObject.gameObject)) continue;
					// プリセットで表示設定されている場合表示
					alreadyReturned.Add(enableObject.gameObject);
					yield return enableObject.gameObject;
				}
			}
			
			public bool IsContainsObject(GameObject target) {
				// ReSharper disable once LoopCanBeConvertedToQuery
				foreach (EnableObject enableObject in this.enableObjects.Where(obj => obj.gameObject == target)) {
					return this.toggleSets.SelectMany(toggleSet => toggleSet.enableObjects)
						.Where(obj => obj.gameObject == target)
						.All(toggleEnableObject => enableObject.enable != toggleEnableObject.enable);
				}
				return false;
			}
			
			public void OnAfterDeserialize() {
				this.enableObjects = new List<EnableObject>();
				foreach (GameObject showObject in this.showObjects) {
					this.enableObjects.Add(new EnableObject { gameObject = showObject, enable = true });
				}

				foreach (ToggleSet toggleSet in this.toggleSets) {
					toggleSet.OnAfterDeserialize();
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
			public List<string> exclusiveTags;

			public List<EnableObject> enableObjects;
			public List<BlendShape> blendShapes;
			public List<MaterialReplace> materialReplaces;
			
			
			[SerializeField] private List<GameObject> showObjects;
			public void OnAfterDeserialize() {
				this.enableObjects = new List<EnableObject>();
				foreach (GameObject showObject in this.showObjects) {
					this.enableObjects.Add(new EnableObject { gameObject = showObject, enable = true });
				}
			}
		}

		[Serializable]
		public struct EnableObject {
			public GameObject gameObject;
			public bool enable;
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