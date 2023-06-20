using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class PresetEditorContent : BindableElement {
		private const string UXML_GUID = "bedb6463b3877b64a9b5092edf576fed";
		
		private readonly ToolbarToggle _showObjectTabButton;
		private readonly ToolbarToggle _blendShapeTabButton;
		private readonly ToolbarToggle _toggleObjectTabButton;
		
		private readonly ShowObjectPanel _showObjectPanel;
		private readonly BlendShapePanel _blendShapePanel;
		private readonly ToggleObjectPanel _toggleObjectPanel;

		private EditorPanelMode _mode;
		private EditorPanelMode Mode {
			set {
				if (this._mode == value) return;
				this._mode = value;
				this._showObjectTabButton.SetValueWithoutNotify(value == EditorPanelMode.ShowObject);
				this._showObjectPanel.style.display = this._showObjectTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;
				
				this._blendShapeTabButton.SetValueWithoutNotify(value == EditorPanelMode.BlendShape);
				this._blendShapePanel.style.display = this._blendShapeTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;
				
				this._toggleObjectTabButton.SetValueWithoutNotify(value == EditorPanelMode.ToggleObject);
				this._toggleObjectPanel.style.display = this._toggleObjectTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		public PresetEditorContent() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._showObjectTabButton = this.Q<ToolbarToggle>("ShowObjectTabButton");
			this._showObjectTabButton.RegisterValueChangedCallback(ShowObjectTabButtonClicked);
			this._showObjectPanel = this.Q<ShowObjectPanel>("ShowObjectPanel");

			this._blendShapeTabButton = this.Q<ToolbarToggle>("BlendShapeTabButton");
			this._blendShapeTabButton.RegisterValueChangedCallback(BlendShapeTabButtonClicked);
			this._blendShapePanel = this.Q<BlendShapePanel>("BlendShapePanel");

			this._toggleObjectTabButton = this.Q<ToolbarToggle>("ToggleObjectTabButton");
			this._toggleObjectTabButton.RegisterValueChangedCallback(ToggleObjectTabButtonClicked);
			this._toggleObjectPanel = this.Q<ToggleObjectPanel>("ToggleObjectPanel");

			this.Mode = EditorPanelMode.ShowObject;
		}
		
		public void Unbind() {
			BindingExtensions.Unbind(this);
			this._showObjectPanel.Unbind();
		}

		private void ShowObjectTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._showObjectTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = EditorPanelMode.ShowObject;
		}

		private void BlendShapeTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._blendShapeTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = EditorPanelMode.BlendShape;
		}

		private void ToggleObjectTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._toggleObjectTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = EditorPanelMode.ToggleObject;
		}


		public new class UxmlFactory : UxmlFactory<PresetEditorContent, UxmlTraits> { }

		public new class UxmlTraits : BindableElement.UxmlTraits { }

		private enum EditorPanelMode {
			ShowObject,
			BlendShape,
			ToggleObject,
		}
	}
}