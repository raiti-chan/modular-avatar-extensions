using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[AddComponentMenu("Modular Avatar/Raitichan/MAEx Ear Idle Animator Generator")]
	public class MaExEarIdleAnimatorGenerator : MaExAnimatorGeneratorModuleBase<MaExEarIdleAnimatorGenerator> {
		public Transform leftEar;
		public Transform rightEar;
		[Range(1.0f, 10.0f)]
		public float multiplier = 1.0f;
		public AnimationClip animationSource;

	}
}