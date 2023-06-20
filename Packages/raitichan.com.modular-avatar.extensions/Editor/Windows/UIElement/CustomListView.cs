using System;
using System.Collections.Generic;
using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class CustomListView : BindableElement {
		private const string UXML_GUID = "75e8cb2f03bd602419dca8d3914c38f0";

		private readonly List<SerializedProperty> _data = new List<SerializedProperty>();
		private SerializedProperty _serializedProperty;
		private Func<VisualElement> _makeItem;
		private Action<SerializedProperty, VisualElement, int> _bindItem;

		private readonly ListView _listView;
		private readonly IntegerField _arraySizeField;

		private readonly ToolbarButton _addButton;
		private readonly ToolbarButton _removeButton;
		private readonly ToolbarButton _upButton;
		private readonly ToolbarButton _downButton;

		public event Action<SerializedProperty> onAdd;
		public event Action<SerializedProperty, int> onRemove;
		public event Action<SerializedProperty, int> onUp;
		public event Action<SerializedProperty, int> onDown;
		public event Action<SerializedProperty> onSelectionChanged;
		public Func<VisualElement> MakeItem {
			get => this._makeItem;
			set {
				if (this._makeItem == value) return;
				this._makeItem = value;
				this.Refresh(false);
			}
		}
		public Action<SerializedProperty, VisualElement, int> BindItem {
			get => this._bindItem;
			set {
				if (this._bindItem == value) return;
				this._bindItem = value;
				this.Refresh(false);
			}
		}

		public int ItemHeight {
			get => this._listView.itemHeight;
			set => this._listView.itemHeight = value;
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
			this._arraySizeField.RegisterValueChangedCallback(evt => this.Refresh(true));

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

		public void Unbind() {
			BindingExtensions.Unbind(this);
			this._serializedProperty = null;
			this._arraySizeField.bindingPath = "Array.size";
			this.Refresh(true);
		}

		public void Refresh(bool updateArray) {
			if (updateArray) {
				this._data.Clear();

				if (this._serializedProperty != null) {
					int size = this._serializedProperty.arraySize;
					for (int i = 0; i < size; i++) {
						this._data.Add(this._serializedProperty.GetArrayElementAtIndex(i));
					}
				}
			}

			this.ButtonStateChange();
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
			if (this.onAdd != null) {
				this.onAdd(this._serializedProperty);
				return;
			}

			this._serializedProperty.InsertArrayElementAtIndex(this._serializedProperty.arraySize);
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		private void RemoveButtonClicked() {
			if (this._serializedProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex < 0 || this._data.Count <= selectIndex) return;

			if (this.onRemove != null) {
				this.onRemove(this._serializedProperty, selectIndex);
				return;
			}

			this._serializedProperty.DeleteArrayElementAtIndex(selectIndex);
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
			this._listView.selectedIndex = -1;
		}

		private void UpButtonClicked() {
			if (this._serializedProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex == 0) return;

			if (this.onUp != null) {
				this.onUp(this._serializedProperty, selectIndex);
				return;
			}

			this._serializedProperty.MoveArrayElement(selectIndex, selectIndex - 1);
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
			this._listView.selectedIndex = selectIndex - 1;
		}

		private void DownButtonClicked() {
			if (this._serializedProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex == this._data.Count - 1) return;

			if (this.onDown != null) {
				this.onDown(this._serializedProperty, selectIndex);
				return;
			}

			this._serializedProperty.MoveArrayElement(selectIndex, selectIndex + 1);
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
			this._listView.selectedIndex = selectIndex + 1;
		}

		private VisualElement ListViewMakeItem() {
			if (this.MakeItem != null) {
				return this.MakeItem();
			}

			return new PropertyField();
		}

		private void ListViewBindItem(VisualElement element, int index) {
			if (this.BindItem != null) {
				this.BindItem(this._serializedProperty, element, index);
				return;
			}

			if (!(element is IBindable bindable)) {
				throw new InvalidOperationException("Can't find BindableElement: please provide BindableVisualElements or provide your own Listview.bindItem callback");
			}

			SerializedProperty parentProperty = this._data[index];
			bindable.BindProperty(parentProperty);
		}

		private void ListViewSelectionChanged(List<object> obj) {
			this.ButtonStateChange();
			if (this.onSelectionChanged == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (0 <= selectIndex && selectIndex < this._data.Count) {
				this.onSelectionChanged(this._data[selectIndex]);
			} else {
				this.onSelectionChanged(null);
			}
		}

		public override void HandleEvent(EventBase evt) {
			if (evt.GetType() == SerializedObjectBindEventHelper.Type) {
				this.Unbind();
			} else if (evt.GetType() == SerializedPropertyBindEventHelper.Type) {
				SerializedPropertyBindEventHelper serializedPropertyBindEvent = new SerializedPropertyBindEventHelper(evt);
				this._serializedProperty = serializedPropertyBindEvent.GetBindProperty();
				this._listView.selectedIndex = -1;
				this.Refresh(true);
			}

			base.HandleEvent(evt);
		}

		public new class UxmlFactory : UxmlFactory<CustomListView, UxmlTraits> { }

		public new class UxmlTraits : BindableElement.UxmlTraits {
			private readonly UxmlIntAttributeDescription _itemHeight = new UxmlIntAttributeDescription() {
				name = "item-height",
				defaultValue = 30
			};

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
				base.Init(ve, bag, cc);
				if (!(ve is CustomListView customListView)) return;
				customListView.ItemHeight = this._itemHeight.GetValueFromBag(bag, cc);
			}
		}
	}
}