using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class ToggleSetPanel : VisualElement {
		private const string UXML_GUID = "9dd14dd985ef24842a7bb001e45e917f";

		private readonly TwoPaneSplitView _splitView;
		private readonly CustomListView _toggleSetListView;
		private readonly CustomBindableElement _toggleSetView;
		private readonly ShowObjectPanel _showObjectPanel;
		private readonly BlendShapePanel _blendShapePanel;
		private readonly MaterialReplacePanel _materialReplacePanel;

		public float SplitViewDimension {
			get => this._splitView.FixedPaneCurrentDimension;
			set => this._splitView.FixedPaneInitialDimension = value;
		}

		public ToggleSetPanel() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._splitView = this.Q<TwoPaneSplitView>("SplitView");

			this._toggleSetListView = this._splitView.Q<CustomListView>("ToggleSetListView");
			this._toggleSetListView.OnRemove = ToggleSetListViewOnRemove;
			this._toggleSetListView.MakeItem = () => new ToggleSetElement();
			this._toggleSetListView.BindItem = this.ToggleSetListViewBindItem;
			this._toggleSetListView.onSelectionChanged += this.ToggleSetListViewOnSelectionChanged;
			this._toggleSetListView.RegisterCallback<CustomBindablePreBindEvent>(ToggleSetListViewPreBind);

			this._toggleSetView = this._splitView.Q<CustomBindableElement>("ToggleSetView");
			this._showObjectPanel = this._toggleSetView.Q<ShowObjectPanel>("ShowObjectPanel");
			this._blendShapePanel = this._toggleSetView.Q<BlendShapePanel>("BlendShapePanel");
			this._materialReplacePanel = this._toggleSetView.Q<MaterialReplacePanel>("MaterialReplacePanel");
		}

		private void ToggleSetListViewOnRemove(SerializedProperty serializedProperty, int index) {
			using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewToggleRemoveCommand(index, () => {
				       serializedProperty.DeleteArrayElementAtIndex(index);
				       serializedProperty.serializedObject.ApplyModifiedProperties();
				       this._toggleSetListView.SelectedIndex = -1;
			       }))) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}

		private void ToggleSetListViewBindItem(SerializedProperty serializedProperty, VisualElement element, int i) {
			if (!(element is ToggleSetElement toggleSetElement)) return;

			SerializedProperty parentProperty = serializedProperty.GetArrayElementAtIndex(i);
			toggleSetElement.BindProperty(parentProperty, this._toggleSetListView.ObjWrapper);
			toggleSetElement.ToggleSetIndex = i;
		}

		private void ToggleSetListViewOnSelectionChanged(SerializedProperty selected) {
			if (selected != null) {
				this._toggleSetView.BindProperty(selected, this._toggleSetView.ObjWrapper);
				this._blendShapePanel.ControlLayer = this._toggleSetListView.SelectedIndex;
				this._showObjectPanel.ControlLayer = this._toggleSetListView.SelectedIndex;
				this._materialReplacePanel.ControlLayer = this._toggleSetListView.SelectedIndex;
			} else {
				this._toggleSetView.Unbind();
			}
		}

		private static void ToggleSetListViewPreBind(CustomBindablePreBindEvent evt) {
			foreach (SerializedProperty serializedProperty in evt.BindingProperty.GetArrayElements()) {
				SerializedProperty defaultValueProperty = serializedProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.defaultValue));
				SerializedProperty previewProperty = serializedProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.preview));
				previewProperty.boolValue = defaultValueProperty.boolValue;
			}

			evt.BindingProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}

		public new class UxmlFactory : UxmlFactory<ToggleSetPanel, UxmlTraits> { }
	}
}