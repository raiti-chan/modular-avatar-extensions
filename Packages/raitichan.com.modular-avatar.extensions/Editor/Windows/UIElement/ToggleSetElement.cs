using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity;
using UnityEditor;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class ToggleSetElement : CustomBindableElement {
		private const string UXML_GUID = "ba74e8fbd99239c47b9a1e49c653e690";

		public int ToggleSetIndex { get; set; }

		public ToggleSetElement() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this.RegisterCallback<ChangeEvent<bool>>(this.PreviewToggleOnChanged, TrickleDown.TrickleDown);
		}

		private void PreviewToggleOnChanged(ChangeEvent<bool> evt) {
			if (!this.IsBound) return;
			if (!(evt.target is ToggleButton)) return;
			this.BindingProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.preview)).boolValue = evt.newValue;
			this.BindingProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			using (ToggleSetPreviewChangeEvent pooled = ToggleSetPreviewChangeEvent.GetPooled(this.ToggleSetIndex, evt.newValue)) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}
	}

	public class ToggleSetPreviewChangeEvent : EventBase<ToggleSetPreviewChangeEvent> {
		public int ToggleSetIndex { get; private set; }
		public bool NewValue { get; private set; }
		
		protected override void Init() {
			base.Init();
			this.LocalInit();
		}

		private void LocalInit() {
			EventBaseHelper.SetPropagation(this, EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable);
		}

		public ToggleSetPreviewChangeEvent() {
			this.LocalInit();
		}

		public static ToggleSetPreviewChangeEvent GetPooled(int toggleSetIndex, bool newValue) {
			ToggleSetPreviewChangeEvent evt = EventBase<ToggleSetPreviewChangeEvent>.GetPooled();
			evt.ToggleSetIndex = toggleSetIndex;
			evt.NewValue = newValue;
			return evt;
		}
	}
}