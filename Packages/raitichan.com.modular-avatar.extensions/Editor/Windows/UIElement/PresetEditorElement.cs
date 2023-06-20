using System;
using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class PresetEditorElement : BindableElement {
		private const string UXML_GUID = "c4dcaa0840844d10a8bf49adf26f10f6";

		public Action<PresetEditorElement> onSelectDefaultValue;
		public int ElementIndex { get; set; }
		public Toggle DefaultToggle { get; }

		public PresetEditorElement() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this.DefaultToggle = this.Q<Toggle>("DefaultToggle");
			this.DefaultToggle.RegisterValueChangedCallback(DefaultToggleChanged);
		}

		private void DefaultToggleChanged(ChangeEvent<bool> evt) {
			if (!evt.newValue) {
				this.DefaultToggle.SetValueWithoutNotify(true);
				return;
			}
			this.onSelectDefaultValue?.Invoke(this);
		}

		public override void HandleEvent(EventBase evt) {
			if (evt.GetType() == SerializedObjectBindEventHelper.Type) {
				this.Q<TextField>("DisplayNameField").bindingPath = "displayName";
				this.Q<PropertyField>("MenuIconField").bindingPath = "menuIcon";
			}
			base.HandleEvent(evt);
		}
		
	}
}