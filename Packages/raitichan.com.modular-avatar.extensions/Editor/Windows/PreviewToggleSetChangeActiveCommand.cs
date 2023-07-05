using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewToggleSetChangeActiveCommand : IPresetEditorPreviewCommand {
		private readonly int _toggleSetIndex;
		private readonly bool _enable;

		public PreviewToggleSetChangeActiveCommand(int toggleSetIndex, bool enable) {
			this._toggleSetIndex = toggleSetIndex;
			this._enable = enable;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			MAExObjectPreset.Preset preset = data.presets[context.SelectPresetIndex];
			if (this._toggleSetIndex < 0 || preset.toggleSets.Count <= this._toggleSetIndex) return;

			MAExObjectPreset.ToggleSet toggleSet = preset.toggleSets[this._toggleSetIndex];

			foreach (GameObject toggleSetToggleObject in toggleSet.showObjects) {
				new PreviewObjectChangeActiveCommand(toggleSetToggleObject, this._toggleSetIndex, this._enable).Process(data, controller, context);
			}

			if (this._enable) {
				foreach (MAExObjectPreset.BlendShape toggleSetBlendShape in toggleSet.blendShapes) {
					SkinnedMeshRenderer skinnedMeshRenderer = toggleSetBlendShape.skinnedMeshRenderer;
					foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in toggleSetBlendShape.weights) {
						new PreviewBlendShapeChangeWeightCommand(skinnedMeshRenderer, blendShapeWeight.key, this._toggleSetIndex, blendShapeWeight.value).Process(data, controller, context);
					}
				}
				
				foreach (MAExObjectPreset.MaterialReplace toggleSetMaterialReplace in toggleSet.materialReplaces) {
					Renderer renderer = toggleSetMaterialReplace.renderer;
					for (int materialIndex = 0; materialIndex < toggleSetMaterialReplace.materials.Count; materialIndex++) {
						if (toggleSetMaterialReplace.materials[materialIndex] == null) continue;
						new PreviewMaterialChangeCommand(renderer, materialIndex, this._toggleSetIndex, toggleSetMaterialReplace.materials[materialIndex]).Process(data, controller, context);
					}
				}
			} else {
				foreach (MAExObjectPreset.BlendShape toggleSetBlendShape in toggleSet.blendShapes) {
					SkinnedMeshRenderer skinnedMeshRenderer = toggleSetBlendShape.skinnedMeshRenderer;
					foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in toggleSetBlendShape.weights) {
						new PreviewBlendShapeResetWeightCommand(skinnedMeshRenderer, blendShapeWeight.key, this._toggleSetIndex).Process(data, controller, context);
					}
				}
				
				foreach (MAExObjectPreset.MaterialReplace toggleSetMaterialReplace in toggleSet.materialReplaces) {
					Renderer renderer = toggleSetMaterialReplace.renderer;
					for (int materialIndex = 0; materialIndex < toggleSetMaterialReplace.materials.Count; materialIndex++) {
						if (toggleSetMaterialReplace.materials[materialIndex] == null) continue;
						new PreviewMaterialResetCommand(renderer, materialIndex, this._toggleSetIndex).Process(data, controller, context);
					}
				}
			}
		}
	}
}