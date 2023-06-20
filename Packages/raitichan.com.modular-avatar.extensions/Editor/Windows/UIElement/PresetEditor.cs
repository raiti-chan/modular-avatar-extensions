using System;
using System.Collections.Generic;
using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class PresetEditor : BindableElement {
		private const string UXML_GUID = "f51391710a8f70244b90fbba3b6d7968";

		private readonly List<SerializedProperty> _data = new List<SerializedProperty>();

		private readonly IntegerField _defaultPresetField;
		private readonly ListView _listView;
		private readonly ToolbarButton _addButton;
		private readonly ToolbarButton _removeButton;
		private readonly ToolbarButton _upButton;
		private readonly ToolbarButton _downButton;
		private readonly PresetEditorContent _content;

		private SerializedProperty _serializedProperty;

		public PresetEditor() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);
			VisualElement listPanel = this.Q<VisualElement>("ListPanel");
			listPanel.Q<IntegerField>("_ArraySizeField").RegisterValueChangedCallback(evt => this.Refresh(true));
			
			this._defaultPresetField = this.Q<IntegerField>("_DefaultValueField");
			this._defaultPresetField.RegisterValueChangedCallback(evt => this.Refresh(false));

			this._addButton = listPanel.Q<ToolbarButton>("AddButton");
			this._addButton.clicked += AddButtonClicked;
			this._addButton.SetEnabled(false);

			this._removeButton = listPanel.Q<ToolbarButton>("RemoveButton");
			this._removeButton.clicked += RemoveButtonClicked;
			this._removeButton.SetEnabled(false);

			this._upButton = listPanel.Q<ToolbarButton>("UpButton");
			this._upButton.clicked += UpButtonClicked;
			this._upButton.SetEnabled(false);

			this._downButton = listPanel.Q<ToolbarButton>("DownButton");
			this._downButton.clicked += DownButtonClicked;
			this._downButton.SetEnabled(false);

			this._content = this.Q<PresetEditorContent>("PresetEditorContent");

			this._listView = listPanel.Q<ListView>("ListView");
			this._listView.itemsSource = this._data;
			this._listView.makeItem = ListViewMakeItem;
			this._listView.bindItem = this.ListViewBindItem;
			this._listView.onSelectionChanged += this.ListViewSelectionChanged;
			this._listView.selectionType = SelectionType.Single;
			if (!(this._listView.contentContainer.parent.parent is ScrollView scrollView)) return;
			scrollView.showVertical = true;
		}

		private void Refresh(bool updateArray) {
			if (updateArray) {
				this._data.Clear();

				int size = this._serializedProperty.arraySize;
				for (int i = 0; i < size; i++) {
					this._data.Add(this._serializedProperty.GetArrayElementAtIndex(i));
				}

				this.ButtonStateChange();
			}

			this._listView.Refresh();
		}
		
		private void ButtonStateChange() {
			this._addButton.SetEnabled(this._serializedProperty != null);
			int selectIndex = this._listView.selectedIndex;
			this._removeButton.SetEnabled(0 <= selectIndex && selectIndex < this._data.Count);
			this._upButton.SetEnabled(0 < selectIndex && selectIndex < this._data.Count);
			this._downButton.SetEnabled(0 <= selectIndex && selectIndex < this._data.Count - 1);
		}

		private void AddButtonClicked() {
			if (this._serializedProperty == null) return;
			this._serializedProperty.InsertArrayElementAtIndex(this._serializedProperty.arraySize);
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		private void RemoveButtonClicked() {
			if (this._serializedProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex < 0 || this._data.Count <= selectIndex) return;
			this._serializedProperty.DeleteArrayElementAtIndex(selectIndex);
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
			this._listView.selectedIndex = -1;
		}

		private void UpButtonClicked() {
			if (this._serializedProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex == 0) return;
			this._serializedProperty.MoveArrayElement(selectIndex, selectIndex - 1);
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
			this._listView.selectedIndex = selectIndex - 1;
		}
		
		private void DownButtonClicked() {
			if (this._serializedProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex == this._data.Count - 1) return;
			this._serializedProperty.MoveArrayElement(selectIndex, selectIndex + 1);
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
			this._listView.selectedIndex = selectIndex + 1;
		}


		private void ListViewSelectionChanged(List<object> obj) {
			this.ButtonStateChange();
			int selectIndex = this._listView.selectedIndex;
			if (0 <= selectIndex && selectIndex < this._data.Count) {
				this._content.BindProperty(this._data[selectIndex]);
			} else {
				this._content.Unbind();
			}

		}

		private VisualElement ListViewMakeItem() {
			PresetEditorElement element = new PresetEditorElement {
				onSelectDefaultValue = this.ChangeDefaultToggle
			};
			return element;
		}

		private void ChangeDefaultToggle(PresetEditorElement element) {
			this._defaultPresetField.value = element.ElementIndex;
		}

		private void ListViewBindItem(VisualElement element, int i) {
			if (!(element is IBindable bindable)) {
				bindable = element.Query().Where(visualElement => visualElement is IBindable).First() as IBindable;
			}

			if (bindable == null) {
				throw new InvalidOperationException("Can't find BindableElement: please provide BindableVisualElements or provide your own Listview.bindItem callback");
			}

			SerializedProperty parentProperty = this._data[i];
			bindable.BindProperty(parentProperty);
			
			if (!(element is PresetEditorElement presetEditorElement)) return;
			presetEditorElement.ElementIndex = i;
			presetEditorElement.DefaultToggle.SetValueWithoutNotify(this._defaultPresetField.value == i);
		}
		

		public override void HandleEvent(EventBase evt) {
			if (evt.GetType() == SerializedPropertyBindEventHelper.Type) {
				SerializedPropertyBindEventHelper serializedPropertyBindEvent = new SerializedPropertyBindEventHelper(evt);
				this._serializedProperty = serializedPropertyBindEvent.GetBindProperty();
			}

			base.HandleEvent(evt);
		}

		public new class UxmlFactory : UxmlFactory<PresetEditor, UxmlTraits> { }

		public new class UxmlTraits : BindableElement.UxmlTraits { }
	}
}