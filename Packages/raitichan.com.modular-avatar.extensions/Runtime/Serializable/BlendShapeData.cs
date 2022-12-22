using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Serializable {
	[Serializable]
	public class BlendShapeData {
		public SkinnedMeshRenderer skinnedMeshRenderer;
		public List<BlendShapeIndexAndWeight> blendShapeIndexAndWeights;

		[Serializable]
		public struct BlendShapeIndexAndWeight {
			public int index;
			public float weight;
		}

		public static Dictionary<int, float> GetWeightDictionary(IEnumerable<BlendShapeData> blendShapeDataList,
			SkinnedMeshRenderer skinnedMeshRenderer) {
			return blendShapeDataList
				.Where(blendShapeData => blendShapeData.skinnedMeshRenderer == skinnedMeshRenderer)
				.SelectMany(blendShapeData => blendShapeData.blendShapeIndexAndWeights)
				.GroupBy(blendShapeData => blendShapeData.index)
				.ToDictionary(grouping => grouping.Key, grouping => grouping.Last().weight);
		}

		public static IEnumerable<BlendShapeIndexAndWeight> GetAllIndexAndWeight(IEnumerable<BlendShapeData> blendShapeDataList,
			SkinnedMeshRenderer skinnedMeshRenderer) {
			return blendShapeDataList
				.Where(blendShapeData => blendShapeData.skinnedMeshRenderer == skinnedMeshRenderer)
				.SelectMany(blendShapeData => blendShapeData.blendShapeIndexAndWeights)
				.GroupBy(blendShapeData => blendShapeData.index)
				.Select(grouping => new BlendShapeIndexAndWeight { index = grouping.Key, weight = grouping.Last().weight });
		}

		public static IEnumerable<SkinnedMeshRenderer> GetAllSkinnedMeshRenderer(IEnumerable<BlendShapeData> blendShapeDataList) {
			return blendShapeDataList
				.Select(blendShapeData => blendShapeData.skinnedMeshRenderer)
				.Distinct();
		}
	}
}