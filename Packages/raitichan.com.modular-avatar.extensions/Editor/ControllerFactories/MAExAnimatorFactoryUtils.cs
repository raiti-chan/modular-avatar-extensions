using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	public static class MAExAnimatorFactoryUtils {
		public static AnimatorControllerLayer CreateToggleLayerToAnimatorController(AnimatorController controller, string parameterName,
			AnimationClip offClip, AnimationClip onClip, bool isInvert = false) {
			AnimatorStateMachine stateMachine = new AnimatorStateMachine() {
				name = parameterName + "_Toggle"
			};
			AnimatorState offState = stateMachine.AddState(parameterName + "_OFF");
			offState.motion = offClip;
			offState.writeDefaultValues = false;
			AssetDatabase.AddObjectToAsset(offState, controller);

			AnimatorState onState = stateMachine.AddState(parameterName + "_ON");
			onState.motion = onClip;
			onState.writeDefaultValues = false;
			AssetDatabase.AddObjectToAsset(onState, controller);

			stateMachine.defaultState = isInvert == false ? offState : onState;

			AnimatorStateTransition offToOn = offState.AddTransition(onState);
			offToOn.hasExitTime = false;
			offToOn.exitTime = 0;
			offToOn.hasFixedDuration = true;
			offToOn.duration = 0;
			offToOn.offset = 0;
			offToOn.interruptionSource = TransitionInterruptionSource.None;
			offToOn.AddCondition(isInvert == false ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, parameterName);
			AnimatorStateTransition onToOff = onState.AddTransition(offState);
			onToOff.hasExitTime = false;
			onToOff.exitTime = 0;
			onToOff.hasFixedDuration = true;
			onToOff.duration = 0;
			onToOff.offset = 0;
			onToOff.interruptionSource = TransitionInterruptionSource.None;
			onToOff.AddCondition(isInvert == false ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, 0, parameterName);

			AnimatorControllerLayer layer = new AnimatorControllerLayer {
				name = stateMachine.name,
				avatarMask = null,
				blendingMode = AnimatorLayerBlendingMode.Override,
				defaultWeight = 1.0f,
				syncedLayerIndex = -1,
				syncedLayerAffectsTiming = false,
				iKPass = false,
				stateMachine = stateMachine
			};
			controller.AddLayer(layer);
			controller.AddParameter(parameterName, AnimatorControllerParameterType.Bool);
			return layer;
		}

		public static AnimatorControllerLayer CreateIdleLayerToAnimatorController(AnimatorController controller, string layerName,
			AnimationClip clip, string stateName = "Idle") {
			AnimatorStateMachine stateMachine = new AnimatorStateMachine {
				name = layerName
			};
			AnimatorState state = stateMachine.AddState(stateName);
			stateMachine.defaultState = state;
			state.motion = clip;
			state.writeDefaultValues = false;
			AssetDatabase.AddObjectToAsset(state, controller);
			AssetDatabase.AddObjectToAsset(stateMachine, controller);

			AnimatorControllerLayer layer = new AnimatorControllerLayer {
				name = stateMachine.name,
				avatarMask = null,
				blendingMode = AnimatorLayerBlendingMode.Override,
				defaultWeight = 1.0f,
				syncedLayerIndex = -1,
				syncedLayerAffectsTiming = false,
				iKPass = false,
				stateMachine = stateMachine
			};
			controller.AddLayer(layer);
			return layer;
		}


		// TODO: 将来的にアニメーション生成支援ライブラリの方へ移動
		public static string GetBindingPath(Transform ear) {
			Stack<Transform> pathStack = new Stack<Transform>();
			Transform current = ear;
			while (current.GetComponent<VRCAvatarDescriptor>() == null) {
				pathStack.Push(current);
				current = current.parent;
				if (current == null) {
					return "None";
				}
			}

			StringBuilder pathBuilder = new StringBuilder();
			foreach (Transform transform in pathStack) {
				pathBuilder.Append(transform.name);
				pathBuilder.Append("/");
			}


			pathBuilder.Remove(pathBuilder.Length - 1, 1);
			return pathBuilder.ToString();
		}
	}
}