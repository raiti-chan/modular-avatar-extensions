using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectChangeActiveCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;
		private readonly int _layer;
		private readonly bool _active;

		public PreviewObjectChangeActiveCommand(GameObject gameObject, int layer, bool active) {
			this._gameObject = gameObject;
			this._layer = layer;
			this._active = active;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);
			if (this._active) {
				layerStack.AddLayer(this._layer, true);
				controller.SetActive(this._gameObject, layerStack.GetTopLayer().Value);
			} else {
				layerStack.RemoveLayer(this._layer);
				PresetPreviewContext.LayerValue<bool> topLayer = layerStack.GetTopLayer();
				if (topLayer.Layer == PresetPreviewContext.NONE_LAYER) {
					controller.ResetActive(this._gameObject);
				} else {
					controller.SetActive(this._gameObject, topLayer.Value);
				}
			}
		}
	}
}