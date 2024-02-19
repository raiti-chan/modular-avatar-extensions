using nadena.dev.ndmf;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	// ReSharper disable once ClassNeverInstantiated.Global
	public class EarIdleAnimatorFactory : ControllerFactoryBase<MAExEarIdleAnimatorGenerator> {
		public override void PreProcess(BuildContext context) {
		}

		public override RuntimeAnimatorController CreateController(BuildContext context) {
			AnimatorController controller = new AnimatorController();
			AssetDatabase.AddObjectToAsset(controller, context.AssetContainer);
			AnimationClip clip = this.CreateClip();
			AssetDatabase.AddObjectToAsset(clip, controller);
			MAExAnimatorFactoryUtils.CreateIdleLayerToAnimatorController(controller, "EarAnimation", clip, "Ear Idle");
			return controller;
		}

		public override void PostProcess(BuildContext context) {
		}

		private enum Axis {
			X,
			Y,
			Z,
			W
		}

		private AnimationClip CreateClip() {
			AnimationClip source = this.Target.animationSource;
			AnimationClip clip = Object.Instantiate(source);
			clip.name = "EarAnimationClip";
			clip.ClearCurves();
			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source)) {
				EditorCurveBinding newBinding = binding;
				Axis axis = binding.propertyName == "m_LocalRotation.x" ? Axis.X :
					binding.propertyName == "m_LocalRotation.y" ? Axis.Y :
					binding.propertyName == "m_LocalRotation.z" ? Axis.Z : Axis.W;
				Transform targetTransform = binding.path == "L" ? this.Target.leftEar : this.Target.rightEar;
				newBinding.path = MAExAnimatorFactoryUtils.GetBindingPath(targetTransform);
				AnimationCurve curve = this.TransformCurve(AnimationUtility.GetEditorCurve(source, binding), targetTransform, axis);
				clip.SetCurve(newBinding.path, newBinding.type, newBinding.propertyName, curve);
			}

			AnimationUtility.SetAnimationClipSettings(clip, AnimationUtility.GetAnimationClipSettings(source));
			return clip;
		}

		private AnimationCurve TransformCurve(AnimationCurve source, Transform transform, Axis axis) {
			AnimationCurve newCurve = new AnimationCurve {
				preWrapMode = source.preWrapMode,
				postWrapMode = source.postWrapMode
			};
			float targetRotation = axis == Axis.X ? transform.localRotation.x :
				axis == Axis.Y ? transform.localRotation.y :
				axis == Axis.Z ? transform.localRotation.z : transform.localRotation.w;

			Keyframe firstKey = source.keys.MinBy(key => key.time);
			float offsetValue = firstKey.value - targetRotation;
			float baseValue = firstKey.value - offsetValue;

			foreach (Keyframe sourceKey in source.keys) {
				Keyframe newKey = sourceKey;
				newKey.value = (sourceKey.value - offsetValue - baseValue) * this.Target.multiplier + baseValue;
				newCurve.AddKey(newKey);
			}

			return newCurve;
		}

		public static AnimationClip CreateClip(AnimationClip source, Transform leftEar, Transform rightEar, float multiplier) {
			AnimationClip clip = Object.Instantiate(source);
			clip.name = "EarAnimationClip";
			clip.ClearCurves();
			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source)) {
				EditorCurveBinding newBinding = binding;
				Axis axis = binding.propertyName == "m_LocalRotation.x" ? Axis.X :
					binding.propertyName == "m_LocalRotation.y" ? Axis.Y :
					binding.propertyName == "m_LocalRotation.z" ? Axis.Z : Axis.W;
				Transform targetTransform = binding.path == "L" ? leftEar : rightEar;
				newBinding.path = MAExAnimatorFactoryUtils.GetBindingPath(targetTransform);
				AnimationCurve curve = TransformCurve(AnimationUtility.GetEditorCurve(source, binding), targetTransform, axis, multiplier);
				clip.SetCurve(newBinding.path, newBinding.type, newBinding.propertyName, curve);
			}

			AnimationUtility.SetAnimationClipSettings(clip, AnimationUtility.GetAnimationClipSettings(source));
			return clip;
		}


		private static AnimationCurve TransformCurve(AnimationCurve source, Transform transform, Axis axis, float multiplier) {
			AnimationCurve newCurve = new AnimationCurve {
				preWrapMode = source.preWrapMode,
				postWrapMode = source.postWrapMode
			};
			float targetRotation = axis == Axis.X ? transform.localRotation.x :
				axis == Axis.Y ? transform.localRotation.y :
				axis == Axis.Z ? transform.localRotation.z : transform.localRotation.w;

			Keyframe firstKey = source.keys.MinBy(key => key.time);
			float offsetValue = firstKey.value - targetRotation;
			float baseValue = firstKey.value - offsetValue;

			foreach (Keyframe sourceKey in source.keys) {
				Keyframe newKey = sourceKey;
				newKey.value = (sourceKey.value - offsetValue - baseValue) * multiplier + baseValue;
				newCurve.AddKey(newKey);
			}

			return newCurve;
		}
	}
}