using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public class MultiTagField : CustomBaseField {
		private TextElement _textElement;
		private VisualElement _arrowElement;

		public Func<IEnumerable<string>> CreateItemSource { get; set; }
		public Func<IEnumerable<string>> CreateSelectedItemSource { get; set; }
		public Action OnNone { get; set; }
		public Action<string, bool> OnTag { get; set; }
		public Action<string> OnCreate { get; set; }


		public MultiTagField() : base(null, null) {
			this.AddToClassList(BasePopupField<int, string>.ussClassName);
			this.LabelElement.AddToClassList(BasePopupField<int, string>.labelUssClassName);
			this.VisualInput.AddToClassList(BasePopupField<int, string>.inputUssClassName);

			this._textElement = new TagFieldTextElement {
				pickingMode = PickingMode.Ignore
			};
			this._textElement.AddToClassList(BasePopupField<int, string>.textUssClassName);
			this.VisualInput.Add(this._textElement);

			this._arrowElement = new VisualElement {
				pickingMode = PickingMode.Ignore
			};
			this._arrowElement.AddToClassList(BasePopupField<int, string>.arrowUssClassName);
			this.VisualInput.Add(this._arrowElement);
		}

		protected override void ExecuteDefaultActionAtTarget(EventBase evt) {
			base.ExecuteDefaultActionAtTarget(evt);
			if (evt == null) return;
			bool flag = false;
			switch (evt) {
				case KeyDownEvent keyDownEvent: {
					if (keyDownEvent.keyCode == KeyCode.Space ||
					    keyDownEvent.keyCode == KeyCode.KeypadEnter ||
					    keyDownEvent.keyCode == KeyCode.Return) {
						flag = true;
					}

					break;
				}
				case MouseDownEvent mouseDownEvent: {
					if (mouseDownEvent.button == 0 &&
					    this.VisualInput.ContainsPoint(this.VisualInput.WorldToLocal(mouseDownEvent.mousePosition))) {
						flag = true;
					}

					break;
				}
			}

			if (!flag) return;
			this.ShowMenu();
			evt.StopPropagation();
		}

		private void ShowMenu() {
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("None"), false, this.OnNoneClicked);
			menu.AddSeparator("");
			HashSet<string> selectedItem = new HashSet<string>(this.CreateSelectedItemSource());
			foreach (string itemName in this.CreateItemSource()) {
				bool selected = selectedItem.Contains(itemName);
				menu.AddItem(new GUIContent(itemName), selected, this.OnTagItemClicked, new Tuple<string, bool>(itemName, selected));
			}

			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Create new tag"), false, this.OnCreateNewTagClicked);
			menu.DropDown(this.VisualInput.worldBound);
		}

		public void TextUpdate() {
			string[] tags = this.CreateSelectedItemSource().ToArray();
			if (tags.Length <= 0) {
				this._textElement.text = "None";
				return;
			}

			if (tags.Length == 1) {
				this._textElement.text = tags[0];
				return;
			}

			this._textElement.text = "Multi Select";
		}

		private void OnNoneClicked() {
			this.OnNone?.Invoke();
			this.TextUpdate();
		}

		private void OnTagItemClicked(object itemNameAndSelected) {
			if (!(itemNameAndSelected is Tuple<string, bool> tuple)) return;
			(string itemName, bool selected) = tuple;
			this.OnTag?.Invoke(itemName, selected);
			this.TextUpdate();
		}

		private void OnCreateNewTagClicked() {
			string newTag = TagCreateWindow.ShowModalWindow(GUIUtility.GUIToScreenRect(this.VisualInput.worldBound));
			if (string.IsNullOrEmpty(newTag)) return;
			this.OnCreate?.Invoke(newTag);
			this.TextUpdate();
		}

		private class TagFieldTextElement : TextElement {
			protected override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode) {
				string textToMeasure = this.text;
				if (string.IsNullOrEmpty(textToMeasure)) {
					textToMeasure = " ";
				}

				return this.MeasureTextSize(textToMeasure, desiredWidth, widthMode, desiredHeight, heightMode);
			}
		}

		public class TagCreateWindow : EditorWindow {
			public static string ShowModalWindow(Rect position) {
				TagCreateWindow window = CreateInstance<TagCreateWindow>();
				window.titleContent = new GUIContent("Create Tag");
				position.size = new Vector2(300, 60);
				window.minSize = position.size;
				window.maxSize = position.size;
				window.position = position;
				window.ShowModalUtility();
				if (window._isCancel) {
					return null;
				}

				return window._text;
			}

			private string _text;
			private bool _isCancel = true;

			private void OnGUI() {
				GUILayout.FlexibleSpace();
				this._text = EditorGUILayout.TextField("Tag Name", this._text);
				GUILayout.Space(5);
				using (new GUILayout.HorizontalScope()) {
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Cancel")) {
						this.Close();
					}

					if (GUILayout.Button("Create")) {
						this._isCancel = false;
						this.Close();
					}
				}

				GUILayout.FlexibleSpace();
			}
		}

		public new class UxmlFactory : UxmlFactory<MultiTagField, UxmlTraits> { }

		public new class UxmlTraits : CustomBaseField.UxmlTraits { }
	}
}