using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectChangeEnableInToggleLayerCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;
		private readonly int _layer;
		private readonly bool _enable;

		public PreviewObjectChangeEnableInToggleLayerCommand(GameObject gameObject, int layer, bool enable) {
			this._gameObject = gameObject;
			this._layer = layer;
			this._enable = enable;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);
			new PreviewObjectRemoveInToggleLayerCommand(this._gameObject, this._layer).Process(data, controller, context);
			new PreviewObjectAddInToggleLayerCommand(this._gameObject, this._layer, this._enable).Process(data, controller, context);
			controller.SetActive(this._gameObject, layerStack.GetTopLayer().Value);
		}
	}
}