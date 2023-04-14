using System;
using System.Reflection;
using UnityEditor;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity {
	public static class SerializedObjectHelper {
		private const string IS_VALID_PROPERTY_NAME = "isValid";

		private static Func<SerializedObject, bool> _isValidFunction;

		public static bool IsValid(SerializedObject target) {
			if (_isValidFunction != null) {
				return _isValidFunction.Invoke(target);
			}

			PropertyInfo propertyInfo = typeof(SerializedObject).GetProperty(IS_VALID_PROPERTY_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
			if (propertyInfo == null) throw new NullReferenceException($"NotFound Property : {IS_VALID_PROPERTY_NAME}");
			_isValidFunction = ExpressionTreeUtils.CreateInstanceValueGetFunction<SerializedObject, bool>(propertyInfo);
			return _isValidFunction.Invoke(target);
		}
	}
}