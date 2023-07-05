using System.Collections.Generic;
using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity;
using UnityEditor;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public class CustomBindableElement : VisualElement, ICustomBindable {
		private Dictionary<IBindable, string> _originPassDictionary;
		private bool _boundFlag;

		public IBinding binding { get; set; }
		public SerializedObjectUpdateWrapperHelper ObjWrapper { get; private set; }
		public SerializedProperty BindingProperty { get; private set; }
		// ReSharper disable once MemberCanBeProtected.Global
		public bool IsBound { get; private set; }

		public string bindingPath { get; set; }
		// ReSharper disable once MemberCanBePrivate.Global
		public bool IsBindStopping { get; set; }


		public virtual void Bind(SerializedObject serializedObject) {
			SerializedObjectUpdateWrapperHelper objWrapper = new SerializedObjectUpdateWrapperHelper(serializedObject);
			if (!string.IsNullOrEmpty(this.bindingPath)) {
				SerializedProperty serializedProperty = serializedObject.FindProperty(this.bindingPath);
				this.BindProperty(serializedProperty, objWrapper);
			} else {
				foreach (VisualElement child in this.TraverseChildren(p => !(p is IBindable || (p is IBindStoppable stoppable && stoppable.IsBindStopping)))) {
					switch (child) {
						case ICustomBindable customBindable: {
							if (customBindable.IsBindStopping) continue;
							if (string.IsNullOrEmpty(customBindable.bindingPath)) continue;
							SerializedProperty childProperty = serializedObject.FindProperty(customBindable.bindingPath);
							if (childProperty == null) continue;
							customBindable.BindProperty(childProperty, objWrapper);
							break;
						}
						case IBindable bindable: {
							if (child is IBindStoppable bindStoppable && bindStoppable.IsBindStopping) continue;
							if (string.IsNullOrEmpty(bindable.bindingPath)) continue;
							SerializedProperty childProperty = serializedObject.FindProperty(bindable.bindingPath);
							if (childProperty == null) continue;
							bindable.bindingPath = childProperty.propertyPath;
							BindingExtensionsHelper.Bind(child, objWrapper, childProperty);
							break;
						}
					}
				}
			}
		}

		public virtual void BindProperty(SerializedProperty serializedProperty, SerializedObjectUpdateWrapperHelper objWrapper = null) {
			if (objWrapper == null) objWrapper = new SerializedObjectUpdateWrapperHelper(serializedProperty.serializedObject);

			using (SerializedObjectBindEventHelper pooled = SerializedObjectBindEventHelper.GetPooled(serializedProperty.serializedObject)) {
				if (BindingExtensionsHelper.SendBindingEvent(pooled.SerializedObjectBindEvent, this)) return;
			}

			using (CustomBindablePreBindEvent pooled = CustomBindablePreBindEvent.GetPooled(serializedProperty)) {
				pooled.target = this;
				if (BindingExtensionsHelper.SendBindingEvent(pooled, this)) return;
			}

			this.RemoveBinding();
			this.SaveChildBindingPath();
			BindingExtensionsHelper.DoBindProperty(this, objWrapper, serializedProperty);
			this.ObjWrapper = objWrapper;
			this.BindingProperty = serializedProperty;

			foreach (VisualElement child in this.TraverseChildren(p => !(p is IBindable || (p is IBindStoppable stoppable && stoppable.IsBindStopping)))) {
				switch (child) {
					case ICustomBindable customBindable: {
						if (customBindable.IsBindStopping) continue;
						if (string.IsNullOrEmpty(customBindable.bindingPath)) continue;
						SerializedProperty childProperty = serializedProperty.FindPropertyRelative(customBindable.bindingPath);
						if (childProperty == null) continue;
						customBindable.BindProperty(childProperty, objWrapper);
						continue;
					}
					case IBindable bindable: {
						if (child is IBindStoppable bindStoppable && bindStoppable.IsBindStopping) continue;
						if (string.IsNullOrEmpty(bindable.bindingPath)) continue;
						SerializedProperty childProperty = serializedProperty.FindPropertyRelative(bindable.bindingPath);
						if (childProperty == null) continue;
						bindable.bindingPath = childProperty.propertyPath;
						BindingExtensionsHelper.Bind(child, objWrapper, null);
						continue;
					}
				}
			}

			if (this.panel == null) {
				this.IsBound = true;
				this._boundFlag = true;
				return;
			}
			this.SendBoundEvent();
		}

		public virtual void Unbind() {
			this.RemoveBinding();
			foreach (VisualElement child in this.TraverseChildren(p => !(p is ICustomBindable))) {
				switch (child) {
					case ICustomBindable customBindable:
						customBindable.Unbind();
						continue;
					case IBindable bindable:
						BindingExtensionsHelper.RemoveBinding(bindable);
						continue;
				}
			}
		}

		public virtual void RemoveBinding() {
			BindingExtensionsHelper.RemoveBinding(this);
			this.IsBound = false;
			this.ObjWrapper = null;
			this.BindingProperty = null;
			this.LoadChildBindingPath();
		}

		private void SendBoundEvent() {
			using (CustomBindableBoundEvent pooled = CustomBindableBoundEvent.GetPooled(this.BindingProperty)) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}

		private void SaveChildBindingPath() {
			if (this._originPassDictionary == null) {
				this._originPassDictionary = new Dictionary<IBindable, string>();
			}

			foreach (VisualElement child in this.TraverseChildren(p => !(p is IBindable))) {
				if (child is IBindable bindable) {
					this._originPassDictionary[bindable] = bindable.bindingPath;
				}
			}
		}

		private void LoadChildBindingPath() {
			if (this._originPassDictionary == null) return;
			foreach (KeyValuePair<IBindable, string> pair in this._originPassDictionary) {
				pair.Key.bindingPath = pair.Value;
			}
		}

		protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
			base.ExecuteDefaultActionAtTarget(evt);
			if (evt.eventTypeId == AttachToPanelEvent.TypeId()) {
				if (!this._boundFlag) return;
				this._boundFlag = false;
				this.SendBoundEvent();
			} else if (evt.eventTypeId == CustomBindableBoundEvent.TypeId()) {
				this.IsBound = true;
			}
		}

		public new class UxmlFactory : UxmlFactory<CustomBindableElement, UxmlTraits> { }

		public new class UxmlTraits : BindableElement.UxmlTraits {
			private readonly UxmlBoolAttributeDescription _isBindStopping = new UxmlBoolAttributeDescription {
				name = "is-bind-stopping",
				defaultValue = false
			};

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
				base.Init(ve, bag, cc);
				if (!(ve is CustomBindableElement customBindableElement)) return;
				customBindableElement.IsBindStopping = this._isBindStopping.GetValueFromBag(bag, cc);
			}
		}
	}
}