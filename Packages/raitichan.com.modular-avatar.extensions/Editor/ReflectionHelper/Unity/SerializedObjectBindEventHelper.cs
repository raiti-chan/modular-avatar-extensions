using System;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity {
	public static class SerializedObjectBindEventHelper {
		private const string ASSEMBLY_NAME = "UnityEditor";
		private const string NAMESPACE = "UnityEditor.UIElements";
		private const string CLASS_NAME = NAMESPACE + ".SerializedObjectBindEvent";
		private const string TYPE_NAME = CLASS_NAME + ", " + ASSEMBLY_NAME;

		private static Type _type;
		public static Type Type {
			get {
				if (_type != null) return _type;
				Type type = Type.GetType(TYPE_NAME) ?? throw new NullReferenceException($"Notfound Type : {TYPE_NAME}");
				_type = type;
				return _type;
			}
		}
	}
}