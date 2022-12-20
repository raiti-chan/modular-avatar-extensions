using nadena.dev.modular_avatar.core;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[DisallowMultipleComponent]
	[RequireComponent(typeof(ModularAvatarMenuInstaller))]
	[RequireComponent(typeof(ModularAvatarParameters))]
	[AddComponentMenu("Modular Avatar/MAEx Toggle Animator Generator")]
	public class MAExToggleAnimatorGenerator : MAExAnimatorGeneratorModuleBase<MAExToggleAnimatorGenerator> {
		public string parameterName;
		public string displayName;
		public Texture2D menuIcon;
		public bool isInvert;
		public bool isInternal;
		public bool saved;
		public bool defaultValue;
	}
}