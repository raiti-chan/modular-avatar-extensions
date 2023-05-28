using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using raitichan.com.modular_avatar.extensions.Editor.ControllerFactories;
using raitichan.com.modular_avatar.extensions.Enums;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEngine;
using static raitichan.com.modular_avatar.extensions.Modules.MAExGestureExpressionAnimationGenerator;

namespace raitichan.com.modular_avatar.extensions.Editor.ModuleExtensions {
	public static class MAExGestureExpressionAnimationGeneratorExtension {
		private static readonly StringBuilder _LOGS = new StringBuilder();

		public static string GetLastLog() {
			return _LOGS.ToString();
		}

		public static void ImportAnimationClip(this MAExGestureExpressionAnimationGenerator target,
			AnimationClip animationClip, LeftAndRight leftAndRight, Gesture gesture) {
			target.GetGestureAnimation(leftAndRight, gesture).ImportAnimationClip(animationClip, target.GetFaceMesh());
		}

		public static void ImportAnimationClip(this GestureAnimationSet target,
			AnimationClip animationClip, SkinnedMeshRenderer faceMesh, Gesture gesture) {
			target.GetGestureAnimation(gesture).ImportAnimationClip(animationClip, faceMesh);
		}

		// ReSharper disable once MemberCanBePrivate.Global
		public static void ImportAnimationClip(this GestureAnimation target,
			AnimationClip animationClip, SkinnedMeshRenderer faceMesh) {
			_LOGS.Clear();
			Mesh sharedMesh = faceMesh.sharedMesh;
			target.blendShapeWeights = new BlendShapeWeight[sharedMesh.blendShapeCount];
			string faceMeshPath = MAExAnimatorFactoryUtils.GetBindingPath(faceMesh.transform);
			Regex regex = new Regex(@"blendShape\.(?<BlendShapeName>.+)");
			AnimationClip additionalClip = new AnimationClip();

			foreach (EditorCurveBinding editorCurveBinding in AnimationUtility.GetCurveBindings(animationClip)) {
				AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, editorCurveBinding);
				if (editorCurveBinding.path != faceMeshPath) {
					// 顔メッシュじゃない場合
					additionalClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type, editorCurveBinding.propertyName, curve);
					continue;
				}

				Match match = regex.Match(editorCurveBinding.propertyName);
				if (!match.Success) {
					// ブレンドシェイプアニメーションじゃない場合
					additionalClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type, editorCurveBinding.propertyName, curve);
					continue;
				}

				string blendShapeName = match.Groups["BlendShapeName"].Value;
				int blendShapeIndex = sharedMesh.GetBlendShapeIndex(blendShapeName);
				if (blendShapeIndex < 0) {
					// ブレンドシェイプが見つからない場合無視
					_LOGS.AppendLine($"Not found BlendShape : {blendShapeName}");
					continue;
				}

				float weight;
				switch (curve.length) {
					case 0:
						// キーフレームが見つからない
						_LOGS.AppendLine($"Not found KeyFrame : {editorCurveBinding.path}.{editorCurveBinding.propertyName}");
						continue;
					case 1:
						weight = curve[0].value;
						break;
					case 2:
						weight = curve.keys.MinBy(keyframe => keyframe.time).value;
						_LOGS.AppendLine($"Not Support Motion Time Animation : {editorCurveBinding.path}.{editorCurveBinding.propertyName}");
						// TODO: キーフレームが2つの場合前後を記録 (未実装)
						break;
					default:
						additionalClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type, editorCurveBinding.propertyName, curve);
						continue;
				}

				target.blendShapeWeights[blendShapeIndex].enable = true;
				target.blendShapeWeights[blendShapeIndex].weight = weight;
			}

			EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(additionalClip);
			EditorCurveBinding[] objectReferenceCurveBindings = AnimationUtility.GetObjectReferenceCurveBindings(additionalClip);
			if (curveBindings.Length <= 0 && objectReferenceCurveBindings.Length <= 0) return;
			if (!EditorUtility.DisplayDialog("Import",
				    "I found an animation other than facial expressions. Do you want to save them as additional animations?", "Yes", "No")) return;
			string path = EditorUtility.SaveFilePanelInProject("Save Additional", "AdditionalAnimation", "anim", "");
			AssetDatabase.CreateAsset(additionalClip, path);
			AssetDatabase.Refresh();
			target.additionalAnimationClipData.Add(new AdditionalAnimationClipData {
				animationClip = additionalClip
			});
		}

		public static AnimationClip ExportAnimationClip(this MAExGestureExpressionAnimationGenerator target,
			LeftAndRight leftAndRight, Gesture gesture) {
			return target.GetGestureAnimation(leftAndRight, gesture).ExportAnimationClip(target.GetFaceMesh());
		}

		public static AnimationClip ExportAnimationClip(this GestureAnimationSet target,
			SkinnedMeshRenderer faceMesh, Gesture gesture) {
			return target.GetGestureAnimation(gesture).ExportAnimationClip(faceMesh);
		}

		// ReSharper disable once MemberCanBePrivate.Global
		public static AnimationClip ExportAnimationClip(this GestureAnimation target, SkinnedMeshRenderer faceMesh) {
			AnimationClip animationClip = new AnimationClip();
			AnimationClipSettings animationClipSettings = AnimationUtility.GetAnimationClipSettings(animationClip);
			animationClipSettings.loopTime = target.isLoop;
			AnimationUtility.SetAnimationClipSettings(animationClip, animationClipSettings);
			string faceMeshPath = MAExAnimatorFactoryUtils.GetBindingPath(faceMesh.transform);
			int faceMeshBlendShapeCount = faceMesh.sharedMesh.blendShapeCount;

			for (int i = 0; i < target.blendShapeWeights.Length; i++) {
				if (!target.blendShapeWeights[i].enable) continue;
				if (faceMeshBlendShapeCount <= i) break;
				string blendShapeName = $"blendShape.{faceMesh.sharedMesh.GetBlendShapeName(i)}";
				AnimationCurve curve = new AnimationCurve();
				curve.AddKey(new Keyframe(0, target.blendShapeWeights[i].weight));
				animationClip.SetCurve(faceMeshPath, typeof(SkinnedMeshRenderer), blendShapeName, curve);
			}

			foreach (AnimationClip clip in
			         target.additionalAnimationClipData.Select(additionalAnimationClipData => additionalAnimationClipData.animationClip)) {
				foreach (EditorCurveBinding editorCurveBinding in AnimationUtility.GetCurveBindings(clip)) {
					AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, editorCurveBinding);
					animationClip.SetCurve(editorCurveBinding.path, editorCurveBinding.type, editorCurveBinding.propertyName, curve);
				}

				foreach (EditorCurveBinding objectReferenceCurveBinding in AnimationUtility.GetObjectReferenceCurveBindings(clip)) {
					ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, objectReferenceCurveBinding);
					AnimationUtility.SetObjectReferenceCurve(animationClip, objectReferenceCurveBinding, keyframes);
				}
			}

			return animationClip;
		}
	}
}