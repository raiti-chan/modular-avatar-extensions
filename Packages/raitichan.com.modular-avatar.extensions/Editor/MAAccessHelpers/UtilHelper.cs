using System;
using System.Reflection;
using raitichan.com.modular_avatar.extensions.Editor.ControllerFactories;
using UnityEditor.Animations;

namespace raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers {
	public static class UtilHelper {
		private const string ASSEMBLY_NAME = "nadena.dev.modular-avatar.core.editor";
		private const string NAMESPACE = "nadena.dev.modular_avatar.core.editor";
		private const string CLASS_NAME = NAMESPACE + ".Util";
		private const string TYPE_NAME = CLASS_NAME + ", " + ASSEMBLY_NAME;


		private const string CREATE_ANIMATOR_METHOD_NAME = "CreateAnimator";

		private static Func<AnimatorController, AnimatorController> _createAnimatorFunction;

		public static AnimatorController CreateAnimator(AnimatorController toClone = null) {
			if (_createAnimatorFunction != null) {
				return _createAnimatorFunction.Invoke(toClone);
			}

			Type type = Type.GetType(TYPE_NAME);
			if (type == null) throw new NullReferenceException($"Notfound Type : {TYPE_NAME}");

			MethodInfo methodInfo =
				type.GetMethod(CREATE_ANIMATOR_METHOD_NAME, BindingFlags.Static | BindingFlags.Public);
			if (methodInfo == null) throw new NullReferenceException($"NotFound Method : {CREATE_ANIMATOR_METHOD_NAME}");

			_createAnimatorFunction = ExpressionTreeUtils.CreateMethodCallFunction<AnimatorController, AnimatorController>(methodInfo);
			return _createAnimatorFunction.Invoke(toClone);
		}
	}
}