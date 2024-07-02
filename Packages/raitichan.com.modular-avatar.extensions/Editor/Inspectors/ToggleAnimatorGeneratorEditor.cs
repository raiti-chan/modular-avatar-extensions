using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExToggleAnimatorGenerator))]
	public class ToggleAnimatorGeneratorEditor : MAExEditorBase {
		private SerializedProperty _parameterNameProperty;
		private SerializedProperty _displayNameProperty;
		private SerializedProperty _isInvertProperty;
		private SerializedProperty _isInternalProperty;
		private SerializedProperty _savedProperty;
		private SerializedProperty _defaultValueProperty;
		private SerializedProperty _blendShapeDataListProperty;
		private SerializedProperty _additionalToggleObjectsProperty;
		private SerializedProperty _additionalInvertToggleObjectsProperty;

		private void OnEnable() {
			this._parameterNameProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.parameterName));
			this._displayNameProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.displayName));
			this._isInvertProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.isInvert));
			this._isInternalProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.isInternal));
			this._savedProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.saved));
			this._defaultValueProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.defaultValue));
			this._blendShapeDataListProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.blendShapeDataList));
			this._additionalToggleObjectsProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.additionalToggleObjects));
			this._additionalInvertToggleObjectsProperty = this.serializedObject.FindProperty(nameof(MAExToggleAnimatorGenerator.additionalInvertToggleObjects));
		}

		protected override void OnInnerInspectorGUI() {
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this._parameterNameProperty);
			EditorGUILayout.PropertyField(this._displayNameProperty);
			EditorGUILayout.PropertyField(this._isInvertProperty);
			EditorGUILayout.PropertyField(this._isInternalProperty);
			EditorGUILayout.PropertyField(this._savedProperty);
			EditorGUILayout.PropertyField(this._defaultValueProperty);
			// TODO: ブレンドシェイプのUIを作る
			EditorGUILayout.PropertyField(this._blendShapeDataListProperty);
			EditorGUILayout.PropertyField(this._additionalToggleObjectsProperty);
			EditorGUILayout.PropertyField(this._additionalInvertToggleObjectsProperty);

			this.serializedObject.ApplyModifiedProperties();
		}
	}
}