using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.Editor.Utils {
	public static class MAAnimatorUtils {
		public static AnimatorControllerLayer CreateIdleLayerToAnimatorController(AnimatorController controller, string layerName, AnimationClip clip,
			string stateName = "Idle") {
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
				name = "EarAnimation",
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