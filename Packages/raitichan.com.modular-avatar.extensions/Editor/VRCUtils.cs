using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.Editor {
	public static class VRCUtils {
		public static string GetPathInAvatar(Transform target) {
			Stack<Transform> parentStack = new Stack<Transform>();
			Transform current = target;

			do {
				if (current.gameObject.GetComponent<VRCAvatarDescriptor>() != null) break;
				parentStack.Push(current);
				current = current.parent;
			} while (current != null);

			if (current == null) return null;

			StringBuilder stringBuilder = new StringBuilder();
			while (parentStack.Count > 0) {
				current = parentStack.Pop();
				stringBuilder.Append(current.name);
				stringBuilder.Append('/');
			}

			stringBuilder.Remove(stringBuilder.Length - 1, 1);
			return stringBuilder.ToString();
		}
	}
}