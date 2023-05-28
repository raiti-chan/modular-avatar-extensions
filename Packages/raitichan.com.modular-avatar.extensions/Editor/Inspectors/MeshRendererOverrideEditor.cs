using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	[CustomEditor(typeof(MAExMeshRendererOverride))]
	public class MeshRendererOverrideEditor : MAExEditorBase {

		private SerializedProperty _anchorOverrideProperty;
		private SerializedProperty _rootBoneProperty;
		private SerializedProperty _boundsProperty;

		private void OnEnable() {
			this._anchorOverrideProperty = this.serializedObject.FindProperty(nameof(MAExMeshRendererOverride.AnchorOverride));
			this._rootBoneProperty = this.serializedObject.FindProperty(nameof(MAExMeshRendererOverride.RootBone));
			this._boundsProperty = this.serializedObject.FindProperty(nameof(MAExMeshRendererOverride.Bounds));
		}

		protected override void OnInnerInspectorGUI() {
			this.serializedObject.Update();
			EditorGUILayout.PropertyField(this._anchorOverrideProperty);
			EditorGUILayout.PropertyField(this._rootBoneProperty);
			EditorGUILayout.PropertyField(this._boundsProperty);
			this.serializedObject.ApplyModifiedProperties();
		}

		[DrawGizmo(GizmoType.Selected)]
		private static void DrawGizmo(MAExMeshRendererOverride component, GizmoType gizmoType) {
			Vector3 center = component.Bounds.center;
			Vector3 size = component.Bounds.size;
			if (component.RootBone != null) {
				center += component.RootBone.position;
				size.Scale(component.RootBone.lossyScale);
			}
			Gizmos.DrawWireCube(center, size);
		}
		
		
	}
}