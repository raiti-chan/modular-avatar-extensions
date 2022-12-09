using System;
using System.Reflection;
using raitichan.com.modular_avatar.extensions.Editor.ControllerFactories;

namespace raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers {
	public static class LocalizationHelper {
		
		private const string ASSEMBLY_NAME = "nadena.dev.modular-avatar.core.editor";
		private const string NAMESPACE = "nadena.dev.modular_avatar.core.editor";
		private const string CLASS_NAME = NAMESPACE + ".Localization";
		private const string TYPE_NAME = CLASS_NAME + ", " + ASSEMBLY_NAME;

		
		private const string SHOW_LANGUAGE_UI_METHOD_NAME = "ShowLanguageUI";

		private static Action _showLanguageUiFunction;
		public static void ShowLanguageUI() {
			if (_showLanguageUiFunction != null) {
				_showLanguageUiFunction.Invoke();
				return;
			}
			
			Type type = Type.GetType(TYPE_NAME);
			if (type == null) throw new NullReferenceException($"Notfound Type : {TYPE_NAME}");
			
			MethodInfo methodInfo =
				type.GetMethod(SHOW_LANGUAGE_UI_METHOD_NAME, BindingFlags.Static | BindingFlags.Public);
			if (methodInfo == null) throw new NullReferenceException($"NotFound Method : {SHOW_LANGUAGE_UI_METHOD_NAME}");

			_showLanguageUiFunction = ExpressionTreeUtils.CreateMethodCallAction(methodInfo);
			_showLanguageUiFunction.Invoke();
		}
	}
}