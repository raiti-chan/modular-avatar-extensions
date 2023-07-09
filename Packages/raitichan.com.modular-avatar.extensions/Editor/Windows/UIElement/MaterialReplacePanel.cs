using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.ModularAvatar;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class MaterialReplacePanel : VisualElement {
		private const string UXML_GUID = "8ddbf42a15e949c4c8a31a60b51c90bb";

		private readonly TwoPaneSplitView _splitView;
		private readonly CustomListView _materialReplaceListView;
		private readonly CustomBindableElement _materialReplaceView;

		public int ControlLayer { get; set; } = PresetPreviewContext.PRESET_LAYER;
		
		public float SplitViewDimension {
			get => this._splitView.FixedPaneCurrentDimension;
			set => this._splitView.FixedPaneInitialDimension = value;
		}

		public MaterialReplacePanel() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._splitView = this.Q<TwoPaneSplitView>("SplitView");

			this._materialReplaceListView = this._splitView.Q<CustomListView>("MaterialReplaceListView");
			this._materialReplaceListView.OnAdd = MaterialReplaceListViewOnAdd;
			this._materialReplaceListView.OnRemove = MaterialReplaceListViewOnRemove;
			this._materialReplaceListView.MakeItem = MaterialReplaceListViewMakeItem;
			this._materialReplaceListView.onSelectionChanged += MaterialReplaceListViewOnSelectionChanged;

			this._materialReplaceView = this._splitView.Q<CustomBindableElement>("MaterialReplaceView");
			CustomListView materialListView = this._materialReplaceView.Q<CustomListView>("MaterialListView");
			materialListView.MakeItem = () => new MaterialReplaceElement();
			materialListView.BindItem = MaterialListViewBindItem;
			materialListView.RegisterCallback<MaterialChangeEvent>(MaterialListViewOnChangeMaterial);
		}
		
		private void SendMaterialChangeEvent(Renderer renderer, int materialIndex, Material material) {
			using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewMaterialChangeCommand(renderer, materialIndex, this.ControlLayer, material))) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}

		private void SendMaterialResetEvent(Renderer renderer, int materialIndex) {
			using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewMaterialResetCommand(renderer, materialIndex, this.ControlLayer))) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}
		
		private static void MaterialReplaceListViewOnAdd(SerializedProperty serializedProperty) {
			Component targetObject = serializedProperty.serializedObject.targetObject as Component;
			if (targetObject == null) return;
			foreach (GameObject gameObject in GameObjectTreeViewWindow.ShowModalWindow(RuntimeUtilHelper.FindAvatarInParents(targetObject.transform).transform)) {
				Renderer renderer = gameObject.GetComponent<Renderer>();
				if (renderer == null) continue;
				AddMaterialReplace(serializedProperty, renderer);
			}

			serializedProperty.serializedObject.ApplyModifiedProperties();
		}
		
		
		private void MaterialReplaceListViewOnRemove(SerializedProperty serializedProperty, int index) {
			SerializedProperty materialReplaceProperty = serializedProperty.GetArrayElementAtIndex(index);
			Renderer renderer = materialReplaceProperty.FindPropertyRelative(nameof(MAExObjectPreset.MaterialReplace.renderer)).objectReferenceValue as Renderer;
			int[] materialIndexes = materialReplaceProperty.FindPropertyRelative(nameof(MAExObjectPreset.MaterialReplace.materials)).GetArrayElements()
				.ToObjectReferenceValues()
				.Select((obj, i) => (obj, i))
				.Where(t => t.obj != null)
				.Select(t => t.i)
				.ToArray();
			serializedProperty.DeleteArrayElementAtIndex(index);
			serializedProperty.serializedObject.ApplyModifiedProperties();
			foreach (int materialIndex in materialIndexes) {
				this.SendMaterialResetEvent(renderer, materialIndex);
			}
			
			this._materialReplaceListView.SelectedIndex = index - 1;
		}

		private static VisualElement MaterialReplaceListViewMakeItem() {
			CustomBindableElement root = new CustomBindableElement {
				style = {
					flexGrow = 1,
					justifyContent = Justify.Center
				}
			};

			root.Add(new ReadOnlyObjectField {
				style = {
					marginLeft = 30
				},
				objectType = typeof(Renderer),
				bindingPath = nameof(MAExObjectPreset.MaterialReplace.renderer)
			});
			return root;
		}


		private void MaterialReplaceListViewOnSelectionChanged(SerializedProperty selected) {
			if (selected != null) {
				this._materialReplaceView.BindProperty(selected, this._materialReplaceListView.ObjWrapper);
			} else {
				this._materialReplaceView.Unbind();
			}
		}


		private void MaterialListViewBindItem(SerializedProperty serializedProperty, VisualElement element, int index) {
			if (!(element is MaterialReplaceElement materialReplaceElement)) return;
			Renderer renderer = this._materialReplaceListView.SelectedProperty.FindPropertyRelative(nameof(MAExObjectPreset.MaterialReplace.renderer)).objectReferenceValue as Renderer;
			if (renderer == null) return;
			materialReplaceElement.MaterialIndex = index;
			materialReplaceElement.OriginField.SetValueWithoutNotify(renderer.sharedMaterials[index]);
			SerializedProperty materialProperty = serializedProperty.GetArrayElementAtIndex(index);
			materialReplaceElement.ReplaceField.SetValueWithoutNotify(materialProperty.objectReferenceValue);
			materialReplaceElement.ReplaceField.BindProperty(materialProperty);
		}
		
		
		private void MaterialListViewOnChangeMaterial(MaterialChangeEvent evt) {
			Renderer renderer = this._materialReplaceListView.SelectedProperty
				.FindPropertyRelative(nameof(MAExObjectPreset.MaterialReplace.renderer))
				.objectReferenceValue as Renderer;
			if (renderer == null) return;
			if (evt.Material == null) {
				this.SendMaterialResetEvent(renderer, evt.MaterialIndex);
			} else {
				this.SendMaterialChangeEvent(renderer, evt.MaterialIndex, evt.Material);
			}
		}

		protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
			base.ExecuteDefaultActionAtTarget(evt);
			if (evt == null) return;
			if (!this._materialReplaceListView.IsBound) return;
			if (evt.eventTypeId == EventBase<DragUpdatedEvent>.TypeId()) {
				if (!DragAndDrop.objectReferences.Any(obj =>
					    obj is Renderer ||
					    obj is GameObject gameObject && gameObject.GetComponent<Renderer>() != null)) return;
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				evt.StopPropagation();
			} else if (evt.eventTypeId == EventBase<DragPerformEvent>.TypeId()) {
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				DragAndDrop.AcceptDrag();
				foreach (Object obj in DragAndDrop.objectReferences) {
					if (!(obj is Renderer renderer)) {
						if (!(obj is GameObject gameObject)) continue;
						renderer = gameObject.GetComponent<Renderer>();
						if (renderer == null) continue;
					}
					AddMaterialReplace(this._materialReplaceListView.BindingProperty,renderer);
				}

				this._materialReplaceListView.BindingProperty.serializedObject.ApplyModifiedProperties();
				evt.StopPropagation();
			}
		}

		private static void AddMaterialReplace(SerializedProperty serializedProperty, Renderer renderer) {
			if (serializedProperty.GetArrayElements()
			    .FindPropertyRelative(nameof(MAExObjectPreset.MaterialReplace.renderer))
			    .ToObjectReferenceValues()
			    .Contains(renderer)) return;
			int lastIndex = serializedProperty.arraySize;
			serializedProperty.InsertArrayElementAtIndex(lastIndex);
			SerializedProperty addedProperty = serializedProperty.GetArrayElementAtIndex(lastIndex);
			addedProperty.FindPropertyRelative(nameof(MAExObjectPreset.MaterialReplace.renderer)).objectReferenceValue = renderer;
			SerializedProperty materialsProperty = addedProperty.FindPropertyRelative(nameof(MAExObjectPreset.MaterialReplace.materials));
			materialsProperty.ClearArray();
			for (int i = 0; i < renderer.sharedMaterials.Length; i++) {
				materialsProperty.InsertArrayElementAtIndex(0);
			}
		}

		public new class UxmlFactory : UxmlFactory<MaterialReplacePanel> { }
	}
}