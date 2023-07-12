using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.ModularAvatar;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class EnableObjectPanel : VisualElement {
		private const string UXML_GUID = "5cfe5c5e85ded69478e6f12dd7eaa598";

		private readonly CustomListView _enableObjectListView;

		public int ControlLayer { get; set; } = PresetPreviewContext.PRESET_LAYER;

		public EnableObjectPanel() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._enableObjectListView = this.Q<CustomListView>("EnableObjectListView");
			this._enableObjectListView.OnAdd = this.ShowObjectListViewOnAdd;
			this._enableObjectListView.OnRemove = this.ShowObjectListViewOnRemove;
			this._enableObjectListView.MakeItem = () => new EnableObjectElement();
			this._enableObjectListView.RegisterCallback<EnableObjectChangeEvent>(this.EnableObjectListViewOnChangeEnable);
		}

		private void SendObjectAddEvent(GameObject gameObject, bool enable) {
			if (gameObject == null) return;
			if (ControlLayer == PresetPreviewContext.PRESET_LAYER) {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectAddInPresetLayerCommand(gameObject, enable))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			} else {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectAddInToggleLayerCommand(gameObject, this.ControlLayer, enable))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			}
		}

		private void SendObjectRemoveEvent(GameObject gameObject) {
			if (gameObject == null) return;
			if (ControlLayer == PresetPreviewContext.PRESET_LAYER) {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectRemoveInPresetLayerCommand(gameObject))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			} else {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectRemoveInToggleLayerCommand(gameObject, this.ControlLayer))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			}
		}

		private void SendObjectEnableChangeEvent(GameObject gameObject, bool enable) {
			if (gameObject == null) return;
			if (ControlLayer == PresetPreviewContext.PRESET_LAYER) {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectChangeEnableInPresetLayerCommand(gameObject, enable))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			} else {
				using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewObjectChangeEnableInToggleLayerCommand(gameObject, this.ControlLayer, enable))) {
					pooled.target = this;
					this.SendEvent(pooled);
				}
			}
		}
		
		private void ShowObjectListViewOnAdd(SerializedProperty serializedProperty) {
			Component targetObject = serializedProperty.serializedObject.targetObject as Component;
			if (targetObject == null) return;
			foreach (GameObject gameObject in GameObjectTreeViewWindow.ShowModalWindow(RuntimeUtilHelper.FindAvatarInParents(targetObject.transform).transform)) {
				AddEnableObject(serializedProperty, gameObject);
				this.SendObjectAddEvent(gameObject, true);
			}

			serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		private void ShowObjectListViewOnRemove(SerializedProperty serializedProperty, int index) {
			GameObject gameObject = serializedProperty.GetArrayElementAtIndex(index)
				.FindPropertyRelative(nameof(MAExObjectPreset.EnableObject.gameObject)).objectReferenceValue as GameObject;
			serializedProperty.DeleteArrayElementAtIndex(index);
			serializedProperty.serializedObject.ApplyModifiedProperties();
			this.SendObjectRemoveEvent(gameObject);
		}

		private void EnableObjectListViewOnChangeEnable(EnableObjectChangeEvent evt) {
			this.SendObjectEnableChangeEvent(evt.GameObject, evt.Enable);
		}


		protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
			base.ExecuteDefaultActionAtTarget(evt);
			if (evt == null) return;
			if (!this._enableObjectListView.IsBound) return;
			if (evt.eventTypeId == EventBase<DragUpdatedEvent>.TypeId()) {
				if (!DragAndDrop.objectReferences.Any(obj => obj is GameObject)) return;
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				evt.StopPropagation();
			} else if (evt.eventTypeId == EventBase<DragPerformEvent>.TypeId()) {
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				DragAndDrop.AcceptDrag();
				foreach (Object obj in DragAndDrop.objectReferences) {
					if (!(obj is GameObject gameObject)) continue;
					AddEnableObject(this._enableObjectListView.BindingProperty, gameObject);
					this.SendObjectAddEvent(gameObject, true);
				}
				this._enableObjectListView.BindingProperty.serializedObject.ApplyModifiedProperties();
				evt.StopPropagation();
			}
		}

		private static void AddEnableObject(SerializedProperty serializedProperty, GameObject gameObject) {
			if (serializedProperty.GetArrayElements()
			    .FindPropertyRelative(nameof(MAExObjectPreset.EnableObject.gameObject))
			    .ToObjectReferenceValues()
			    .Contains(gameObject)) return;
			int lastIndex = serializedProperty.arraySize;
			serializedProperty.InsertArrayElementAtIndex(lastIndex);
			SerializedProperty addedProperty = serializedProperty.GetArrayElementAtIndex(lastIndex);
			addedProperty.FindPropertyRelative(nameof(MAExObjectPreset.EnableObject.gameObject)).objectReferenceValue = gameObject;
			addedProperty.FindPropertyRelative(nameof(MAExObjectPreset.EnableObject.enable)).boolValue = true;
		}

		public new class UxmlFactory : UxmlFactory<EnableObjectPanel, UxmlTraits> { }
	}
}