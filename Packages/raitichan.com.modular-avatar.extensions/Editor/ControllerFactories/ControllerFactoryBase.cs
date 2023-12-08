using nadena.dev.ndmf;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	public interface IControllerFactory {
		void PreProcess(BuildContext context);
		RuntimeAnimatorController CreateController(BuildContext context);
		void PostProcess(BuildContext context);
	}
	
	public abstract class ControllerFactoryBase<T> : IRuntimeAnimatorFactory<T>, IControllerFactory where T : MAExAnimatorGeneratorModuleBase {
		public abstract void PreProcess(BuildContext context);
		public abstract RuntimeAnimatorController CreateController(BuildContext context);
		public abstract void PostProcess(BuildContext context);

		public T Target { get; set; }
	}
}