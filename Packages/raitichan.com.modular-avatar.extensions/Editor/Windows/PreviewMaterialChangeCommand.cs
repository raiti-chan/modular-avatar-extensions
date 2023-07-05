using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewMaterialChangeCommand : IPresetEditorPreviewCommand {
		private readonly Renderer _renderer;
		private readonly int _materialIndex;
		private readonly int _layer;
		private readonly Material _material;

		public PreviewMaterialChangeCommand(Renderer renderer, int materialIndex, int layer, Material material) {
			this._renderer = renderer;
			this._materialIndex = materialIndex;
			this._layer = layer;
			this._material = material;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<Material> layerStack = context.GetMaterialLayerStack(this._renderer, this._materialIndex);
			if (this._layer != PresetPreviewContext.PRESET_LAYER && !data.presets[context.SelectPresetIndex].toggleSets[this._layer].preview) return;
			layerStack.AddLayer(this._layer, this._material);
			controller.SetMaterial(this._renderer, this._materialIndex, layerStack.GetTopLayer().Value);
		}
	}
}