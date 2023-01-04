using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace raitichan.com.modular_avatar.extensions.Editor.UnityUtils {
	public static class SerializedPropertyExtensions {
		public static IEnumerable<SerializedProperty> GetArrayElements(this SerializedProperty target) {
			if (!target.isArray) throw new ArgumentException("Not array property");
			int count = target.arraySize;
			for (int i = 0; i < count; i++) {
				yield return target.GetArrayElementAtIndex(i);
			}
		}

		public static IEnumerable<SerializedProperty> FindPropertyRelative(this IEnumerable<SerializedProperty> target, string propertyName) {
			return target.Select(property => property.FindPropertyRelative(propertyName));
		}

		public static IEnumerable<int> ToIntValues(this IEnumerable<SerializedProperty> target) {
			return target.Select(property => property.intValue);
		}

		public static IEnumerable<Object> ToObjectReferenceValues(this IEnumerable<SerializedProperty> target) {
			return target.Select(property => property.objectReferenceValue);
		}
		
		
		
	}
}