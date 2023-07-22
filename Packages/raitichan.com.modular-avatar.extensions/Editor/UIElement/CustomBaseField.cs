using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public abstract class CustomBaseField : VisualElement {
		public Label LabelElement { get; private set; }
		public string Label {
			get => this.LabelElement.text;
			set {
				if (this.LabelElement.text == value) return;
				this.LabelElement.text = value;
				if (string.IsNullOrEmpty(this.LabelElement.text)) {
					this.AddToClassList(BaseField<int>.noLabelVariantUssClassName);
					this.LabelElement.RemoveFromHierarchy();
				} else if (!this.Contains(this.LabelElement)) {
					this.Insert(0, this.LabelElement);
					this.RemoveFromClassList(BaseField<int>.noLabelVariantUssClassName);
				}
			}
		}

		private VisualElement _visualInput;

		public VisualElement VisualInput {
			get => this._visualInput;
			set {
				if (this._visualInput != null) {
					if (this._visualInput.parent == this) {
						this._visualInput.RemoveFromHierarchy();
					}
					this._visualInput = null;
				}

				if (value != null) {
					this._visualInput = value;
				} else {
					this._visualInput = new VisualElement {
						pickingMode = PickingMode.Ignore
					};
				}

				this._visualInput.focusable = true;
				this._visualInput.AddToClassList(BaseField<int>.inputUssClassName);
				this.Add(this._visualInput);
			}
		}

		protected CustomBaseField(string label) {
			// this.isCompositeRoot = true;
			// this.excludeFromFocusRing = true;
			// this.delegatesFocus = true;
			this.focusable = true;
			this.tabIndex = 0;
			this.AddToClassList(BaseField<int>.ussClassName);
			this.LabelElement = new Label {
				focusable = true,
				tabIndex = -1
			};
			this.LabelElement.AddToClassList(BaseField<int>.labelUssClassName);
			if (label != null) {
				this.Label = label;
			} else {
				this.AddToClassList(BaseField<int>.noLabelVariantUssClassName);
			}
		}

		protected CustomBaseField(string label, VisualElement visualInput) : this(label) {
			this.VisualInput = visualInput;
		}

		public new class UxmlTraits : VisualElement.UxmlTraits {
			private readonly UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription {
				name = "label"
			};

			protected UxmlTraits() {
				this.focusIndex.defaultValue = 0;
				this.focusable.defaultValue = true;
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
				base.Init(ve, bag, cc);
				if (!(ve is CustomBaseField customBaseField)) return;
				customBaseField.Label = this._label.GetValueFromBag(bag, cc);
			}
		}
	}
}