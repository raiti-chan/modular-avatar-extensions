using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectRemoveInToggleLayerCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;
		private readonly int _layer;

		public PreviewObjectRemoveInToggleLayerCommand(GameObject gameObject, int layer) {
			this._gameObject = gameObject;
			this._layer = layer;
		}


		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);
			layerStack.RemoveLayer(this._layer);

			if (data.IsContainsObject(this._gameObject)) {
				layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, data.IsDefaultShowObject(this._gameObject));
			} else {
				layerStack.RemoveLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER);
			}

			if (!layerStack.ContainsLayer(PresetPreviewContext.PRESET_LAYER)) {
				if (data.presets[context.SelectPresetIndex].IsContainsObject(this._gameObject)) {
					layerStack.AddLayer(PresetPreviewContext.PRESET_LAYER, data.presets[context.SelectPresetIndex][this._gameObject].enable);
				}
			}

			PresetPreviewContext.LayerValue<bool> topLayer = layerStack.GetTopLayer();
			if (topLayer.Layer == PresetPreviewContext.NONE_LAYER) {
				controller.ResetActive(this._gameObject);
			} else {
				controller.SetActive(this._gameObject, topLayer.Value);
			}
		}
	}
}