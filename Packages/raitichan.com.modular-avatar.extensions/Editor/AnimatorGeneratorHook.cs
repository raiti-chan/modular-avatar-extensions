using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor {
	internal class AnimatorGeneratorHook {

		private Dictionary<GameObject, bool> _generatorModuleObjectsEnableMap;

		internal void OnPreprocessAvatar(GameObject avatarGameObject) {
			this._generatorModuleObjectsEnableMap = new Dictionary<GameObject, bool>();
			MAExAnimatorGeneratorModuleBase[] generatorModules = avatarGameObject.transform.GetComponentsInChildren<MAExAnimatorGeneratorModuleBase>(true)
				.Where(module => module.enabled)
				.ToArray();
		
			foreach (MAExAnimatorGeneratorModuleBase generatorModule in generatorModules) {
				generatorModule.GetFactory().PreProcess(avatarGameObject);
			}
			
			foreach (MAExAnimatorGeneratorModuleBase generatorModule in generatorModules) {
				RuntimeAnimatorController controller = generatorModule.GetFactory().CreateController(avatarGameObject);
				GameObject targetObject = generatorModule.gameObject;
				if (!this._generatorModuleObjectsEnableMap.ContainsKey(targetObject)) {
					this._generatorModuleObjectsEnableMap[targetObject] = targetObject.activeSelf;
				}
				targetObject.SetActive(false);

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

		// ReSharper disable once UnusedParameter.Global
		internal void OnCleanedUpProcessAvatar(GameObject avatarGameObject) {
			foreach (KeyValuePair<GameObject, bool> keyValuePair in this._generatorModuleObjectsEnableMap.Where(keyValuePair => keyValuePair.Key != null)) {
				keyValuePair.Key.SetActive(keyValuePair.Value);
			}
		}
	}
}