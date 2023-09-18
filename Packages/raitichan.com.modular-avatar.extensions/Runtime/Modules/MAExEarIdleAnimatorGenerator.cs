using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[AddComponentMenu("Modular Avatar/MAEx Ear Idle Animator Generator")]
	public class MAExEarIdleAnimatorGenerator : MAExAnimatorGeneratorModuleBase<MAExEarIdleAnimatorGenerator> {
		public Transform leftEar;
		public Transform rightEar;
		[Range(1.0f, 10.0f)]
		public float multiplier = 1.0f;
		public AnimationClip animationSource;

	}
}