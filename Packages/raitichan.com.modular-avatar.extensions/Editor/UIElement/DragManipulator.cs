using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public class DragManipulator : PointerManipulator {
		private bool _active;
		private Vector3 _start;

		protected override void RegisterCallbacksOnTarget() {
			this.target.RegisterCallback<PointerDownEvent>(this.OnPointerDown);
			this.target.RegisterCallback<PointerMoveEvent>(this.OnPointerMove);
			this.target.RegisterCallback<PointerUpEvent>(this.OnPointerUp);
		}

		protected override void UnregisterCallbacksFromTarget() {
			this.target.UnregisterCallback<PointerDownEvent>(this.OnPointerDown);
			this.target.UnregisterCallback<PointerMoveEvent>(this.OnPointerMove);
			this.target.UnregisterCallback<PointerUpEvent>(this.OnPointerUp);
		}

		private void OnPointerDown(PointerDownEvent e) {
			if (this._active) {
				e.StopImmediatePropagation();
				return;
			}

			if (!this.CanStartManipulation(e)) return;
			this._active = true;
			this._start = e.localPosition;

			target.CapturePointer(e.pointerId);
			e.StopPropagation();
		}

		private void OnPointerMove(PointerMoveEvent e) {
			if (!this._active || !target.HasPointerCapture(e.pointerId)) return;

			Vector3 diff = this._start - e.localPosition;
			Vector3 delta = diff * -1;
			this.ApplyDelta(delta);
			e.StopPropagation();
		}

		private void OnPointerUp(PointerUpEvent e) {
			if (!this._active || !target.HasPointerCapture(e.pointerId) || !this.CanStopManipulation(e)) return;

			this._active = false;
			target.ReleasePointer(e.pointerId);
			e.StopPropagation();
		}

		protected virtual void ApplyDelta(Vector3 delta) {
			target.style.left = target.resolvedStyle.left + delta.x;
			target.style.top = target.resolvedStyle.top + delta.y;
		}
	}
}