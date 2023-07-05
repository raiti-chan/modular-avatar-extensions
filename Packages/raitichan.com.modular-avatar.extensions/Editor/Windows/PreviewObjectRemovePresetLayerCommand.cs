using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectRemovePresetLayerCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;

		public PreviewObjectRemovePresetLayerCommand(GameObject gameObject) {
			this._gameObject = gameObject;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);
			layerStack.RemoveLayer(PresetPreviewContext.PRESET_LAYER);

			PresetPreviewContext.LayerValue<bool> topLayer = layerStack.GetTopLayer();
			if (topLayer.Layer == PresetPreviewContext.USE_OBJECT_BLOCK_LAYER) {
				if (!data.GetAllReferencedGameObjects().Contains(this._gameObject)) {
					// オブジェクトが未使用になったらUSE_OBJECT_BLOCK_LAYERを削除
					layerStack.RemoveLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER);
					topLayer = layerStack.GetTopLayer();
				}
			}

			if (topLayer.Layer == PresetPreviewContext.NONE_LAYER) {
				controller.ResetActive(this._gameObject);
			} else {
				controller.SetActive(this._gameObject, topLayer.Value);
			}
		}
	}
}