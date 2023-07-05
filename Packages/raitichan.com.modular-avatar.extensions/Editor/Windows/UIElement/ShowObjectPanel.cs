using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.ModularAvatar;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class ShowObjectPanel : VisualElement {
		private const string UXML_GUID = "5cfe5c5e85ded69478e6f12dd7eaa598";

		private readonly CustomListView _showObjectListView;

		public int ControlLayer { get; set; } = PresetPreviewContext.PRESET_LAYER;

		public ShowObjectPanel() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._showObjectListView = this.Q<CustomListView>("ShowObjectListView");
			this._showObjectListView.OnAdd = this.ShowObjectListViewOnAdd;
			this._showObjectListView.OnRemove = this.ShowObjectListViewOnRemove;
			this._showObjectListView.MakeItem = ShowObjectListViewMakeItem;
		}

		private void SendObjectAddEvent(GameObject gameObject) {
			if (gameObject == null) return;
			if (ControlLayer == PresetPreviewContext.PRESET_LAYER) {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectAddPresetLayerCommand(gameObject))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			} else {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectAddToggleLayerCommand(gameObject, this.ControlLayer))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			}
		}

		private void SendObjectRemoveEvent(GameObject gameObject) {
			if (gameObject == null) return;
			if (ControlLayer == PresetPreviewContext.PRESET_LAYER) {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectRemovePresetLayerCommand(gameObject))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			} else {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectRemoveToggleLayerCommand(gameObject, this.ControlLayer))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			}
		}
		
		private void ShowObjectListViewOnAdd(SerializedProperty serializedProperty) {
			int lastIndex = serializedProperty.arraySize;
			Component targetObject = serializedProperty.serializedObject.targetObject as Component;
			if (targetObject == null) return;
			foreach (GameObject gameObject in GameObjectTreeViewWindow.ShowModalWindow(RuntimeUtilHelper.FindAvatarInParents(targetObject.transform).transform)) {
				if (serializedProperty.GetArrayElements().ToObjectReferenceValues().Contains(gameObject)) continue;
				serializedProperty.InsertArrayElementAtIndex(lastIndex);
				serializedProperty.GetArrayElementAtIndex(lastIndex).objectReferenceValue = gameObject;
				lastIndex++;
				this.SendObjectAddEvent(gameObject);
			}

			serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		private void ShowObjectListViewOnRemove(SerializedProperty serializedProperty, int index) {
			GameObject gameObject = serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue as GameObject;
			serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = null;
			serializedProperty.DeleteArrayElementAtIndex(index);
			serializedProperty.serializedObject.ApplyModifiedProperties();
			this.SendObjectRemoveEvent(gameObject);
		}

		private static VisualElement ShowObjectListViewMakeItem() {
			VisualElement root = new VisualElement {
				style = {
					flexGrow = 1,
					justifyContent = Justify.Center
				}
			};

			root.Add(new ReadOnlyObjectField {
				style = {
					marginLeft = 30
				},
				objectType = typeof(GameObject)
			});
			return root;
		}

		protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
			base.ExecuteDefaultActionAtTarget(evt);
			if (evt == null) return;
			if (!this._showObjectListView.IsBound) return;
			if (evt.eventTypeId == EventBase<DragUpdatedEvent>.TypeId()) {
				if (!DragAndDrop.objectReferences.Any(obj => obj is GameObject)) return;
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				evt.StopPropagation();
			} else if (evt.eventTypeId == EventBase<DragPerformEvent>.TypeId()) {
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				DragAndDrop.AcceptDrag();
				foreach (Object obj in DragAndDrop.objectReferences) {
					if (!(obj is GameObject gameObject)) continue;
					if (this._showObjectListView.BindingProperty.GetArrayElements().ToObjectReferenceValues().Contains(obj)) continue;
					int lastIndex = this._showObjectListView.BindingProperty.arraySize;
					this._showObjectListView.BindingProperty.InsertArrayElementAtIndex(lastIndex);
					SerializedProperty addedProperty = this._showObjectListView.BindingProperty.GetArrayElementAtIndex(lastIndex);
					addedProperty.objectReferenceValue = obj;
					this.SendObjectAddEvent(gameObject);
				}

				this._showObjectListView.BindingProperty.serializedObject.ApplyModifiedProperties();
				evt.StopPropagation();
			}
		}

		public new class UxmlFactory : UxmlFactory<ShowObjectPanel, UxmlTraits> { }
	}
}