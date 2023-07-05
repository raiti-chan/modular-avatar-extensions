using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewReloadPresetCommand : IPresetEditorPreviewCommand {
		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			controller.ResetPreviewObject();
			int selectIndex = context.SelectPresetIndex;
			context.ClearLayerStack();
			// 使用オブジェクトを非表示
			foreach (GameObject gameObject in data.GetAllReferencedGameObjects()) {
				context.GetShowObjectLayerStack(gameObject).AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, false);
				controller.SetActive(gameObject, false);
			}

			// プリセット適用
			MAExObjectPreset.Preset preset = data.presets[selectIndex];
			foreach (GameObject showObject in preset.showObjects.Where(showObject => showObject != null)) {
				context.GetShowObjectLayerStack(showObject).AddLayer(PresetPreviewContext.PRESET_LAYER, true);
				controller.SetActive(showObject, true);
			}

			foreach (MAExObjectPreset.BlendShape blendShape in preset.blendShapes.Where(shape => shape.skinnedMeshRenderer != null)) {
				SkinnedMeshRenderer skinnedMeshRenderer = blendShape.skinnedMeshRenderer;
				foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in blendShape.weights) {
					context.GetBlendShapeLayerStack(skinnedMeshRenderer, blendShapeWeight.key).AddLayer(PresetPreviewContext.PRESET_LAYER, blendShapeWeight.value);
					controller.SetBlendShapeWeight(skinnedMeshRenderer, blendShapeWeight.key, blendShapeWeight.value);
				}
			}
			
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
				MAExObjectPreset.ToggleSet toggle = preset.toggleSets[i];
				if (toggle.preview) {
					foreach (GameObject toggleShowObject in toggle.showObjects) {
						PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(toggleShowObject);
						layerStack.AddLayer(PresetPreviewContext.TOGGLE_BLOCK_LAYER, false);
						layerStack.AddLayer(i, true);
						controller.SetActive(toggleShowObject, layerStack.GetTopLayer().Value);
					}

					foreach (MAExObjectPreset.BlendShape blendShape in toggle.blendShapes.Where(shape => shape.skinnedMeshRenderer != null)) {
						SkinnedMeshRenderer skinnedMeshRenderer = blendShape.skinnedMeshRenderer;
						foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in blendShape.weights) {
							PresetPreviewContext.LayerStack<float> layerStack = context.GetBlendShapeLayerStack(skinnedMeshRenderer, blendShapeWeight.key);
							layerStack.AddLayer(i, blendShapeWeight.value);
							controller.SetBlendShapeWeight(skinnedMeshRenderer, blendShapeWeight.key, layerStack.GetTopLayer().Value);
						}
					}
					
					foreach (MAExObjectPreset.MaterialReplace materialReplace in toggle.materialReplaces.Where(replace => replace.renderer != null)) {
						Renderer renderer = materialReplace.renderer;
						for (int materialIndex = 0; materialIndex < materialReplace.materials.Count; materialIndex++) {
							if (materialReplace.materials[materialIndex] == null) continue;
							context.GetMaterialLayerStack(renderer, materialIndex).AddLayer(i, materialReplace.materials[materialIndex]);
							controller.SetMaterial(renderer, materialIndex, materialReplace.materials[materialIndex]);
						}
					}
				} else {
					// プリセット規定レイヤーで表示されているオブジェクトは非表示に
					foreach (GameObject toggleShowObject in toggle.showObjects) {
						PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(toggleShowObject);
						layerStack.AddLayer(PresetPreviewContext.TOGGLE_BLOCK_LAYER, false);
						controller.SetActive(toggleShowObject, layerStack.GetTopLayer().Value);
					}
				}
			}
			
		}
	}
}