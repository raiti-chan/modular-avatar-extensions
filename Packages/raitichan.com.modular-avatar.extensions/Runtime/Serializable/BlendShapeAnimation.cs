using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Serializable {
	[Serializable]
	public class BlendShapeAnimation {
		public SkinnedMeshRenderer target;
		public List<BlendShapeWeight> blendShapeWeights;

		public Dictionary<string, BlendShapeWeight> GetWeightDictionary() {
			return this.blendShapeWeights.ToDictionary(weight => weight.key);
		}
	}
}