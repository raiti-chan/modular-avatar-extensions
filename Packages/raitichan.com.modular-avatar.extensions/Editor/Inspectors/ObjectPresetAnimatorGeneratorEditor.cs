using System;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.Serializable;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static raitichan.com.modular_avatar.extensions.Modules.MAExObjectPresetAnimatorGenerator;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExObjectPresetAnimatorGenerator))]
	public class ObjectPresetAnimatorGeneratorEditor : MAExEditorBase {
		private MAExObjectPresetAnimatorGenerator _target;
		private VisualElement _innerRoot;
		private InspectorMode _inspectorMode;

		private void OnEnable() {
			this._target = this.target as MAExObjectPresetAnimatorGenerator;
			this._inspectorMode = InspectorMode.Preset;
			this.OnPresetInspectorEnable();
		}

		private void OnDisable() {
			switch (this._inspectorMode) {
				case InspectorMode.Preset:
					this.OnPresetInspectorDisable();
					break;
				case InspectorMode.Toggle:
					this.OnToggleInspectorDisable();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected override VisualElement CreateInnerInspectorGUI() {
			this._innerRoot = new VisualElement();

			string uxmlPath = AssetDatabase.GUIDToAssetPath("7c6128fee4512a448a11e88ed6fb572c");
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this._innerRoot);
			this._innerRoot.Add(base.CreateInnerInspectorGUI());

			this._innerRoot.Q<Button>("PresetButton").clicked += () => this.ChangeInspectorMode(InspectorMode.Preset);
			this._innerRoot.Q<Button>("ToggleButton").clicked += () => this.ChangeInspectorMode(InspectorMode.Toggle);

			this._innerRoot.Q<VisualElement>("PresetElement").Add(new IMGUIContainer(this.OnPresetInspectorGUI));
			this._innerRoot.Q<VisualElement>("ToggleElement").Add(new IMGUIContainer(this.OnToggleInspectorGUI));

			return this._innerRoot;
		}

		private void ChangeInspectorMode(InspectorMode mode) {
			if (this._inspectorMode == mode) return;
			this._inspectorMode = mode;
			switch (mode) {
				case InspectorMode.Preset:
					this.OnToggleInspectorDisable();
					this.OnPresetInspectorEnable();
					break;
				case InspectorMode.Toggle:
					this.OnPresetInspectorDisable();
					this.OnToggleInspectorEnable();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}

			VisualElement presetElement = this._innerRoot.Q<VisualElement>("PresetElement");
			presetElement.style.display = mode == InspectorMode.Preset ? DisplayStyle.Flex : DisplayStyle.None;

			VisualElement toggleElement = this._innerRoot.Q<VisualElement>("ToggleElement");
			toggleElement.style.display = mode == InspectorMode.Toggle ? DisplayStyle.Flex : DisplayStyle.None;
		}

		private SerializedProperty _defaultPresetProperty;

		private ReorderableList _dataReorderableList;
		private int _selectedDataIndex;
		private int _SelectedDataIndex {
			get => Mathf.Max(this._selectedDataIndex, 0);
			set => this._selectedDataIndex = value;
		}
		private bool _changeSelectedData;
		private SerializedProperty _currentDataProperty;
		private SerializedProperty _dataDisplayNameProperty;
		private SerializedProperty _dataMenuIconProperty;

		private void CheckDataReorderableListSelectChange() {
			int newSelectedPresetDataIndex = Mathf.Min(this._dataReorderableList.index, this._dataReorderableList.count - 1);
			if (newSelectedPresetDataIndex != this._selectedDataIndex) {
				this._SelectedDataIndex = newSelectedPresetDataIndex;
				this._changeSelectedData = true;
			} else {
				this._changeSelectedData = false;
			}
		}

		private ReorderableList _objectReorderableList;
		private ReorderableList _blendShapeReorderableList;
		private int _selectedBlendShapeIndex;
		private int _SelectedBlendShapeIndex {
			get => Mathf.Max(this._selectedBlendShapeIndex, 0);
			set => this._selectedBlendShapeIndex = value;
		}
		private bool _changeSelectedBlendShape;
		private SerializedProperty _currentBlendShapeProperty;
		private string[] _blendShapeNames;

		private void CheckBlendShapeReorderableList() {
			int newSelectedBlendShapeIndex = Mathf.Min(this._blendShapeReorderableList.index, this._blendShapeReorderableList.count - 1);
			if (newSelectedBlendShapeIndex != this._selectedBlendShapeIndex) {
				this._SelectedBlendShapeIndex = newSelectedBlendShapeIndex;
				this._changeSelectedBlendShape = true;
			} else {
				this._changeSelectedBlendShape = false;
			}
		}

		private void UpdateBlendShapeNames() {
			SkinnedMeshRenderer renderer =
				this._currentBlendShapeProperty.FindPropertyRelative(nameof(BlendShapeData.skinnedMeshRenderer)).objectReferenceValue as SkinnedMeshRenderer;
			if (renderer != null) {
				this._blendShapeNames = new string[renderer.sharedMesh.blendShapeCount];
				for (int i = 0; i < this._blendShapeNames.Length; i++) {
					this._blendShapeNames[i] = renderer.sharedMesh.GetBlendShapeName(i);
				}
			} else {
				this._blendShapeNames = new[] { "None" };
			}
		}

		private ReorderableList _indexAndWeightReorderableList;

		private void OnPresetInspectorEnable() {
			this.CreatePresetInspectorReorderableLists();
			this._SelectedDataIndex = -1;
			this._SelectedBlendShapeIndex = -1;

			this._defaultPresetProperty = this.serializedObject.FindProperty(nameof(MAExObjectPresetAnimatorGenerator.defaultPreset));
			this._dataReorderableList.serializedProperty = this.serializedObject.FindProperty(nameof(MAExObjectPresetAnimatorGenerator.presetData));

			if (this._dataReorderableList.count == 0) return;
			this._currentDataProperty = this._dataReorderableList.serializedProperty.GetArrayElementAtIndex(this._SelectedDataIndex);
			this._dataDisplayNameProperty = this._currentDataProperty.FindPropertyRelative(nameof(PresetData.displayName));
			this._dataMenuIconProperty = this._currentDataProperty.FindPropertyRelative(nameof(PresetData.menuIcon));

			this._objectReorderableList.serializedProperty = this._currentDataProperty.FindPropertyRelative(nameof(PresetData.enableObjects));
			this._blendShapeReorderableList.serializedProperty = this._currentDataProperty.FindPropertyRelative(nameof(PresetData.blendShapes));

			if (this._blendShapeReorderableList.count == 0) return;
			this._currentBlendShapeProperty = this._blendShapeReorderableList.serializedProperty.GetArrayElementAtIndex(this._SelectedBlendShapeIndex);
			this.UpdateBlendShapeNames();

			this._indexAndWeightReorderableList.serializedProperty =
				this._currentBlendShapeProperty.FindPropertyRelative(nameof(BlendShapeData.blendShapeIndexAndWeights));
		}

		private void OnPresetInspectorDisable() {
			this._dataReorderableList = null;
		}

		private void CreatePresetInspectorReorderableLists() {
			// プリセット
			this._dataReorderableList = new ReorderableList(this.serializedObject, null);
			this._dataReorderableList.drawHeaderCallback += rect => {
				GUI.Label(rect, "Presets");
				rect.x = rect.width - 80;
				rect.width = 80;
				GUI.Label(rect, "DefaultPreset");
			};
			this._dataReorderableList.drawElementCallback += (rect, index, active, focused) => {
				SerializedProperty displayNameProperty =
					this._dataReorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative(nameof(PresetData.displayName));
				GUI.Label(rect, displayNameProperty.stringValue);
				rect.x = rect.width + 5;
				rect.width = 15;
				if (EditorGUI.Toggle(rect, index == this._defaultPresetProperty.intValue)) {
					this._defaultPresetProperty.intValue = index;
				}
			};
			this._dataReorderableList.onReorderCallbackWithDetails += (list, index, newIndex) => {
				if (index == this._defaultPresetProperty.intValue) {
					this._defaultPresetProperty.intValue = newIndex;
				}
			};
			this._dataReorderableList.onCanAddCallback += list => list.count < 256;
			this._dataReorderableList.onAddCallback += list => {
				int lastIndex = list.count;
				list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
				SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
				addedProperty.FindPropertyRelative(nameof(PresetData.displayName)).stringValue = "New Preset";
				addedProperty.FindPropertyRelative(nameof(PresetData.menuIcon)).objectReferenceValue = null;
				addedProperty.FindPropertyRelative(nameof(PresetData.enableObjects)).ClearArray();
				addedProperty.FindPropertyRelative(nameof(PresetData.blendShapes)).ClearArray();
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
				if (list.index < 0) list.index = 0;
			};
			this._dataReorderableList.onRemoveCallback += list => {
				list.serializedProperty.DeleteArrayElementAtIndex(list.index);
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
			};

			// 表示オブジェクト
			this._objectReorderableList = new ReorderableList(this.serializedObject, null);
			this._objectReorderableList.drawHeaderCallback += rect => GUI.Label(rect, "Show Objects");
			this._objectReorderableList.drawElementCallback += (rect, index, active, focused) => {
				rect.y += 1.0f;
				rect.height -= 3.0f;
				EditorGUI.PropertyField(rect, this._objectReorderableList.serializedProperty.GetArrayElementAtIndex(index), new GUIContent(""));
			};
			this._objectReorderableList.onAddCallback += list => {
				int lastIndex = list.count;
				list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
				list.serializedProperty.GetArrayElementAtIndex(lastIndex).objectReferenceValue = null;
			};
			this._objectReorderableList.onRemoveCallback += list => {
				list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue = null;
				list.serializedProperty.DeleteArrayElementAtIndex(list.index);
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
			};

			// ブレンドシェイプオブジェクト
			this._blendShapeReorderableList = new ReorderableList(this.serializedObject, null);
			this._blendShapeReorderableList.drawHeaderCallback += rect => GUI.Label(rect, "Blend Shape Target Renderers");
			this._blendShapeReorderableList.drawElementCallback += (rect, index, active, focused) => {
				rect.y += 1.0f;
				rect.height -= 3.0f;
				SerializedProperty skinnedMeshRendererProperty = this._blendShapeReorderableList.serializedProperty.GetArrayElementAtIndex(index)
					.FindPropertyRelative(nameof(BlendShapeData.skinnedMeshRenderer));
				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(rect, skinnedMeshRendererProperty, new GUIContent(""));
				if (EditorGUI.EndChangeCheck()) {
					this.UpdateBlendShapeNames();
				}
			};
			this._blendShapeReorderableList.onAddCallback += list => {
				int lastIndex = list.count;
				list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
				SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
				addedProperty.FindPropertyRelative(nameof(BlendShapeData.skinnedMeshRenderer)).objectReferenceValue = null;
				addedProperty.FindPropertyRelative(nameof(BlendShapeData.blendShapeIndexAndWeights)).ClearArray();
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
				if (list.index < 0) list.index = 0;
			};
			this._blendShapeReorderableList.onRemoveCallback += list => {
				list.serializedProperty.DeleteArrayElementAtIndex(list.index);
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
			};

			// ブレンドシェイプリスト
			this._indexAndWeightReorderableList = new ReorderableList(this.serializedObject, null);
			this._indexAndWeightReorderableList.drawHeaderCallback += rect => GUI.Label(rect, "Blend Shapes");
			this._indexAndWeightReorderableList.drawElementCallback += (rect, index, active, focused) => {
				SerializedProperty indexAndWeightProperty = this._indexAndWeightReorderableList.serializedProperty.GetArrayElementAtIndex(index);
				SerializedProperty indexProperty = indexAndWeightProperty.FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.index));
				SerializedProperty weightProperty = indexAndWeightProperty.FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.weight));
				rect.y += 1.0f;
				rect.height -= 3.0f;
				Rect popupRect = rect;
				popupRect.width = popupRect.width / 3 * 1;
				indexProperty.intValue = EditorGUI.Popup(popupRect, indexProperty.intValue, this._blendShapeNames);
				Rect sliderRect = rect;
				sliderRect.x = popupRect.width + popupRect.x + 10;
				sliderRect.width -= popupRect.width + 10;
				weightProperty.floatValue = EditorGUI.Slider(sliderRect, weightProperty.floatValue, 0.0f, 100.0f);
			};
			this._indexAndWeightReorderableList.onAddCallback += list => {
				int lastIndex = list.count;
				list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
				SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
				addedProperty.FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.index)).intValue = 0;
				addedProperty.FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.weight)).floatValue = 0;
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
			};
			this._indexAndWeightReorderableList.onRemoveCallback += list => {
				list.serializedProperty.DeleteArrayElementAtIndex(list.index);
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
			};
		}

		private void OnPresetInspectorGUI() {
			serializedObject.Update();
			this._dataReorderableList.DoLayoutList();
			if (this._dataReorderableList.count == 0) {
				this.serializedObject.ApplyModifiedProperties();
				return;
			}

			this.CheckDataReorderableListSelectChange();
			// プリセットデータの選択が変更されたらプロパティを更新
			if (this._changeSelectedData) {
				this._currentDataProperty = this._dataReorderableList.serializedProperty.GetArrayElementAtIndex(this._SelectedDataIndex);
				this._dataDisplayNameProperty = this._currentDataProperty.FindPropertyRelative(nameof(PresetData.displayName));
				this._dataMenuIconProperty = this._currentDataProperty.FindPropertyRelative(nameof(PresetData.menuIcon));

				this._objectReorderableList.serializedProperty = this._currentDataProperty.FindPropertyRelative(nameof(PresetData.enableObjects));
				this._blendShapeReorderableList.serializedProperty = this._currentDataProperty.FindPropertyRelative(nameof(PresetData.blendShapes));
				this._blendShapeReorderableList.index = -1;
			}


			GUILayout.Label(this._target.presetData[this._SelectedDataIndex].displayName);
			GUILayout.Button("Preview");
			EditorGUILayout.PropertyField(this._dataDisplayNameProperty);
			EditorGUILayout.PropertyField(this._dataMenuIconProperty);

			this._objectReorderableList.DoLayoutList();
			this._blendShapeReorderableList.DoLayoutList();
			if (this._blendShapeReorderableList.count == 0) {
				this.serializedObject.ApplyModifiedProperties();
				return;
			}

			this.CheckBlendShapeReorderableList();
			// ブレンドシェイプターゲットが変更されたらプロパティを更新
			if (this._changeSelectedBlendShape || this._changeSelectedData) {
				this._currentBlendShapeProperty = this._blendShapeReorderableList.serializedProperty.GetArrayElementAtIndex(this._SelectedBlendShapeIndex);
				this.UpdateBlendShapeNames();

				this._indexAndWeightReorderableList.serializedProperty =
					this._currentBlendShapeProperty.FindPropertyRelative(nameof(BlendShapeData.blendShapeIndexAndWeights));
			}

			GUILayout.Label(this._target.presetData[this._SelectedDataIndex].blendShapes[this._SelectedBlendShapeIndex].skinnedMeshRenderer == null
				? "None Skinned Mesh Renderer"
				: this._target.presetData[this._SelectedDataIndex].blendShapes[this._SelectedBlendShapeIndex].skinnedMeshRenderer.name);

			this._indexAndWeightReorderableList.DoLayoutList();

			serializedObject.ApplyModifiedProperties();
		}


		private void OnToggleInspectorEnable() {
			SerializedProperty toggleSetDataProperty = this.serializedObject.FindProperty(nameof(MAExObjectPresetAnimatorGenerator.toggleSetData));
			this._dataReorderableList = new ReorderableList(this.serializedObject, toggleSetDataProperty);
		}

		private void OnToggleInspectorDisable() {
			this._dataReorderableList = null;
		}

		private void OnToggleInspectorGUI() {
			serializedObject.Update();

			serializedObject.ApplyModifiedProperties();
		}

		private enum InspectorMode {
			Preset,
			Toggle
		}
	}
}