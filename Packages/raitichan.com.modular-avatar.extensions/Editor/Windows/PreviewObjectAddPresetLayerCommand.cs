using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectAddPresetLayerCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;

		public PreviewObjectAddPresetLayerCommand(GameObject gameObject) {
			this._gameObject = gameObject;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);
			layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, false);
			layerStack.AddLayer(PresetPreviewContext.PRESET_LAYER, true);
			controller.SetActive(this._gameObject, layerStack.GetTopLayer().Value);
		}
	}
}