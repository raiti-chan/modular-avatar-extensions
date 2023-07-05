using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
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

		public static IEnumerable<string> ToStringValues(this IEnumerable<SerializedProperty> target) {
			return target.Select(property => property.stringValue);
		}

		public static IEnumerable<int> ToIntValues(this IEnumerable<SerializedProperty> target) {
			return target.Select(property => property.intValue);
		}

		public static IEnumerable<Object> ToObjectReferenceValues(this IEnumerable<SerializedProperty> target) {
			return target.Select(property => property.objectReferenceValue);
		}

		public static void SetDefaultValue(this SerializedProperty target) {
			switch (target.propertyType) {
				case SerializedPropertyType.Generic:
					if (target.isArray) {
						target.ClearArray();
					} else {
						SerializedProperty clonedTarget = target.Copy();
						clonedTarget.Next(true);
						int depth = clonedTarget.depth;
						do {
							clonedTarget.SetDefaultValue();
						} while (clonedTarget.Next(false) && clonedTarget.depth == depth);
					}
					break;
				case SerializedPropertyType.Integer:
					target.intValue = default;
					break;
				case SerializedPropertyType.Boolean:
					target.boolValue = default;
					break;
				case SerializedPropertyType.Float:
					target.floatValue = default;
					break;
				case SerializedPropertyType.String:
					target.stringValue = default;
					break;
				case SerializedPropertyType.Color:
					target.colorValue = default;
					break;
				case SerializedPropertyType.ObjectReference:
					target.objectReferenceValue = default;
					break;
				case SerializedPropertyType.LayerMask:
					// ?
					break;
				case SerializedPropertyType.Enum:
					target.enumValueIndex = default;
					break;
				case SerializedPropertyType.Vector2:
					target.vector2Value = default;
					break;
				case SerializedPropertyType.Vector3:
					target.vector3Value = default;
					break;
				case SerializedPropertyType.Vector4:
					target.vector4Value = default;
					break;
				case SerializedPropertyType.Rect:
					target.rectValue = default;
					break;
				case SerializedPropertyType.ArraySize:
					break;
				case SerializedPropertyType.Character:
					target.stringValue = default;
					break;
				case SerializedPropertyType.AnimationCurve:
					target.animationCurveValue = default;
					break;
				case SerializedPropertyType.Bounds:
					target.boundsValue = default;
					break;
				case SerializedPropertyType.Gradient:
					// ?
					break;
				case SerializedPropertyType.Quaternion:
					target.quaternionValue = default;
					break;
				case SerializedPropertyType.ExposedReference:
					target.exposedReferenceValue = default;
					break;
				case SerializedPropertyType.FixedBufferSize:
					break;
				case SerializedPropertyType.Vector2Int:
					target.vector2IntValue = default;
					break;
				case SerializedPropertyType.Vector3Int:
					target.vector3IntValue = default;
					break;
				case SerializedPropertyType.RectInt:
					target.rectIntValue = default;
					break;
				case SerializedPropertyType.BoundsInt:
					target.boundsIntValue = default;
					break;
				case SerializedPropertyType.ManagedReference:
					target.managedReferenceValue = default;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
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

			public void Dispose() { }
		}
	}
}