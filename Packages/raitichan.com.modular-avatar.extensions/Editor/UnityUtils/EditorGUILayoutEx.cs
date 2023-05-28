using UnityEditor;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.UnityUtils {
	public static class EditorGUILayoutEx {
		public static T ObjectField<T>(T value, bool arrowSceneObjects) where T : Object {
			return EditorGUILayout.ObjectField(value, typeof(T), arrowSceneObjects) as T;
		}
	}
}