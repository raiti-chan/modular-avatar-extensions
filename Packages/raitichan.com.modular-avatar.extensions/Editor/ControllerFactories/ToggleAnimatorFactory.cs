using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using static raitichan.com.modular_avatar.extensions.Serializable.BlendShapeData;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class ToggleAnimatorFactory : IRuntimeAnimatorFactory<MAExToggleAnimatorGenerator> {
		public MAExToggleAnimatorGenerator Target { get; set; }

		public void PreProcess(GameObject avatarGameObject) { }

		public RuntimeAnimatorController CreateController(GameObject avatarGameObject) {
			AnimatorController controller = UtilHelper.CreateAnimator();
			string path = MAExAnimatorFactoryUtils.GetBindingPath(this.Target.transform);

			AnimationClip offClip = new AnimationClip { name = $"{this.Target.parameterName}_OFF" };
			AnimationCurve offCurve = new AnimationCurve();
			offCurve.AddKey(new Keyframe(0, 0));
			offClip.SetCurve(path, typeof(GameObject), "m_IsActive", offCurve);
			this.AddBlendShapeCurve(offClip, this.Target.isInvert);
			AssetDatabase.AddObjectToAsset(offClip, controller);

			AnimationClip onClip = new AnimationClip { name = $"{this.Target.parameterName}_ON" };
			AnimationCurve onCurve = new AnimationCurve();
			onCurve.AddKey(new Keyframe(0, 1));
			onClip.SetCurve(path, typeof(GameObject), "m_IsActive", onCurve);
			this.AddBlendShapeCurve(onClip, !this.Target.isInvert);
			AssetDatabase.AddObjectToAsset(onClip, controller);


			MAExAnimatorFactoryUtils.CreateToggleLayerToAnimatorController(controller, this.Target.parameterName, offClip, onClip, this.Target.isInvert);
			return controller;
		}

		private void AddBlendShapeCurve(AnimationClip clip, bool isNotDefault) {
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in GetAllSkinnedMeshRenderer(this.Target.blendShapeDataList)) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(skinnedMeshRenderer.transform);
				foreach (BlendShapeIndexAndWeight indexAndWight in GetAllIndexAndWights(this.Target.blendShapeDataList, skinnedMeshRenderer)) {
					string blendShapeName = $"blendShape.{skinnedMeshRenderer.sharedMesh.GetBlendShapeName(indexAndWight.index)}";
					AnimationCurve curve = new AnimationCurve();
					curve.AddKey(new Keyframe(0, isNotDefault ? indexAndWight.weight : skinnedMeshRenderer.GetBlendShapeWeight(indexAndWight.index)));
					clip.SetCurve(path, typeof(SkinnedMeshRenderer), blendShapeName, curve);
				}
			}
		}

		public void PostProcess(GameObject avatarGameObject) {
			GameObject targetObject = this.Target.gameObject;
			ModularAvatarMenuInstaller menuInstaller = targetObject.GetComponent<ModularAvatarMenuInstaller>();
			VRCExpressionsMenu expressionsMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
			expressionsMenu.controls.Add(new VRCExpressionsMenu.Control {
				name = this.Target.displayName,
				icon = this.Target.menuIcon,
				type = VRCExpressionsMenu.Control.ControlType.Toggle,
				parameter = new VRCExpressionsMenu.Control.Parameter { name = this.Target.parameterName },
			});
			AssetDatabase.CreateAsset(expressionsMenu, UtilHelper.GenerateAssetPath());
			menuInstaller.menuToAppend = expressionsMenu;

			ModularAvatarParameters parameters = targetObject.GetComponent<ModularAvatarParameters>();
			parameters.parameters.Add(new ParameterConfig {
				nameOrPrefix = this.Target.parameterName,
				internalParameter = this.Target.isInternal,
				isPrefix = false,
				syncType = ParameterSyncType.Bool,
				saved = this.Target.saved,
				defaultValue = this.Target.defaultValue ? 1 : 0
			});
		}
	}
}