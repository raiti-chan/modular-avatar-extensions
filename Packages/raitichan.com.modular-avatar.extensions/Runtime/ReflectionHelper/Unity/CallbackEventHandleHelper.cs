using System;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity {
	public static class CallbackEventHandleHelper {
		private static readonly ErrorMessageCreator ERROR_MESSAGE_CREATOR = new ErrorMessageCreator(typeof(CallbackEventHandler));

		private const string HANDLE_EVENT_AT_TARGET_PHASE_METHOD_NAME = "HandleEventAtTargetPhase";
		private static Action<CallbackEventHandler, EventBase> _handleEventAtTargetPhase;

		public static void HandleEventAtTargetPhase(CallbackEventHandler target, EventBase evt) {
			if (_handleEventAtTargetPhase != null) {
				_handleEventAtTargetPhase(target, evt);
				return;
			}

			_handleEventAtTargetPhase = ExpressionTreeUtils.CreateNonPublicInstanceMethodCallAction<CallbackEventHandler, EventBase>(HANDLE_EVENT_AT_TARGET_PHASE_METHOD_NAME);
			_handleEventAtTargetPhase(target, evt);

		}
	}
}