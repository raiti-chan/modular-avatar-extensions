using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity;
using UnityEditor;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public interface ICustomBindable : IBindable, IBindStoppable {
		SerializedProperty BindingProperty { get; }
		void Bind(SerializedObject serializedObject);
		void BindProperty(SerializedProperty serializedProperty, SerializedObjectUpdateWrapperHelper objWrapper = null);
		void Unbind();
		void RemoveBinding();
	}

	public class CustomBindablePreBindEvent : EventBase<CustomBindablePreBindEvent> {
		public SerializedProperty BindingProperty { get; private set; }
		
		protected override void Init() {
			base.Init();
			this.LocalInit();
		}

		private void LocalInit() {
			EventBaseHelper.SetPropagation(this, EventPropagation.None);
		}

		public CustomBindablePreBindEvent() {
			this.LocalInit();
		}

		public static CustomBindablePreBindEvent GetPooled(SerializedProperty serializedProperty) {
			CustomBindablePreBindEvent evt = EventBase<CustomBindablePreBindEvent>.GetPooled();
			evt.BindingProperty = serializedProperty;
			return evt;
		}
	}
	
	
	public class CustomBindableBoundEvent : EventBase<CustomBindableBoundEvent> {

		public SerializedProperty BindingProperty { get; private set; }

		protected override void Init() {
			base.Init();
			this.LocalInit();
		}

		private void LocalInit() {
			EventBaseHelper.SetPropagation(this, EventPropagation.None);
		}

		public CustomBindableBoundEvent() {
			this.LocalInit();
		}

		public static CustomBindableBoundEvent GetPooled(SerializedProperty serializedProperty) {
			CustomBindableBoundEvent evt = EventBase<CustomBindableBoundEvent>.GetPooled();
			evt.BindingProperty = serializedProperty;
			return evt;
		}
	}
}