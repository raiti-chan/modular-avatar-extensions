using System;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Serializable {
	[Serializable]
	public struct BlendShapeWeight {
		public string key;
		[Range(0.0f, 100.0f)]
		public float weight;
	}
}