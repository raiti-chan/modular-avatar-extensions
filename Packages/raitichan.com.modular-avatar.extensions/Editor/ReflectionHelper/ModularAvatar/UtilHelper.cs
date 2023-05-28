using System;
using System.Reflection;
using raitichan.com.modular_avatar.extensions.ReflectionHelper;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.ModularAvatar {
	public static class UtilHelper {
		private const string ASSEMBLY_NAME = "nadena.dev.modular-avatar.core.editor";
		private const string NAMESPACE = "nadena.dev.modular_avatar.core.editor";
		private const string CLASS_NAME = NAMESPACE + ".Util";
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

		private const string GENERATE_ASSET_PATH_METHOD_NAME = "GenerateAssetPath";
		private static Func<string> _generateAssetPathFunction;

		public static string GenerateAssetPath() {
			if (_generateAssetPathFunction != null) {
				return _generateAssetPathFunction.Invoke();
			}

			MethodInfo methodInfo = _Type.GetMethod(GENERATE_ASSET_PATH_METHOD_NAME, BindingFlags.Static | BindingFlags.Public);
			if (methodInfo == null) throw new NullReferenceException($"NotFound Method : {GENERATE_ASSET_PATH_METHOD_NAME}");
			_generateAssetPathFunction = ExpressionTreeUtils.CreateStaticMethodCallFunction<string>(methodInfo);
			return _generateAssetPathFunction.Invoke();
		}
	}
}