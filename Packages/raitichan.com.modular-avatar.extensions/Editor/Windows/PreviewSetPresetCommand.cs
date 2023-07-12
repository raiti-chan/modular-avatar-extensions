using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewSetPresetCommand : IPresetEditorPreviewCommand {
		private readonly int _presetIndex;
		private readonly bool _isReload;

		public PreviewSetPresetCommand(int presetIndex) {
			this._presetIndex = presetIndex;
		}
		
		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			controller.ResetPreviewObject();
			context.ClearLayerStack();
			if (this._presetIndex < 0 || data.presets.Count <= this._presetIndex) return;
			
			context.SelectPresetIndex = this._presetIndex;
			SetPreview(data, controller, context, true);
		}
		
		public static void SetPreview(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context, bool useDefaultValueInToggle) {
			// デフォルトで非表示のオブジェクトを隠す
			foreach (GameObject defaultHideObject in data.GetDefaultHideObjects()) {
				context.GetShowObjectLayerStack(defaultHideObject).AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, false);
				controller.SetActive(defaultHideObject, false);
			}

			// デフォルトで表示のオブジェクトを表示
			foreach (GameObject defaultShowObject in data.GetDefaultShowObjects()) {
				context.GetShowObjectLayerStack(defaultShowObject).AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, true);
				controller.SetActive(defaultShowObject, true);
			}

			// プリセット適用
			MAExObjectPreset.Preset preset = data.presets[context.SelectPresetIndex];
			// プリセットで表示されるオブジェクト
			foreach (GameObject gameObject in preset.GetHideObjects()) {
				context.GetShowObjectLayerStack(gameObject).AddLayer(PresetPreviewContext.PRESET_LAYER, false);
				controller.SetActive(gameObject, false);
			}

			// プリセットで非表示のオブジェクト
			foreach (GameObject gameObject in preset.GetShowObjects()) {
				context.GetShowObjectLayerStack(gameObject).AddLayer(PresetPreviewContext.PRESET_LAYER, true);
				controller.SetActive(gameObject, true);
			}

			// プリセットのブレンドシェイプ
			foreach (MAExObjectPreset.BlendShape blendShape in preset.blendShapes.Where(shape => shape.skinnedMeshRenderer != null)) {
				SkinnedMeshRenderer skinnedMeshRenderer = blendShape.skinnedMeshRenderer;
				foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in blendShape.weights) {
					context.GetBlendShapeLayerStack(skinnedMeshRenderer, blendShapeWeight.key).AddLayer(PresetPreviewContext.PRESET_LAYER, blendShapeWeight.value);
					controller.SetBlendShapeWeight(skinnedMeshRenderer, blendShapeWeight.key, blendShapeWeight.value);
				}
			}

			// プリセットのマテリアル
			foreach (MAExObjectPreset.MaterialReplace materialReplace in preset.materialReplaces.Where(replace => replace.renderer != null)) {
				Renderer renderer = materialReplace.renderer;
				for (int materialIndex = 0; materialIndex < materialReplace.materials.Count; materialIndex++) {
					if (materialReplace.materials[materialIndex] == null) continue;
					context.GetMaterialLayerStack(renderer, materialIndex).AddLayer(PresetPreviewContext.PRESET_LAYER, materialReplace.materials[materialIndex]);
					controller.SetMaterial(renderer, materialIndex, materialReplace.materials[materialIndex]);
				}
			}

			// トグル適用
			for (int i = 0; i < preset.toggleSets.Count; i++) {
				MAExObjectPreset.ToggleSet toggleSet = preset.toggleSets[i];
				if (useDefaultValueInToggle ? !toggleSet.defaultValue : !toggleSet.preview) continue;
				foreach (MAExObjectPreset.EnableObject enableObject in toggleSet.enableObjects.Where(obj => obj.gameObject != null)) {
					PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(enableObject.gameObject);
					layerStack.AddLayer(i, enableObject.enable);
					controller.SetActive(enableObject.gameObject, layerStack.GetTopLayer().Value);
				}

				foreach (MAExObjectPreset.BlendShape blendShape in toggleSet.blendShapes.Where(shape => shape.skinnedMeshRenderer != null)) {
					SkinnedMeshRenderer skinnedMeshRenderer = blendShape.skinnedMeshRenderer;
					foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in blendShape.weights) {
						PresetPreviewContext.LayerStack<float> layerStack = context.GetBlendShapeLayerStack(skinnedMeshRenderer, blendShapeWeight.key);
						layerStack.AddLayer(i, blendShapeWeight.value);
						controller.SetBlendShapeWeight(skinnedMeshRenderer, blendShapeWeight.key, layerStack.GetTopLayer().Value);
					}
				}

				foreach (MAExObjectPreset.MaterialReplace materialReplace in toggleSet.materialReplaces.Where(replace => replace.renderer != null)) {
					Renderer renderer = materialReplace.renderer;
					for (int materialIndex = 0; materialIndex < materialReplace.materials.Count; materialIndex++) {
						if (materialReplace.materials[materialIndex] == null) continue;
						PresetPreviewContext.LayerStack<Material> layerStack = context.GetMaterialLayerStack(renderer, materialIndex);
						layerStack.AddLayer(i, materialReplace.materials[materialIndex]);
						controller.SetMaterial(renderer, materialIndex, materialReplace.materials[materialIndex]);
					}
				}
			}
		}
	}
}