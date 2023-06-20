using System;
using System.Reflection;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using raitichan.com.modular_avatar.extensions.ReflectionHelper;
using UnityEditor;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity {
	public class SerializedPropertyBindEventHelper {
		private const string ASSEMBLY_NAME = "UnityEditor";
		private const string NAMESPACE = "UnityEditor.UIElements";
		private const string CLASS_NAME = NAMESPACE + ".SerializedPropertyBindEvent";
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

		private readonly EventBase _eventBase;

		public SerializedPropertyBindEventHelper(EventBase eventBase) {
			if (eventBase.GetType() != Type) {
				throw new InvalidParameterException($"{nameof(eventBase)} is not SerializedPropertyBindEvent");
			}

			this._eventBase = eventBase;
		}

		private const string BIND_PROPERTY_PROPERTY_NAME = "bindProperty";
		private static Func<EventBase,SerializedProperty> _getBindPropertyFunction;

		public SerializedProperty GetBindProperty() {
			if (_getBindPropertyFunction != null) {
				return _getBindPropertyFunction.Invoke(this._eventBase);
			}

			PropertyInfo propertyInfo = Type.GetProperty(BIND_PROPERTY_PROPERTY_NAME, BindingFlags.Instance | BindingFlags.Public);
			if (propertyInfo == null) throw new NullReferenceException($"NotFound Property : {BIND_PROPERTY_PROPERTY_NAME}");

			_getBindPropertyFunction = ExpressionTreeUtils.CreateInstanceValueGetFunction<EventBase, SerializedProperty>(propertyInfo, Type);
			return _getBindPropertyFunction.Invoke(this._eventBase);
		}
	}
}