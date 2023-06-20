using System;
using System.Collections.Generic;
using raitichan.com.modular_avatar.extensions.Serializable;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[DisallowMultipleComponent]
	[AddComponentMenu("Modular Avatar/MAEx Object Preset")]
	public class MAExObjectPreset : MAExAnimatorGeneratorModuleBase<MAExObjectPreset> {
		public string displayName;
		public Texture2D menuIcon;

		public string parameterName;
		public int defaultValue;
		public bool isInternal;
		public bool saved;

		public Preset[] presets;
		
		[Serializable]
		public class Preset {
			public string displayName;
			public Texture2D menuIcon;

			public List<GameObject> showObjects;
			public List<BlendShapeData> BlendShapes;

			public List<ToggleSet> toggleSets;
		}

		[Serializable]
		public class ToggleSet {
			public string displayName;
			public Texture2D menuIcon;

			public string parameterName;
			public bool defaultValue;
			public bool isInternal;
			public bool saved;

			public List<GameObject> toggleObjects;
			public List<BlendShapeData> BlendShapes;
		}
	}
}