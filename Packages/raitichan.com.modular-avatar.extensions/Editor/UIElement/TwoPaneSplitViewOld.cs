using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public class TwoPaneSplitViewOld : VisualElement {
		private const string _USS_GUID = "399e3944d7ca4d279e8f53ba184d4d15";

		private const string _USS_CLASS_NAME = "unity-two-pane-split-view";
		private const string _CONTENT_CONTAINER_CLASS_NAME = "unity-two-pane-split-view__content-container";
		private const string _HANDLE_DRAG_LINE_CLASS_NAME = "unity-two-pane-split-view__dragline";
		private const string _HANDLE_DRAG_LINE_VERTICAL_CLASS_NAME = _HANDLE_DRAG_LINE_CLASS_NAME + "--vertical";
		private const string _HANDLE_DRAG_LINE_HORIZONTAL_CLASS_NAME = _HANDLE_DRAG_LINE_CLASS_NAME + "--horizontal";
		private const string _HANDLE_DRAG_LINE_ANCHOR_CLASS_NAME = "unity-two-pane-split-view__dragline-anchor";
		private const string _HANDLE_DRAG_LINE_ANCHOR_VERTICAL_CLASS_NAME = _HANDLE_DRAG_LINE_ANCHOR_CLASS_NAME + "--vertical";
		private const string _HANDLE_DRAG_LINE_ANCHOR_HORIZONTAL_CLASS_NAME = _HANDLE_DRAG_LINE_ANCHOR_CLASS_NAME + "--horizontal";
		private const string _VERTICAL_CLASS_NAME = "unity-two-pane-split-view--vertical";
		private const string _HORIZONTAL_CLASS_NAME = "unity-two-pane-split-view--horizontal";

		private static readonly StyleSheet _STYLE_SHEET;

		static TwoPaneSplitViewOld() {
			string ussPath = AssetDatabase.GUIDToAssetPath(_USS_GUID);
			_STYLE_SHEET = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
		}

		private readonly VisualElement _content;
		private VisualElement _leftPane;
		private VisualElement _rightPane;
		private readonly VisualElement _dragLine;
		private readonly VisualElement _dragLineAnchor;
		private TwoPaneSplitViewOrientation _orientation;
		private bool _collapseMode;
		private int _fixedPaneIndex;
		private float _fixedPaneInitialDimension;
		private TwoPaneSplitViewResizer _resizer;

		public VisualElement FixedPane { get; private set; }
		public VisualElement FlexedPane { get; private set; }


		/// <summary>
		/// 0 for setting first child as the fixed pane, 1 for the second child element.
		/// </summary>
		public int FixedPaneIndex {
			get => this._fixedPaneIndex;
			set {
				if (value == this._fixedPaneIndex)
					return;

				Init(value, this._fixedPaneInitialDimension, this._orientation);
			}
		}

		/// <summary>
		/// The initial width or height for the fixed pane.
		/// </summary>
		public float FixedPaneInitialDimension {
			get => this._fixedPaneInitialDimension;
			set {
				Init(this._fixedPaneIndex, value, this._orientation);
				this.FixedPaneCurrentDimension = value;
			}
		}

		/// <summary>
		/// Orientation of the split view.
		/// </summary>
		public TwoPaneSplitViewOrientation Orientation {
			get => this._orientation;
			set {
				if (value == this._orientation) return;
				Init(this._fixedPaneIndex, this._fixedPaneInitialDimension, value);
			}
		}

		public float FixedPaneCurrentDimension { get; private set; } = -1;

		public float FixedPaneDimension {
			get => string.IsNullOrEmpty(this.viewDataKey) ? this._fixedPaneInitialDimension : this.FixedPaneCurrentDimension;
			set {
				if (Math.Abs(value - this.FixedPaneCurrentDimension) < 0.000001f) return;
				this.FixedPaneCurrentDimension = value;
			}
		}

		public TwoPaneSplitViewOld() {
			AddToClassList(_USS_CLASS_NAME);
			this.styleSheets.Add(_STYLE_SHEET);

			this._content = new VisualElement {
				name = "unity-content-container",
				pickingMode = PickingMode.Ignore
			};
			this._content.AddToClassList(_CONTENT_CONTAINER_CLASS_NAME);
			this.hierarchy.Add(_content);

			// Create drag anchor line.
			this._dragLineAnchor = new VisualElement {
				name = "unity-dragline-anchor"
			};
			this._dragLineAnchor.AddToClassList(_HANDLE_DRAG_LINE_ANCHOR_CLASS_NAME);
			this.hierarchy.Add(this._dragLineAnchor);

			// Create drag
			this._dragLine = new VisualElement {
				name = "unity-dragline"
			};
			this._dragLine.AddToClassList(_HANDLE_DRAG_LINE_CLASS_NAME);
			this._dragLineAnchor.Add(_dragLine);
		}

		/// <summary>
		/// Parameterized constructor.
		/// </summary>
		/// <param name="fixedPaneIndex">0 for setting first child as the fixed pane, 1 for the second child element.</param>
		/// <param name="fixedPaneStartDimension">Set an initial width or height for the fixed pane.</param>
		/// <param name="orientation">Orientation of the split view.</param>
		public TwoPaneSplitViewOld(int fixedPaneIndex, float fixedPaneStartDimension, TwoPaneSplitViewOrientation orientation) : this() {
			this.Init(fixedPaneIndex, fixedPaneStartDimension, orientation);
		}

		/// <summary>
		/// Collapse one of the panes of the split view. This will hide the resizer and make the other child take up all available space.
		/// </summary>
		/// <param name="index">Index of child to collapse.</param>
		public void CollapseChild(int index) {
			if (this._leftPane == null)
				return;

			this._dragLine.style.display = DisplayStyle.None;
			this._dragLineAnchor.style.display = DisplayStyle.None;
			if (index == 0) {
				this._rightPane.style.width = StyleKeyword.Initial;
				this._rightPane.style.height = StyleKeyword.Initial;
				this._rightPane.style.flexGrow = 1;
				this._leftPane.style.display = DisplayStyle.None;
			} else {
				this._leftPane.style.width = StyleKeyword.Initial;
				this._leftPane.style.height = StyleKeyword.Initial;
				this._leftPane.style.flexGrow = 1;
				this._rightPane.style.display = DisplayStyle.None;
			}

			this._collapseMode = true;
		}

		/// <summary>
		/// Un-collapse the split view. This will restore the split view to the state it was before the previous collapse.
		/// </summary>
		public void UnCollapse() {
			if (this._leftPane == null)
				return;

			this._leftPane.style.display = DisplayStyle.Flex;
			this._rightPane.style.display = DisplayStyle.Flex;

			this._dragLine.style.display = DisplayStyle.Flex;
			this._dragLineAnchor.style.display = DisplayStyle.Flex;

			this._leftPane.style.flexGrow = 0;
			this._rightPane.style.flexGrow = 0;
			this._collapseMode = false;

			this.Init(_fixedPaneIndex, _fixedPaneInitialDimension, _orientation);
		}

		private void Init(int fixedPaneIndex, float fixedPaneInitialDimension, TwoPaneSplitViewOrientation orientation) {
			this._orientation = orientation;
			this._fixedPaneIndex = fixedPaneIndex;
			this._fixedPaneInitialDimension = fixedPaneInitialDimension;

			this._content.RemoveFromClassList(_HORIZONTAL_CLASS_NAME);
			this._content.RemoveFromClassList(_VERTICAL_CLASS_NAME);
			this._content.AddToClassList(this._orientation == TwoPaneSplitViewOrientation.Horizontal ? _HORIZONTAL_CLASS_NAME : _VERTICAL_CLASS_NAME);

			// Create drag anchor line.
			this._dragLineAnchor.RemoveFromClassList(_HANDLE_DRAG_LINE_ANCHOR_HORIZONTAL_CLASS_NAME);
			this._dragLineAnchor.RemoveFromClassList(_HANDLE_DRAG_LINE_ANCHOR_VERTICAL_CLASS_NAME);
			this._dragLineAnchor.AddToClassList(this._orientation == TwoPaneSplitViewOrientation.Horizontal
				? _HANDLE_DRAG_LINE_ANCHOR_HORIZONTAL_CLASS_NAME
				: _HANDLE_DRAG_LINE_ANCHOR_VERTICAL_CLASS_NAME);

			// Create drag
			this._dragLine.RemoveFromClassList(_HANDLE_DRAG_LINE_HORIZONTAL_CLASS_NAME);
			this._dragLine.RemoveFromClassList(_HANDLE_DRAG_LINE_VERTICAL_CLASS_NAME);
			this._dragLine.AddToClassList(this._orientation == TwoPaneSplitViewOrientation.Horizontal ? _HANDLE_DRAG_LINE_HORIZONTAL_CLASS_NAME : _HANDLE_DRAG_LINE_VERTICAL_CLASS_NAME);

			if (this._resizer != null) {
				this._dragLineAnchor.RemoveManipulator(this._resizer);
				this._resizer = null;
			}

			if (_content.childCount != 2) {
				this.RegisterCallback<GeometryChangedEvent>(this.OnPostDisplaySetup);
			} else {
				this.PostDisplaySetup();
			}
		}

		private void OnPostDisplaySetup(GeometryChangedEvent evt) {
			if (this._content.childCount != 2) {
				Debug.LogError("TwoPaneSplitView needs exactly 2 children.");
				return;
			}

			this.PostDisplaySetup();

			this.UnregisterCallback<GeometryChangedEvent>(this.OnPostDisplaySetup);
		}

		private void PostDisplaySetup() {
			if (this._content.childCount != 2) {
				Debug.LogError("TwoPaneSplitView needs exactly 2 children.");
				return;
			}

			if (this.FixedPaneDimension < 0) {
				this.FixedPaneDimension = this._fixedPaneInitialDimension;
			}

			float dimension = this.FixedPaneDimension;

			this._leftPane = this._content[0];
			if (this._fixedPaneIndex == 0) {
				this.FixedPane = this._leftPane;
			} else {
				this.FlexedPane = this._leftPane;
			}

			this._rightPane = this._content[1];
			if (this._fixedPaneIndex == 1) {
				this.FixedPane = this._rightPane;
			} else {
				this.FlexedPane = this._rightPane;
			}

			this.FixedPane.style.flexBasis = StyleKeyword.Null;
			this.FixedPane.style.flexShrink = StyleKeyword.Null;
			this.FixedPane.style.flexGrow = StyleKeyword.Null;
			this.FlexedPane.style.flexGrow = StyleKeyword.Null;
			this.FlexedPane.style.flexShrink = StyleKeyword.Null;
			this.FlexedPane.style.flexBasis = StyleKeyword.Null;

			this.FixedPane.style.width = StyleKeyword.Null;
			this.FixedPane.style.height = StyleKeyword.Null;
			this.FlexedPane.style.width = StyleKeyword.Null;
			this.FlexedPane.style.height = StyleKeyword.Null;

			if (this._orientation == TwoPaneSplitViewOrientation.Horizontal) {
				this.FixedPane.style.width = dimension;
				this.FixedPane.style.height = StyleKeyword.Null;
			} else {
				this.FixedPane.style.width = StyleKeyword.Null;
				this.FixedPane.style.height = dimension;
			}

			this.FixedPane.style.flexShrink = 0;
			this.FixedPane.style.flexGrow = 0;
			this.FlexedPane.style.flexGrow = 1;
			this.FlexedPane.style.flexShrink = 0;
			this.FlexedPane.style.flexBasis = 0;

			if (this._orientation == TwoPaneSplitViewOrientation.Horizontal) {
				if (this._fixedPaneIndex == 0) {
					this._dragLineAnchor.style.left = dimension;
				} else {
					this._dragLineAnchor.style.left = this.resolvedStyle.width - dimension;
				}
			} else {
				if (this._fixedPaneIndex == 0) {
					this._dragLineAnchor.style.top = dimension;
				} else {
					this._dragLineAnchor.style.top = this.resolvedStyle.height - dimension;
				}
			}

			int direction;
			if (this._fixedPaneIndex == 0) {
				direction = 1;
			} else {
				direction = -1;
			}

			this._resizer = new TwoPaneSplitViewResizer(this, direction, this._orientation);

			this._dragLineAnchor.AddManipulator(this._resizer);

			this.RegisterCallback<GeometryChangedEvent>(this.OnSizeChange);
		}

		private void OnSizeChange(GeometryChangedEvent evt) {
			this.OnSizeChange();
		}

		private void OnSizeChange() {
			if (this._collapseMode) return;

			float maxLength = this.resolvedStyle.width;
			float fixedPaneLength = this.FixedPane.resolvedStyle.width;
			float fixedPaneMinLength = this.FixedPane.resolvedStyle.minWidth.value;
			float flexedPaneMinLength = this.FlexedPane.resolvedStyle.minWidth.value;

			if (this._orientation == TwoPaneSplitViewOrientation.Vertical) {
				maxLength = this.resolvedStyle.height;
				fixedPaneLength = this.FixedPane.resolvedStyle.height;
				fixedPaneMinLength = this.FixedPane.resolvedStyle.minHeight.value;
				flexedPaneMinLength = this.FlexedPane.resolvedStyle.minHeight.value;
			}

			if (maxLength >= fixedPaneLength + flexedPaneMinLength) {
				// Big enough to account for current fixed pane size and flexed pane minimum size, so we let the layout 
				// dictates where the dragger should be.
				this.SetDragLineOffset(this._fixedPaneIndex == 0 ? fixedPaneLength : maxLength - fixedPaneLength);
			} else if (maxLength >= fixedPaneMinLength + flexedPaneMinLength) {
				// Big enough to account for fixed and flexed pane minimum sizes, so we resize the fixed pane and adjust
				// where the dragger should be.
				float newDimension = maxLength - flexedPaneMinLength;
				this.SetFixedPaneDimension(newDimension);
				this.SetDragLineOffset(this._fixedPaneIndex == 0 ? newDimension : flexedPaneMinLength);
			} else {
				// Not big enough for fixed and flexed pane minimum sizes
				this.SetFixedPaneDimension(fixedPaneMinLength);
				this.SetDragLineOffset(this._fixedPaneIndex == 0 ? fixedPaneMinLength : flexedPaneMinLength);
			}
		}

		public override VisualElement contentContainer => _content;

		private void SetDragLineOffset(float offset) {
			if (this._orientation == TwoPaneSplitViewOrientation.Horizontal) {
				this._dragLineAnchor.style.left = offset;
			} else {
				this._dragLineAnchor.style.top = offset;
			}
		}

		private void SetFixedPaneDimension(float dimension) {
			if (this._orientation == TwoPaneSplitViewOrientation.Horizontal) {
				this.FixedPane.style.width = dimension;
			} else {
				this.FixedPane.style.height = dimension;
			}
		}


		public new class UxmlFactory : UxmlFactory<TwoPaneSplitViewOld, UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits {
			private readonly UxmlIntAttributeDescription _fixedPaneIndex = new UxmlIntAttributeDescription {
				name = "fixed-pane-index",
				defaultValue = 0
			};
			private readonly UxmlIntAttributeDescription _fixedPaneInitialDimension = new UxmlIntAttributeDescription {
				name = "fixed-pane-initial-dimension",
				defaultValue = 100
			};
			private readonly UxmlEnumAttributeDescription<TwoPaneSplitViewOrientation> _orientation = new UxmlEnumAttributeDescription<TwoPaneSplitViewOrientation> {
				name = "orientation",
				defaultValue = TwoPaneSplitViewOrientation.Horizontal
			};

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription {
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
				base.Init(ve, bag, cc);
				int fixedPaneIndex = _fixedPaneIndex.GetValueFromBag(bag, cc);
				int fixedPaneInitialSize = _fixedPaneInitialDimension.GetValueFromBag(bag, cc);
				TwoPaneSplitViewOrientation orientation = _orientation.GetValueFromBag(bag, cc);

				((TwoPaneSplitViewOld)ve).Init(fixedPaneIndex, fixedPaneInitialSize, orientation);
			}
		}
	}

	/// <summary>
	/// Determines the orientation of the two resizable panes.
	/// </summary>
	public enum TwoPaneSplitViewOrientation {
		/// <summary>
		/// Split view panes layout is left/right with vertical resizable split.
		/// </summary>
		Horizontal,
		/// <summary>
		/// Split view panes layout is top/bottom with horizontal resizable split.
		/// </summary>
		Vertical
	}

	public class TwoPaneSplitViewResizer : MouseManipulator {
		private Vector2 _start;
		private bool _active;
		private readonly TwoPaneSplitViewOld _splitViewOld;

		private readonly int _direction;
		private readonly TwoPaneSplitViewOrientation _orientation;

		private VisualElement FixedPane => this._splitViewOld.FixedPane;
		private VisualElement FlexedPane => this._splitViewOld.FlexedPane;

		private float FixedPaneMinDimension {
			get {
				if (this._orientation == TwoPaneSplitViewOrientation.Horizontal) return this.FixedPane.resolvedStyle.minWidth.value;
				return this.FixedPane.resolvedStyle.minHeight.value;
			}
		}

		private float FlexedPaneMinDimension {
			get {
				if (this._orientation == TwoPaneSplitViewOrientation.Horizontal) return this.FlexedPane.resolvedStyle.minWidth.value;
				return this.FlexedPane.resolvedStyle.minHeight.value;
			}
		}

		public TwoPaneSplitViewResizer(TwoPaneSplitViewOld splitViewOld, int dir, TwoPaneSplitViewOrientation orientation) {
			this._orientation = orientation;
			this._splitViewOld = splitViewOld;
			this._direction = dir;
			this.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
			this._active = false;
		}

		protected override void RegisterCallbacksOnTarget() {
			this.target.RegisterCallback<MouseDownEvent>(this.OnMouseDown);
			this.target.RegisterCallback<MouseMoveEvent>(this.OnMouseMove);
			this.target.RegisterCallback<MouseUpEvent>(this.OnMouseUp);
		}

		protected override void UnregisterCallbacksFromTarget() {
			this.target.UnregisterCallback<MouseDownEvent>(this.OnMouseDown);
			this.target.UnregisterCallback<MouseMoveEvent>(this.OnMouseMove);
			this.target.UnregisterCallback<MouseUpEvent>(this.OnMouseUp);
		}

		private void ApplyDelta(float delta) {
			float oldDimension = this._orientation == TwoPaneSplitViewOrientation.Horizontal ? this.FixedPane.resolvedStyle.width : this.FixedPane.resolvedStyle.height;
			float newDimension = oldDimension + delta;

			if (newDimension < oldDimension && newDimension < this.FixedPaneMinDimension) {
				newDimension = this.FixedPaneMinDimension;
			}

			float maxDimension = this._orientation == TwoPaneSplitViewOrientation.Horizontal ? this._splitViewOld.resolvedStyle.width : this._splitViewOld.resolvedStyle.height;
			maxDimension -= this.FlexedPaneMinDimension;
			if (newDimension > oldDimension && newDimension > maxDimension) {
				newDimension = maxDimension;
			}

			if (this._orientation == TwoPaneSplitViewOrientation.Horizontal) {
				this.FixedPane.style.width = newDimension;
				if (this._splitViewOld.FixedPaneIndex == 0) {
					this.target.style.left = newDimension;
				} else {
					this.target.style.left = this._splitViewOld.resolvedStyle.width - newDimension;
				}
			} else {
				this.FixedPane.style.height = newDimension;
				if (this._splitViewOld.FixedPaneIndex == 0) {
					this.target.style.top = newDimension;
				} else {
					this.target.style.top = this._splitViewOld.resolvedStyle.height - newDimension;
				}
			}

			this._splitViewOld.FixedPaneDimension = newDimension;
		}

		private void OnMouseDown(MouseDownEvent e) {
			if (this._active) {
				e.StopImmediatePropagation();
				return;
			}

			if (!CanStartManipulation(e)) return;
			this._start = e.localMousePosition;

			this._active = true;
			this.target.CaptureMouse();
			e.StopPropagation();
		}

		private void OnMouseMove(MouseMoveEvent e) {
			if (!this._active || !this.target.HasMouseCapture())
				return;

			Vector2 diff = e.localMousePosition - _start;
			float mouseDiff = diff.x;
			if (_orientation == TwoPaneSplitViewOrientation.Vertical)
				mouseDiff = diff.y;

			float delta = _direction * mouseDiff;

			this.ApplyDelta(delta);

			e.StopPropagation();
		}

		private void OnMouseUp(MouseUpEvent e) {
			if (!this._active || !this.target.HasMouseCapture() || !CanStopManipulation(e))
				return;

			this._active = false;
			this.target.ReleaseMouse();
			e.StopPropagation();
		}
	}
}