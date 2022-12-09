using raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers;
using raitichan.com.modular_avatar.extensions.Editor.Utils;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class BlinkAnimationFactory : IRuntimeAnimatorFactory<MAExBlinkAnimatorGenerator> {
		public MAExBlinkAnimatorGenerator Target { get; set; }

		public RuntimeAnimatorController CreateController(GameObject avatarGameObject) {
			AnimatorController controller = UtilHelper.CreateAnimator();
			AnimationClip clip = this.CreateClip();
			MAAnimatorUtils.CreateIdleLayerToAnimatorController(controller, "BlinkAnimation", clip, "Blink Idle");
			
			return controller;
		}

		private AnimationClip CreateClip() {
			AnimationClip source = this.Target.animationSource;
			AnimationClip clip = Object.Instantiate(source);
			clip.name = "BlinkAnimationClip";
			clip.ClearCurves();

			string blendShapeName = this.Target.faceMesh.sharedMesh.GetBlendShapeName(this.Target.blendShapeIndex);
			
			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source)) {
				EditorCurveBinding newBinding = binding;
				newBinding.path = MAAnimatorUtils.GetBindingPath(this.Target.faceMesh.transform);
				newBinding.propertyName = $"blendShape.{blendShapeName}";
				AnimationCurve curve = AnimationUtility.GetEditorCurve(source, binding);
				clip.SetCurve(newBinding.path, newBinding.type, newBinding.propertyName, curve);
			}
			
			AnimationUtility.SetAnimationClipSettings(clip, AnimationUtility.GetAnimationClipSettings(source));
			return clip;
		}
	}
}