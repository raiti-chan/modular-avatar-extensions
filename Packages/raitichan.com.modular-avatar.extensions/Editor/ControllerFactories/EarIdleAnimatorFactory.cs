﻿using raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class EarIdleAnimatorFactory : IRuntimeAnimatorFactory<MAExEarIdleAnimatorGenerator> {
		public MAExEarIdleAnimatorGenerator Target { get; set; }

		public void PreProcess(GameObject avatarGameObject) { }

		public RuntimeAnimatorController CreateController(GameObject avatarGameObject) {
			AnimatorController controller = UtilHelper.CreateAnimator();
			AnimationClip clip = this.CreateClip();
			MAExAnimatorFactoryUtils.CreateIdleLayerToAnimatorController(controller, "EarAnimation", clip, "Ear Idle");
			return controller;
		}

		public void PostProcess(GameObject avatarGameObject) { }

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
	}
}