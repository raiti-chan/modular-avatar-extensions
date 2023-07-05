using raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PresetChangeEvent : EventBase<PresetChangeEvent> {
		public IPresetEditorPreviewCommand command { get; private set; }
		
		protected override void Init() {
			base.Init();
			this.LocalInit();
		}

		private void LocalInit() {
			EventBaseHelper.SetPropagation(this, EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable);
		}

		public PresetChangeEvent() {
			this.LocalInit();
		}

		public static PresetChangeEvent GetPooled(IPresetEditorPreviewCommand command) {
			PresetChangeEvent evt = EventBase<PresetChangeEvent>.GetPooled();
			evt.command = command;
			return evt;
		}
	}
}