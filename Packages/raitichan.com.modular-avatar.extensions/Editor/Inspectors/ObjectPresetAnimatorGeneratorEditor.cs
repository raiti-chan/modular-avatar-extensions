using System;
using System.Linq;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Editor.UnityHelpers;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Editor.Windows;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.Serializable;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static raitichan.com.modular_avatar.extensions.Modules.MAExObjectPresetAnimatorGenerator;
using Object = UnityEngine.Object;

// TODO: いつかリファクタリングする!!!

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExObjectPresetAnimatorGenerator))]
	public class ObjectPresetAnimatorGeneratorEditor : MAExEditorBase {
		private MAExObjectPresetAnimatorGenerator _target;
		private VisualElement _innerRoot;
		private InspectorMode _inspectorMode;

		private bool _isPreviewPreset;
		private bool _isPreviewToggleSet;
		private int _selectedPresetIndex;
		private int _selectedToggleIndex;

		private void OnEnable() {
			this._target = this.target as MAExObjectPresetAnimatorGenerator;
			this._inspectorMode = InspectorMode.Preset;
			this._selectedPresetIndex = -1;
			this._selectedToggleIndex = -1;
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
					this._innerRoot.Q<Button>("PresetButton").AddToClassList("selected-inspector-mode");
					this._innerRoot.Q<Button>("ToggleButton").RemoveFromClassList("selected-inspector-mode");
					break;
				case InspectorMode.Toggle:
					this.OnPresetInspectorDisable();
					this.OnToggleInspectorEnable();
					this._innerRoot.Q<Button>("ToggleButton").AddToClassList("selected-inspector-mode");
					this._innerRoot.Q<Button>("PresetButton").RemoveFromClassList("selected-inspector-mode");
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
		private SerializedProperty _isToggleInvertProperty;

		private ReorderableList _dataReorderableList;
		private int _selectedDataIndex;
		private int _SelectedDataIndex {
			get => Mathf.Max(this._selectedDataIndex, 0);
			set => this._selectedDataIndex = value;
		}
		private bool _changeSelectedData;
		private SerializedProperty _currentDataProperty;
		private SerializedProperty _dataDisplayNameProperty;
		private SerializedProperty _dataParameterNameProperty;
		private SerializedProperty _dataMenuIconProperty;
		private SerializedProperty _dataIsInternalProperty;
		private SerializedProperty _dataSavedProperty;
		private SerializedProperty _dataDefaultValueProperty;

		private void CheckDataReorderableListSelectChange() {
			int newSelectedPresetDataIndex = Mathf.Min(this._dataReorderableList.index, this._dataReorderableList.count - 1);
			if (newSelectedPresetDataIndex != this._selectedDataIndex) {
				this._SelectedDataIndex = newSelectedPresetDataIndex;
				this._changeSelectedData = true;
			} else {
				this._changeSelectedData = false;
			}

			switch (this._inspectorMode) {
				case InspectorMode.Preset:
					this._selectedPresetIndex = this._selectedDataIndex;
					break;
				case InspectorMode.Toggle:
					this._selectedToggleIndex = this._selectedDataIndex;
					break;
				default:
					throw new ArgumentOutOfRangeException();
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
			this._SelectedDataIndex = this._selectedPresetIndex;
			this._SelectedBlendShapeIndex = -1;

			this._defaultPresetProperty = this.serializedObject.FindProperty(nameof(MAExObjectPresetAnimatorGenerator.defaultPreset));
			this._dataReorderableList.serializedProperty = this.serializedObject.FindProperty(nameof(MAExObjectPresetAnimatorGenerator.presetData));
			this._dataReorderableList.index = this._selectedPresetIndex;

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
			this._objectReorderableList = null;
			this._blendShapeReorderableList = null;
			this._indexAndWeightReorderableList = null;

			this._selectedDataIndex = -1;
			this._selectedBlendShapeIndex = -1;

			this._blendShapeNames = null;

			this._currentDataProperty = null;
			this._currentBlendShapeProperty = null;
			this._dataDisplayNameProperty = null;
			this._dataMenuIconProperty = null;
			this._defaultPresetProperty = null;
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
			this._objectReorderableList.drawHeaderCallback += rect => {
				GUI.Label(rect, "Show Objects");
				if (!rect.Contains(Event.current.mousePosition)) return;
				ReorderableList list = this._objectReorderableList;
				// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
				switch (Event.current.type) {
					case EventType.DragUpdated:
						if (!DragAndDrop.objectReferences.Any(referenceObject => referenceObject is GameObject)) break;
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Event.current.Use();
						break;
					case EventType.DragPerform:
						DragAndDrop.AcceptDrag();
						foreach (Object reference in DragAndDrop.objectReferences) {
							if (!(reference is GameObject)) continue;
							if (list.serializedProperty.GetArrayElements().ToObjectReferenceValues().Contains(reference)) continue;

							int lastIndex = list.count;
							list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
							SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
							addedProperty.objectReferenceValue = reference;
						}

						list.serializedProperty.serializedObject.ApplyModifiedProperties();
						DragAndDrop.activeControlID = 0;
						Event.current.Use();
						break;
				}
			};
			this._objectReorderableList.drawElementCallback += (rect, index, active, focused) => {
				rect.y += 1.0f;
				rect.height -= 3.0f;
				EditorGUI.PropertyField(rect, this._objectReorderableList.serializedProperty.GetArrayElementAtIndex(index), new GUIContent(""));
			};
			this._objectReorderableList.onAddCallback += list => {
				int lastIndex = list.count;
				foreach (GameObject gameObject in GameObjectTreeViewWindow.ShowModalWindow(RuntimeUtil.FindAvatarInParents(this._target.transform).transform)) {
					if (list.serializedProperty.GetArrayElements().ToObjectReferenceValues().Contains(gameObject)) continue;
					list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
					list.serializedProperty.GetArrayElementAtIndex(lastIndex).objectReferenceValue = gameObject;
					lastIndex++;
				}
			};
			this._objectReorderableList.onRemoveCallback += list => {
				list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue = null;
				list.serializedProperty.DeleteArrayElementAtIndex(list.index);
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
			};

			// ブレンドシェイプオブジェクト
			this._blendShapeReorderableList = new ReorderableList(this.serializedObject, null);
			this._blendShapeReorderableList.drawHeaderCallback += rect => {
				GUI.Label(rect, "Blend Shape Target Renderers");
				if (!rect.Contains(Event.current.mousePosition)) return;
				ReorderableList list = this._blendShapeReorderableList;
				// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
				switch (Event.current.type) {
					case EventType.DragUpdated:
						if (!DragAndDrop.objectReferences.Any(referenceObject => {
							    SkinnedMeshRenderer skinnedMeshRenderer;
							    if (referenceObject is SkinnedMeshRenderer renderer) {
								    skinnedMeshRenderer = renderer;
							    } else {
								    if (!(referenceObject is GameObject gameObject)) return false;
								    skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
								    if (skinnedMeshRenderer == null) return false;
							    }

							    if (skinnedMeshRenderer.sharedMesh == null) return false;
							    return skinnedMeshRenderer.sharedMesh.blendShapeCount != 0;
						    })) break;

						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Event.current.Use();
						break;
					case EventType.DragPerform:
						DragAndDrop.AcceptDrag();
						foreach (Object reference in DragAndDrop.objectReferences) {
							SkinnedMeshRenderer skinnedMeshRenderer;
							if (reference is SkinnedMeshRenderer renderer) {
								skinnedMeshRenderer = renderer;
							} else {
								if (!(reference is GameObject gameObject)) continue;
								skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
								if (skinnedMeshRenderer == null) continue;
							}

							if (skinnedMeshRenderer.sharedMesh == null) continue;
							if (skinnedMeshRenderer.sharedMesh.blendShapeCount == 0) continue;
							if (list.serializedProperty.GetArrayElements()
							    .FindPropertyRelative(nameof(BlendShapeData.skinnedMeshRenderer))
							    .ToObjectReferenceValues()
							    .Contains(skinnedMeshRenderer)) continue;

							int lastIndex = list.count;
							list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
							SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
							addedProperty.FindPropertyRelative(nameof(BlendShapeData.skinnedMeshRenderer)).objectReferenceValue = skinnedMeshRenderer;
							addedProperty.FindPropertyRelative(nameof(BlendShapeData.blendShapeIndexAndWeights)).ClearArray();
						}

						list.serializedProperty.serializedObject.ApplyModifiedProperties();
						if (list.index < 0) list.index = 0;
						DragAndDrop.activeControlID = 0;
						Event.current.Use();

						break;
				}
			};
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
				rect.width = rect.width / 3 * 1;
				GUI.Label(rect, this._blendShapeNames[indexProperty.intValue]);
				rect.x += rect.width + 10;
				rect.width += rect.width - 10;
				weightProperty.floatValue = EditorGUI.Slider(rect, weightProperty.floatValue, 0.0f, 100.0f);
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
			this._indexAndWeightReorderableList.onAddDropdownCallback += (rect, list) => {
				GenericMenu menu = new GenericMenu();
				for (int i = 0; i < this._blendShapeNames.Length; i++) {
					if (list.serializedProperty.GetArrayElements()
					    .FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.index))
					    .ToIntValues()
					    .Contains(i)) {
						menu.AddDisabledItem(new GUIContent(this._blendShapeNames[i]));
					} else {
						menu.AddItem(new GUIContent(this._blendShapeNames[i]), false, data => {
							int index = (int)data;
							int lastIndex = list.count;
							list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
							SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
							addedProperty.FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.index)).intValue = index;
							addedProperty.FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.weight)).floatValue = 0;
							list.serializedProperty.serializedObject.ApplyModifiedProperties();
						}, i);
					}
				}

				menu.DropDown(rect);
			};
		}

		private void OnPresetInspectorGUI() {
			if (this._inspectorMode != InspectorMode.Preset) return;
			if (!SerializedObjectHelper.IsValid(this.serializedObject)) return;
			this.serializedObject.Update();
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

			GUI.backgroundColor = this._isPreviewPreset ? Color.green : Color.white;
			if (GUILayout.Button(this._isPreviewPreset ? "Return" : "Preview")) {
				this._isPreviewPreset = !this._isPreviewPreset;
			}

			GUI.backgroundColor = Color.white;

			EditorGUILayout.PropertyField(this._dataDisplayNameProperty);
			EditorGUILayout.PropertyField(this._dataMenuIconProperty);

			this._objectReorderableList.DoLayoutList();
			this._blendShapeReorderableList.DoLayoutList();
			if (this._blendShapeReorderableList.count == 0) {
				this.serializedObject.ApplyModifiedProperties();
				return;
			}

			this.CheckBlendShapeReorderableList();
			// ブレンドシェイプターゲットの選択が変更されたらプロパティを更新
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
			this.CreateToggleInspectorReorderableLists();
			this._selectedDataIndex = this._selectedToggleIndex;
			this._SelectedBlendShapeIndex = -1;

			this._isToggleInvertProperty = this.serializedObject.FindProperty(nameof(MAExObjectPresetAnimatorGenerator.isToggleInvert));
			this._dataReorderableList.serializedProperty = this.serializedObject.FindProperty(nameof(MAExObjectPresetAnimatorGenerator.toggleSetData));
			this._dataReorderableList.index = this._selectedToggleIndex;

			if (this._dataReorderableList.count == 0) return;
			this._currentDataProperty = this._dataReorderableList.serializedProperty.GetArrayElementAtIndex(this._SelectedDataIndex);
			this._dataDisplayNameProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.displayName));
			this._dataParameterNameProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.parameterName));
			this._dataMenuIconProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.menuIcon));
			this._dataIsInternalProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.isInternal));
			this._dataSavedProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.saved));
			this._dataDefaultValueProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.defaultValue));

			this._objectReorderableList.serializedProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.toggleObjects));
			this._blendShapeReorderableList.serializedProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.blendShapes));

			if (this._blendShapeReorderableList.count == 0) return;
			this._currentBlendShapeProperty = this._blendShapeReorderableList.serializedProperty.GetArrayElementAtIndex(this._SelectedBlendShapeIndex);
			this.UpdateBlendShapeNames();

			this._indexAndWeightReorderableList.serializedProperty =
				this._currentBlendShapeProperty.FindPropertyRelative(nameof(BlendShapeData.blendShapeIndexAndWeights));
		}

		private void OnToggleInspectorDisable() {
			this._dataReorderableList = null;
			this._objectReorderableList = null;
			this._blendShapeReorderableList = null;
			this._indexAndWeightReorderableList = null;

			this._selectedDataIndex = -1;
			this._selectedBlendShapeIndex = -1;

			this._blendShapeNames = null;

			this._currentDataProperty = null;
			this._currentBlendShapeProperty = null;
			this._dataDisplayNameProperty = null;
			this._dataParameterNameProperty = null;
			this._dataMenuIconProperty = null;
			this._dataIsInternalProperty = null;
			this._dataSavedProperty = null;
			this._dataDefaultValueProperty = null;
			this._isToggleInvertProperty = null;
		}

		private void CreateToggleInspectorReorderableLists() {
			// Toggle グループ
			this._dataReorderableList = new ReorderableList(this.serializedObject, null);
			this._dataReorderableList.drawHeaderCallback += rect => GUI.Label(rect, "Toggle Sets");
			this._dataReorderableList.onAddCallback += list => {
				int lastIndex = list.count;
				list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
				SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
				addedProperty.FindPropertyRelative(nameof(ToggleSetData.displayName)).stringValue = "New Toggle set";
				addedProperty.FindPropertyRelative(nameof(ToggleSetData.parameterName)).stringValue = $"toggle_{lastIndex}";
				addedProperty.FindPropertyRelative(nameof(ToggleSetData.menuIcon)).objectReferenceValue = null;
				addedProperty.FindPropertyRelative(nameof(ToggleSetData.isInternal)).boolValue = true;
				addedProperty.FindPropertyRelative(nameof(ToggleSetData.saved)).boolValue = true;
				addedProperty.FindPropertyRelative(nameof(ToggleSetData.defaultValue)).boolValue = !this._isToggleInvertProperty.boolValue;
				addedProperty.FindPropertyRelative(nameof(ToggleSetData.toggleObjects)).ClearArray();
				addedProperty.FindPropertyRelative(nameof(ToggleSetData.blendShapes)).ClearArray();
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
				if (list.index < 0) list.index = 0;
			};
			this._dataReorderableList.onRemoveCallback += list => {
				list.serializedProperty.DeleteArrayElementAtIndex(list.index);
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
			};

			// 非表示オブジェクト
			this._objectReorderableList = new ReorderableList(this.serializedObject, null);
			this._objectReorderableList.drawHeaderCallback += rect => {
				GUI.Label(rect, "Hide Objects");
				if (!rect.Contains(Event.current.mousePosition)) return;
				ReorderableList list = this._objectReorderableList;
				// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
				switch (Event.current.type) {
					case EventType.DragUpdated:
						if (!DragAndDrop.objectReferences.Any(referenceObject => referenceObject is GameObject)) break;
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Event.current.Use();
						break;
					case EventType.DragPerform:
						DragAndDrop.AcceptDrag();
						foreach (Object reference in DragAndDrop.objectReferences) {
							if (!(reference is GameObject)) continue;
							if (list.serializedProperty.GetArrayElements().ToObjectReferenceValues().Contains(reference)) continue;

							int lastIndex = list.count;
							list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
							SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
							addedProperty.objectReferenceValue = reference;
						}

						list.serializedProperty.serializedObject.ApplyModifiedProperties();
						DragAndDrop.activeControlID = 0;
						Event.current.Use();
						break;
				}
			};
			this._objectReorderableList.drawElementCallback += (rect, index, active, focused) => {
				rect.y += 1.0f;
				rect.height -= 3.0f;
				EditorGUI.PropertyField(rect, this._objectReorderableList.serializedProperty.GetArrayElementAtIndex(index), new GUIContent(""));
			};
			this._objectReorderableList.onAddCallback += list => {
				int lastIndex = list.count;
				foreach (GameObject gameObject in GameObjectTreeViewWindow.ShowModalWindow(RuntimeUtil.FindAvatarInParents(this._target.transform).transform)) {
					if (list.serializedProperty.GetArrayElements().ToObjectReferenceValues().Contains(gameObject)) continue;
					list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
					list.serializedProperty.GetArrayElementAtIndex(lastIndex).objectReferenceValue = gameObject;
					lastIndex++;
				}
			};
			this._objectReorderableList.onRemoveCallback += list => {
				list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue = null;
				list.serializedProperty.DeleteArrayElementAtIndex(list.index);
				list.serializedProperty.serializedObject.ApplyModifiedProperties();
			};

			// ブレンドシェイプオブジェクト
			this._blendShapeReorderableList = new ReorderableList(this.serializedObject, null);
			this._blendShapeReorderableList.drawHeaderCallback += rect => {
				GUI.Label(rect, "Blend Shape Target Renderers");
				if (!rect.Contains(Event.current.mousePosition)) return;
				ReorderableList list = this._blendShapeReorderableList;
				// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
				switch (Event.current.type) {
					case EventType.DragUpdated:
						if (!DragAndDrop.objectReferences.Any(referenceObject => {
							    SkinnedMeshRenderer skinnedMeshRenderer;
							    if (referenceObject is SkinnedMeshRenderer renderer) {
								    skinnedMeshRenderer = renderer;
							    } else {
								    if (!(referenceObject is GameObject gameObject)) return false;
								    skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
								    if (skinnedMeshRenderer == null) return false;
							    }

							    if (skinnedMeshRenderer.sharedMesh == null) return false;
							    return skinnedMeshRenderer.sharedMesh.blendShapeCount != 0;
						    })) break;

						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Event.current.Use();
						break;
					case EventType.DragPerform:
						DragAndDrop.AcceptDrag();
						foreach (Object reference in DragAndDrop.objectReferences) {
							SkinnedMeshRenderer skinnedMeshRenderer;
							if (reference is SkinnedMeshRenderer renderer) {
								skinnedMeshRenderer = renderer;
							} else {
								if (!(reference is GameObject gameObject)) continue;
								skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
								if (skinnedMeshRenderer == null) continue;
							}

							if (skinnedMeshRenderer.sharedMesh == null) continue;
							if (skinnedMeshRenderer.sharedMesh.blendShapeCount == 0) continue;
							if (list.serializedProperty.GetArrayElements()
							    .FindPropertyRelative(nameof(BlendShapeData.skinnedMeshRenderer))
							    .ToObjectReferenceValues()
							    .Contains(skinnedMeshRenderer)) continue;

							int lastIndex = list.count;
							list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
							SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
							addedProperty.FindPropertyRelative(nameof(BlendShapeData.skinnedMeshRenderer)).objectReferenceValue = skinnedMeshRenderer;
							addedProperty.FindPropertyRelative(nameof(BlendShapeData.blendShapeIndexAndWeights)).ClearArray();
						}

						list.serializedProperty.serializedObject.ApplyModifiedProperties();
						if (list.index < 0) list.index = 0;
						DragAndDrop.activeControlID = 0;
						Event.current.Use();

						break;
				}
			};
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
				rect.width = rect.width / 3 * 1;
				GUI.Label(rect, this._blendShapeNames[indexProperty.intValue]);
				rect.x += rect.width + 10;
				rect.width += rect.width - 10;
				weightProperty.floatValue = EditorGUI.Slider(rect, weightProperty.floatValue, 0.0f, 100.0f);
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
			this._indexAndWeightReorderableList.onAddDropdownCallback += (rect, list) => {
				GenericMenu menu = new GenericMenu();
				for (int i = 0; i < this._blendShapeNames.Length; i++) {
					if (list.serializedProperty.GetArrayElements()
					    .FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.index))
					    .ToIntValues()
					    .Contains(i)) {
						menu.AddDisabledItem(new GUIContent(this._blendShapeNames[i]));
					} else {
						menu.AddItem(new GUIContent(this._blendShapeNames[i]), false, data => {
							int index = (int)data;
							int lastIndex = list.count;
							list.serializedProperty.InsertArrayElementAtIndex(lastIndex);
							SerializedProperty addedProperty = list.serializedProperty.GetArrayElementAtIndex(lastIndex);
							addedProperty.FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.index)).intValue = index;
							addedProperty.FindPropertyRelative(nameof(BlendShapeData.BlendShapeIndexAndWeight.weight)).floatValue = 0;
							list.serializedProperty.serializedObject.ApplyModifiedProperties();
						}, i);
					}
				}

				menu.DropDown(rect);
			};
		}

		private void OnToggleInspectorGUI() {
			if (this._inspectorMode != InspectorMode.Toggle) return;
			if (!SerializedObjectHelper.IsValid(this.serializedObject)) return;
			serializedObject.Update();
			EditorGUILayout.PropertyField(this._isToggleInvertProperty);
			this._dataReorderableList.DoLayoutList();
			if (this._dataReorderableList.count == 0) {
				this.serializedObject.ApplyModifiedProperties();
				return;
			}

			this.CheckDataReorderableListSelectChange();
			// プリセットデータの選択が変更されたらプロパティを更新
			if (this._changeSelectedData) {
				this._currentDataProperty = this._dataReorderableList.serializedProperty.GetArrayElementAtIndex(this._SelectedDataIndex);
				this._dataDisplayNameProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.displayName));
				this._dataParameterNameProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.parameterName));
				this._dataMenuIconProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.menuIcon));
				this._dataIsInternalProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.isInternal));
				this._dataSavedProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.saved));
				this._dataDefaultValueProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.defaultValue));

				this._objectReorderableList.serializedProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.toggleObjects));
				this._blendShapeReorderableList.serializedProperty = this._currentDataProperty.FindPropertyRelative(nameof(ToggleSetData.blendShapes));
				this._blendShapeReorderableList.index = -1;
			}

			GUILayout.Label(this._target.toggleSetData[this._SelectedDataIndex].displayName);
			GUI.backgroundColor = this._isPreviewToggleSet ? Color.green : Color.white;
			if (GUILayout.Button(this._isPreviewToggleSet ? "Return" : "Preview")) {
				this._isPreviewToggleSet = !this._isPreviewToggleSet;
			}

			GUI.backgroundColor = Color.white;
			EditorGUILayout.PropertyField(this._dataDisplayNameProperty);
			EditorGUILayout.PropertyField(this._dataParameterNameProperty);
			EditorGUILayout.PropertyField(this._dataMenuIconProperty);
			EditorGUILayout.PropertyField(this._dataIsInternalProperty);
			EditorGUILayout.PropertyField(this._dataSavedProperty);
			EditorGUILayout.PropertyField(this._dataDefaultValueProperty);

			this._objectReorderableList.DoLayoutList();
			this._blendShapeReorderableList.DoLayoutList();
			if (this._blendShapeReorderableList.count == 0) {
				this.serializedObject.ApplyModifiedProperties();
				return;
			}

			this.CheckBlendShapeReorderableList();
			// ブレンドシェイプターゲットの選択が変更されたらプロパティを更新
			if (this._changeSelectedBlendShape || this._changeSelectedData) {
				this._currentBlendShapeProperty = this._blendShapeReorderableList.serializedProperty.GetArrayElementAtIndex(this._SelectedBlendShapeIndex);
				this.UpdateBlendShapeNames();

				this._indexAndWeightReorderableList.serializedProperty =
					this._currentBlendShapeProperty.FindPropertyRelative(nameof(BlendShapeData.blendShapeIndexAndWeights));
			}
			
			GUILayout.Label(this._target.toggleSetData[this._SelectedDataIndex].blendShapes[this._SelectedBlendShapeIndex].skinnedMeshRenderer == null
				? "None Skinned Mesh Renderer"
				: this._target.toggleSetData[this._SelectedDataIndex].blendShapes[this._SelectedBlendShapeIndex].skinnedMeshRenderer.name);

			this._indexAndWeightReorderableList.DoLayoutList();

			serializedObject.ApplyModifiedProperties();
		}


		private enum InspectorMode {
			Preset,
			Toggle
		}
	}
}