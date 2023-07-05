using raitichan.com.modular_avatar.extensions.Editor.Windows;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExObjectPreset))]
	public class ObjectPresetEditor : MAExEditorBase {
		private MAExObjectPreset _target;

		private void OnEnable() {
			this._target = this.target as MAExObjectPreset;
		}

		protected override void OnInnerInspectorGUI() {
			using (new EditorGUI.DisabledScope(!ObjectPresetEditorWindow.CanShowWindow(this._target))) {
				if (GUILayout.Button("Show Editor Window")) {
					ObjectPresetEditorWindow.ShowWindow(this._target);
				}
			}
		}
	}
}