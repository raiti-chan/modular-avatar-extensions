using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class BlendShapePanel : VisualElement {

		public BlendShapePanel() {
			this.Add(new Label("BlendShape"));
		}
		
		public new class UxmlFactory : UxmlFactory<BlendShapePanel, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits { }
		
	}
}