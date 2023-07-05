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
	}
}