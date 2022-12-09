using nadena.dev.modular_avatar.core.editor;
using raitichan.com.modular_avatar.extensions.Editor.ControllerFactories;
using raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExBlinkAnimatorGenerator))]
	public class BlinkAnimatorGeneratorEditor : MAExEditorBase {
		private SerializedProperty _animatorProperty;
		private SerializedProperty _faceMeshProperty;
		private SerializedProperty _blendShapeIndexProperty;
		private SerializedProperty _animationSourceProperty;

		private void OnEnable() {
			this._faceMeshProperty = this.serializedObject.FindProperty(nameof(MAExBlinkAnimatorGenerator.faceMesh));
			this._blendShapeIndexProperty = this.serializedObject.FindProperty(nameof(MAExBlinkAnimatorGenerator.blendShapeIndex));
			this._animationSourceProperty = this.serializedObject.FindProperty(nameof(MAExBlinkAnimatorGenerator.animationSource));
			
		}

		protected override void OnInnerInspectorGUI() {
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this._faceMeshProperty, new GUIContent("Face Mesh"));
			
			if (this._faceMeshProperty.objectReferenceValue is SkinnedMeshRenderer skinnedMeshRenderer) {
				int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
				string[] blendShapeName = new string[blendShapeCount];
				for (int i = 0; i < blendShapeCount; i++) {
					blendShapeName[i] = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
				}

				this._blendShapeIndexProperty.intValue = EditorGUILayout.Popup("Target BlendShape", this._blendShapeIndexProperty.intValue, blendShapeName);
			} else {
				this._blendShapeIndexProperty.intValue = 0;
			}

			EditorGUILayout.PropertyField(this._animationSourceProperty, new GUIContent("Source Animation"));

			serializedObject.ApplyModifiedProperties();

			LocalizationHelper.ShowLanguageUI();
		}
	}
}