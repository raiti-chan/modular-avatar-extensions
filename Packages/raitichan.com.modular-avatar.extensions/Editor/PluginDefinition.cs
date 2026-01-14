using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;

[assembly: ExportsPlugin(typeof(raitichan.com.modular_avatar.extensions.Editor.PluginDefinition))]

namespace raitichan.com.modular_avatar.extensions.Editor {
	public class PluginDefinition : Plugin<PluginDefinition> {
		public override string QualifiedName => "raitichan.com.modular-avatar.extensions";
		public override string DisplayName => "Modular Avatar Ex";

		protected override void Configure() {
			Sequence seq = this.InPhase(BuildPhase.Generating);
			seq.Run(RenderQueueTogglePath.Instance);
			seq.Run(AnimatorGeneratorPath.Instance);

			
			seq = this.InPhase(BuildPhase.Optimizing);
			seq.Run(MMDSetupPath.Instance);

		}

		internal class RenderQueueTogglePath : Pass<RenderQueueTogglePath> {
			protected override void Execute(BuildContext context) {
				RenderQueueToggleHook renderQueueToggleHook = new RenderQueueToggleHook(context);
				renderQueueToggleHook.OnProcessAvatar();
			}
		}

		internal class AnimatorGeneratorPath : Pass<AnimatorGeneratorPath> {
			private static AnimatorGeneratorHook _animatorGeneratorHook;

			protected override void Execute(BuildContext context) {
				_animatorGeneratorHook = new AnimatorGeneratorHook();
				_animatorGeneratorHook.OnProcessAvatar(context);
			}
		}
		
		internal class MMDSetupPath : Pass<MMDSetupPath> {
			private static MMDSetupHook _mmdSetupHook;
			
			protected override void Execute(BuildContext context) {
				_mmdSetupHook = new MMDSetupHook();
				_mmdSetupHook.OnProcessAvatar(context.AvatarRootObject);
			}
		}
	}
}