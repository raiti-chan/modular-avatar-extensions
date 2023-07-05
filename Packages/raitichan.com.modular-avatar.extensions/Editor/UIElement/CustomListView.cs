using System;
using System.Collections.Generic;
using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public class CustomListView : CustomBindableElement {
		private const string UXML_GUID = "75e8cb2f03bd602419dca8d3914c38f0";

		private readonly List<SerializedProperty> _data = new List<SerializedProperty>();
		private Func<VisualElement> _makeItem;
		private Action<SerializedProperty, VisualElement, int> _bindItem;

		private readonly ListView _listView;
		private readonly IntegerField _arraySizeField;
		private readonly ToolbarButton _addButton;
		private readonly ToolbarButton _removeButton;
		private readonly ToolbarButton _upButton;
		private readonly ToolbarButton _downButton;

		public int SelectedIndex {
			get => this._listView.selectedIndex;
			set => this._listView.selectedIndex = value;
		}
		public int Count => this._data.Count;
		public SerializedProperty SelectedProperty {
			get {
				if (this.BindingProperty == null) return null;
				int selectedIndex = this.SelectedIndex;
				if (selectedIndex < 0 || this.BindingProperty.arraySize <= selectedIndex) return null;
				return this.BindingProperty.GetArrayElementAtIndex(selectedIndex);
			}
		}
		public Rect AddDropDownPos {
			get {
				Rect addButtonLayout = this._addButton.layout;
				Vector2 localPos = new Vector2(addButtonLayout.xMin, addButtonLayout.height);
				Vector2 worldPos = this.LocalToWorld(localPos);
				return new Rect(worldPos, Vector2.zero);
			}
		}
		public Action<SerializedProperty> OnAdd { get; set; }
		public Action<SerializedProperty, int> OnRemove { get; set; }
		public Action<SerializedProperty, int> OnUp { get; set; }
		public Action<SerializedProperty, int> OnDown { get; set; }
		public event Action<SerializedProperty> onSelectionChanged;
		public Func<VisualElement> MakeItem {
			get => this._makeItem;
			set {
				if (this._makeItem == value) return;
				this._makeItem = value;
				this.Refresh();
			}
		}
		public Action<SerializedProperty, VisualElement, int> BindItem {
			get => this._bindItem;
			set {
				if (this._bindItem == value) return;
				this._bindItem = value;
				this.Refresh();
			}
		}


		public string Title {
			get => this.Q<Label>("Title").text;
			set => this.Q<Label>("Title").text = value;
		}

		public int ItemHeight {
			get => this._listView.itemHeight;
			set => this._listView.itemHeight = value;
		}

		public bool CanSelect {
			get => this._listView.selectionType == SelectionType.Single;
			set => this._listView.selectionType = value ? SelectionType.Single : SelectionType.None;
		}

		public bool CanReorder {
			get => this._upButton.style.display == DisplayStyle.Flex;
			set {
				this._upButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
				this._downButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		public bool CanResize {
			get => this._addButton.style.display == DisplayStyle.Flex;
			set {
				this._addButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
				this._removeButton.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
			}
		} 


		public CustomListView() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._listView = this.Q<ListView>("ShowObjectList");
			this._listView.itemsSource = this._data;
			this._listView.makeItem = this.ListViewMakeItem;
			this._listView.bindItem = this.ListViewBindItem;
			this._listView.onSelectionChanged += this.ListViewSelectionChanged;
			this._listView.contentContainer.pickingMode = PickingMode.Ignore;
			this._listView.contentContainer.parent.pickingMode = PickingMode.Ignore;
			this._listView.contentContainer.parent.parent.pickingMode = PickingMode.Ignore;
			if (this._listView.contentContainer.parent.parent is ScrollView scrollView) {
				scrollView.showVertical = true;
			}

			this._arraySizeField = this.Q<IntegerField>("_ArraySizeField");
			this._arraySizeField.RegisterValueChangedCallback(evt => this.Refresh());

			this._addButton = this.Q<ToolbarButton>("AddButton");
			this._addButton.clicked += AddButtonClicked;
			this._addButton.SetEnabled(false);

			this._removeButton = this.Q<ToolbarButton>("RemoveButton");
			this._removeButton.clicked += RemoveButtonClicked;
			this._removeButton.SetEnabled(false);

			this._upButton = this.Q<ToolbarButton>("UpButton");
			this._upButton.clicked += UpButtonClicked;
			this._upButton.SetEnabled(false);

			this._downButton = this.Q<ToolbarButton>("DownButton");
			this._downButton.clicked += DownButtonClicked;
			this._downButton.SetEnabled(false);
		}

		public void Refresh() {
			if (this.BindingProperty != null) {
				if (this._data.Count != this.BindingProperty.arraySize) {
					this._data.Clear();

					if (this.BindingProperty != null) {
						int size = this.BindingProperty.arraySize;
						for (int i = 0; i < size; i++) {
							this._data.Add(this.BindingProperty.GetArrayElementAtIndex(i));
						}
					}
				}
			} else {
				this._data.Clear();
			}

			this.ButtonStateChange();
			this._listView.Refresh();
		}

		private void ButtonStateChange() {
			this._addButton.SetEnabled(this.BindingProperty != null);
			int selectIndex = this._listView.selectedIndex;
			this._removeButton.SetEnabled(0 <= selectIndex && selectIndex < this._data.Count);
			this._upButton.SetEnabled(0 < selectIndex && selectIndex < this._data.Count);
			this._downButton.SetEnabled(0 <= selectIndex && selectIndex < this._data.Count - 1);
		}

		private void AddButtonClicked() {
			if (this.BindingProperty == null) return;
			if (this.OnAdd != null) {
				this.OnAdd(this.BindingProperty);
				return;
			}

			int lastIndex = this.BindingProperty.arraySize;
			this.BindingProperty.InsertArrayElementAtIndex(lastIndex);
			SerializedProperty addedProperty = this.BindingProperty.GetArrayElementAtIndex(lastIndex);
			addedProperty.SetDefaultValue();
			this.BindingProperty.serializedObject.ApplyModifiedProperties();
		}

		private void RemoveButtonClicked() {
			if (this.BindingProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex < 0 || this._data.Count <= selectIndex) return;

			if (this.OnRemove != null) {
				this.OnRemove(this.BindingProperty, selectIndex);
				return;
			}

			this.BindingProperty.DeleteArrayElementAtIndex(selectIndex);
			this.BindingProperty.serializedObject.ApplyModifiedProperties();
			this.SelectedIndex = -1;
		}

		private void UpButtonClicked() {
			if (this.BindingProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex == 0) return;

			if (this.OnUp != null) {
				this.OnUp(this.BindingProperty, selectIndex);
				return;
			}

			this.BindingProperty.MoveArrayElement(selectIndex, selectIndex - 1);
			this.BindingProperty.serializedObject.ApplyModifiedProperties();
			this.SelectedIndex = selectIndex - 1;
		}

		private void DownButtonClicked() {
			if (this.BindingProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex == this._data.Count - 1) return;

			if (this.OnDown != null) {
				this.OnDown(this.BindingProperty, selectIndex);
				return;
			}

			this.BindingProperty.MoveArrayElement(selectIndex, selectIndex + 1);
			this.BindingProperty.serializedObject.ApplyModifiedProperties();
			this.SelectedIndex = selectIndex + 1;
		}

		private VisualElement ListViewMakeItem() {
			if (this.MakeItem != null) {
				return this.MakeItem();
			}

			BindStoppableElement bindStoppableElement = new BindStoppableElement {
				IsBindStopping = true
			};
			bindStoppableElement.Add(new PropertyField());

			return bindStoppableElement;
		}

		private void ListViewBindItem(VisualElement element, int index) {
			if (this.BindItem != null) {
				this.BindItem(this.BindingProperty, element, index);
				return;
			}

			if (!(element is IBindable bindable)) {
				bindable = element.Query().Where(x => x is IBindable).First() as IBindable;
				element = bindable as VisualElement;
			}

			switch (bindable) {
				case null:
					throw new InvalidOperationException("Can't find BindableElement: please provide BindableVisualElements or provide your own Listview.bindItem callback");
				case ICustomBindable customBindable:
					customBindable.BindProperty(this._data[index], this.ObjWrapper);
					break;
				default:
					bindable.bindingPath = this._data[index].propertyPath;
					BindingExtensionsHelper.Bind(element, this.ObjWrapper, null);
					break;
			}
		}

		private int _oldSelectIndex = -1;

		private void ListViewSelectionChanged(List<object> obj) {
			this.ButtonStateChange();
			if (this.onSelectionChanged == null) return;
			int selectIndex = this.SelectedIndex;
			if (this._oldSelectIndex == selectIndex) return;
			if (0 <= selectIndex && selectIndex < this._data.Count) {
				this.onSelectionChanged(this._data[selectIndex]);
			} else {
				this.onSelectionChanged(null);
			}

			this._oldSelectIndex = selectIndex;
		}

		public override void RemoveBinding() {
			base.RemoveBinding();
			this.Refresh();
			this._arraySizeField.SetValueWithoutNotify(-1);
			this.SelectedIndex = -1;
		}

		public new class UxmlFactory : UxmlFactory<CustomListView, UxmlTraits> { }

		public new class UxmlTraits : CustomBindableElement.UxmlTraits {
			private readonly UxmlStringAttributeDescription _title = new UxmlStringAttributeDescription {
				name = "title",
				defaultValue = ""
			};
			private readonly UxmlIntAttributeDescription _itemHeight = new UxmlIntAttributeDescription {
				name = "item-height",
				defaultValue = 30
			};

			private readonly UxmlBoolAttributeDescription _canSelect = new UxmlBoolAttributeDescription {
				name = "can-select",
				defaultValue = true
			};

			private readonly UxmlBoolAttributeDescription _canReorder = new UxmlBoolAttributeDescription {
				name = "can-reorder",
				defaultValue = true
			};

			private readonly UxmlBoolAttributeDescription _canResize = new UxmlBoolAttributeDescription {
				name = "can-resize",
				defaultValue = true
			};

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
				base.Init(ve, bag, cc);
				if (!(ve is CustomListView customListView)) return;
				customListView.Title = this._title.GetValueFromBag(bag, cc);
				customListView.ItemHeight = this._itemHeight.GetValueFromBag(bag, cc);
				customListView.CanSelect = this._canSelect.GetValueFromBag(bag, cc);
				customListView.CanReorder = this._canReorder.GetValueFromBag(bag, cc);
				customListView.CanResize = this._canResize.GetValueFromBag(bag, cc);
			}
		}
	}
}