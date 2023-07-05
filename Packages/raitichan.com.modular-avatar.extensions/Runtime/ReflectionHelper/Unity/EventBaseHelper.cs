using System;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity {
	public static class EventBaseHelper {
		private static Action<EventBase, EventPropagation> _setPropagation;

		public static void SetPropagation(EventBase target, EventPropagation propagation) {
			if (_setPropagation != null) {
				_setPropagation(target, propagation);
				return;
			}

			_setPropagation = ExpressionTreeUtils.CreateNonPublicInstancePropertyGetFunction<EventBase, EventPropagation>("propagation", tArg0: EventPropagationHelper.Type);
			_setPropagation(target, propagation);
		}
	}

	[Flags]
	public enum EventPropagation {
		None = 0,
		Bubbles = 1,
		TricklesDown = 2,
		Cancellable = 4,
	}

	public static class EventPropagationHelper {
		private const string ASSEMBLY_NAME = "UnityEngine.UIElementsModule";
		private const string NAMESPACE = "UnityEngine.UIElements";
		private const string CLASS_NAME = NAMESPACE + ".EventBase+EventPropagation";
		private const string TYPE_NAME = CLASS_NAME + ", " + ASSEMBLY_NAME;

		private static Type _type;
		public static Type Type {
			get {
				if (_type != null) return _type;
				Type type = Type.GetType(TYPE_NAME) ?? throw new NullReferenceException($"Not Found : {TYPE_NAME}");
				_type = type;
				return _type;
			}
		}
	}
}