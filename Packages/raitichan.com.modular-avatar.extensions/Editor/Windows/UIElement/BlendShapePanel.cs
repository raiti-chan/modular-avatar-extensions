using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.ModularAvatar;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class BlendShapePanel : VisualElement {
		private const string UXML_GUID = "8e4dfd5912b768b4f996e76c6f47b96c";

		private readonly TwoPaneSplitViewOld _splitView;
		private readonly CustomListView _blendShapeListView;
		private readonly CustomBindableElement _blendShapeView;

		private readonly CustomListView _weightListView;

		public int ControlLayer { get; set; } = PresetPreviewContext.PRESET_LAYER;
		
		public float SplitViewDimension {
			get => this._splitView.FixedPaneCurrentDimension;
			set => this._splitView.FixedPaneInitialDimension = value;
		}

		public BlendShapePanel() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._splitView = this.Q<TwoPaneSplitViewOld>("SplitView");

			this._blendShapeListView = this._splitView.Q<CustomListView>("BlendShapeListView");
			this._blendShapeListView.OnAdd = BlendShapeListViewOnAdd;
			this._blendShapeListView.OnRemove = this.BlendShapeListViewOnRemove;
			this._blendShapeListView.MakeItem = BlendShapeListViewMakeItem;
			this._blendShapeListView.onSelectionChanged += BlendShapeListViewOnSelectionChanged;

			this._blendShapeView = this._splitView.Q<CustomBindableElement>("BlendShapeView");
			this._weightListView = this._blendShapeView.Q<CustomListView>("WeightListView");
			this._weightListView.OnAdd = this.WeightListViewOnAdd;
			this._weightListView.OnRemove = this.WeightListViewOnRemove;
			this._weightListView.MakeItem = () => new WeightListElement();
			this._weightListView.RegisterCallback<BlendShapeChangeEvent>(this.WeightListViewOnChangeWeight);
		}

		private void SendBlendShapeChangeEvent(SkinnedMeshRenderer skinnedMeshRenderer, string blendShapeName, float weigh) {
			using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewBlendShapeChangeWeightCommand(skinnedMeshRenderer, blendShapeName, this.ControlLayer, weigh))) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}

		private void SendBlendShapeResetEvent(SkinnedMeshRenderer skinnedMeshRenderer, string blendShapeName) {
			using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewBlendShapeResetWeightCommand(skinnedMeshRenderer, blendShapeName, this.ControlLayer))) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}

		private static void BlendShapeListViewOnAdd(SerializedProperty serializedProperty) {
			Component targetObject = serializedProperty.serializedObject.targetObject as Component;
			if (targetObject == null) return;
			foreach (GameObject gameObject in GameObjectTreeViewWindow.ShowModalWindow(RuntimeUtilHelper.FindAvatarInParents(targetObject.transform).transform)) {
				SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
				if (skinnedMeshRenderer == null) continue;
				if (serializedProperty.GetArrayElements()
				    .FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.skinnedMeshRenderer))
				    .ToObjectReferenceValues()
				    .Contains(skinnedMeshRenderer)) continue;
				int lastIndex = serializedProperty.arraySize;
				serializedProperty.InsertArrayElementAtIndex(lastIndex);
				SerializedProperty blendShapeProperty = serializedProperty.GetArrayElementAtIndex(lastIndex);
				blendShapeProperty.FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.skinnedMeshRenderer)).objectReferenceValue = skinnedMeshRenderer;
				blendShapeProperty.FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.weights)).ClearArray();
			}

			serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		private void BlendShapeListViewOnRemove(SerializedProperty serializedProperty, int index) {
			SerializedProperty blendShapeProperty = serializedProperty.GetArrayElementAtIndex(index);
			SkinnedMeshRenderer skinnedMeshRenderer = blendShapeProperty.FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.skinnedMeshRenderer)).objectReferenceValue as SkinnedMeshRenderer;
			string[] blendShapeNames = blendShapeProperty.FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.weights)).GetArrayElements()
				.FindPropertyRelative(nameof(MAExObjectPreset.BlendShapeWeight.key))
				.ToStringValues()
				.ToArray();
			serializedProperty.DeleteArrayElementAtIndex(index);
			serializedProperty.serializedObject.ApplyModifiedProperties();
			foreach (string blendShapeName in blendShapeNames) {
				this.SendBlendShapeResetEvent(skinnedMeshRenderer, blendShapeName);
			}

			this._blendShapeListView.SelectedIndex = index - 1;
		}

		private static VisualElement BlendShapeListViewMakeItem() {
			CustomBindableElement root = new CustomBindableElement() {
				style = {
					flexGrow = 1,
					justifyContent = Justify.Center
				}
			};

			root.Add(new ReadOnlyObjectField {
				style = {
					marginLeft = 30
				},
				objectType = typeof(SkinnedMeshRenderer),
				bindingPath = nameof(MAExObjectPreset.BlendShape.skinnedMeshRenderer)
			});

			return root;
		}

		private void BlendShapeListViewOnSelectionChanged(SerializedProperty selected) {
			if (selected != null) {
				this._blendShapeView.BindProperty(selected, this._blendShapeListView.ObjWrapper);
			} else {
				this._blendShapeView.Unbind();
			}
		}


		private void WeightListViewOnAdd(SerializedProperty serializedProperty) {
			SkinnedMeshRenderer skinnedMeshRenderer = this._blendShapeListView.SelectedProperty
				.FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.skinnedMeshRenderer))
				.objectReferenceValue as SkinnedMeshRenderer;
			if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null) return;

			GenericMenu menu = new GenericMenu();
			foreach (string blendShapeName in skinnedMeshRenderer.sharedMesh.GetBlendShapeNames()) {
				if (serializedProperty.GetArrayElements()
				    .FindPropertyRelative(nameof(MAExObjectPreset.BlendShapeWeight.key))
				    .ToStringValues()
				    .Contains(blendShapeName)) {
					menu.AddDisabledItem(new GUIContent(blendShapeName));
				} else {
					menu.AddItem(new GUIContent(blendShapeName), false, () => {
						int lastIndex = serializedProperty.arraySize;
						serializedProperty.InsertArrayElementAtIndex(lastIndex);
						SerializedProperty addedProperty = serializedProperty.GetArrayElementAtIndex(lastIndex);
						addedProperty.FindPropertyRelative(nameof(MAExObjectPreset.BlendShapeWeight.key)).stringValue = blendShapeName;
						addedProperty.FindPropertyRelative(nameof(MAExObjectPreset.BlendShapeWeight.value)).floatValue = 0;
						serializedProperty.serializedObject.ApplyModifiedProperties();
						this.SendBlendShapeChangeEvent(skinnedMeshRenderer, blendShapeName, 0);
					});
				}
			}

			menu.DropDown(this._weightListView.AddDropDownPos);
		}

		private void WeightListViewOnRemove(SerializedProperty serializedProperty, int selectIndex) {
			string blendShapeName = serializedProperty.GetArrayElementAtIndex(selectIndex).FindPropertyRelative(nameof(MAExObjectPreset.BlendShapeWeight.key)).stringValue;
			serializedProperty.DeleteArrayElementAtIndex(selectIndex);
			serializedProperty.serializedObject.ApplyModifiedProperties();

			SkinnedMeshRenderer skinnedMeshRenderer = this._blendShapeListView.SelectedProperty
				.FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.skinnedMeshRenderer))
				.objectReferenceValue as SkinnedMeshRenderer;

			if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null) return;
			this.SendBlendShapeResetEvent(skinnedMeshRenderer, blendShapeName);
		}


		private void WeightListViewOnChangeWeight(BlendShapeChangeEvent evt) {
			SkinnedMeshRenderer skinnedMeshRenderer = this._blendShapeListView.SelectedProperty
				.FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.skinnedMeshRenderer))
				.objectReferenceValue as SkinnedMeshRenderer;

			if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null) return;
			this.SendBlendShapeChangeEvent(skinnedMeshRenderer, evt.BlendShapeName, evt.Weight);
		}

		protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
			base.ExecuteDefaultActionAtTarget(evt);
			if (evt == null) return;
			if (!this._blendShapeListView.IsBound) return;
			if (evt.eventTypeId == EventBase<DragUpdatedEvent>.TypeId()) {
				if (!DragAndDrop.objectReferences.Any(obj =>
					    obj is SkinnedMeshRenderer ||
					    obj is GameObject gameObject && gameObject.GetComponent<SkinnedMeshRenderer>() != null)) return;
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				evt.StopPropagation();
			} else if (evt.eventTypeId == EventBase<DragPerformEvent>.TypeId()) {
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				DragAndDrop.AcceptDrag();
				foreach (Object obj in DragAndDrop.objectReferences) {
					if (!(obj is SkinnedMeshRenderer skinnedMeshRenderer)) {
						if (!(obj is GameObject gameObject)) continue;
						skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
						if (skinnedMeshRenderer == null) continue;
					}

					if (this._blendShapeListView.BindingProperty.GetArrayElements()
					    .FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.skinnedMeshRenderer))
					    .ToObjectReferenceValues()
					    .Contains(skinnedMeshRenderer)) continue;

					int lastIndex = this._blendShapeListView.BindingProperty.arraySize;
					this._blendShapeListView.BindingProperty.InsertArrayElementAtIndex(lastIndex);
					SerializedProperty addedProperty = this._blendShapeListView.BindingProperty.GetArrayElementAtIndex(lastIndex);
					addedProperty.FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.skinnedMeshRenderer)).objectReferenceValue = skinnedMeshRenderer;
					addedProperty.FindPropertyRelative(nameof(MAExObjectPreset.BlendShape.weights)).ClearArray();
				}

				this._blendShapeListView.BindingProperty.serializedObject.ApplyModifiedProperties();
				evt.StopPropagation();
			}
		}

		public new class UxmlFactory : UxmlFactory<BlendShapePanel, UxmlTraits> { }
	}
}