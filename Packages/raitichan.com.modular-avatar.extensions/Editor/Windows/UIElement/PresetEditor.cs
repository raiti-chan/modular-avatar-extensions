using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class PresetEditor : VisualElement {
		private const string UXML_GUID = "f51391710a8f70244b90fbba3b6d7968";

		private readonly IntegerField _defaultPresetField;
		private readonly CustomListView _listView;
		private readonly PresetEditorContent _content;

		public PresetEditor() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._listView = this.Q<CustomListView>("ListView");
			this._listView.OnRemove = this.ListViewOnRemove;
			this._listView.OnUp = this.ListViewOnUp;
			this._listView.OnDown = this.ListViewOnDown;
			this._listView.MakeItem = this.ListViewMakeItem;
			this._listView.BindItem = this.ListViewBindItem;
			this._listView.onSelectionChanged += this.ListViewSelectionChanged;


			this._content = this.Q<PresetEditorContent>("PresetEditorContent");

			this._defaultPresetField = this.Q<IntegerField>("_DefaultValueField");
			this._defaultPresetField.RegisterValueChangedCallback(OnDefaultPresetFieldChange);
		}

		private void OnDefaultPresetFieldChange(ChangeEvent<int> evt) {
			if (evt.newValue < 0 || this._listView.Count <= evt.newValue) {
				this._defaultPresetField.SetValueWithoutNotify(0);
			}

			this._listView.Refresh();
		}

		private void ListViewOnRemove(SerializedProperty serialized, int selectIndex) {
			serialized.DeleteArrayElementAtIndex(selectIndex);
			serialized.serializedObject.ApplyModifiedProperties();
			this._listView.SelectedIndex = -1;

			if (this._defaultPresetField.value == selectIndex) this._defaultPresetField.value = 0;
		}
		
		private void ListViewOnUp(SerializedProperty serialized, int selectIndex) {
			this._defaultPresetField.value--;
			serialized.MoveArrayElement(selectIndex, selectIndex - 1);
			serialized.serializedObject.ApplyModifiedProperties();
			this._listView.SelectedIndex = selectIndex - 1;
		}

		private void ListViewOnDown(SerializedProperty serialized, int selectIndex) {
			this._defaultPresetField.value++;
			serialized.MoveArrayElement(selectIndex, selectIndex + 1);
			serialized.serializedObject.ApplyModifiedProperties();
			this._listView.SelectedIndex = selectIndex + 1;
		}

		private VisualElement ListViewMakeItem() {
			PresetEditorElement element = new PresetEditorElement {
				onSelectDefaultValue = this.ChangeDefaultToggle
			};
			return element;
		}
		
		private void ListViewBindItem(SerializedProperty serializedProperty, VisualElement element, int i) {
			if (!(element is PresetEditorElement presetEditorElement)) return;

			SerializedProperty parentProperty = serializedProperty.GetArrayElementAtIndex(i);
			presetEditorElement.BindProperty(parentProperty, this._listView.ObjWrapper);
			presetEditorElement.ElementIndex = i;
			presetEditorElement.DefaultToggle.SetValueWithoutNotify(this._defaultPresetField.value == i);
		}
		
		private void ListViewSelectionChanged(SerializedProperty selected) {
			using (PresetChangeEvent evt = PresetChangeEvent.GetPooled(new PreviewSetPresetCommand(this._listView.SelectedIndex))) {
				evt.target = this;
				this.SendEvent(evt);
			}

			if (selected != null) {
				this._content.BindProperty(selected);
			} else {
				this._content.Unbind();
			}
		}

		private void ChangeDefaultToggle(PresetEditorElement element) {
			this._defaultPresetField.value = element.ElementIndex;
		}




		public new class UxmlFactory : UxmlFactory<PresetEditor, UxmlTraits> { }
	}
}