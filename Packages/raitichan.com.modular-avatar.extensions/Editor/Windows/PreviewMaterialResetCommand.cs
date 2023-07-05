using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewMaterialResetCommand : IPresetEditorPreviewCommand {
		private readonly Renderer _renderer;
		private readonly int _materialIndex;
		private readonly int _layer;

		public PreviewMaterialResetCommand(Renderer renderer, int materialIndex, int layer) {
			this._renderer = renderer;
			this._materialIndex = materialIndex;
			this._layer = layer;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<Material> layerStack = context.GetMaterialLayerStack(this._renderer, this._materialIndex);
			layerStack.RemoveLayer(this._layer);
			PresetPreviewContext.LayerValue<Material> topLayer = layerStack.GetTopLayer();
			if (topLayer.Layer == PresetPreviewContext.NONE_LAYER) {
				controller.ResetMaterial(this._renderer, this._materialIndex);
			} else {
				controller.SetMaterial(this._renderer, this._materialIndex, topLayer.Value);
			}
		}
	}
}