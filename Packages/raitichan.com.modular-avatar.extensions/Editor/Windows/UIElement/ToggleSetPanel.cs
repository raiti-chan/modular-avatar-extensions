using System.Collections.Generic;
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
		private readonly ToggleSetContent _toggleSetContent;

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
			this._toggleSetListView.OnUp = ToggleSetListViewOnUp;
			this._toggleSetListView.OnDown = ToggleSetListViewOnDown;
			this._toggleSetListView.MakeItem = () => new ToggleSetElement();
			this._toggleSetListView.BindItem = this.ToggleSetListViewBindItem;
			this._toggleSetListView.onSelectionChanged += this.ToggleSetListViewOnSelectionChanged;
			this._toggleSetListView.RegisterCallback<CustomBindablePreBindEvent>(ToggleSetListViewPreBind);
			this._toggleSetListView.RegisterCallback<ToggleSetPreviewChangeEvent>(this.ToggleSetListViewOnPreviewChanged);
			this._toggleSetListView.RegisterCallback<ToggleSetDefaultValueChangeEvent>(this.ToggleSetListViewOnDefaultValueChanged);
			this._toggleSetContent = this._splitView.Q<ToggleSetContent>("ToggleSetContent");
		}

		private void SendToggleUpdateEvent() {
			using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewReloadPresetCommand())) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}

		private void ToggleSetListViewOnRemove(SerializedProperty serializedProperty, int index) {
			serializedProperty.DeleteArrayElementAtIndex(index);
			serializedProperty.serializedObject.ApplyModifiedProperties();
			this._toggleSetListView.SelectedIndex = -1;
			this.SendToggleUpdateEvent();
		}

		private void ToggleSetListViewOnUp(SerializedProperty serializedProperty, int index) {
			serializedProperty.MoveArrayElement(index, index - 1);
			serializedProperty.serializedObject.ApplyModifiedProperties();
			this._toggleSetListView.SelectedIndex = index - 1;
			this.SendToggleUpdateEvent();
		}

		private void ToggleSetListViewOnDown(SerializedProperty serializedProperty, int index) {
			serializedProperty.MoveArrayElement(index, index + 1);
			serializedProperty.serializedObject.ApplyModifiedProperties();
			this._toggleSetListView.SelectedIndex = index + 1;
			this.SendToggleUpdateEvent();
		}

		private void ToggleSetListViewBindItem(SerializedProperty serializedProperty, VisualElement element, int i) {
			if (!(element is ToggleSetElement toggleSetElement)) return;

			SerializedProperty parentProperty = serializedProperty.GetArrayElementAtIndex(i);
			toggleSetElement.BindProperty(parentProperty, this._toggleSetListView.ObjWrapper);
			toggleSetElement.ToggleSetIndex = i;
		}

		private void ToggleSetListViewOnSelectionChanged(SerializedProperty selected) {
			if (selected != null) {
				this._toggleSetContent.BindProperty(selected, this._toggleSetListView.ObjWrapper);
				this._toggleSetContent.ControlLayer = this._toggleSetListView.SelectedIndex;
			} else {
				this._toggleSetContent.Unbind();
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

		private void ToggleSetListViewOnPreviewChanged(ToggleSetPreviewChangeEvent evt) {
			if (evt.NewValue) {
				SerializedProperty listProperty = this._toggleSetListView.BindingProperty;
				SerializedProperty targetToggleProperty = listProperty.GetArrayElementAtIndex(evt.ToggleSetIndex);
				SerializedProperty targetTagsProperty = targetToggleProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.exclusiveTags));
				HashSet<string> tags = new HashSet<string>(targetTagsProperty.GetArrayElements().ToStringValues());
				int count = listProperty.arraySize;
				for (int i = 0; i < count; i++) {
					if (i == evt.ToggleSetIndex) continue;
					SerializedProperty toggleSetProperty = listProperty.GetArrayElementAtIndex(i);
					SerializedProperty tagsProperty = toggleSetProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.exclusiveTags));
					foreach (string tag in tagsProperty.GetArrayElements().ToStringValues()) {
						if (!tags.Contains(tag)) continue;
						toggleSetProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.preview)).boolValue = false;
						using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewToggleSetChangeActiveCommand(i, false))) {
							pooled.target = this;
							this.SendEvent(pooled);
						}
					}
				}

				listProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			}

			using (PresetChangeEvent pooled = PresetChangeEvent.GetPooled(new PreviewToggleSetChangeActiveCommand(evt.ToggleSetIndex, evt.NewValue))) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}

		private void ToggleSetListViewOnDefaultValueChanged(ToggleSetDefaultValueChangeEvent evt) {
			if (!evt.NewValue) return;
			SerializedProperty listProperty = this._toggleSetListView.BindingProperty;
			SerializedProperty targetToggleProperty = listProperty.GetArrayElementAtIndex(evt.ToggleSetIndex);
			SerializedProperty targetTagsProperty = targetToggleProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.exclusiveTags));
			HashSet<string> tags = new HashSet<string>(targetTagsProperty.GetArrayElements().ToStringValues());
			int count = listProperty.arraySize;
			for (int i = 0; i < count; i++) {
				if (i == evt.ToggleSetIndex) continue;
				SerializedProperty toggleSetProperty = listProperty.GetArrayElementAtIndex(i);
				SerializedProperty tagsProperty = toggleSetProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.exclusiveTags));
				foreach (string tag in tagsProperty.GetArrayElements().ToStringValues()) {
					if (!tags.Contains(tag)) continue;
					toggleSetProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.defaultValue)).boolValue = false;
				}
			}

			listProperty.serializedObject.ApplyModifiedProperties();
		}

		public new class UxmlFactory : UxmlFactory<ToggleSetPanel, UxmlTraits> { }
	}
}