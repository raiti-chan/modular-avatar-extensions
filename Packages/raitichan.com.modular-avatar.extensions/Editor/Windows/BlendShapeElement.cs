using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class BlendShapeElement : VisualElement {
		
		public int Index { get; set; }
		public string Text { get => this._slider.label; set => this._slider.label = value; }
		public float Value => this._slider.value;
		public bool Enable => this._toggle.value;

		public event Action<BlendShapeElement> onChangeValue;

		public event Action<BlendShapeElement> onChangeEnable;

		private readonly Toggle _toggle;
		private readonly Slider _slider;
		private readonly FloatField _float;

		public BlendShapeElement() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath("d74c809103c8f9e4e9a98252f7b440d8");
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this._toggle = this.Q<Toggle>("Toggle");
			this._toggle.RegisterValueChangedCallback(this.OnToggleChanged);
			
			this._slider = this.Q<Slider>("Slider");
			this._slider.RegisterValueChangedCallback(this.OnSliderChanged);
			
			this._float = this.Q<FloatField>("Float");
			this._float.RegisterValueChangedCallback(this.OnFloatChanged);
		}

		public void SetValueWithoutNotify(bool enable, float value) {
			this._toggle.value = enable;
			this._slider.value = value;
			this._float.value = value;
		}

		private void OnToggleChanged(ChangeEvent<bool> evt) {
			this.onChangeEnable?.Invoke(this);
		}

		private void OnSliderChanged(ChangeEvent<float> evt) {
			this._float.SetValueWithoutNotify(evt.newValue);
			this.onChangeValue?.Invoke(this);
		}
		
		
		private void OnFloatChanged(ChangeEvent<float> evt) {
			this._slider.SetValueWithoutNotify(evt.newValue);
			this.onChangeValue?.Invoke(this);
		}

	}
}