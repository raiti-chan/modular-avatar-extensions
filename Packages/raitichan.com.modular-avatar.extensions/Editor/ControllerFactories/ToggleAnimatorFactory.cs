using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.Serializable;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class ToggleAnimatorFactory : ControllerFactoryBase<MAExToggleAnimatorGenerator> {
		private readonly List<BlendShapeData> _blendShapeData = new List<BlendShapeData>();
		
		public override void PreProcess(BuildContext context) {
			this._blendShapeData.Clear();
			this._blendShapeData.AddRange(BlendShapeData.GetAllSkinnedMeshRenderer(this.Target.blendShapeDataList)
				.Select(renderer => new BlendShapeData {
					skinnedMeshRenderer = renderer,
					blendShapeIndexAndWeights = BlendShapeData.GetAllIndexAndWeight(this.Target.blendShapeDataList, renderer).ToList()
				}));
		}

		public override RuntimeAnimatorController CreateController(BuildContext context) {
			AnimatorController controller = new AnimatorController();
			AssetDatabase.AddObjectToAsset(controller, context.AssetContainer);
			string path = MAExAnimatorFactoryUtils.GetBindingPath(this.Target.transform);
			{
				if (this.Target.GetComponent<ModularAvatarBoneProxy>() is { } boneProxy) {
					// BoneProxyがあった場合BoneProxyの参照先をパスにする
					// TODO: 通常BoneProxyが解決してくれる筈だけど動かない原因を突き止める
					if (boneProxy.target != null) {
						path = $"{MAExAnimatorFactoryUtils.GetBindingPath(boneProxy.target)}/{Target.name}";
					}
				}
			}

			List<string> additionalPaths = new List<string>();
			foreach (GameObject additionalTarget in this.Target.additionalToggleObjects) {
				if (additionalTarget.GetComponent<ModularAvatarBoneProxy>() is { } boneProxy) {
					// BoneProxyがあった場合BoneProxyの参照先をパスにする
					// TODO: 通常BoneProxyが解決してくれる筈だけど動かない原因を突き止める
					if (boneProxy.target != null) {
						additionalPaths.Add($"{MAExAnimatorFactoryUtils.GetBindingPath(boneProxy.target)}/{additionalTarget.name}");
					}
				} else {
					additionalPaths.Add($"{MAExAnimatorFactoryUtils.GetBindingPath(additionalTarget.transform)}");
				}
			}

			List<string> additionalInvertPaths = new List<string>();
			foreach (GameObject additionalTarget in this.Target.additionalInvertToggleObjects) {
				if (additionalTarget.GetComponent<ModularAvatarBoneProxy>() is { } boneProxy) {
					// BoneProxyがあった場合BoneProxyの参照先をパスにする
					// TODO: 通常BoneProxyが解決してくれる筈だけど動かない原因を突き止める
					if (boneProxy.target != null) {
						additionalInvertPaths.Add($"{MAExAnimatorFactoryUtils.GetBindingPath(boneProxy.target)}/{additionalTarget.name}");
					}
				} else {
					additionalInvertPaths.Add($"{MAExAnimatorFactoryUtils.GetBindingPath(additionalTarget.transform)}");
				}
			}

			AnimationClip offClip = new AnimationClip { name = $"{this.Target.parameterName}_OFF" };
			{
				AnimationCurve offCurve = new AnimationCurve();
				offCurve.AddKey(new Keyframe(0, 0));
				offClip.SetCurve(path, typeof(GameObject), "m_IsActive", offCurve);
			}
			this.AddBlendShapeCurve(offClip, this.Target.isInvert == this.Target.defaultValue);
			foreach (string additionalPath in additionalPaths) {
				AnimationCurve offCurve = new AnimationCurve();
				offCurve.AddKey(new Keyframe(0, 0));
				offClip.SetCurve(additionalPath, typeof(GameObject), "m_IsActive", offCurve);
			}
			foreach (string additionalInvertPath in additionalInvertPaths) {
				AnimationCurve offCurve = new AnimationCurve();
				offCurve.AddKey(new Keyframe(0, 1));
				offClip.SetCurve(additionalInvertPath, typeof(GameObject), "m_IsActive", offCurve);
			}
			AssetDatabase.AddObjectToAsset(offClip, controller);

			AnimationClip onClip = new AnimationClip { name = $"{this.Target.parameterName}_ON" };
			{
				AnimationCurve onCurve = new AnimationCurve();
				onCurve.AddKey(new Keyframe(0, 1));
				onClip.SetCurve(path, typeof(GameObject), "m_IsActive", onCurve);
			}
			this.AddBlendShapeCurve(onClip, this.Target.isInvert != this.Target.defaultValue);
			foreach (string additionalPath in additionalPaths) {
				AnimationCurve onCurve = new AnimationCurve();
				onCurve.AddKey(new Keyframe(0, 1));
				onClip.SetCurve(additionalPath, typeof(GameObject), "m_IsActive", onCurve);
			}
			foreach (string additionalInvertPath in additionalInvertPaths) {
				AnimationCurve onCurve = new AnimationCurve();
				onCurve.AddKey(new Keyframe(0, 0));
				onClip.SetCurve(additionalInvertPath, typeof(GameObject), "m_IsActive", onCurve);
			}
			AssetDatabase.AddObjectToAsset(onClip, controller);


			MAExAnimatorFactoryUtils.CreateToggleLayerToAnimatorController(controller, this.Target.parameterName, offClip, onClip, this.Target.isInvert,
				this.Target.defaultValue);

			return controller;
		}

		public override void PostProcess(BuildContext context) {
			GameObject targetObject = this.Target.gameObject;
			ModularAvatarMenuInstaller menuInstaller = targetObject.GetComponent<ModularAvatarMenuInstaller>();
			if (menuInstaller == null) menuInstaller = targetObject.AddComponent<ModularAvatarMenuInstaller>();
			menuInstaller.menuToAppend = null;

			ModularAvatarMenuItem menuItem = targetObject.GetComponent<ModularAvatarMenuItem>();
			if (menuItem == null && targetObject.GetComponent<ModularAvatarMenuGroup>() == null) {
				targetObject.AddComponent<ModularAvatarMenuGroup>();
			}

			if (menuItem == null) {
				GameObject menuItemObject = new GameObject(this.Target.displayName);
				menuItemObject.transform.SetParent(targetObject.transform);
				menuItem = menuItemObject.AddComponent<ModularAvatarMenuItem>();
			}

			menuItem.Control.icon = this.Target.menuIcon;
			menuItem.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
			menuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter { name = this.Target.parameterName };
			/*
			VRCExpressionsMenu expressionsMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
			expressionsMenu.controls.Add(new VRCExpressionsMenu.Control {
				name = this.Target.displayName,
				icon = this.Target.menuIcon,
				type = VRCExpressionsMenu.Control.ControlType.Toggle,
				parameter = new VRCExpressionsMenu.Control.Parameter { name = this.Target.parameterName },
			});
			AssetDatabase.AddObjectToAsset(expressionsMenu, context.AssetContainer);
			menuInstaller.menuToAppend = expressionsMenu;
			*/

			ModularAvatarParameters parameters = targetObject.GetComponent<ModularAvatarParameters>();
			if (parameters == null) parameters = targetObject.AddComponent<ModularAvatarParameters>();
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
			
			foreach (GameObject additionalToggleObject in this.Target.additionalToggleObjects) {
				additionalToggleObject.SetActive(this.Target.isInvert != this.Target.defaultValue);
			}
			
			foreach (GameObject additionalInvertToggleObject in this.Target.additionalInvertToggleObjects) {
				additionalInvertToggleObject.SetActive(this.Target.isInvert == this.Target.defaultValue);
			}

			// ブレンドシェイプは、現在の状態をデフォルトとするのでいらない
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

	}
}