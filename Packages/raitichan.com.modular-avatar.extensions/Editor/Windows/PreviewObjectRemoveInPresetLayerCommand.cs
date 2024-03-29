﻿using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewObjectRemoveInPresetLayerCommand : IPresetEditorPreviewCommand {
		private readonly GameObject _gameObject;

		public PreviewObjectRemoveInPresetLayerCommand(GameObject gameObject) {
			this._gameObject = gameObject;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<bool> layerStack = context.GetShowObjectLayerStack(this._gameObject);
			layerStack.RemoveLayer(PresetPreviewContext.PRESET_LAYER);
			
			if (data.IsContainsObject(this._gameObject)) {
				layerStack.AddLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER, data.IsDefaultShowObject(this._gameObject));
			} else {
				layerStack.RemoveLayer(PresetPreviewContext.USE_OBJECT_BLOCK_LAYER);
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