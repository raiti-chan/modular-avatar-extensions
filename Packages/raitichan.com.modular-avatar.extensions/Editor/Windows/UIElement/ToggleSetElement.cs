using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
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
			using (PresetChangeEvent presetChangeEvent = PresetChangeEvent.GetPooled(new PreviewToggleSetChangeActiveCommand(this.ToggleSetIndex, evt.newValue))) {
				presetChangeEvent.target = this;
				this.SendEvent(presetChangeEvent);
			}
		}
	}
}