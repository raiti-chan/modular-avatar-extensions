using System.Linq;
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
			if (this._enable) {
				foreach (MAExObjectPreset.EnableObject enableObject in toggleSet.enableObjects.Where(obj => obj.gameObject != null)) {
					PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(enableObject.gameObject);
					layerStack.AddLayer(this._toggleSetIndex, enableObject.enable);
					controller.SetActive(enableObject.gameObject, layerStack.GetTopLayer().Value);
				}

				foreach (MAExObjectPreset.BlendShape blendShape in toggleSet.blendShapes.Where(shape => shape.skinnedMeshRenderer != null)) {
					SkinnedMeshRenderer skinnedMeshRenderer = blendShape.skinnedMeshRenderer;
					foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in blendShape.weights) {
						PresetPreviewContext.LayerStack<float> layerStack = context.GetBlendShapeLayerStack(skinnedMeshRenderer, blendShapeWeight.key);
						layerStack.AddLayer(this._toggleSetIndex, blendShapeWeight.value);
						controller.SetBlendShapeWeight(skinnedMeshRenderer, blendShapeWeight.key, layerStack.GetTopLayer().Value);
					}
				}
				
				foreach (MAExObjectPreset.MaterialReplace materialReplace in toggleSet.materialReplaces.Where(replace => replace.renderer != null)) {
					Renderer renderer = materialReplace.renderer;
					for (int materialIndex = 0; materialIndex < materialReplace.materials.Count; materialIndex++) {
						if (materialReplace.materials[materialIndex] == null) continue;
						PresetPreviewContext.LayerStack<Material> layerStack = context.GetMaterialLayerStack(renderer, materialIndex);
						layerStack.AddLayer(this._toggleSetIndex, materialReplace.materials[materialIndex]);
						controller.SetMaterial(renderer, materialIndex, materialReplace.materials[materialIndex]);
					}
				}
			} else {
				foreach (MAExObjectPreset.EnableObject enableObject in toggleSet.enableObjects.Where(obj => obj.gameObject != null)) {
					PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(enableObject.gameObject);
					layerStack.RemoveLayer(this._toggleSetIndex);
					controller.SetActive(enableObject.gameObject, layerStack.GetTopLayer().Value);
				}
				
				foreach (MAExObjectPreset.BlendShape blendShape in toggleSet.blendShapes.Where(shape => shape.skinnedMeshRenderer != null)) {
					SkinnedMeshRenderer skinnedMeshRenderer = blendShape.skinnedMeshRenderer;
					foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in blendShape.weights) {
						PresetPreviewContext.LayerStack<float> layerStack = context.GetBlendShapeLayerStack(skinnedMeshRenderer, blendShapeWeight.key);
						layerStack.RemoveLayer(this._toggleSetIndex);
						controller.SetBlendShapeWeight(skinnedMeshRenderer, blendShapeWeight.key, layerStack.GetTopLayer().Value);
					}
				}
				
				foreach (MAExObjectPreset.MaterialReplace materialReplace in toggleSet.materialReplaces.Where(replace => replace.renderer != null)) {
					Renderer renderer = materialReplace.renderer;
					for (int materialIndex = 0; materialIndex < materialReplace.materials.Count; materialIndex++) {
						if (materialReplace.materials[materialIndex] == null) continue;
						PresetPreviewContext.LayerStack<Material> layerStack = context.GetMaterialLayerStack(renderer, materialIndex);
						layerStack.RemoveLayer(this._toggleSetIndex);
						controller.SetMaterial(renderer, materialIndex, materialReplace.materials[materialIndex]);
					}
				}
			}
		}
	}
}