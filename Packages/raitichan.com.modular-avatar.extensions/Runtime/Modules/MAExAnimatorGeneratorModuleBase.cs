using System;
using System.Linq;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.Modules {

	public abstract class MAExAnimatorGeneratorModuleBase : AvatarTagComponent {

		public abstract VRCAvatarDescriptor.AnimLayerType LayerType { get; }
		public abstract bool DeleteAttachedAnimator { get; }
		public abstract MergeAnimatorPathMode PathMode { get; }
		public abstract bool MatchAvatarWriteDefaults { get; }

		public abstract IRuntimeAnimatorFactory GetFactory();
	}
	
	public abstract class MAExAnimatorGeneratorModuleBase<ModuleType> : MAExAnimatorGeneratorModuleBase where ModuleType : MAExAnimatorGeneratorModuleBase<ModuleType> {
		
		public override VRCAvatarDescriptor.AnimLayerType LayerType => VRCAvatarDescriptor.AnimLayerType.FX;
		public override bool DeleteAttachedAnimator => true;
		public override MergeAnimatorPathMode PathMode => MergeAnimatorPathMode.Absolute;
		public override bool MatchAvatarWriteDefaults => false;

		private IRuntimeAnimatorFactory _factory;
		public override IRuntimeAnimatorFactory GetFactory() {
			return _factory ?? (_factory = GetFactory(this));
		}

		private static IRuntimeAnimatorFactory<ModuleType> GetFactory(MAExAnimatorGeneratorModuleBase<ModuleType> module) {
			Type factoryType = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => !type.IsAbstract && !type.IsInterface)
				.First(type => typeof(IRuntimeAnimatorFactory<ModuleType>).IsAssignableFrom(type));
			IRuntimeAnimatorFactory<ModuleType> instance = (IRuntimeAnimatorFactory<ModuleType>)System.Activator.CreateInstance(factoryType);
			instance.Target = (ModuleType)module;
			return instance;
		}
	}


}