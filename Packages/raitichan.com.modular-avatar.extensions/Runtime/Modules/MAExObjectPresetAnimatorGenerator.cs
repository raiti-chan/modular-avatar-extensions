using System;
using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Serializable;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ModularAvatarMenuInstaller))]
	[RequireComponent(typeof(ModularAvatarParameters))]
	[AddComponentMenu("Modular Avatar/MAEx Object Preset Animator Generator")]
	public class MAExObjectPresetAnimatorGenerator : MAExAnimatorGeneratorModuleBase<MAExObjectPresetAnimatorGenerator> {
		public string displayName;
		public string parameterName;
		public Texture2D menuIcon;
		public bool isInternal;
		public bool saved;
		public int defaultPreset;
		public List<PresetData> presetData;
		public bool isToggleInvert;
		public List<ToggleSetData> toggleSetData;

		[Serializable]
		public class PresetData {
			public string displayName;
			public Texture2D menuIcon;
			public List<GameObject> enableObjects;
			public List<BlendShapeData> blendShapes;
		}

		[Serializable]
		public class ToggleSetData {
			public string displayName;
			public string parameterName;
			public Texture2D menuIcon;
			public bool isInternal;
			public bool saved;
			public bool defaultValue;
			public List<GameObject> toggleObjects;
			public List<BlendShapeData> blendShapes;
		}
	}
}