using System;
using System.Reflection;
using raitichan.com.modular_avatar.extensions.ReflectionHelper;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity {
	public static class BaseFieldHelper<TValueType> {
		private const string VISUAL_INPUT_PROPERTY_NAME = "visualInput";
		private static Func<BaseField<TValueType>, VisualElement> _visualInputFunction;

		public static VisualElement VisualInput(BaseField<TValueType> target) {
			if (_visualInputFunction != null) {
				return _visualInputFunction.Invoke(target);
			}

			PropertyInfo propertyInfo = typeof(BaseField<TValueType>).GetProperty(VISUAL_INPUT_PROPERTY_NAME, BindingFlags.Instance | BindingFlags.NonPublic);
			if (propertyInfo == null) throw new NullReferenceException($"NotFound Property : {VISUAL_INPUT_PROPERTY_NAME}");
			_visualInputFunction = ExpressionTreeUtils.CreateInstanceValueGetFunction<BaseField<TValueType>, VisualElement>(propertyInfo);
			return _visualInputFunction.Invoke(target);
		}
	}
}