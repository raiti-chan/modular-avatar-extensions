using System.Collections.Generic;
using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.ModularAvatar;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class ShowObjectPanel : BindableElement {
		private const string UXML_GUID = "5cfe5c5e85ded69478e6f12dd7eaa598";

		private readonly List<SerializedProperty> _data = new List<SerializedProperty>();

		private readonly IntegerField _arraySizeField;
		private readonly ListView _listView;
		private readonly ToolbarButton _addButton;
		private readonly ToolbarButton _removeButton;
		private readonly ToolbarButton _upButton;
		private readonly ToolbarButton _downButton;

		private SerializedProperty _serializedProperty;

		public ShowObjectPanel() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

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

			this._listView = this.Q<ListView>("ShowObjectList");
			this._listView.itemsSource = this._data;
			this._listView.makeItem = ListViewMakeItem;
			this._listView.bindItem = this.ListViewBindItem;
			this._listView.onSelectionChanged += this.ListViewSelectionChanged;
			this._listView.selectionType = SelectionType.Single;
			this._listView.contentContainer.pickingMode = PickingMode.Ignore;
			this._listView.contentContainer.parent.pickingMode = PickingMode.Ignore;
			if (!(this._listView.contentContainer.parent.parent is ScrollView scrollView)) return;
			scrollView.showVertical = true;
			scrollView.pickingMode = PickingMode.Ignore;
		}

		private void Refresh(bool updateArray) {
			if (updateArray) {
				this._data.Clear();

				if (this._serializedProperty != null) {
					int size = this._serializedProperty.arraySize;
					for (int i = 0; i < size; i++) {
						this._data.Add(this._serializedProperty.GetArrayElementAtIndex(i));
					}
				}

				this.ButtonStateChange();
			}

			this._listView.Refresh();
		}

		public void Unbind() {
			this._serializedProperty = null;
			this.bindingPath = "showObjects";
			this._arraySizeField.bindingPath = "Array.size";
			this.Refresh(true);
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
			int lastIndex = this._serializedProperty.arraySize;
			Component targetObject = this._serializedProperty.serializedObject.targetObject as Component;
			if (targetObject == null) return;
			foreach (GameObject gameObject in GameObjectTreeViewWindow.ShowModalWindow(RuntimeUtilHelper.FindAvatarInParents(targetObject.transform).transform)) {
				if (this._serializedProperty.GetArrayElements().ToObjectReferenceValues().Contains(gameObject)) continue;
				this._serializedProperty.InsertArrayElementAtIndex(lastIndex);
				this._serializedProperty.GetArrayElementAtIndex(lastIndex).objectReferenceValue = gameObject;
				lastIndex++;
			}
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		private void RemoveButtonClicked() {
			if (this._serializedProperty == null) return;
			int selectIndex = this._listView.selectedIndex;
			if (selectIndex < 0 || this._data.Count <= selectIndex) return;
			this._serializedProperty.GetArrayElementAtIndex(selectIndex).objectReferenceValue = null;
			this._serializedProperty.DeleteArrayElementAtIndex(selectIndex);
			this._serializedProperty.serializedObject.ApplyModifiedProperties();
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
		}

		private static VisualElement ListViewMakeItem() {
			VisualElement root = new VisualElement {
				style = {
					flexGrow = 1,
					justifyContent = Justify.Center
				}
			};

			root.Add(new ObjectField {
				objectType = typeof(GameObject)
			});
			return root;
		}

		private void ListViewBindItem(VisualElement element, int i) {
			if (!(element is IBindable bindable)) {
				bindable = element.Query().Where(visualElement => visualElement is IBindable).First() as IBindable;
			}

			if (bindable == null) {
				throw new System.InvalidOperationException("Can't find BindableElement: please provide BindableVisualElements or provide your own Listview.bindItem callback");
			}

			SerializedProperty parentProperty = this._data[i];
			bindable.BindProperty(parentProperty);
		}

		protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
			base.ExecuteDefaultActionAtTarget(evt);
			if (evt == null) return;
			if (this._serializedProperty == null) return;
			if (evt.eventTypeId == EventBase<DragUpdatedEvent>.TypeId()) {
				if (!DragAndDrop.objectReferences.Any(obj => obj is GameObject)) return;
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				evt.StopPropagation();
			} else if (evt.eventTypeId == EventBase<DragPerformEvent>.TypeId()) {
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				DragAndDrop.AcceptDrag();
				foreach (Object obj in DragAndDrop.objectReferences) {
					if (!(obj is GameObject)) continue;
					if (this._serializedProperty.GetArrayElements().ToObjectReferenceValues().Contains(obj)) continue;
					int lastIndex = this._serializedProperty.arraySize;
					this._serializedProperty.InsertArrayElementAtIndex(lastIndex);
					SerializedProperty addedProperty = this._serializedProperty.GetArrayElementAtIndex(lastIndex);
					addedProperty.objectReferenceValue = obj;
				}

				this._serializedProperty.serializedObject.ApplyModifiedProperties();
				evt.StopPropagation();
			} else if (evt.eventTypeId == EventBase<DragLeaveEvent>.TypeId()) { }
		}

		public override void HandleEvent(EventBase evt) {
			if (evt.GetType() == SerializedObjectBindEventHelper.Type) {
				this.bindingPath = "showObjects";
				this._arraySizeField.bindingPath = "Array.size";
			} else if (evt.GetType() == SerializedPropertyBindEventHelper.Type) {
				SerializedPropertyBindEventHelper serializedPropertyBindEvent = new SerializedPropertyBindEventHelper(evt);
				this._serializedProperty = serializedPropertyBindEvent.GetBindProperty();
				this._listView.selectedIndex = -1;
				this.Refresh(true);
			}

			base.HandleEvent(evt);
		}

		public new class UxmlFactory : UxmlFactory<ShowObjectPanel, UxmlTraits> { }

		public new class UxmlTraits : BindableElement.UxmlTraits { }
	}
}