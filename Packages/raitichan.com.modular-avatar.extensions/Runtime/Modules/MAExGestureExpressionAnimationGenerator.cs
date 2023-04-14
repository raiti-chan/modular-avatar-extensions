using System;
using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Enums;
using raitichan.com.modular_avatar.extensions.ScriptableObjects;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[AddComponentMenu("Modular Avatar/MAEx GestureExpression Animator Generator")]
	public class MAExGestureExpressionAnimationGenerator : MAExAnimatorGeneratorModuleBase<MAExGestureExpressionAnimationGenerator> {
		public string facePath;
		public GestureAnimationSet leftAnimationSet;
		public GestureAnimationSet rightAnimationSet;

		public SkinnedMeshRenderer GetFaceMesh() {
			return RuntimeUtil.FindAvatarInParents(this.transform)?.transform.Find(this.facePath).GetComponent<SkinnedMeshRenderer>();
		}

		public GestureAnimation GetGestureAnimation(LeftAndRight leftAndRight, Gesture gesture) {
			switch (leftAndRight) {
				case LeftAndRight.Left:
					return this.leftAnimationSet.GetGestureAnimation(gesture);
				case LeftAndRight.Right:
					return this.rightAnimationSet.GetGestureAnimation(gesture);
				default:
					throw new ArgumentOutOfRangeException(nameof(leftAndRight), leftAndRight, null);
			}
		}

		public MAExGestureExpressionAnimationPreset ExportGestureExpressionAnimationPreset() {
			MAExGestureExpressionAnimationPreset preset = ScriptableObject.CreateInstance<MAExGestureExpressionAnimationPreset>();
			preset.leftAnimationSet = this.leftAnimationSet.DeepClone();
			preset.rightAnimationSet = this.rightAnimationSet.DeepClone();
			return preset;
		}

		public MAExGestureExpressionAnimationPreset ExportGestureExpressionAnimationPreset(MAExGestureExpressionAnimationPreset exportTarget) {
			exportTarget.leftAnimationSet = this.leftAnimationSet.DeepClone();
			exportTarget.rightAnimationSet = this.rightAnimationSet.DeepClone();
			return exportTarget;
		}

		public void ImportGestureExpressionAnimationPreset(MAExGestureExpressionAnimationPreset target) {
			this.leftAnimationSet = target.leftAnimationSet.DeepClone();
			this.rightAnimationSet = target.rightAnimationSet.DeepClone();
		}

		[Serializable]
		public class GestureAnimationSet {
			public GestureAnimation fistAnimation;
			public GestureAnimation handOpenAnimation;
			public GestureAnimation fingerPointAnimation;
			public GestureAnimation victoryAnimation;
			public GestureAnimation rockNRollAnimation;
			public GestureAnimation handGunAnimation;
			public GestureAnimation thumbsUpAnimation;

			public GestureAnimation GetGestureAnimation(Gesture gesture) {
				switch (gesture) {
					case Gesture.Fist:
						return this.fistAnimation;
					case Gesture.HandOpen:
						return this.handOpenAnimation;
					case Gesture.FingerPoint:
						return this.fingerPointAnimation;
					case Gesture.Victory:
						return this.victoryAnimation;
					case Gesture.RockNRoll:
						return this.rockNRollAnimation;
					case Gesture.HandGun:
						return this.handGunAnimation;
					case Gesture.ThumbsUp:
						return this.thumbsUpAnimation;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			public GestureAnimationSet DeepClone() {
				return new GestureAnimationSet {
					fistAnimation = this.fistAnimation.DeepClone(),
					handOpenAnimation = this.handOpenAnimation.DeepClone(),
					fingerPointAnimation = this.fingerPointAnimation.DeepClone(),
					victoryAnimation = this.victoryAnimation.DeepClone(),
					rockNRollAnimation = this.rockNRollAnimation.DeepClone(),
					handGunAnimation = this.handGunAnimation.DeepClone(),
					thumbsUpAnimation = this.thumbsUpAnimation.DeepClone()
				};
			}
		}

		[Serializable]
		public class GestureAnimation {
			public bool isLoop;
			public BlendShapeWeight[] blendShapeWeights;
			public List<AdditionalAnimationClipData> additionalAnimationClipData;

			public void Reset() {
				BlendShapeWeight[] newBlendShapeWeights = new BlendShapeWeight[this.blendShapeWeights.Length];
				this.blendShapeWeights = newBlendShapeWeights;
			}

			public GestureAnimation DeepClone() {
				return new GestureAnimation {
					blendShapeWeights = (BlendShapeWeight[])this.blendShapeWeights.Clone(),
					additionalAnimationClipData = new List<AdditionalAnimationClipData>(this.additionalAnimationClipData)
				};
			}
		}

		[Serializable]
		public struct AdditionalAnimationClipData {
			public AnimationClip animationClip;
		}

		[Serializable]
		public struct BlendShapeWeight {
			public bool enable;
			public float weight;
		}
	}
}