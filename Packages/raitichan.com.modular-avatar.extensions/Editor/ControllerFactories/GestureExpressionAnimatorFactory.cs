using System.Collections.Generic;
using nadena.dev.ndmf;
using raitichan.com.modular_avatar.extensions.Editor.ModuleExtensions;
using raitichan.com.modular_avatar.extensions.Enums;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class GestureExpressionAnimatorFactory : ControllerFactoryBase<MAExGestureExpressionAnimationGenerator> {
		public override void PreProcess(BuildContext context) {
		}

		public override RuntimeAnimatorController CreateController(BuildContext context) { 
			AnimatorController controller = new AnimatorController();
			AssetDatabase.AddObjectToAsset(controller, context.AssetContainer);

			GestureAnimationLayerData leftAnimationLayerData = new GestureAnimationLayerData {
				leftAndRight = LeftAndRight.Left,
				fistAnimationClip = this.CreateAnimationClip(LeftAndRight.Left, Gesture.Fist, controller),
				handOpenAnimationClip = this.CreateAnimationClip(LeftAndRight.Left, Gesture.HandOpen, controller),
				fingerPointAnimationClip = this.CreateAnimationClip(LeftAndRight.Left, Gesture.FingerPoint, controller),
				victoryAnimationClip = this.CreateAnimationClip(LeftAndRight.Left, Gesture.Victory, controller),
				rockNRollAnimationClip = this.CreateAnimationClip(LeftAndRight.Left, Gesture.RockNRoll, controller),
				handGunAnimationClip = this.CreateAnimationClip(LeftAndRight.Left, Gesture.HandGun, controller),
				thumbsUpAnimationClip = this.CreateAnimationClip(LeftAndRight.Left, Gesture.ThumbsUp, controller)
			};
			GestureAnimationLayerData rightAnimationLayerData = new GestureAnimationLayerData {
				leftAndRight = LeftAndRight.Right,
				fistAnimationClip = this.CreateAnimationClip(LeftAndRight.Right, Gesture.Fist, controller),
				handOpenAnimationClip = this.CreateAnimationClip(LeftAndRight.Right, Gesture.HandOpen, controller),
				fingerPointAnimationClip = this.CreateAnimationClip(LeftAndRight.Right, Gesture.FingerPoint, controller),
				victoryAnimationClip = this.CreateAnimationClip(LeftAndRight.Right, Gesture.Victory, controller),
				rockNRollAnimationClip = this.CreateAnimationClip(LeftAndRight.Right, Gesture.RockNRoll, controller),
				handGunAnimationClip = this.CreateAnimationClip(LeftAndRight.Right, Gesture.HandGun, controller),
				thumbsUpAnimationClip = this.CreateAnimationClip(LeftAndRight.Right, Gesture.ThumbsUp, controller)
			};

			CreateGestureLayerToAnimatorController(controller, leftAnimationLayerData);
			CreateGestureLayerToAnimatorController(controller, rightAnimationLayerData);

			return controller;
		}

		public override void PostProcess(BuildContext context) {
		}

		private AnimationClip CreateAnimationClip(LeftAndRight leftAndRight, Gesture gesture, Object saveTo) {
			AnimationClip animationClip = this.Target.ExportAnimationClip(leftAndRight, gesture);
			animationClip.name = $"{leftAndRight}_{gesture}";
			AssetDatabase.AddObjectToAsset(animationClip, saveTo);
			return animationClip;
		}


		private static void CreateGestureLayerToAnimatorController(AnimatorController controller, GestureAnimationLayerData layerData) {
			AnimatorStateMachine stateMachine = new AnimatorStateMachine {
				name = $"{layerData.leftAndRight} Hand",
				entryPosition = Vector3.zero,
				exitPosition = new Vector3(0, -50, 0),
				anyStatePosition = new Vector3(0, 50, 0)
			};
			AssetDatabase.AddObjectToAsset(stateMachine, controller);
			string parameterName = layerData.leftAndRight == LeftAndRight.Left ? "GestureLeft" : "GestureRight";

			AnimatorState idleState = stateMachine.AddState("idle", new Vector3(300, 0, 0));
			stateMachine.defaultState = idleState;
			idleState.motion = MAExAnimatorFactoryUtils.GetEmptyAnimationClip();
			idleState.writeDefaultValues = false;

			AnimatorState fistState = stateMachine.AddState("Fist", new Vector3(300, 50, 0));
			fistState.motion = layerData.fistAnimationClip;
			fistState.writeDefaultValues = false;

			AnimatorState handOpenState = stateMachine.AddState("Hand Open", new Vector3(300, 100, 0));
			handOpenState.motion = layerData.handOpenAnimationClip;
			handOpenState.writeDefaultValues = false;

			AnimatorState fingerPointState = stateMachine.AddState("Finger Point", new Vector3(300, 150, 0));
			fingerPointState.motion = layerData.fingerPointAnimationClip;
			fingerPointState.writeDefaultValues = false;

			AnimatorState victoryState = stateMachine.AddState("Victory", new Vector3(300, 200, 0));
			victoryState.motion = layerData.victoryAnimationClip;
			victoryState.writeDefaultValues = false;

			AnimatorState rockNRollState = stateMachine.AddState("RockNRoll", new Vector3(300, 250, 0));
			rockNRollState.motion = layerData.rockNRollAnimationClip;
			rockNRollState.writeDefaultValues = false;

			AnimatorState handGunState = stateMachine.AddState("HandGun", new Vector3(300, 300, 0));
			handGunState.motion = layerData.handGunAnimationClip;
			handGunState.writeDefaultValues = false;

			AnimatorState thumbsUpState = stateMachine.AddState("ThumbsUp", new Vector3(300, 350, 0));
			thumbsUpState.motion = layerData.thumbsUpAnimationClip;
			thumbsUpState.writeDefaultValues = false;


			AnimatorState idleEndState = stateMachine.AddState("idle End", new Vector3(600, 0, 0));
			idleEndState.motion = MAExAnimatorFactoryUtils.GetEmptyAnimationClip();
			idleEndState.writeDefaultValues = false;

			AnimatorState fistEndState = stateMachine.AddState("Fist End", new Vector3(600, 50, 0));
			fistEndState.motion = layerData.fistAnimationClip;
			fistEndState.writeDefaultValues = false;

			AnimatorState handOpenEndState = stateMachine.AddState("Hand Open End", new Vector3(600, 100, 0));
			handOpenEndState.motion = layerData.handOpenAnimationClip;
			handOpenEndState.writeDefaultValues = false;

			AnimatorState fingerPointEndState = stateMachine.AddState("Finger Point End", new Vector3(600, 150, 0));
			fingerPointEndState.motion = layerData.fingerPointAnimationClip;
			fingerPointEndState.writeDefaultValues = false;

			AnimatorState victoryEndState = stateMachine.AddState("Victory End", new Vector3(600, 200, 0));
			victoryEndState.motion = layerData.victoryAnimationClip;
			victoryEndState.writeDefaultValues = false;

			AnimatorState rockNRollEndState = stateMachine.AddState("RockNRoll End", new Vector3(600, 250, 0));
			rockNRollEndState.motion = layerData.rockNRollAnimationClip;
			rockNRollEndState.writeDefaultValues = false;

			AnimatorState handGunEndState = stateMachine.AddState("HandGun End", new Vector3(600, 300, 0));
			handGunEndState.motion = layerData.handGunAnimationClip;
			handGunEndState.writeDefaultValues = false;

			AnimatorState thumbsUpEndState = stateMachine.AddState("ThumbsUp End", new Vector3(600, 350, 0));
			thumbsUpEndState.motion = layerData.thumbsUpAnimationClip;
			thumbsUpEndState.writeDefaultValues = false;

			AddTransitionNeq(idleState, idleEndState, parameterName, 0);
			AddTransitionNeq(fistState, fistEndState, parameterName, 1);
			AddTransitionNeq(handOpenState, handOpenEndState, parameterName, 2);
			AddTransitionNeq(fingerPointState, fingerPointEndState, parameterName, 3);
			AddTransitionNeq(victoryState, victoryEndState, parameterName, 4);
			AddTransitionNeq(rockNRollState, rockNRollEndState, parameterName, 5);
			AddTransitionNeq(handGunState, handGunEndState, parameterName, 6);
			AddTransitionNeq(thumbsUpState, thumbsUpEndState, parameterName, 7);

			(AnimatorState, int)[] tos = {
				(idleState, 0),
				(fistState, 1),
				(handOpenState, 2),
				(fingerPointState, 3),
				(victoryState, 4),
				(rockNRollState, 5),
				(handGunState, 6),
				(thumbsUpState, 7)
			};

			AddTransitionsEq(idleEndState, parameterName, tos);
			AddTransitionsEq(fistEndState, parameterName, tos);
			AddTransitionsEq(handOpenEndState, parameterName, tos);
			AddTransitionsEq(fingerPointEndState, parameterName, tos);
			AddTransitionsEq(victoryEndState, parameterName, tos);
			AddTransitionsEq(rockNRollEndState, parameterName, tos);
			AddTransitionsEq(handGunEndState, parameterName, tos);
			AddTransitionsEq(thumbsUpEndState, parameterName, tos);

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
			controller.AddParameter(parameterName, AnimatorControllerParameterType.Int);
		}

		private static void AddTransitionNeq(AnimatorState from, AnimatorState to, string parameterName, int parameter) {
			AnimatorStateTransition transition = from.AddTransition(to);
			transition.hasExitTime = false;
			transition.exitTime = 0;
			transition.hasFixedDuration = true;
			transition.duration = 0;
			transition.offset = 0;
			transition.interruptionSource = TransitionInterruptionSource.None;
			transition.AddCondition(AnimatorConditionMode.NotEqual, parameter, parameterName);
		}

		private static void AddTransitionsEq(AnimatorState from, string parameterName, IEnumerable<(AnimatorState state, int parameter)> tos) {
			foreach ((AnimatorState state, int parameter) to in tos) {
				AnimatorStateTransition transition = from.AddTransition(to.state);
				transition.hasExitTime = false;
				transition.exitTime = 0;
				transition.hasFixedDuration = true;
				transition.duration = 0.1f;
				transition.offset = 0;
				transition.interruptionSource = TransitionInterruptionSource.None;
				transition.AddCondition(AnimatorConditionMode.Equals, to.parameter, parameterName);
			}
		}


		private class GestureAnimationLayerData {
			public LeftAndRight leftAndRight;
			public AnimationClip fistAnimationClip;
			public AnimationClip handOpenAnimationClip;
			public AnimationClip fingerPointAnimationClip;
			public AnimationClip victoryAnimationClip;
			public AnimationClip rockNRollAnimationClip;
			public AnimationClip handGunAnimationClip;
			public AnimationClip thumbsUpAnimationClip;
		}
	}
}