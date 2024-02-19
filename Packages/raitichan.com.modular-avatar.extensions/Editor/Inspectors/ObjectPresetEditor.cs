using raitichan.com.modular_avatar.extensions.Editor.Windows;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.ModularAvatar;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

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
		
		[DrawGizmo(GizmoType.Selected)]
		private static void DrawGizmo(MAExObjectPreset component, GizmoType gizmoType) {
			VRCAvatarDescriptor avatar = RuntimeUtilHelper.FindAvatarInParents(component.transform);
			if (avatar == null) return;
			Bounds bounds = CalculateBounds(avatar.gameObject);
			Vector3 center = bounds.center;
			Vector3 size = bounds.size;
			Gizmos.DrawWireCube(center, size);
		}
		
		private static Bounds CalculateBounds(GameObject avatarObj) {
			Bounds rootBounds = new Bounds(avatarObj.transform.position, Vector3.zero);
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in avatarObj.GetComponentsInChildren<SkinnedMeshRenderer>()) {
				Mesh mesh = skinnedMeshRenderer.sharedMesh;
				if (mesh == null) continue;
				Transform centerObject = skinnedMeshRenderer.transform;
				Bounds bounds = mesh.bounds;
				Vector3 worldMin = centerObject.TransformPoint(bounds.min);
				Vector3 worldMax = centerObject.TransformPoint(bounds.max);
				bounds.min = worldMin;
				bounds.max = worldMax;
				rootBounds.Encapsulate(bounds);
			}

			foreach (MeshFilter meshFilter in avatarObj.GetComponentsInChildren<MeshFilter>()) {
				Mesh mesh = meshFilter.sharedMesh;
				if (mesh == null) continue;
				Transform centerObject = meshFilter.transform;
				Bounds bounds = mesh.bounds;
				Vector3 worldMin = centerObject.TransformPoint(bounds.min);
				Vector3 worldMax = centerObject.TransformPoint(bounds.max);
				bounds.min = worldMin;
				bounds.max = worldMax;
				rootBounds.Encapsulate(bounds);
			}

			return rootBounds;
		}
	}
}