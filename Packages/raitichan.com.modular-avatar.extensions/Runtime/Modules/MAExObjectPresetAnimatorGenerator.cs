using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Serializable;
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
		}

	}
}