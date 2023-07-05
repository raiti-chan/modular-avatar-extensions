using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public class BindStoppableElement : VisualElement, IBindStoppable {
		public bool IsBindStopping { get; set; }
		
		public new class UxmlFactory : UxmlFactory<BindStoppableElement, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits {
			private readonly UxmlBoolAttributeDescription _isBindStopping = new UxmlBoolAttributeDescription {
				name = "is-bind-stopping",
				defaultValue = false
			};

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
				base.Init(ve, bag, cc);
				if (!(ve is BindStoppableElement customBindableElement)) return;
				customBindableElement.IsBindStopping = this._isBindStopping.GetValueFromBag(bag, cc);
			}
		}
	}
}