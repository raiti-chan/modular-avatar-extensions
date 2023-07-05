using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.UIElement {
	public static class UIElementUtils {
		public static IEnumerable<VisualElement> TraverseChildren(this VisualElement target, Func<VisualElement, bool> isIncludeChildren = null) {
			Queue<VisualElement> queue = new Queue<VisualElement>(target.hierarchy.Children());

			while (queue.Count > 0) {
				VisualElement current = queue.Dequeue();
				yield return current;
				if (isIncludeChildren != null && !isIncludeChildren(current)) continue;
				foreach (VisualElement child in current.hierarchy.Children()) {
					queue.Enqueue(child);
				}
			}
		}
	}
}