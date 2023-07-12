using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectAddInPresetLayerCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;
		private readonly bool _enable;

		public PreviewObjectAddInPresetLayerCommand(GameObject gameObject, bool enable) {
			this._gameObject = gameObject;
			this._enable = enable;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);

			if (this._enable) {
				// 表示で追加された場合は全体で非表示オブジェクトにマークされる
				layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, false);
			} else {
				// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
				if (layerStack.GetTopLayer().Layer == PresetPreviewContext.NONE_LAYER) {
					// layerStackが空の場合は 新規で追加されたため 表示でマーク
					layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, true);
				} else {
					layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, data.IsDefaultShowObject(this._gameObject));
				}
			}

			if (data.presets[context.SelectPresetIndex].IsContainsObject(this._gameObject)) {
				layerStack.AddLayer(PresetPreviewContext.PRESET_LAYER, this._enable);
			}

			controller.SetActive(this._gameObject, layerStack.GetTopLayer().Value);
		}
	}
}