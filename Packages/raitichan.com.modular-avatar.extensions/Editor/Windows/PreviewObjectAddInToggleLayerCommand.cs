using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectAddInToggleLayerCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;
		private readonly int _layer;
		private readonly bool _enable;

		public PreviewObjectAddInToggleLayerCommand(GameObject gameObject, int layer, bool enable) {
			this._gameObject = gameObject;
			this._layer = layer;
			this._enable = enable;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);

			if (this._enable) {
				if (layerStack.GetTopLayer().Layer == PresetPreviewContext.NONE_LAYER) {
					// layerStackが空の場合は新規で追加されたため、非表示でマーク
					layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, false);
				} else {
					if (data.presets[context.SelectPresetIndex].enableObjects
					    .Where(enableObject => enableObject.gameObject == this._gameObject)
					    .All(enableObject => enableObject.enable)) {
						layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, false);
					}
				}
			} else {
				if (layerStack.GetTopLayer().Layer == PresetPreviewContext.NONE_LAYER) {
					// layerStackが空の場合は新規で追加されたため、表示でマーク
					layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, true);
				}
			}
			
			if (data.presets[context.SelectPresetIndex].IsContainsObject(this._gameObject)) {
				layerStack.AddLayer(PresetPreviewContext.PRESET_LAYER, !this._enable);
			} else {
				layerStack.RemoveLayer(PresetPreviewContext.PRESET_LAYER);
			}
			
			if (data.presets[context.SelectPresetIndex].toggleSets[this._layer].preview) {
				layerStack.AddLayer(this._layer, this._enable);
			}
			
			controller.SetActive(this._gameObject, layerStack.GetTopLayer().Value);
		}
	}
}