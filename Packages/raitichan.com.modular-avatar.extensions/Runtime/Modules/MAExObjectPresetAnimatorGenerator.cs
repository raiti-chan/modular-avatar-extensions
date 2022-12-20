using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ModularAvatarMenuInstaller))]
	[RequireComponent(typeof(ModularAvatarParameters))]
	[AddComponentMenu("Modular Avatar/MAEx Object Preset Animator Generator")]
	public class MAExObjectPresetAnimatorGenerator : MAExAnimatorGeneratorModuleBase<MAExObjectPresetAnimatorGenerator> {

		public string parameterName;
		public string displayName;
		public Texture2D menuIcon;
		public bool isInternal;
		public bool saved;
		public List<PresetData> presetData;

		[Serializable]
		public class PresetData {
			public string displayName;
			public Texture2D menuIcon;
			public List<GameObject> enableObjects;
			public List<BlendShapeData> blendShapes;

			public Dictionary<int, float>GetAllBlendShapeIndexAndWeight(SkinnedMeshRenderer skinnedMeshRenderer) {
				return this.blendShapes
					.Where(blendShapeData => blendShapeData.skinnedMeshRenderer == skinnedMeshRenderer)
					.SelectMany(blendShapeData => blendShapeData.BlendShapeIndexAndWeights)
					.GroupBy(blendShapeData => blendShapeData.index)
					.ToDictionary(grouping => grouping.Key, grouping => grouping.Last().weight);
			}

			[Serializable]
			public class BlendShapeData {
				public SkinnedMeshRenderer skinnedMeshRenderer;
				public List<BlendShapeIndexAndWeight> BlendShapeIndexAndWeights;

				[Serializable]
				public struct BlendShapeIndexAndWeight {
					public int index;
					public float weight;
				}
			}
			
		}

	}
}