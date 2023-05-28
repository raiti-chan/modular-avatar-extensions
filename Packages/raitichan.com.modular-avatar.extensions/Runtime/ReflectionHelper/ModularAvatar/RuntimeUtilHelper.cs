using System;
using System.Reflection;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.ReflectionHelper.ModularAvatar {
	public static class RuntimeUtilHelper {
		private const string ASSEMBLY_NAME = "nadena.dev.modular-avatar.core";
		private const string NAMESPACE = "nadena.dev.modular_avatar.core";
		private const string CLASS_NAME = NAMESPACE + ".RuntimeUtil";
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
		
		
		private const string FIND_AVATAR_IN_PARENTS_METHOD_NAME = "FindAvatarInParents";

		private static Func<Transform, VRCAvatarDescriptor> _findAvatarInParentsFunction;
		public static VRCAvatarDescriptor FindAvatarInParents(Transform target) {
			if (_findAvatarInParentsFunction != null) {
				return _findAvatarInParentsFunction.Invoke(target);
			}

			MethodInfo methodInfo = _Type.GetMethod(FIND_AVATAR_IN_PARENTS_METHOD_NAME, BindingFlags.Static | BindingFlags.Public);
			if (methodInfo == null) throw new NullReferenceException($"NotFound Method : {FIND_AVATAR_IN_PARENTS_METHOD_NAME}");

			_findAvatarInParentsFunction = ExpressionTreeUtils.CreateStaticMethodCallFunction<Transform, VRCAvatarDescriptor>(methodInfo);
			return _findAvatarInParentsFunction.Invoke(target);
		}
	}
}