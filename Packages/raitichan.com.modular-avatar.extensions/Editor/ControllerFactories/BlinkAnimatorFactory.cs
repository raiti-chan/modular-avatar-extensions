using nadena.dev.ndmf;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class BlinkAnimatorFactory : ControllerFactoryBase<MAExBlinkAnimatorGenerator> {
		
		public override void PreProcess(BuildContext context) { }

		public override RuntimeAnimatorController CreateController(BuildContext context) {
			AnimatorController controller = new AnimatorController();
			AssetDatabase.AddObjectToAsset(controller, context.AssetContainer);
			AnimationClip clip = this.CreateClip();
			AssetDatabase.AddObjectToAsset(clip, controller);
			MAExAnimatorFactoryUtils.CreateIdleLayerToAnimatorController(controller, "BlinkAnimation", clip, "Blink Idle");

			return controller;
		}

		public override void PostProcess(BuildContext context) { }
		
		private AnimationClip CreateClip() {
			AnimationClip source = this.Target.animationSource;
			AnimationClip clip = Object.Instantiate(source);
			clip.name = "BlinkAnimationClip";
			clip.ClearCurves();

			string blendShapeName = this.Target.faceMesh.sharedMesh.GetBlendShapeName(this.Target.blendShapeIndex);

			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source)) {
				EditorCurveBinding newBinding = binding;
				newBinding.path = MAExAnimatorFactoryUtils.GetBindingPath(this.Target.faceMesh.transform);
				newBinding.propertyName = $"blendShape.{blendShapeName}";
				AnimationCurve curve = AnimationUtility.GetEditorCurve(source, binding);
				clip.SetCurve(newBinding.path, newBinding.type, newBinding.propertyName, curve);
			}

			AnimationUtility.SetAnimationClipSettings(clip, AnimationUtility.GetAnimationClipSettings(source));
			return clip;
		}
	}
}