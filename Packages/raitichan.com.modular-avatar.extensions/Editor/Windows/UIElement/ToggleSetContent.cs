using System;
using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class ToggleSetContent : CustomBindableElement {
		private const string UXML_GUID = "054fe14302b47e84196e2bf938d4b8d2";

		private readonly ToolbarToggle _enableObjectTabButton;
		private readonly ToolbarToggle _blendShapeTabButton;
		private readonly ToolbarToggle _materialTabButton;

		private readonly EnableObjectPanel _enableObjectPanel;
		private readonly BlendShapePanel _blendShapePanel;
		private readonly MaterialReplacePanel _materialReplacePanel;

		private float _contentDimension;
		
		private ToggleSetPanelMode _mode;
		private ToggleSetPanelMode Mode {
			set {
				if (this._mode == value) return;
				switch (this._mode) {
					case ToggleSetPanelMode.EnableObject:
						break;
					case ToggleSetPanelMode.BlendShape:
						this._contentDimension = this._blendShapePanel.SplitViewDimension;
						break;
					case ToggleSetPanelMode.Material:
						this._contentDimension = this._materialReplacePanel.SplitViewDimension;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				this._enableObjectTabButton.SetValueWithoutNotify(value == ToggleSetPanelMode.EnableObject);
				this._enableObjectPanel.style.display = this._enableObjectTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;

				this._blendShapeTabButton.SetValueWithoutNotify(value == ToggleSetPanelMode.BlendShape);
				this._blendShapePanel.style.display = this._blendShapeTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;
				this._blendShapePanel.SplitViewDimension = this._contentDimension;

				this._materialTabButton.SetValueWithoutNotify(value == ToggleSetPanelMode.Material);
				this._materialReplacePanel.style.display = this._materialTabButton.value ? DisplayStyle.Flex : DisplayStyle.None;
				this._materialReplacePanel.SplitViewDimension = this._contentDimension;
				this._mode = value;
				
			}
		}

		public int ControlLayer {
			set {
				this._enableObjectPanel.ControlLayer = value;
				this._blendShapePanel.ControlLayer = value;
				this._materialReplacePanel.ControlLayer = value;
			}
		}

		public ToggleSetContent() {
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
		}
		
		private void EnableObjectTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._enableObjectTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = ToggleSetPanelMode.EnableObject;
		}

		private void BlendShapeTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._blendShapeTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = ToggleSetPanelMode.BlendShape;
		}

		private void MaterialTabButtonClicked(ChangeEvent<bool> evt) {
			if (evt.newValue == false) {
				this._materialTabButton.SetValueWithoutNotify(true);
			}

			this.Mode = ToggleSetPanelMode.Material;
		}
		
		public new class UxmlFactory : UxmlFactory<ToggleSetContent, UxmlTraits> { }
		
		private enum ToggleSetPanelMode {
			EnableObject,
			BlendShape,
			Material,
		}
	}
}