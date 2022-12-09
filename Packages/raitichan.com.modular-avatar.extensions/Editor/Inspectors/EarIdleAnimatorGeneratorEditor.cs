using raitichan.com.modular_avatar.extensions.Editor.ControllerFactories;
using raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExEarIdleAnimatorGenerator))]
	public class EarIdleAnimatorGeneratorEditor : MAExEditorBase {
		private SerializedProperty _leftEarProperty;
		private SerializedProperty _rightEarProperty;
		private SerializedProperty _multiplierProperty;
		private SerializedProperty _animationSourceProperty;

		private void OnEnable() {
			this._leftEarProperty = this.serializedObject.FindProperty(nameof(MAExEarIdleAnimatorGenerator.leftEar));
			this._rightEarProperty = this.serializedObject.FindProperty(nameof(MAExEarIdleAnimatorGenerator.rightEar));
			this._multiplierProperty = this.serializedObject.FindProperty(nameof(MAExEarIdleAnimatorGenerator.multiplier));
			this._animationSourceProperty = this.serializedObject.FindProperty(nameof(MAExEarIdleAnimatorGenerator.animationSource));
		}

		protected override void OnInnerInspectorGUI() {
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this._leftEarProperty, new GUIContent("Target Ear L"));
			EditorGUILayout.PropertyField(this._rightEarProperty, new GUIContent("Target Ear R"));
			EditorGUILayout.PropertyField(this._multiplierProperty, new GUIContent("Multiplier"));
			EditorGUILayout.PropertyField(this._animationSourceProperty, new GUIContent("Source Animation"));

			serializedObject.ApplyModifiedProperties();
			
			LocalizationHelper.ShowLanguageUI();
		}
	}
}