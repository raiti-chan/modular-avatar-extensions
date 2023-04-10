using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class BlinkAnimatorFactory : IRuntimeAnimatorFactory<MAExBlinkAnimatorGenerator> {
		public MAExBlinkAnimatorGenerator Target { get; set; }

		public void PreProcess(GameObject avatarGameObject) { }

		public RuntimeAnimatorController CreateController(GameObject avatarGameObject) {
			AnimatorController controller = MAExUtils.CreateAnimator();
			AnimationClip clip = this.CreateClip();
			AssetDatabase.AddObjectToAsset(clip, controller);
			MAExAnimatorFactoryUtils.CreateIdleLayerToAnimatorController(controller, "BlinkAnimation", clip, "Blink Idle");

			return controller;
		}

		public void PostProcess(GameObject avatarGameObject) { }

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