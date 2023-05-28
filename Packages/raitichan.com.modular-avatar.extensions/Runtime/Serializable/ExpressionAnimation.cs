using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Serializable {
	[Serializable]
	public class ExpressionAnimation {
		public List<BlendShapeAnimation> blendShapeAnimations;

		public Dictionary<SkinnedMeshRenderer, BlendShapeAnimation> GetBlendShapeAnimationDictionary() {
			return this.blendShapeAnimations.ToDictionary(animation => animation.target);
		}
	}
}