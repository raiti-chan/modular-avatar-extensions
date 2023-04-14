using raitichan.com.modular_avatar.extensions.Editor.ControllerFactories;
using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.ModularAvatar;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExEarIdleAnimatorGenerator))]
	public class EarIdleAnimatorGeneratorEditor : MAExEditorBase {
		private MAExEarIdleAnimatorGenerator _target;
		private SerializedProperty _leftEarProperty;
		private SerializedProperty _rightEarProperty;
		private SerializedProperty _multiplierProperty;
		private SerializedProperty _animationSourceProperty;

		private void OnEnable() {
			this._target = this.target as MAExEarIdleAnimatorGenerator;
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

			if (GUILayout.Button("Export Animation Clip")) {
				string path = EditorUtility.SaveFilePanelInProject("Export Animation Clip", "EarIdleAnimation", "anim", "");
				if (!string.IsNullOrEmpty(path)) {
					using (this.serializedObject.CreateReadScope()) {
						AnimationClip source = this._target.animationSource;
						Transform leftEar = this._target.leftEar;
						Transform rightEar = this._target.rightEar;
						float multiplier = this._target.multiplier;
						AnimationClip animationClip = EarIdleAnimatorFactory.CreateClip(source, leftEar, rightEar, multiplier);
						AssetDatabase.CreateAsset(animationClip, path);
						AssetDatabase.Refresh();
					}
				}
			}

			serializedObject.ApplyModifiedProperties();

			LocalizationHelper.ShowLanguageUI();
		}
	}
}