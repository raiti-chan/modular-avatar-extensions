using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor {
	internal class AnimatorGeneratorHook {

		// ReSharper disable once MemberCanBeMadeStatic.Global
		internal void OnPreprocessAvatar(GameObject avatarGameObject) {
			MAExAnimatorGeneratorModuleBase[] generatorModules = avatarGameObject.transform.GetComponentsInChildren<MAExAnimatorGeneratorModuleBase>(true)
				.Where(module => module.enabled)
				.ToArray();
		
			foreach (MAExAnimatorGeneratorModuleBase generatorModule in generatorModules) {
				generatorModule.GetFactory().PreProcess(avatarGameObject);
			}
			
			foreach (MAExAnimatorGeneratorModuleBase generatorModule in generatorModules) {
				RuntimeAnimatorController controller = generatorModule.GetFactory().CreateController(avatarGameObject);
				GameObject targetObject = generatorModule.gameObject;

				ModularAvatarMergeAnimator mergeAnimator = targetObject.AddComponent<ModularAvatarMergeAnimator>();
				mergeAnimator.animator = controller;
				mergeAnimator.layerType = generatorModule.LayerType;
				mergeAnimator.deleteAttachedAnimator = generatorModule.DeleteAttachedAnimator;
				mergeAnimator.pathMode = generatorModule.PathMode;
				mergeAnimator.matchAvatarWriteDefaults = generatorModule.MatchAvatarWriteDefaults;
			}
			
			foreach (MAExAnimatorGeneratorModuleBase generatorModule in generatorModules) {
				generatorModule.GetFactory().PostProcess(avatarGameObject);
			}
		}
	}
}