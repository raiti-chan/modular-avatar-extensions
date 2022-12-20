using nadena.dev.modular_avatar.core.editor;
using UnityEditor;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor {
	[InitializeOnLoad]
	public class ModularAvatarExtensionsProcessor {

		static ModularAvatarExtensionsProcessor() {
			AvatarProcessor.BeforeProcessing += ProcessAvatar;
		}

		private static AnimatorGeneratorHook _animatorGeneratorHook;

		private static void ProcessAvatar(GameObject avatarGameObject) {
			_animatorGeneratorHook = new AnimatorGeneratorHook();
			_animatorGeneratorHook.OnPreprocessAvatar(avatarGameObject);
		}
		
	}
}