using System.Reflection;
using raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Inspectors {
	public class MAExEditorBase : UnityEditor.Editor {

		private VisualElement _rootElement;
		
		public sealed override VisualElement CreateInspectorGUI() {
			VisualElement outOfAvatarWarning = new IMGUIContainer(() => InspectorCommonHelper.DisplayOutOfAvatarWarning(targets));
			VisualElement inner = this.CreateInnerInspectorGUI();
			this._rootElement = new MAExVisualElement();
			this._rootElement.Add(outOfAvatarWarning);
			this._rootElement.Add(inner);

			return this._rootElement;
		}

		protected virtual VisualElement CreateInnerInspectorGUI() {
			InspectorElement throwaway = new InspectorElement();
			MethodInfo mInfo = typeof(InspectorElement).GetMethod("CreateIMGUIInspectorFromEditor", BindingFlags.NonPublic | BindingFlags.Instance);
			return mInfo?.Invoke(throwaway, new object[] {this.serializedObject, this, false}) as VisualElement;
		}

		public sealed override void OnInspectorGUI() {
			this.OnInnerInspectorGUI();
		}

		protected virtual void OnInnerInspectorGUI() {
			base.OnInspectorGUI();
		}

		private class MAExVisualElement : VisualElement {}
	}
}