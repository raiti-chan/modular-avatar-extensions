using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class PresetEditor : VisualElement {
		private const string UXML_GUID = "f51391710a8f70244b90fbba3b6d7968";

		private readonly IntegerField _defaultPresetField;
		private readonly CustomListView _listView;
		private readonly PresetEditorContent _content;

		public string bindingPath {
			get => this._listView.bindingPath;
			set => this._listView.bindingPath = value;
		}


		public PresetEditor() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._listView = this.Q<CustomListView>("ListView");
			this._listView.MakeItem = this.ListViewMakeItem;
			this._listView.BindItem = this.ListViewBindItem;
			this._listView.onSelectionChanged += this.ListViewSelectionChanged;
			this._content = this.Q<PresetEditorContent>("PresetEditorContent");

			this._defaultPresetField = this.Q<IntegerField>("_DefaultValueField");
			this._defaultPresetField.RegisterValueChangedCallback(evt => this._listView.Refresh(false));
		}

		private void ListViewSelectionChanged(SerializedProperty selected) {
			if (selected != null) {
				this._content.BindProperty(selected);
			} else {
				this._content.Unbind();
			}
		}

		private BindableElement ListViewMakeItem() {
			PresetEditorElement element = new PresetEditorElement {
				onSelectDefaultValue = this.ChangeDefaultToggle
			};
			return element;
		}

		private void ChangeDefaultToggle(PresetEditorElement element) {
			this._defaultPresetField.value = element.ElementIndex;
		}

		
		private void ListViewBindItem(SerializedProperty serializedProperty, VisualElement element, int i) {
			if (!(element is PresetEditorElement presetEditorElement)) return;

			SerializedProperty parentProperty = serializedProperty.GetArrayElementAtIndex(i);
			presetEditorElement.BindProperty(parentProperty);
			presetEditorElement.ElementIndex = i;
			presetEditorElement.DefaultToggle.SetValueWithoutNotify(this._defaultPresetField.value == i);
		}
		

		public new class UxmlFactory : UxmlFactory<PresetEditor, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits {
			private readonly UxmlStringAttributeDescription _bindingPath = new UxmlStringAttributeDescription {
				name = "binding-path"
			};

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
				base.Init(ve, bag, cc);
				string valueFromBag = this._bindingPath.GetValueFromBag(bag, cc);
				if (string.IsNullOrEmpty(valueFromBag) || !(ve is PresetEditor presetEditor)) return;
				presetEditor.bindingPath = valueFromBag;
			}
		}
	}
}