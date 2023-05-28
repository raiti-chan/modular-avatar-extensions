using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions {

	public interface IRuntimeAnimatorFactory {
		void PreProcess(GameObject avatarGameObject);
		RuntimeAnimatorController CreateController(GameObject avatarGameObject);
		void PostProcess(GameObject avatarGameObject);
	}
	
	public interface IRuntimeAnimatorFactory<ModuleType> : IRuntimeAnimatorFactory where ModuleType : MAExAnimatorGeneratorModuleBase<ModuleType> {
		ModuleType Target { get; set; }
	}	
}