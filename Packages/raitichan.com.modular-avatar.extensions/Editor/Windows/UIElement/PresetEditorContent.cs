using System;
using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class PresetEditorContent : CustomBindableElement {
		private const string UXML_GUID = "bedb6463b3877b64a9b5092edf576fed";

		private readonly ToolbarToggle _enableObjectTabButton;
		private readonly ToolbarToggle _blendShapeTabButton;
		private readonly ToolbarToggle _materialTabButton;
		private readonly ToolbarToggle _toggleObjectTabButton;

		private readonly EnableObjectPanel _enableObjectPanel;
		private readonly BlendShapePanel _blendShapePanel;
		private readonly MaterialReplacePanel _materialReplacePanel;
		private readonly ToggleSetPanel _toggleSetPanel;

		private float _contentDimension;

		private EditorPanelMode _mode;
		private EditorPanelMode Mode {
			set {
				if (this._mode == value) return;
				switch (this._mode) {
					case EditorPanelMode.ShowObject:
						break;
					case EditorPanelMode.BlendShape:
						this._contentDimension = this._blendShapePanel.SplitViewDimension;
						break;
					case EditorPanelMode.Material:

						this._contentDimension = this._materialReplacePanel.SplitViewDimension;
						break;
					case EditorPanelMode.ToggleObject:
						this._contentDimension = this._toggleSetPanel.SplitViewDimension;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(value), value, null);
				}

				this._enableObjectTabButton.SetValueWithoutNotify(value == EditorPanelMode.ShowObject);
				this._enableObjectPanel.style.display = this._enableObjectTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;

				this._blendShapeTabButton.SetValueWithoutNotify(value == EditorPanelMode.BlendShape);
				this._blendShapePanel.style.display = this._blendShapeTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;
				this._blendShapePanel.SplitViewDimension = this._contentDimension;

				this._materialTabButton.SetValueWithoutNotify(value == EditorPanelMode.Material);
				this._materialReplacePanel.style.display = this._materialTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;
				this._materialReplacePanel.SplitViewDimension = this._contentDimension;

				this._toggleObjectTabButton.SetValueWithoutNotify(value == EditorPanelMode.ToggleObject);
				this._toggleSetPanel.style.display = this._toggleObjectTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;
				this._toggleSetPanel.SplitViewDimension = this._contentDimension;
				this._mode = value;
			}
		}

		public PresetEditorContent() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._enableObjectTabButton = this.Q<ToolbarToggle>("EnableObjectTabButton");
			this._enableObjectTabButton.RegisterValueChangedCallback(this.EnableObjectTabButtonClicked);
			this._enableObjectPanel = this.Q<EnableObjectPanel>("EnableObjectPanel");

			this._blendShapeTabButton = this.Q<ToolbarToggle>("BlendShapeTabButton");
			this._blendShapeTabButton.RegisterValueChangedCallback(this.BlendShapeTabButtonClicked);
			this._blendShapePanel = this.Q<BlendShapePanel>("BlendShapePanel");

			this._materialTabButton = this.Q<ToolbarToggle>("MaterialTabButton");
			this._materialTabButton.RegisterValueChangedCallback(this.MaterialTabButtonClicked);
			this._materialReplacePanel = this.Q<MaterialReplacePanel>("MaterialReplacePanel");

			this._toggleObjectTabButton = this.Q<ToolbarToggle>("ToggleSetTabButton");
			this._toggleObjectTabButton.RegisterValueChangedCallback(this.ToggleObjectTabButtonClicked);
			this._toggleSetPanel = this.Q<ToggleSetPanel>("ToggleSetPanel");

			this.Mode = EditorPanelMode.ShowObject;
		}

		private void EnableObjectTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._enableObjectTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = EditorPanelMode.ShowObject;
		}

		private void BlendShapeTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._blendShapeTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = EditorPanelMode.BlendShape;
		}

		private void MaterialTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._materialTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = EditorPanelMode.Material;
		}

		private void ToggleObjectTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._toggleObjectTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = EditorPanelMode.ToggleObject;
		}


		public new class UxmlFactory : UxmlFactory<PresetEditorContent, UxmlTraits> { }

		private enum EditorPanelMode {
			ShowObject,
			BlendShape,
			Material,
			ToggleObject,
		}
	}
}