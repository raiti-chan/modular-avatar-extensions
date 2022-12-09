using nadena.dev.modular_avatar.core.editor;
using UnityEditor;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor {
	[InitializeOnLoad]
	public class ModularAvatarExtensionsProcessor {

		static ModularAvatarExtensionsProcessor() {
			AvatarProcessor.BeforeProcessing += ProcessAvatar;
			AvatarProcessor.CleanedUpProcessing += CleanedUpProcessing;
		}

		private static AnimatorGeneratorHook _animatorGeneratorHook;

		private static void ProcessAvatar(GameObject avatarGameObject) {
			_animatorGeneratorHook = new AnimatorGeneratorHook();
			_animatorGeneratorHook.OnPreprocessAvatar(avatarGameObject);
		}

		private static void CleanedUpProcessing(GameObject avatarGameObject) {
			_animatorGeneratorHook.OnCleanedUpProcessAvatar(avatarGameObject);
		}
		
		
	}
}