using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class WeightListElement : CustomBindableElement {
		private const string UXML_GUID = "6d83f04b01b7e274d8adeaf260957a3e";

		private readonly Slider _weightSlider;
		private readonly FloatField _weightField;

		public WeightListElement() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._weightSlider = this.Q<Slider>("WeightSlider");
			this._weightSlider.RegisterValueChangedCallback(this.OnSliderChanged);
			
			this._weightField = this.Q<FloatField>("WeightField");
			this._weightField.RegisterValueChangedCallback(this.OnFloatChanged);
		}

		private void OnSliderChanged(ChangeEvent<float> evt) {
			this._weightField.value = Mathf.Floor(evt.newValue * 10) / 10f;
		}

		private void OnFloatChanged(ChangeEvent<float> evt) {
			this._weightSlider.SetValueWithoutNotify(evt.newValue);
			this.OnWeightChanged(evt);
		}

		private void OnWeightChanged(ChangeEvent<float> evt) {
			if (this.BindingProperty == null) return;
			if (!this.IsBound) return;
			string blendShapeName = this.BindingProperty.FindPropertyRelative(nameof(MAExObjectPreset.BlendShapeWeight.key)).stringValue;
			using (BlendShapeChangeEvent pooled = BlendShapeChangeEvent.GetPooled(blendShapeName, evt.newValue)) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
			evt.StopPropagation();
		}

		protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
			base.ExecuteDefaultActionAtTarget(evt);
			if (evt.eventTypeId == AttachToPanelEvent.TypeId()) {
				this._weightSlider.SetValueWithoutNotify(this._weightField.value);
			}
		}
	}

	public class BlendShapeChangeEvent : EventBase<BlendShapeChangeEvent> {
		public string BlendShapeName { get; private set; }
		public float Weight { get; private set; }

		protected override void Init() {
			base.Init();
			this.LocalInit();
		}

		private void LocalInit() {
			EventBaseHelper.SetPropagation(this, EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable);
		}

		public BlendShapeChangeEvent() {
			this.LocalInit();
		}

		public static BlendShapeChangeEvent GetPooled(string blendShapeName, float newWeight) {
			BlendShapeChangeEvent evt = EventBase<BlendShapeChangeEvent>.GetPooled();
			evt.BlendShapeName = blendShapeName;
			evt.Weight = newWeight;
			return evt;
		}
		
	}
}