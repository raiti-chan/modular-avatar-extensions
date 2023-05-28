using System;
using System.Reflection;
using raitichan.com.modular_avatar.extensions.ReflectionHelper;
using Object = UnityEngine.Object;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.ModularAvatar {
	public static class InspectorCommonHelper {
		private const string ASSEMBLY_NAME = "nadena.dev.modular-avatar.core.editor";
		private const string NAMESPACE = "nadena.dev.modular_avatar.core.editor";
		private const string CLASS_NAME = NAMESPACE + ".InspectorCommon";
		private const string TYPE_NAME = CLASS_NAME + ", " + ASSEMBLY_NAME;

		
		private const string DISPLAY_OUT_OF_AVATAR_WARNING_METHOD_NAME = "DisplayOutOfAvatarWarning";

		private static Action<Object[]> _displayOutOfAvatarWarningFunction;
		public static void DisplayOutOfAvatarWarning(Object[] targets) {
			if (_displayOutOfAvatarWarningFunction != null) {
				_displayOutOfAvatarWarningFunction.Invoke(targets);
				return;
			}
			
			Type type = Type.GetType(TYPE_NAME);
			if (type == null) throw new NullReferenceException($"Notfound Type : {TYPE_NAME}");
			
			MethodInfo methodInfo =
				type.GetMethod(DISPLAY_OUT_OF_AVATAR_WARNING_METHOD_NAME, BindingFlags.Static | BindingFlags.NonPublic);
			if (methodInfo == null) throw new NullReferenceException($"NotFound Method : {DISPLAY_OUT_OF_AVATAR_WARNING_METHOD_NAME}");

			_displayOutOfAvatarWarningFunction = ExpressionTreeUtils.CreateStaticMethodCallAction<Object[]>(methodInfo);
			_displayOutOfAvatarWarningFunction.Invoke(targets);
		}
	}
}