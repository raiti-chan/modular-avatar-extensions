using raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers;
using UnityEditor;
using UnityEditor.Animations;

namespace raitichan.com.modular_avatar.extensions.Editor {
	public static class MAExUtils {
		public static AnimatorController CreateAnimator() {
			AnimatorController controller = new AnimatorController();
			AssetDatabase.CreateAsset(controller, UtilHelper.GenerateAssetPath());
			return controller;
		}
	}
}