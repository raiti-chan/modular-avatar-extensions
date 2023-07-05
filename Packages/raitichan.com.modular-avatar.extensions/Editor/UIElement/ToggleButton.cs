using UnityEditor;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public class ToggleButton : Toggle {
		private const string LIGHT_USS_GUID = "b260aa74f1ec843418d6c4e4bf70d4ca";
		private const string DARK_USS_GUID = "95d9b1ead4193c54b9e239ec4f174a10";
		private const string USS_CLASS_NAME = "toggle-button";

		private static readonly StyleSheet LIGHT_STYLE_SHEET;
		private static readonly StyleSheet DARK_STYLE_SHEET;

		static ToggleButton() {
			string lightPath = AssetDatabase.GUIDToAssetPath(LIGHT_USS_GUID);
			LIGHT_STYLE_SHEET = AssetDatabase.LoadAssetAtPath<StyleSheet>(lightPath);
			
			string darkPath = AssetDatabase.GUIDToAssetPath(DARK_USS_GUID);
			DARK_STYLE_SHEET = AssetDatabase.LoadAssetAtPath<StyleSheet>(darkPath);
		}

		public ToggleButton() {
			this.styleSheets.Add(EditorGUIUtility.isProSkin ? DARK_STYLE_SHEET : LIGHT_STYLE_SHEET);
			this.RemoveFromClassList(ussClassName);
			this.AddToClassList(Button.ussClassName);
			this.AddToClassList(USS_CLASS_NAME);
			this.labelElement.focusable = false;
		}

		public new class UxmlFactory : UxmlFactory<ToggleButton, UxmlTraits> { }
	}
}