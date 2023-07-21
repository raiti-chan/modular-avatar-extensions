using System;
using System.Collections.Generic;
using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity;
using UnityEditor;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class ToggleSetElement : CustomBindableElement {
		private const string UXML_GUID = "ba74e8fbd99239c47b9a1e49c653e690";

		private readonly MultiTagField _exclusiveTagField;
		
		public Func<IEnumerable<string>> CreateExclusiveTags {
			set => this._exclusiveTagField.CreateItemSource = value;
		}
		
		public int ToggleSetIndex { get; set; }
		
		public ToggleSetElement() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._exclusiveTagField = this.Q<MultiTagField>("ExclusiveTagField");
			this._exclusiveTagField.CreateSelectedItemSource = this.ExclusiveTagFieldCreateSelectedItemSource;
			this._exclusiveTagField.OnNone = this.ExclusiveTagFieldOnNone;
			this._exclusiveTagField.OnTag = this.ExclusiveTagFieldOnTag;
			this._exclusiveTagField.OnCreate = this.ExclusiveTagOnCreate;

			this.RegisterCallback<ChangeEvent<bool>>(this.OnPreviewChanged, TrickleDown.TrickleDown);
			this.RegisterCallback<ChangeEvent<bool>>(this.OnDefaultValueChanged);
		}

		private IEnumerable<string> ExclusiveTagFieldCreateSelectedItemSource() {
			SerializedProperty exclusiveTagsProperty = this.BindingProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.exclusiveTags));
			foreach (string tag in exclusiveTagsProperty.GetArrayElements().ToStringValues()) {
				yield return tag;
			}
		}
		
		private void ExclusiveTagFieldOnNone() {
			SerializedProperty exclusiveTagsProperty = this.BindingProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.exclusiveTags));
			exclusiveTagsProperty.ClearArray();
			exclusiveTagsProperty.serializedObject.ApplyModifiedProperties();
		}
		
		private void ExclusiveTagFieldOnTag(string tag, bool selected) {
			SerializedProperty exclusiveTagsProperty = this.BindingProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.exclusiveTags));
			int arraySize = exclusiveTagsProperty.arraySize;
			if (selected) {
				for (int i = 0; i < arraySize; i++) {
					SerializedProperty currentProperty = exclusiveTagsProperty.GetArrayElementAtIndex(i);
					if (currentProperty.stringValue != tag) continue;
					exclusiveTagsProperty.DeleteArrayElementAtIndex(i);
					exclusiveTagsProperty.serializedObject.ApplyModifiedProperties();
					return;
				}
			} else {
				exclusiveTagsProperty.InsertArrayElementAtIndex(arraySize);
				SerializedProperty addedProperty = exclusiveTagsProperty.GetArrayElementAtIndex(arraySize);
				addedProperty.stringValue = tag;
				addedProperty.serializedObject.ApplyModifiedProperties();
			}
		}
		
		private void ExclusiveTagOnCreate(string tag) {
			SerializedProperty exclusiveTagsProperty = this.BindingProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.exclusiveTags));
			int arraySize = exclusiveTagsProperty.arraySize;
			exclusiveTagsProperty.InsertArrayElementAtIndex(arraySize);
			SerializedProperty addedProperty = exclusiveTagsProperty.GetArrayElementAtIndex(arraySize);
			addedProperty.stringValue = tag;
			addedProperty.serializedObject.ApplyModifiedProperties();
		}

		private void OnPreviewChanged(ChangeEvent<bool> evt) {
			if (!this.IsBound) return;
			if (!(evt.target is ToggleButton)) return;
			this.BindingProperty.FindPropertyRelative(nameof(MAExObjectPreset.ToggleSet.preview)).boolValue = evt.newValue;
			this.BindingProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			using (ToggleSetPreviewChangeEvent pooled = ToggleSetPreviewChangeEvent.GetPooled(this.ToggleSetIndex, evt.newValue)) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}

		private void OnDefaultValueChanged(ChangeEvent<bool> evt) {
			if (!this.IsBound) return;
			if (!(evt.target is Toggle toggle)) return;
			if (toggle.name != "DefaultValueToggle") return;
			using (ToggleSetDefaultValueChangeEvent pooled = ToggleSetDefaultValueChangeEvent.GetPooled(this.ToggleSetIndex, evt.newValue)) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
		}

		protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
			base.ExecuteDefaultActionAtTarget(evt);
			if (evt.eventTypeId == CustomBindableBoundEvent.TypeId()) {
				this._exclusiveTagField.TextUpdate();
			}
		}
	}

	public class ToggleSetPreviewChangeEvent : EventBase<ToggleSetPreviewChangeEvent> {
		public int ToggleSetIndex { get; private set; }
		public bool NewValue { get; private set; }
		
		protected override void Init() {
			base.Init();
			this.LocalInit();
		}

		private void LocalInit() {
			EventBaseHelper.SetPropagation(this, EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable);
		}

		public ToggleSetPreviewChangeEvent() {
			this.LocalInit();
		}

		public static ToggleSetPreviewChangeEvent GetPooled(int toggleSetIndex, bool newValue) {
			ToggleSetPreviewChangeEvent evt = EventBase<ToggleSetPreviewChangeEvent>.GetPooled();
			evt.ToggleSetIndex = toggleSetIndex;
			evt.NewValue = newValue;
			return evt;
		}
	}

	public class ToggleSetDefaultValueChangeEvent : EventBase<ToggleSetDefaultValueChangeEvent> {
		public int ToggleSetIndex { get; private set; }
		public bool NewValue { get; private set; }
		
		protected override void Init() {
			base.Init();
			this.LocalInit();
		}

		private void LocalInit() {
			EventBaseHelper.SetPropagation(this, EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable);
		}

		public ToggleSetDefaultValueChangeEvent() {
			this.LocalInit();
		}

		public static ToggleSetDefaultValueChangeEvent GetPooled(int toggleSetIndex, bool newValue) {
			ToggleSetDefaultValueChangeEvent evt = EventBase<ToggleSetDefaultValueChangeEvent>.GetPooled();
			evt.ToggleSetIndex = toggleSetIndex;
			evt.NewValue = newValue;
			return evt;
		}
	}
}