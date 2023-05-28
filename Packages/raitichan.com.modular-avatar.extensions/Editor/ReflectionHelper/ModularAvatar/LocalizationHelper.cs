using System;
using System.Reflection;
using raitichan.com.modular_avatar.extensions.ReflectionHelper;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.ModularAvatar {
	public static class LocalizationHelper {
		private const string ASSEMBLY_NAME = "nadena.dev.modular-avatar.core.editor";
		private const string NAMESPACE = "nadena.dev.modular_avatar.core.editor";
		private const string CLASS_NAME = NAMESPACE + ".Localization";
		private const string TYPE_NAME = CLASS_NAME + ", " + ASSEMBLY_NAME;

		private static Type _type;
		private static Type _Type {
			get {
				if (_type != null) return _type;
				Type type = Type.GetType(TYPE_NAME) ?? throw new NullReferenceException($"Notfound Type : {TYPE_NAME}");
				_type = type;
				return _type;
			}
		}


		private const string GET_SELECTED_LOCALIZATION_METHOD_NAME = "GetSelectedLocalization";
		private static Func<string> _getSelectedLocalizationFunction;

		public static string GetSelectedLocalization() {
			if (_getSelectedLocalizationFunction != null) {
				return _getSelectedLocalizationFunction.Invoke();
			}

			MethodInfo methodInfo = _Type.GetMethod(GET_SELECTED_LOCALIZATION_METHOD_NAME, BindingFlags.Static | BindingFlags.Public);
			if (methodInfo == null) throw new NullReferenceException($"NotFound Method : {SHOW_LANGUAGE_UI_METHOD_NAME}");

			_getSelectedLocalizationFunction = ExpressionTreeUtils.CreateStaticMethodCallFunction<string>(methodInfo);
			return _getSelectedLocalizationFunction.Invoke();
		}

		private const string SHOW_LANGUAGE_UI_METHOD_NAME = "ShowLanguageUI";
		private static Action _showLanguageUiFunction;

		public static void ShowLanguageUI() {
			if (_showLanguageUiFunction != null) {
				_showLanguageUiFunction.Invoke();
				return;
			}

			MethodInfo methodInfo = _Type.GetMethod(SHOW_LANGUAGE_UI_METHOD_NAME, BindingFlags.Static | BindingFlags.Public);
			if (methodInfo == null) throw new NullReferenceException($"NotFound Method : {SHOW_LANGUAGE_UI_METHOD_NAME}");

			_showLanguageUiFunction = ExpressionTreeUtils.CreateStaticMethodCallAction(methodInfo);
			_showLanguageUiFunction.Invoke();
		}
	}
}