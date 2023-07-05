using System.Collections.Generic;
using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public class ReadOnlyObjectField : BaseField<Object> {
		private System.Type _objectType;
		private readonly ReadOnlyObjectFieldDisplay _objectFieldDisplay;

		public System.Type objectType {
			set {
				if (this._objectType == value) return;
				this._objectType = value;
				this._objectFieldDisplay.Update();
			}
			get => this._objectType;
		}

		public ReadOnlyObjectField() : this(null) { }

		private ReadOnlyObjectField(string label) : base(label, null) {
			this.labelElement.focusable = false;
			this.AddToClassList(ObjectField.ussClassName);
			this.labelElement.AddToClassList(ObjectField.labelUssClassName);

			this._objectFieldDisplay = new ReadOnlyObjectFieldDisplay(this) {
				focusable = false
			};
			this._objectFieldDisplay.AddToClassList(ObjectField.objectUssClassName);

			VisualElement visualInput = BaseFieldHelper<Object>.VisualInput(this);
			visualInput.focusable = false;
			visualInput.AddToClassList(ObjectField.inputUssClassName);
			visualInput.Add(this._objectFieldDisplay);
		}

		private Object TryReadComponentFromGameObject(Object obj, System.Type type) {
			GameObject gameObject = obj as GameObject;
			if (gameObject == null || !type.IsSubclassOf(typeof(Component))) return obj;
			Component component = gameObject.GetComponent(this.objectType);
			if (component == null) return obj;
			return component;
		}

		public override void SetValueWithoutNotify(Object newValue) {
			newValue = this.TryReadComponentFromGameObject(newValue, this.objectType);
			bool flag = !EqualityComparer<Object>.Default.Equals(this.value, newValue);
			base.SetValueWithoutNotify(newValue);
			if (!flag) return;
			this._objectFieldDisplay.Update();
		}

		public new class UxmlFactory : UxmlFactory<ReadOnlyObjectField, UxmlTraits> { }

		public new class UxmlTraits : BaseField<Object>.UxmlTraits { }


		private class ReadOnlyObjectFieldDisplay : VisualElement {
			private const string USS_CLASS_NAME = "unity-object-field-display";
			private const string ICON_USS_CLASS_NAME = USS_CLASS_NAME + "__icon";
			private const string LABEL_USS_CLASS_NAME = USS_CLASS_NAME + "__label";


			private readonly ReadOnlyObjectField _objectField;
			private readonly Image _objectIcon;
			private readonly Label _objectLabel;

			public ReadOnlyObjectFieldDisplay(ReadOnlyObjectField readOnlyObjectField) {
				this.AddToClassList(USS_CLASS_NAME);

				this._objectIcon = new Image {
					scaleMode = ScaleMode.ScaleAndCrop,
					pickingMode = PickingMode.Ignore
				};
				this._objectIcon.AddToClassList(ICON_USS_CLASS_NAME);

				this._objectLabel = new Label {
					pickingMode = PickingMode.Ignore
				};
				this._objectLabel.AddToClassList(LABEL_USS_CLASS_NAME);

				this._objectField = readOnlyObjectField;
				this.Update();
				this.Add(this._objectIcon);
				this.Add(this._objectLabel);
			}

			public void Update() {
				GUIContent guiContent = EditorGUIUtility.ObjectContent(this._objectField.value, this._objectField.objectType);
				this._objectIcon.image = guiContent.image;
				this._objectLabel.text = guiContent.text;
			}

			protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
				base.ExecuteDefaultActionAtTarget(evt);
				if (!(evt is MouseDownEvent mouseDownEvent)) return;
				if (mouseDownEvent.button != 0) return;
				Object gameObject = this._objectField.value;
				if (gameObject is Component component) {
					gameObject = component.gameObject;
				}

				if (gameObject == null) return;

				switch (mouseDownEvent.clickCount) {
					case 1:
						if (!mouseDownEvent.shiftKey && !mouseDownEvent.ctrlKey) {
							EditorGUIUtility.PingObject(gameObject);
						}

						evt.StopPropagation();
						break;
					case 2:
						AssetDatabase.OpenAsset(gameObject);
						GUIUtility.ExitGUI();
						evt.StopPropagation();
						break;
				}
			}
		}
	}
}