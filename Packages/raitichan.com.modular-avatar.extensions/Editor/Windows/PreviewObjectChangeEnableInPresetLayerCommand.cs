using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectChangeEnableInPresetLayerCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;
		private readonly bool _enable;

		public PreviewObjectChangeEnableInPresetLayerCommand(GameObject gameObject, bool enable) {
			this._gameObject = gameObject;
			this._enable = enable;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);
			new PreviewObjectRemoveInPresetLayerCommand(this._gameObject).Process(data, controller, context);
			new PreviewObjectAddInPresetLayerCommand(this._gameObject, this._enable).Process(data, controller, context);
			controller.SetActive(this._gameObject, layerStack.GetTopLayer().Value);
		}
	}
}