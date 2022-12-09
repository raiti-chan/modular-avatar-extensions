using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[AddComponentMenu("Modular Avatar/Raitichan/MAEx Blink Animator Generator")]
	public class MAExBlinkAnimatorGenerator : MaExAnimatorGeneratorModuleBase<MAExBlinkAnimatorGenerator> {
		public SkinnedMeshRenderer faceMesh;
		public int blendShapeIndex;
		public AnimationClip animationSource;
	}
}