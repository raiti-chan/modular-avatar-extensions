using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public abstract class PointerManipulator : MouseManipulator {
		private int _currentPointerId;

		protected bool CanStartManipulation(IPointerEvent e) {
			if (!this.activators.Any(filter => Matches(filter, e))) return false;
			this._currentPointerId = e.pointerId;
			return true;
		}

		protected bool CanStopManipulation(IPointerEvent e) {
			if (e == null) return false;
			return e.pointerId == this._currentPointerId;
		}

		private static bool Matches(ManipulatorActivationFilter filter, IPointerEvent e) {
			if (e == null) return false;
			bool minClickCount = (filter.clickCount == 0 || e.clickCount > filter.clickCount);
			return filter.button == (MouseButton)e.button && HasModifiers(filter, e) && minClickCount;
		}

		private static bool HasModifiers(ManipulatorActivationFilter filter, IPointerEvent e) {
			if (e == null) return false;
			return MatchModifiers(filter, e.altKey, e.ctrlKey, e.shiftKey, e.commandKey);
		}

		private static bool MatchModifiers(ManipulatorActivationFilter filter, bool alt, bool ctrl, bool shift, bool command) {
			EventModifiers modifiers = filter.modifiers;
			bool filterAlt = (modifiers & EventModifiers.Alt) != 0;
			if (filterAlt != alt) {
				return false;
			}

			bool filterCtrl = (modifiers & EventModifiers.Control) != 0;
			if (filterCtrl != ctrl) {
				return false;
			}

			bool filterShift = (modifiers & EventModifiers.Shift) != 0;
			if (filterShift != shift) {
				return false;
			}

			bool filterCommand = (modifiers & EventModifiers.Command) != 0;
			if (filterCommand != command) {
				return false;
			}

			return true;
		}
	}
}