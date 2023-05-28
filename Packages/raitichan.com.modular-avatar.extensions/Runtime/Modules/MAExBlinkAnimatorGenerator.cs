using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[AddComponentMenu("Modular Avatar/MAEx Blink Animator Generator")]
	public class MAExBlinkAnimatorGenerator : MAExAnimatorGeneratorModuleBase<MAExBlinkAnimatorGenerator> {
		public SkinnedMeshRenderer faceMesh;
		public int blendShapeIndex;
		public AnimationClip animationSource;
	}
}