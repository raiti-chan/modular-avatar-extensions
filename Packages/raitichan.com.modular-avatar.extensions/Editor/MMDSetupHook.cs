using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.Editor {
	public class MMDSetupHook {
		public void OnProcessAvatar(GameObject avatarGameObject) {
			MAExMMDSetup mmdSetup = avatarGameObject.GetComponentInChildren<MAExMMDSetup>();
			if (mmdSetup == null) return;
			VRCAvatarDescriptor avatarDescriptor = avatarGameObject.GetComponent<VRCAvatarDescriptor>();
			if (avatarDescriptor == null) return;

			VRCAvatarDescriptor.CustomAnimLayer customAnimLayer = avatarDescriptor.baseAnimationLayers.FirstOrDefault(layer => layer.type == VRCAvatarDescriptor.AnimLayerType.FX);
			if (customAnimLayer.animatorController == null) return;

			AnimatorController targetController = customAnimLayer.animatorController as AnimatorController;
			if (targetController == null) return;

			foreach (AnimatorControllerLayer animatorControllerLayer in targetController.layers) {
				List<AnimatorState> states = GetAllState(animatorControllerLayer.stateMachine).ToList();
				/*
				bool blendShapeFlag = false;
				foreach (AnimatorState animatorState in states) {
					switch (animatorState.motion) {
						case AnimationClip clip: {
							if (!ValidateClip(clip)) {
								blendShapeFlag = true;
							}
							break;
						}
						case BlendTree blendTree: {
							foreach (AnimationClip clip in GetAllClip(blendTree)) {
								if (!ValidateClip(clip)) {
									blendShapeFlag = true;
								}
							}

							break;
						}
					}
				}

				if (blendShapeFlag) {
					animatorControllerLayer.defaultWeight = 0;
					return;
				}
				*/
				foreach (AnimatorState animatorState in states) {
					animatorState.writeDefaultValues = true;
				}
			}
		}

		private static IEnumerable<AnimatorState> GetAllState(AnimatorStateMachine stateMachine) {
			foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachine.stateMachines) {
				foreach (AnimatorState animatorState in GetAllState(childAnimatorStateMachine.stateMachine)) {
					yield return animatorState;
				}
			}

			foreach (ChildAnimatorState childAnimatorState in stateMachine.states) {
				yield return childAnimatorState.state;
			}
		}

		private static IEnumerable<AnimationClip> GetAllClip(BlendTree blendTree) {
			foreach (ChildMotion childMotion in blendTree.children) {
				switch (childMotion.motion) {
					case AnimationClip clip: {
						yield return clip;
						break;
					}
					case BlendTree childBlendTree: {
						foreach (AnimationClip clip in GetAllClip(childBlendTree)) {
							yield return clip;
						}

						break;
					}
				}
			}
		}

		private static readonly Regex BLEND_SHAPE_NAME_REGEX = new Regex(@"^blendShape\..+", RegexOptions.Compiled);

		private static bool ValidateClip(AnimationClip clip) {
			return !AnimationUtility.GetCurveBindings(clip).Any(editorCurveBinding => BLEND_SHAPE_NAME_REGEX.IsMatch(editorCurveBinding.propertyName));
		}
	}
}