using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using raitichan.com.modular_avatar.extensions.Editor.ControllerFactories;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor {
	internal class AnimatorGeneratorHook {

		// ReSharper disable once MemberCanBeMadeStatic.Global
		internal void OnProcessAvatar(BuildContext context) {
			MAExAnimatorGeneratorModuleBase[] generatorModules = context.AvatarRootObject.transform.GetComponentsInChildren<MAExAnimatorGeneratorModuleBase>(true)
				.Where(module => module.enabled)
				.ToArray();
		
			foreach (MAExAnimatorGeneratorModuleBase generatorModule in generatorModules) {
				IControllerFactory factory = generatorModule.GetFactory() as IControllerFactory;
				factory?.PreProcess(context);
			}
			
			foreach (MAExAnimatorGeneratorModuleBase generatorModule in generatorModules) {
				IControllerFactory factory = generatorModule.GetFactory() as IControllerFactory;
				RuntimeAnimatorController controller = factory?.CreateController(context);
				GameObject targetObject = generatorModule.gameObject;

				ModularAvatarMergeAnimator mergeAnimator = targetObject.AddComponent<ModularAvatarMergeAnimator>();
				mergeAnimator.animator = controller;
				mergeAnimator.layerType = generatorModule.LayerType;
				mergeAnimator.deleteAttachedAnimator = generatorModule.DeleteAttachedAnimator;
				mergeAnimator.pathMode = generatorModule.PathMode;
				mergeAnimator.matchAvatarWriteDefaults = generatorModule.MatchAvatarWriteDefaults;
			}
			
			foreach (MAExAnimatorGeneratorModuleBase generatorModule in generatorModules) {
				IControllerFactory factory = generatorModule.GetFactory() as IControllerFactory;
				factory?.PostProcess(context);
				Object.DestroyImmediate(generatorModule);
			}
		}
	}
}