using System;
using raitichan.com.modular_avatar.extensions.Editor.ControllerFactories;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExToggleAnimatorGenerator))]
	public class ToggleAnimatorGeneratorEditor : MAExEditorBase {
		private SerializedProperty _parameterNameProperty;
		private SerializedProperty _isInvertProperty;
		private SerializedProperty _isInternalProperty;
		private SerializedProperty _savedProperty;
		private SerializedProperty _defaultValueProperty;

		private void OnEnable() {
			this._parameterNameProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.parameterName));
			this._isInvertProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.isInvert));
			this._isInternalProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.isInternal));
			this._savedProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.saved));
			this._defaultValueProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.defaultValue));
		}

		protected override void OnInnerInspectorGUI() {
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this._parameterNameProperty);
			EditorGUILayout.PropertyField(this._isInvertProperty);
			EditorGUILayout.PropertyField(this._isInternalProperty);
			EditorGUILayout.PropertyField(this._savedProperty);
			EditorGUILayout.PropertyField(this._defaultValueProperty);

			this.serializedObject.ApplyModifiedProperties();
		}
	}
}