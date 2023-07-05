using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewBlendShapeResetWeightCommand : IPresetEditorPreviewCommand {
		private readonly SkinnedMeshRenderer _skinnedMeshRenderer;
		private readonly string _blendShapeName;
		private readonly int _layer;

		public PreviewBlendShapeResetWeightCommand(SkinnedMeshRenderer skinnedMeshRenderer, string blendShapeName, int layer) {
			this._skinnedMeshRenderer = skinnedMeshRenderer;
			this._blendShapeName = blendShapeName;
			this._layer = layer;
		}


		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<float> layerStack = context.GetBlendShapeLayerStack(this._skinnedMeshRenderer, this._blendShapeName);
			layerStack.RemoveLayer(this._layer);
			PresetPreviewContext.LayerValue<float> topLayer = layerStack.GetTopLayer();
			if (topLayer.Layer == PresetPreviewContext.NONE_LAYER) {
				controller.ResetBlendShapeWeight(this._skinnedMeshRenderer, this._blendShapeName);
			} else {
				controller.SetBlendShapeWeight(this._skinnedMeshRenderer, this._blendShapeName, topLayer.Value);
			}
		}
	}
}