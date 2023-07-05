using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectAddToggleLayerCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;
		private readonly int _layer;

		public PreviewObjectAddToggleLayerCommand(GameObject gameObject, int layer) {
			this._gameObject = gameObject;
			this._layer = layer;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);
			layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, false);
			layerStack.AddLayer(PresetPreviewContext.TOGGLE_BLOCK_LAYER, false);
			if (data.presets[context.SelectPresetIndex].toggleSets[this._layer].preview) {
				layerStack.AddLayer(this._layer, true);
			}
			controller.SetActive(this._gameObject, layerStack.GetTopLayer().Value);
		}
	}
}