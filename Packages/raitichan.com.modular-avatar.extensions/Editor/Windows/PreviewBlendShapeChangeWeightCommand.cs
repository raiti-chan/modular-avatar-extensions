using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewBlendShapeChangeWeightCommand : IPresetEditorPreviewCommand {
		private readonly SkinnedMeshRenderer _skinnedMeshRenderer;
		private readonly string _blendShapeName;
		private readonly int _layer;
		private readonly float _weight;

		public PreviewBlendShapeChangeWeightCommand(SkinnedMeshRenderer skinnedMeshRenderer, string blendShapeName, int layer, float weight) {
			this._skinnedMeshRenderer = skinnedMeshRenderer;
			this._blendShapeName = blendShapeName;
			this._layer = layer;
			this._weight = weight;
		}

		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			PresetPreviewContext.LayerStack<float> layerStack = context.GetBlendShapeLayerStack(this._skinnedMeshRenderer, this._blendShapeName);
			if (this._layer != PresetPreviewContext.PRESET_LAYER && !data.presets[context.SelectPresetIndex].toggleSets[this._layer].preview) return;
			layerStack.AddLayer(this._layer, this._weight);
			controller.SetBlendShapeWeight(this._skinnedMeshRenderer, this._blendShapeName, layerStack.GetTopLayer().Value);
		}
	}
}