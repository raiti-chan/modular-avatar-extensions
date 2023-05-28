using System.Collections.Generic;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.UnityUtils {
	public static class MeshExtensions {
		public static IEnumerable<string> GetBlendShapeNames(this Mesh target) {
			int count = target.blendShapeCount;
			for (int i = 0; i < count; i++) {
				yield return target.GetBlendShapeName(i);
			}
		}
	}
}