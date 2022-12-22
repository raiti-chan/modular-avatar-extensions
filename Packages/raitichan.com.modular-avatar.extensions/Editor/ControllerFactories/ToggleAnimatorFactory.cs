using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.Serializable;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class ToggleAnimatorFactory : IRuntimeAnimatorFactory<MAExToggleAnimatorGenerator> {
		public MAExToggleAnimatorGenerator Target { get; set; }

		private readonly List<BlendShapeData> _blendShapeData = new List<BlendShapeData>();

		public void PreProcess(GameObject avatarGameObject) {
			this._blendShapeData.Clear();
			this._blendShapeData.AddRange(BlendShapeData.GetAllSkinnedMeshRenderer(this.Target.blendShapeDataList)
				.Select(renderer => new BlendShapeData {
					skinnedMeshRenderer = renderer,
					blendShapeIndexAndWeights = BlendShapeData.GetAllIndexAndWeight(this.Target.blendShapeDataList, renderer).ToList()
				}));
		}

		public RuntimeAnimatorController CreateController(GameObject avatarGameObject) {
			AnimatorController controller = UtilHelper.CreateAnimator();
			string path = MAExAnimatorFactoryUtils.GetBindingPath(this.Target.transform);

			AnimationClip offClip = new AnimationClip { name = $"{this.Target.parameterName}_OFF" };
			AnimationCurve offCurve = new AnimationCurve();
			offCurve.AddKey(new Keyframe(0, 0));
			offClip.SetCurve(path, typeof(GameObject), "m_IsActive", offCurve);
			this.AddBlendShapeCurve(offClip, this.Target.isInvert == this.Target.defaultValue);
			AssetDatabase.AddObjectToAsset(offClip, controller);

			AnimationClip onClip = new AnimationClip { name = $"{this.Target.parameterName}_ON" };
			AnimationCurve onCurve = new AnimationCurve();
			onCurve.AddKey(new Keyframe(0, 1));
			onClip.SetCurve(path, typeof(GameObject), "m_IsActive", onCurve);
			this.AddBlendShapeCurve(onClip, this.Target.isInvert != this.Target.defaultValue);
			AssetDatabase.AddObjectToAsset(onClip, controller);


			MAExAnimatorFactoryUtils.CreateToggleLayerToAnimatorController(controller, this.Target.parameterName, offClip, onClip, this.Target.isInvert,
				this.Target.defaultValue);

			return controller;
		}

		private void AddBlendShapeCurve(AnimationClip clip, bool isDefault) {
			foreach (BlendShapeData blendShapeData in this._blendShapeData) {
				SkinnedMeshRenderer renderer = blendShapeData.skinnedMeshRenderer;
				string path = MAExAnimatorFactoryUtils.GetBindingPath(renderer.transform);
				foreach (BlendShapeData.BlendShapeIndexAndWeight indexAndWight in blendShapeData.blendShapeIndexAndWeights) {
					string blendShapeName = $"blendShape.{renderer.sharedMesh.GetBlendShapeName(indexAndWight.index)}";
					AnimationCurve curve = new AnimationCurve();
					curve.AddKey(new Keyframe(0, isDefault ? renderer.GetBlendShapeWeight(indexAndWight.index) : indexAndWight.weight));
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

			// デフォルトの状態に設定(Animatorをブロックされている際にもデフォルト状態で表示されるように)
			// オブジェクト
			targetObject.SetActive(this.Target.isInvert != this.Target.defaultValue);

			// ブレンドシェイプは、現在の状態をデフォルトとするのでいらない
		}
	}
}