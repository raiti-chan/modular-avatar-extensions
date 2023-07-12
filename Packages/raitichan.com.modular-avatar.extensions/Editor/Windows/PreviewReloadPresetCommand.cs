using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PreviewReloadPresetCommand : IPresetEditorPreviewCommand {
		public void Process(MAExObjectPreset data, PreviewAvatarController controller, PresetPreviewContext context) {
			controller.ResetPreviewObject();
			context.ClearLayerStack();
			
			PreviewSetPresetCommand.SetPreview(data, controller, context, false);
		}
	}
}