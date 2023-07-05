using System;
using System.Reflection;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
using raitichan.com.modular_avatar.extensions.ReflectionHelper;
using UnityEditor;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity {
	public class SerializedObjectBindEventHelper : IDisposable {
		private const string ASSEMBLY_NAME = "UnityEditor";
		private const string NAMESPACE = "UnityEditor.UIElements";
		private const string CLASS_NAME = NAMESPACE + ".SerializedObjectBindEvent";
		private const string TYPE_NAME = CLASS_NAME + ", " + ASSEMBLY_NAME;
		private static readonly ErrorMessageCreator ERROR_MESSAGE_CREATOR = new ErrorMessageCreator(CLASS_NAME);

		private static Type _type;
		public static Type Type {
			get {
				if (_type != null) return _type;
				Type type = Type.GetType(TYPE_NAME) ?? throw new NullReferenceException(ERROR_MESSAGE_CREATOR.TypeMessage());
				_type = type;
				return _type;
			}
		}

		#region Static Methods
		
		private const string GET_POOLED_METHOD_NAME = "GetPooled";
		private static Func<SerializedObject, EventBase> _getPooledFunction;

		public static SerializedObjectBindEventHelper GetPooled(SerializedObject serializedObject) {
			if (_getPooledFunction != null) {
				return new SerializedObjectBindEventHelper(_getPooledFunction(serializedObject));
			}

			MethodInfo methodInfo = Type.GetMethod(GET_POOLED_METHOD_NAME, BindingFlags.Public | BindingFlags.Static);
			if (methodInfo == null) throw new NullReferenceException(ERROR_MESSAGE_CREATOR.MethodMessage(GET_POOLED_METHOD_NAME, nameof(SerializedProperty), "SerializedObjectBindEvent"));
			_getPooledFunction = ExpressionTreeUtils.CreateStaticMethodCallFunction<SerializedObject, EventBase>(methodInfo);
			return new SerializedObjectBindEventHelper(_getPooledFunction(serializedObject));
		}

		#endregion



		#region Instance
		
		public EventBase SerializedObjectBindEvent { get; }

		private SerializedObjectBindEventHelper(EventBase serializedObjectBindEvent) {
			if (serializedObjectBindEvent.GetType() != Type) {
				throw new InvalidParameterException($"{nameof(serializedObjectBindEvent)} is not SerializedObjectBindEvent");
			}

			this.SerializedObjectBindEvent = serializedObjectBindEvent;
		}
		
		
		public void Dispose() {
			SerializedObjectBindEvent?.Dispose();
		}

		#endregion

	}
}