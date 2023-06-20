using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class ToggleObjectPanel : VisualElement {

		public ToggleObjectPanel() {
			this.Add(new Label("ToggleObject"));
		}
		
		public new class UxmlFactory : UxmlFactory<ToggleObjectPanel, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits { }
	}
}