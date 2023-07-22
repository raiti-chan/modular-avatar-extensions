using System;
using System.Reflection;
using raitichan.com.modular_avatar.extensions.ReflectionHelper;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity {
	public static class BindingExtensionsHelper {
		private static readonly ErrorMessageCreator ERROR_MESSAGE_CREATOR = new ErrorMessageCreator(typeof(BindingExtensions));

		#region Static Method

		private const string BIND_METHOD_NAME = "Bind";
		private static Action<VisualElement, object, SerializedProperty> _bindMethod;

		public static void Bind(VisualElement visualElement, SerializedObjectUpdateWrapperHelper objWrapper, SerializedProperty parentProperty) {
			if (_bindMethod != null) {
				_bindMethod(visualElement, objWrapper.SerializedObjectUpdateWrapper, parentProperty);
				return;
			}

			MethodInfo methodInfo = typeof(BindingExtensions).GetMethod(BIND_METHOD_NAME, BindingFlags.NonPublic | BindingFlags.Static);
			if (methodInfo == null)
				throw new NullReferenceException(ERROR_MESSAGE_CREATOR.MethodMessage(
					BIND_METHOD_NAME, nameof(VisualElement), SerializedObjectUpdateWrapperHelper.Type.Name, nameof(SerializedProperty), null));
			_bindMethod = ExpressionTreeUtils.CreateStaticMethodCallAction<VisualElement, object, SerializedProperty>(methodInfo, tArg1: SerializedObjectUpdateWrapperHelper.Type);
			_bindMethod(visualElement, objWrapper.SerializedObjectUpdateWrapper, parentProperty);
		}

		private const string DO_BIND_PROPERTY_METHOD_NAME = "DoBindProperty";
		private static Action<IBindable, object, SerializedProperty> _doBindPropertyMethod;

		public static void DoBindProperty(IBindable bindable, SerializedObjectUpdateWrapperHelper objWrapper, SerializedProperty property) {
			if (_doBindPropertyMethod != null) {
				_doBindPropertyMethod(bindable, objWrapper.SerializedObjectUpdateWrapper, property);
				return;
			}

			MethodInfo methodInfo = typeof(BindingExtensions).GetMethod(DO_BIND_PROPERTY_METHOD_NAME, BindingFlags.NonPublic | BindingFlags.Static);
			if (methodInfo == null)
				throw new NullReferenceException(
					ERROR_MESSAGE_CREATOR.MethodMessage(DO_BIND_PROPERTY_METHOD_NAME, nameof(VisualElement), SerializedObjectUpdateWrapperHelper.Type.Name,
					nameof(SerializedProperty), null));
			_doBindPropertyMethod = ExpressionTreeUtils.CreateStaticMethodCallAction<IBindable, object, SerializedProperty>(methodInfo, tArg1: SerializedObjectUpdateWrapperHelper.Type);
			_doBindPropertyMethod(bindable, objWrapper.SerializedObjectUpdateWrapper, property);
		}

		private const string REMOVE_BINDING_METHOD_NAME = "RemoveBinding";
		private static Action<IBindable> _removeBindingMethod;

		public static void RemoveBinding(IBindable target) {
			if (_removeBindingMethod != null) {
				_removeBindingMethod(target);
				return;
			}

			MethodInfo methodInfo = typeof(BindingExtensions).GetMethod(REMOVE_BINDING_METHOD_NAME, BindingFlags.NonPublic | BindingFlags.Static);
			if (methodInfo == null) throw new NullReferenceException(ERROR_MESSAGE_CREATOR.MethodMessage(REMOVE_BINDING_METHOD_NAME, nameof(IBindable), null));
			_removeBindingMethod = ExpressionTreeUtils.CreateStaticMethodCallAction<IBindable>(methodInfo);
			_removeBindingMethod(target);
		}

		
		public static bool SendBindingEvent(EventBase evt, VisualElement target) {
			evt.target = target;
			CallbackEventHandleHelper.HandleEventAtTargetPhase(target, evt);
			return evt.isPropagationStopped;
		}

		#endregion
	}

	public class SerializedObjectUpdateWrapperHelper {
		private const string ASSEMBLY_NAME = "UnityEditor";
		private const string NAMESPACE = "UnityEditor.UIElements";
		private const string CLASS_NAME = NAMESPACE + ".BindingExtensions+SerializedObjectUpdateWrapper";
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

		#region Instance

		public object SerializedObjectUpdateWrapper { get; }
		
		private static Func<SerializedObject, object> _serializedObjectUpdateWrapperConstructor;

		public SerializedObjectUpdateWrapperHelper(SerializedObject so) {
			if (_serializedObjectUpdateWrapperConstructor != null) {
				this.SerializedObjectUpdateWrapper = _serializedObjectUpdateWrapperConstructor(so);
				return;
			}

			ConstructorInfo constructorInfo = Type.GetConstructor(new[] { typeof(SerializedObject) });
			if (constructorInfo == null) throw new NullReferenceException(ERROR_MESSAGE_CREATOR.ConstructorMessage(nameof(SerializedObject)));

			_serializedObjectUpdateWrapperConstructor = ExpressionTreeUtils.CreateConstructor<SerializedObject, object>(constructorInfo);
			this.SerializedObjectUpdateWrapper = _serializedObjectUpdateWrapperConstructor(so);
		}

		private const string UPDATE_IF_NECESSARY_METHOD_NAME = "UpdateIfNecessary";
		private static Action<object> _updateIfNecessaryMethod;
		public void UpdateIfNecessary() {
			if (_updateIfNecessaryMethod != null) {
				_updateIfNecessaryMethod(this.SerializedObjectUpdateWrapper);
				return;
			}

			_updateIfNecessaryMethod = ExpressionTreeUtils.CreatePublicInstanceMethodCallAction<object>(UPDATE_IF_NECESSARY_METHOD_NAME, Type);
			_updateIfNecessaryMethod(this.SerializedObjectUpdateWrapper);
		}

		#endregion
	}
}