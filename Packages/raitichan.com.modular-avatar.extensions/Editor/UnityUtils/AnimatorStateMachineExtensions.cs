using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.UnityUtils {
	public static class AnimatorStateMachineExtensions {
		public static AnimatorState AddStateEx(this AnimatorStateMachine target, string name, Motion motion, float x = 0, float y = 0) {
			AnimatorState animatorState = target.AddState(name, new Vector3(x, y, 0));
			animatorState.motion = motion;
			animatorState.writeDefaultValues = false;
			return animatorState;
		}
	}

	public static class AnimatorStateExtensions {
		public static AnimatorStateTransition AddTransitionEx(this AnimatorState target, AnimatorState dst) {
			AnimatorStateTransition transition = target.AddTransition(dst);
			transition.hasExitTime = false;
			transition.exitTime = 0;
			transition.hasFixedDuration = true;
			transition.duration = 0;
			transition.offset = 0;
			transition.interruptionSource = TransitionInterruptionSource.None;
			return transition;
		}

		public static AnimatorStateTransition InsertTransitionEx(this AnimatorState target, AnimatorState dst, int index) {
			AnimatorStateTransition newTransition = new AnimatorStateTransition() {
				hasExitTime = false,
				exitTime = 0,
				hasFixedDuration = true,
				duration = 0,
				offset = 0,
				interruptionSource = TransitionInterruptionSource.None,
				destinationState = dst,
				hideFlags = HideFlags.HideInHierarchy,
			};
			AssetDatabase.AddObjectToAsset(newTransition, AssetDatabase.GetAssetPath(target));
			AnimatorStateTransition[] transitions = target.transitions;
			ArrayUtility.Insert(ref transitions, index, newTransition);
			target.transitions = transitions;
			return newTransition;
		}
	}

	public static class AnimatorStateTransitionExtensions {
		public static AnimatorStateTransition AddConditionEx(this AnimatorStateTransition target, AnimatorConditionMode mode, float threshold, string parameter) {
			target.AddCondition(mode, threshold, parameter);
			return target;
		}
	}
}