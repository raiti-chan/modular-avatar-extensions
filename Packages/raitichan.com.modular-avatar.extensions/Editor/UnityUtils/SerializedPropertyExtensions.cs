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

	public static class SerializedObjectExtensions {
		public static ReadAndWriteRawDataScope CreateReadWriteScope(this SerializedObject target) {
			return new ReadAndWriteRawDataScope(target);
		}

		public static ReadRawDataScope CreateReadScope(this SerializedObject target) {
			return new ReadRawDataScope(target);
		}
		
		public class ReadAndWriteRawDataScope : IDisposable {

			private readonly SerializedObject _serializedObject;

			public ReadAndWriteRawDataScope(SerializedObject target) {
				this._serializedObject = target;
				this._serializedObject.ApplyModifiedProperties();
			}

			public void Dispose() {
				this._serializedObject.Update();
			}
		}

		public class ReadRawDataScope : IDisposable {

			public ReadRawDataScope(SerializedObject target) {
				target.ApplyModifiedProperties();
			}

			public void Dispose() {
			}
		}
	}


}