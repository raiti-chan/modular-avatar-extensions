using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions {

	public interface IRuntimeAnimatorFactory {
		RuntimeAnimatorController CreateController(GameObject avatarGameObject);
	}
	
	public interface IRuntimeAnimatorFactory<ModuleType> : IRuntimeAnimatorFactory where ModuleType : MaExAnimatorGeneratorModuleBase<ModuleType> {
		ModuleType Target { get; set; }
	}	
}