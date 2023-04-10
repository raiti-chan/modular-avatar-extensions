using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Editor.ModuleExtensions;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Editor.Views;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ScriptableObjects;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using static raitichan.com.modular_avatar.extensions.Modules.MAExGestureExpressionAnimationGenerator;
using Object = UnityEngine.Object;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExGestureExpressionAnimationGenerator))]
	public class GestureExpressionAnimationGeneratorEditor : MAExEditorBase {
		private const string _SELECTED_CLASS_NAME = "selected";

		private MAExGestureExpressionAnimationGenerator _target;
		private SerializedProperty _facePathProperty;
		private VRCAvatarDescriptor _descriptor;
		private AvatarPreviewView _avatarPreviewView;

		private LeftAndRight _leftAndRight;
		private Gesture _gesture;
		private InspectorState _inspectorState;

		private Vector2 _blendShapeSlidersScrollPos;

		private float[] _defaultBlendShapeWeights;
		private string[] _blendShapeNames;
		private GestureAnimationSetView _leftAnimationSet;
		private GestureAnimationSetView _rightAnimationSet;

		private SkinnedMeshRenderer _faceMeshRenderer;
		private SkinnedMeshRenderer _previewFaceMeshRenderer;

		private AnimationClip _importTargetAnimationClip;
		private MAExGestureExpressionAnimationPreset _importTargetPreset;


		private void OnEnable() {
			Undo.undoRedoPerformed += this.UpdatePreview;
			// EditorApplication.hierarchyChanged += this.UpdateInspector;

			this._inspectorState = InspectorState.Default;
			this._target = (MAExGestureExpressionAnimationGenerator)this.target;
			this._facePathProperty = this.serializedObject.FindProperty(nameof(MAExGestureExpressionAnimationGenerator.facePath));
			this._descriptor = RuntimeUtil.FindAvatarInParents(this._target.transform);
			if (this._descriptor == null) {
				this._inspectorState = InspectorState.NotFoundAvatar;
				return;
			}

			this._avatarPreviewView = new AvatarPreviewView(this._descriptor);

			if (string.IsNullOrEmpty(this._facePathProperty.stringValue)) {
				SkinnedMeshRenderer faceMeshRenderer = this._descriptor.VisemeSkinnedMesh;
				if (faceMeshRenderer == null) {
					faceMeshRenderer = this._descriptor.customEyeLookSettings.eyelidsSkinnedMesh;
				}

				if (faceMeshRenderer != null) {
					this._facePathProperty.stringValue = VRCUtils.GetPathInAvatar(faceMeshRenderer.transform);
				}
			}

			Transform faceMeshTransform = this._descriptor.transform.Find(this._facePathProperty.stringValue);
			if (faceMeshTransform == null) {
				this._inspectorState = InspectorState.NotFoundFaceMesh;
				return;
			}

			this._faceMeshRenderer = faceMeshTransform.GetComponent<SkinnedMeshRenderer>();
			if (this._faceMeshRenderer == null) {
				this._inspectorState = InspectorState.NotFoundFaceMesh;
				return;
			}

			using (this.serializedObject.CreateReadWriteScope()) {
				int blendShapeCount = this._faceMeshRenderer.sharedMesh.blendShapeCount;

				this._defaultBlendShapeWeights = new float[blendShapeCount];
				this._blendShapeNames = new string[blendShapeCount];
				for (int i = 0; i < this._defaultBlendShapeWeights.Length; i++) {
					this._defaultBlendShapeWeights[i] = this._faceMeshRenderer.GetBlendShapeWeight(i);
					this._blendShapeNames[i] = this._faceMeshRenderer.sharedMesh.GetBlendShapeName(i);
				}
			}

			this._leftAnimationSet = new GestureAnimationSetView(this._blendShapeNames,
				this.serializedObject.FindProperty(nameof(MAExGestureExpressionAnimationGenerator.leftAnimationSet)));
			this._rightAnimationSet = new GestureAnimationSetView(this._blendShapeNames,
				this.serializedObject.FindProperty(nameof(MAExGestureExpressionAnimationGenerator.rightAnimationSet)));

			this._previewFaceMeshRenderer = this._avatarPreviewView.Descriptor.transform
				.Find(VRCUtils.GetPathInAvatar(this._faceMeshRenderer.transform))
				.GetComponent<SkinnedMeshRenderer>();

			this.UpdatePreview();
		}

		private void OnDisable() {
			Undo.undoRedoPerformed -= this.UpdatePreview;
			// EditorApplication.hierarchyChanged -= this.UpdateInspector;

			this._avatarPreviewView?.Dispose();
			this._avatarPreviewView = null;
			this._defaultBlendShapeWeights = null;
			this._blendShapeNames = null;
			this._leftAnimationSet = null;
			this._rightAnimationSet = null;
			this._faceMeshRenderer = null;
			this._previewFaceMeshRenderer = null;
		}

		private void UpdateInspector() {
			this.OnDisable();
			this.OnEnable();
		}

		private VisualElement _innerRoot;

		private Button _leftButton;
		private Button _rightButton;

		private Button _fistButton;
		private Button _handOpenButton;
		private Button _fingerPointButton;
		private Button _victoryButton;
		private Button _rockNRollButton;
		private Button _handGunButton;
		private Button _thumbsUpButton;

		protected override VisualElement CreateInnerInspectorGUI() {
			this._innerRoot = new VisualElement();
			string uxmlPath = AssetDatabase.GUIDToAssetPath("c827ffa1e9297ea419b39a3d9fe8b97b");
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this._innerRoot);

			this._innerRoot.Q<IMGUIContainer>("PreviewView").onGUIHandler = this.OnAvatarPreviewGUI;

			this._leftButton = this._innerRoot.Q<Button>("Left");
			this._leftButton.clicked += () => this.SetLeftAndRightMode(LeftAndRight.Left);
			this._rightButton = this._innerRoot.Q<Button>("Right");
			this._rightButton.clicked += () => this.SetLeftAndRightMode(LeftAndRight.Right);

			this._fistButton = this._innerRoot.Q<Button>("Fist");
			this._fistButton.clicked += () => this.SetGestureMode(Gesture.Fist);
			this._handOpenButton = this._innerRoot.Q<Button>("HandOpen");
			this._handOpenButton.clicked += () => this.SetGestureMode(Gesture.HandOpen);
			this._fingerPointButton = this._innerRoot.Q<Button>("FingerPoint");
			this._fingerPointButton.clicked += () => this.SetGestureMode(Gesture.FingerPoint);
			this._victoryButton = this._innerRoot.Q<Button>("Victory");
			this._victoryButton.clicked += () => this.SetGestureMode(Gesture.Victory);
			this._rockNRollButton = this._innerRoot.Q<Button>("RockNRoll");
			this._rockNRollButton.clicked += () => this.SetGestureMode(Gesture.RockNRoll);
			this._handGunButton = this._innerRoot.Q<Button>("HandGun");
			this._handGunButton.clicked += () => this.SetGestureMode(Gesture.HandGun);
			this._thumbsUpButton = this._innerRoot.Q<Button>("ThumbsUp");
			this._thumbsUpButton.clicked += () => this.SetGestureMode(Gesture.ThumbsUp);

			this._innerRoot.Add(base.CreateInnerInspectorGUI());
			return this._innerRoot;
		}

		private void OnAvatarPreviewGUI() {
			this.serializedObject.Update();

			switch (this._inspectorState) {
				case InspectorState.Default:
					DrawFaceMeshField();
					break;
				case InspectorState.NotFoundAvatar:
					DrawFaceMeshPathFiled();
					break;
				case InspectorState.NotFoundFaceMesh:
					EditorGUILayout.HelpBox("I can't find the face mesh.\nSpecify a facial mesh for the component to work properly.", MessageType.Error);
					DrawFaceMeshField();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			this.serializedObject.ApplyModifiedProperties();
			if (this._avatarPreviewView == null) return;
			if (this._avatarPreviewView.OnGUILayout(100)) {
				this.Repaint();
			}

			void DrawFaceMeshField() {
				EditorGUI.BeginChangeCheck();
				this._faceMeshRenderer =
					EditorGUILayout.ObjectField("Face Mesh", this._faceMeshRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
				if (!EditorGUI.EndChangeCheck()) return;
				this._facePathProperty.stringValue = this._faceMeshRenderer == null ? "" : VRCUtils.GetPathInAvatar(this._faceMeshRenderer.transform);
				this.UpdateInspector();
			}

			void DrawFaceMeshPathFiled() {
				EditorGUILayout.PropertyField(this._facePathProperty, new GUIContent("Face Mesh Path"));
			}
		}

		protected override void OnInnerInspectorGUI() {
			if (this._inspectorState != InspectorState.Default) return;
			this.serializedObject.Update();

			Queue<int> updateQueue = new Queue<int>();
			switch (this._leftAndRight) {
				case LeftAndRight.Left:
					this._leftAnimationSet.OnGUI(this._gesture, updateQueue, ref this._blendShapeSlidersScrollPos);
					break;
				case LeftAndRight.Right:
					this._rightAnimationSet.OnGUI(this._gesture, updateQueue, ref this._blendShapeSlidersScrollPos);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (this._previewFaceMeshRenderer == null) {
				updateQueue.Clear();
			} else if (updateQueue.Count > 0) {
				using (this.serializedObject.CreateReadScope()) {
					GestureAnimation currentGestureAnimation = this._target.GetGestureAnimation(this._leftAndRight, this._gesture);
					while (updateQueue.Count > 0) {
						int updateIndex = updateQueue.Dequeue();
						BlendShapeWeight blendShapeWeight = currentGestureAnimation.blendShapeWeights[updateIndex];
						float weight = blendShapeWeight.enable ? blendShapeWeight.weight : this._defaultBlendShapeWeights[updateIndex];
						this._previewFaceMeshRenderer.SetBlendShapeWeight(updateIndex, weight);
					}
				}
			}

			// Reset Button
			if (GUILayout.Button("Reset Animation")) {
				bool result = EditorUtility.DisplayDialog("Animation Reset", "Are you sure you want to reset? Undo is possible.", "Yes", "Cansel");
				if (result) {
					using (this.serializedObject.CreateReadWriteScope()) {
						Undo.RecordObject(this._target, "Reset Animation");
						this._target.GetGestureAnimation(this._leftAndRight, this._gesture).Reset();
					}

					this.UpdatePreview();
				}
			}

			GUILayout.BeginHorizontal();
			this._importTargetAnimationClip = EditorGUILayout.ObjectField(this._importTargetAnimationClip, typeof(AnimationClip), false) as AnimationClip;
			EditorGUI.BeginDisabledGroup(this._importTargetAnimationClip == null);
			if (GUILayout.Button("Import AnimationClip", GUILayout.Width(200)) && this._importTargetAnimationClip != null) {
				using (this.serializedObject.CreateReadWriteScope()) {
					Undo.RecordObject(this._target, "Import AnimationClip");
					this._target.ImportAnimationClip(this._importTargetAnimationClip, this._leftAndRight, this._gesture);
				}

				Debug.Log(MAExGestureExpressionAnimationGeneratorExtension.GetLastLog());
				this.UpdatePreview();
				EditorUtility.DisplayDialog("Import AnimationClip", $"Import Animation from {this._importTargetAnimationClip.name}", "OK");
				this._importTargetAnimationClip = null;
			}

			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Export AnimationClip")) {
				string path = EditorUtility.SaveFilePanelInProject("Export AnimationClip",
					$"{this._leftAndRight}_{this._gesture}_Expression", "anim", "Message?");
				if (!string.IsNullOrEmpty(path)) {
					using (this.serializedObject.CreateReadScope()) {
						AnimationClip animationClip = this._target.ExportAnimationClip(this._leftAndRight, this._gesture);
						AssetDatabase.CreateAsset(animationClip, path);
						AssetDatabase.Refresh();
					}
				}
			}

			GUILayout.BeginHorizontal();
			this._importTargetPreset = EditorGUILayout.ObjectField(this._importTargetPreset,
				typeof(MAExGestureExpressionAnimationPreset), false) as MAExGestureExpressionAnimationPreset;
			EditorGUI.BeginDisabledGroup(this._importTargetPreset == null);
			if (GUILayout.Button("Import Preset", GUILayout.Width(200)) &&
			    this._importTargetPreset != null) {
				using (this.serializedObject.CreateReadWriteScope()) {
					Undo.RecordObject(this._target, "Import Preset");
					this._target.ImportGestureExpressionAnimationPreset(this._importTargetPreset);
				}

				this.UpdatePreview();
				EditorUtility.DisplayDialog("Import Preset",
					$"Import Preset from {this._importTargetPreset.name}", "OK");
				this._importTargetPreset = null;
			}

			EditorGUI.EndDisabledGroup();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Export Preset")) {
				string path = EditorUtility.SaveFilePanelInProject("Export Preset", "GestureExpressionAnimationPreset", "asset", "Message?");
				if (!string.IsNullOrEmpty(path)) {
					using (this.serializedObject.CreateReadScope()) {
						MAExGestureExpressionAnimationPreset preset = this._target.ExportGestureExpressionAnimationPreset();
						AssetDatabase.CreateAsset(preset, path);
						AssetDatabase.Refresh();
					}
				}
			}

			GUILayout.EndHorizontal();

			this.serializedObject.ApplyModifiedProperties();
		}

		private void SetLeftAndRightMode(LeftAndRight leftAndRight) {
			this._leftAndRight = leftAndRight;
			this._leftButton.RemoveFromClassList(_SELECTED_CLASS_NAME);
			this._rightButton.RemoveFromClassList(_SELECTED_CLASS_NAME);

			switch (this._leftAndRight) {
				case LeftAndRight.Left:
					this._leftButton.AddToClassList("selected");
					break;
				case LeftAndRight.Right:
					this._rightButton.AddToClassList("selected");
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(leftAndRight), leftAndRight, null);
			}

			this.UpdatePreview();
		}

		private void SetGestureMode(Gesture gesture) {
			this._gesture = gesture;
			this._fistButton.RemoveFromClassList(_SELECTED_CLASS_NAME);
			this._handOpenButton.RemoveFromClassList(_SELECTED_CLASS_NAME);
			this._fingerPointButton.RemoveFromClassList(_SELECTED_CLASS_NAME);
			this._victoryButton.RemoveFromClassList(_SELECTED_CLASS_NAME);
			this._rockNRollButton.RemoveFromClassList(_SELECTED_CLASS_NAME);
			this._handGunButton.RemoveFromClassList(_SELECTED_CLASS_NAME);
			this._thumbsUpButton.RemoveFromClassList(_SELECTED_CLASS_NAME);

			switch (this._gesture) {
				case Gesture.Fist:
					this._fistButton.AddToClassList(_SELECTED_CLASS_NAME);
					break;
				case Gesture.HandOpen:
					this._handOpenButton.AddToClassList(_SELECTED_CLASS_NAME);
					break;
				case Gesture.FingerPoint:
					this._fingerPointButton.AddToClassList(_SELECTED_CLASS_NAME);
					break;
				case Gesture.Victory:
					this._victoryButton.AddToClassList(_SELECTED_CLASS_NAME);
					break;
				case Gesture.RockNRoll:
					this._rockNRollButton.AddToClassList(_SELECTED_CLASS_NAME);
					break;
				case Gesture.HandGun:
					this._handGunButton.AddToClassList(_SELECTED_CLASS_NAME);
					break;
				case Gesture.ThumbsUp:
					this._thumbsUpButton.AddToClassList(_SELECTED_CLASS_NAME);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			this.UpdatePreview();
		}

		private void UpdatePreview() {
			if (this._inspectorState != InspectorState.Default) return;
			GestureAnimation currentGestureAnimation = this._target.GetGestureAnimation(this._leftAndRight, this._gesture);
			int i = 0;
			for (; i < this._defaultBlendShapeWeights.Length; i++) {
				if (currentGestureAnimation?.blendShapeWeights == null) break;
				if (currentGestureAnimation.blendShapeWeights.Length <= i) break;
				BlendShapeWeight blendShapeWeight = currentGestureAnimation.blendShapeWeights[i];
				float weight = blendShapeWeight.enable ? blendShapeWeight.weight : this._defaultBlendShapeWeights[i];
				this._previewFaceMeshRenderer.SetBlendShapeWeight(i, weight);
			}

			for (; i < this._defaultBlendShapeWeights.Length; i++) {
				this._previewFaceMeshRenderer.SetBlendShapeWeight(i, this._defaultBlendShapeWeights[i]);
			}
		}

		private class GestureAnimationSetView {
			private readonly GestureAnimationView _fistGestureAnimationView;
			private readonly GestureAnimationView _handOpenGestureAnimationView;
			private readonly GestureAnimationView _fingerPointGestureAnimationView;
			private readonly GestureAnimationView _victoryGestureAnimationView;
			private readonly GestureAnimationView _rockNRollGestureAnimationView;
			private readonly GestureAnimationView _handGunGestureAnimationView;
			private readonly GestureAnimationView _thumbsUpGestureAnimationView;

			public GestureAnimationSetView(IReadOnlyList<string> blendShapeNames, SerializedProperty gestureAnimationSetProperty) {
				this._fistGestureAnimationView = new GestureAnimationView(blendShapeNames,
					gestureAnimationSetProperty.FindPropertyRelative(nameof(GestureAnimationSet.fistAnimation)));
				this._handOpenGestureAnimationView = new GestureAnimationView(blendShapeNames,
					gestureAnimationSetProperty.FindPropertyRelative(nameof(GestureAnimationSet.handOpenAnimation)));
				this._fingerPointGestureAnimationView = new GestureAnimationView(blendShapeNames,
					gestureAnimationSetProperty.FindPropertyRelative(nameof(GestureAnimationSet.fingerPointAnimation)));
				this._victoryGestureAnimationView = new GestureAnimationView(blendShapeNames,
					gestureAnimationSetProperty.FindPropertyRelative(nameof(GestureAnimationSet.victoryAnimation)));
				this._rockNRollGestureAnimationView = new GestureAnimationView(blendShapeNames,
					gestureAnimationSetProperty.FindPropertyRelative(nameof(GestureAnimationSet.rockNRollAnimation)));
				this._handGunGestureAnimationView = new GestureAnimationView(blendShapeNames,
					gestureAnimationSetProperty.FindPropertyRelative(nameof(GestureAnimationSet.handGunAnimation)));
				this._thumbsUpGestureAnimationView = new GestureAnimationView(blendShapeNames,
					gestureAnimationSetProperty.FindPropertyRelative(nameof(GestureAnimationSet.thumbsUpAnimation)));
			}

			public void OnGUI(Gesture gesture, Queue<int> updateQueue, ref Vector2 scrollPos) {
				switch (gesture) {
					case Gesture.Fist:
						this._fistGestureAnimationView.OnGUI(updateQueue, ref scrollPos);
						break;
					case Gesture.HandOpen:
						this._handOpenGestureAnimationView.OnGUI(updateQueue, ref scrollPos);
						break;
					case Gesture.FingerPoint:
						this._fingerPointGestureAnimationView.OnGUI(updateQueue, ref scrollPos);
						break;
					case Gesture.Victory:
						this._victoryGestureAnimationView.OnGUI(updateQueue, ref scrollPos);
						break;
					case Gesture.RockNRoll:
						this._rockNRollGestureAnimationView.OnGUI(updateQueue, ref scrollPos);
						break;
					case Gesture.HandGun:
						this._handGunGestureAnimationView.OnGUI(updateQueue, ref scrollPos);
						break;
					case Gesture.ThumbsUp:
						this._thumbsUpGestureAnimationView.OnGUI(updateQueue, ref scrollPos);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(gesture), gesture, null);
				}
			}
		}

		private class GestureAnimationView {
			private readonly BlendShapeSliders _blendShapeSliders;
			private readonly AdditionalAnimationClipDataView _additionalAnimationClipDataView;
			private readonly SerializedProperty _isLoopProperty;

			public GestureAnimationView(IReadOnlyList<string> blendShapeNames, SerializedProperty gestureAnimationProperty) {
				this._blendShapeSliders =
					new BlendShapeSliders(blendShapeNames, gestureAnimationProperty.FindPropertyRelative(nameof(GestureAnimation.blendShapeWeights)));
				this._additionalAnimationClipDataView =
					new AdditionalAnimationClipDataView(gestureAnimationProperty.FindPropertyRelative(nameof(GestureAnimation.additionalAnimationClipData)));
				this._isLoopProperty = gestureAnimationProperty.FindPropertyRelative(nameof(GestureAnimation.isLoop));
			}

			public void OnGUI(Queue<int> updateQueue, ref Vector2 scrollPos) {
				this._blendShapeSliders.OnGUI(updateQueue, ref scrollPos);
				EditorGUILayout.PropertyField(this._isLoopProperty);
				this._additionalAnimationClipDataView.OnGUI();
			}
		}

		private class AdditionalAnimationClipDataView {
			private readonly ReorderableList _reorderableList;

			public AdditionalAnimationClipDataView(SerializedProperty additionalAnimationClipDataProperty) {
				this._reorderableList = new ReorderableList(additionalAnimationClipDataProperty.serializedObject, additionalAnimationClipDataProperty);
				this._reorderableList.drawHeaderCallback += this.DrawHeader;
				this._reorderableList.drawElementCallback += this.DrawElement;
				this._reorderableList.onAddCallback += OnAdd;
				this._reorderableList.onRemoveCallback += OnRemove;
			}

			public void OnGUI() {
				this._reorderableList.DoLayoutList();
			}

			private void DrawHeader(Rect rect) {
				Event e = Event.current;
				if (rect.Contains(e.mousePosition)) {
					// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
					switch (e.type) {
						case EventType.DragUpdated:
							if (!DragAndDrop.objectReferences.Any(o => o is AnimationClip)) break;
							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
							e.Use();
							break;
						case EventType.DragPerform:
							DragAndDrop.AcceptDrag();
							foreach (Object reference in DragAndDrop.objectReferences) {
								if (!(reference is AnimationClip)) continue;
								int lastIndex = this._reorderableList.count;
								this._reorderableList.serializedProperty.InsertArrayElementAtIndex(lastIndex);
								SerializedProperty addedProperty = this._reorderableList.serializedProperty.GetArrayElementAtIndex(lastIndex);
								addedProperty.FindPropertyRelative(nameof(AdditionalAnimationClipData.animationClip)).objectReferenceValue = reference;
							}

							DragAndDrop.activeControlID = 0;
							e.Use();
							break;
					}
				}
				GUI.Label(rect, "Additional Animation Clips");
				rect.x += rect.width - 55;
				rect.width = 55;
				GUI.Label(rect, "Is Offset");
			}

			private void DrawElement(Rect rect, int index, bool active, bool focused) {
				SerializedProperty additionalAnimationClipDataProperty = this._reorderableList.serializedProperty.GetArrayElementAtIndex(index);
				SerializedProperty animationClipProperty =
					additionalAnimationClipDataProperty.FindPropertyRelative(nameof(AdditionalAnimationClipData.animationClip));
				GUIContent emptyContent = new GUIContent("");
				EditorGUI.PropertyField(rect, animationClipProperty, emptyContent);
			}

			private static void OnAdd(ReorderableList reorderableList) {
				int lastIndex = reorderableList.count;
				reorderableList.serializedProperty.InsertArrayElementAtIndex(lastIndex);
				SerializedProperty addedProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(lastIndex);
				addedProperty.FindPropertyRelative(nameof(AdditionalAnimationClipData.animationClip)).objectReferenceValue = null;
			}

			private static void OnRemove(ReorderableList reorderableList) {
				int lastIndex = reorderableList.count - 1;
				reorderableList.serializedProperty.DeleteArrayElementAtIndex(lastIndex);
			}
		}

		private class BlendShapeSliders {
			private readonly IReadOnlyList<string> _blendShapeNames;
			private readonly SerializedProperty _blendShapeWeightsProperty;
			private BlendShapeSlider[] _blendShapeSliders;

			public BlendShapeSliders(IReadOnlyList<string> blendShapeNames, SerializedProperty blendShapeWeightsProperty) {
				this._blendShapeNames = blendShapeNames;
				this._blendShapeWeightsProperty = blendShapeWeightsProperty;
			}

			public void OnGUI(Queue<int> updateQueue, ref Vector2 scrollPos) {
				if (this._blendShapeSliders == null) this._blendShapeSliders = this.CreateBlendShapeSliders();

				GUILayout.BeginVertical("box", GUILayout.Height(400));
				GUILayout.Label("Blend Shapes");
				scrollPos = GUILayout.BeginScrollView(scrollPos);

				for (int i = 0; i < this._blendShapeSliders.Length; i++) {
					EditorGUI.BeginChangeCheck();
					this._blendShapeSliders[i].OnGUI();
					if (EditorGUI.EndChangeCheck()) {
						updateQueue.Enqueue(i);
					}
				}

				GUILayout.EndScrollView();
				GUILayout.EndVertical();
			}

			private BlendShapeSlider[] CreateBlendShapeSliders() {
				BlendShapeSlider[] blendShapeSliders = new BlendShapeSlider[this._blendShapeNames.Count];
				for (int i = 0; i < blendShapeSliders.Length; i++) {
					if (this._blendShapeWeightsProperty.arraySize <= i) {
						this._blendShapeWeightsProperty.InsertArrayElementAtIndex(i);
						this._blendShapeWeightsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(BlendShapeWeight.enable)).boolValue = false;
						this._blendShapeWeightsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(BlendShapeWeight.weight)).floatValue = 0.0f;
					}

					blendShapeSliders[i] = new BlendShapeSlider(this._blendShapeNames[i], this._blendShapeWeightsProperty.GetArrayElementAtIndex(i));
				}

				return blendShapeSliders;
			}
		}

		private class BlendShapeSlider {
			private readonly string _blendShapeName;

			private readonly SerializedProperty _enableProperty;
			private readonly SerializedProperty _weightProperty;

			public BlendShapeSlider(string blendShapeName, SerializedProperty blendShapeWeightProperty) {
				this._blendShapeName = blendShapeName;
				this._enableProperty = blendShapeWeightProperty.FindPropertyRelative(nameof(BlendShapeWeight.enable));
				this._weightProperty = blendShapeWeightProperty.FindPropertyRelative(nameof(BlendShapeWeight.weight));
			}

			public void OnGUI() {
				GUILayout.Space(2);
				GUILayout.BeginHorizontal();
				this._enableProperty.boolValue = EditorGUILayout.Toggle(this._enableProperty.boolValue, GUILayout.Width(20));
				this._weightProperty.floatValue = EditorGUILayout.Slider(this._blendShapeName, this._weightProperty.floatValue, 0, 100);
				GUILayout.EndHorizontal();
				GUILayout.Space(2);
			}
		}

		private enum InspectorState {
			Default, // 通常状態
			NotFoundAvatar,
			NotFoundFaceMesh, // 顔メッシュが見つからない
		}
	}
}