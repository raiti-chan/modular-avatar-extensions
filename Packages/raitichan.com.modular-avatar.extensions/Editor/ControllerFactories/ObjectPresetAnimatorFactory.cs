using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using raitichan.com.modular_avatar.extensions.Editor.ReflectionHelper.ModularAvatar;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.Serializable;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class ObjectPresetAnimatorFactory : ControllerFactoryBase<MAExObjectPresetAnimatorGenerator> {
		private static readonly Texture2D _moreIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
			"Packages/nadena.dev.modular-avatar/Runtime/Icons/Icon_More_A.png"
		);

		private static readonly Texture2D _folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
			"Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Expressions Menu/Icons/item_folder.png"
		);
		
		private readonly HashSet<GameObject> _presetObjects = new HashSet<GameObject>();
		private readonly Dictionary<SkinnedMeshRenderer, HashSet<int>> _usedBlendShapeIndexesDictionary = new Dictionary<SkinnedMeshRenderer, HashSet<int>>();


		public override void PreProcess(BuildContext context) {
			// オンオフの切り替わるオブジェクト一覧の取得
			this._presetObjects.Clear();
			IEnumerable<GameObject> presetObjects = this.Target.presetData.SelectMany(presetData => presetData.enableObjects);
			foreach (GameObject enableObject in presetObjects.Where(obj => obj != null)) {
				this._presetObjects.Add(enableObject);
			}

			// デフォルトのブレンドシェイプを使用するシェイプキーのindexの取得
			this._usedBlendShapeIndexesDictionary.Clear();
			IEnumerable<BlendShapeData> presetBlendShapes = this.Target.presetData.SelectMany(presetData => presetData.blendShapes);
			IEnumerable<BlendShapeData> toggleBlendShapes = this.Target.toggleSetData.SelectMany(toggleSetData => toggleSetData.blendShapes);
			foreach (BlendShapeData blendShapeData in presetBlendShapes.Concat(toggleBlendShapes).Where(data => data.skinnedMeshRenderer != null)) {
				if (!this._usedBlendShapeIndexesDictionary.TryGetValue(blendShapeData.skinnedMeshRenderer, out HashSet<int> blendShapeIndexSet)) {
					blendShapeIndexSet = new HashSet<int>();
					this._usedBlendShapeIndexesDictionary[blendShapeData.skinnedMeshRenderer] = blendShapeIndexSet;
				}

				foreach (BlendShapeData.BlendShapeIndexAndWeight blendShapeIndexAndWeight in blendShapeData.blendShapeIndexAndWeights) {
					blendShapeIndexSet.Add(blendShapeIndexAndWeight.index);
				}
			}
		}

		public override RuntimeAnimatorController CreateController(BuildContext context) {
			AnimatorController controller = new AnimatorController();
			AssetDatabase.AddObjectToAsset(controller, context.AssetContainer);

			// プリセットアニメーションの生成
			AnimationClip[] presetClips = new AnimationClip[this.Target.presetData.Count];
			for (int i = 0; i < presetClips.Length; i++) {
				AnimationClip clip = new AnimationClip {
					name = this.Target.presetData[i].displayName
				};

				// オブジェクトon/offアニメーションの生成
				foreach (GameObject usedObject in this._presetObjects) {
					string path = MAExAnimatorFactoryUtils.GetBindingPath(usedObject.transform);
					if (usedObject.GetComponent<ModularAvatarBoneProxy>() is ModularAvatarBoneProxy boneProxy) {
						// BoneProxyがあった場合BoneProxyの参照先をパスにする
						// TODO: 通常BoneProxyが解決してくれる筈だけど動かない原因を突き止める
						if (boneProxy.target != null) {
							path = $"{MAExAnimatorFactoryUtils.GetBindingPath(boneProxy.target)}/{usedObject.name}";
						}
					}
					AnimationCurve curve = new AnimationCurve();
					curve.AddKey(new Keyframe(0, this.Target.presetData[i].enableObjects.Contains(usedObject) ? 1 : 0));
					clip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
				}

				// ブレンドシェイプアニメーションの生成
				foreach (KeyValuePair<SkinnedMeshRenderer, HashSet<int>> writeBlendShapeIndexSet in this._usedBlendShapeIndexesDictionary) {
					SkinnedMeshRenderer renderer = writeBlendShapeIndexSet.Key;
					string path = MAExAnimatorFactoryUtils.GetBindingPath(renderer.transform);
					Dictionary<int, float> weightDictionary =
						BlendShapeData.GetWeightDictionary(this.Target.presetData[i].blendShapes, renderer);
					foreach (int blendShapeIndex in writeBlendShapeIndexSet.Value) {
						string blendShapeName = $"blendShape.{renderer.sharedMesh.GetBlendShapeName(blendShapeIndex)}";
						AnimationCurve curve = new AnimationCurve();
						if (!weightDictionary.TryGetValue(blendShapeIndex, out float weight)) {
							weight = renderer.GetBlendShapeWeight(blendShapeIndex);
						}

						curve.AddKey(new Keyframe(0, weight));
						clip.SetCurve(path, typeof(SkinnedMeshRenderer), blendShapeName, curve);
					}
				}

				AssetDatabase.AddObjectToAsset(clip, controller);
				presetClips[i] = clip;
			}

			MAExAnimatorFactoryUtils.CreateSelectStateLayerToAnimatorController(controller, this.Target.parameterName, presetClips);

			// トグルアニメーションの生成
			AnimationClip[] toggleClips = new AnimationClip[this.Target.toggleSetData.Count];
			for (int i = 0; i < toggleClips.Length; i++) {
				MAExObjectPresetAnimatorGenerator.ToggleSetData toggleSetData = this.Target.toggleSetData[i];
				AnimationClip offClip = new AnimationClip {
					name = $"{toggleSetData.displayName}_OFF"
				};
				AnimationClip onClip = null;

				// オブジェクトoffアニメーションの生成
				foreach (GameObject toggleObject in toggleSetData.toggleObjects) {
					string path = MAExAnimatorFactoryUtils.GetBindingPath(toggleObject.transform);
					if (toggleObject.GetComponent<ModularAvatarBoneProxy>() is ModularAvatarBoneProxy boneProxy) {
						// BoneProxyがあった場合BoneProxyの参照先をパスにする
						// TODO: 通常BoneProxyが解決してくれる筈だけど動かない原因を突き止める
						if (boneProxy.target != null) {
							path = $"{MAExAnimatorFactoryUtils.GetBindingPath(boneProxy.target)}/{toggleObject.name}";
						}
					}
					AnimationCurve offCurve = new AnimationCurve();
					offCurve.AddKey(new Keyframe(0, 0));
					offClip.SetCurve(path, typeof(GameObject), "m_IsActive", offCurve);
					if (this._presetObjects.Contains(toggleObject)) continue;
					// プリセットオブジェクトに含まれないオブジェクトがあった場合onアニメーションを生成する
					if (onClip == null) onClip = new AnimationClip() { name = $"{toggleSetData.displayName}_ON" };
					AnimationCurve onCurve = new AnimationCurve();
					onCurve.AddKey(new Keyframe(0, 1));
					onClip.SetCurve(path, typeof(GameObject), "m_IsActive", onCurve);
				}

				// ブレンドシェイプアニメーションの生成
				foreach (SkinnedMeshRenderer renderer in BlendShapeData.GetAllSkinnedMeshRenderer(toggleSetData.blendShapes)) {
					string path = MAExAnimatorFactoryUtils.GetBindingPath(renderer.transform);
					foreach (BlendShapeData.BlendShapeIndexAndWeight indexAndWeight in
					         BlendShapeData.GetAllIndexAndWeight(toggleSetData.blendShapes, renderer)) {
						string blendShapeName = $"blendShape.{renderer.sharedMesh.GetBlendShapeName(indexAndWeight.index)}";
						AnimationCurve curve = new AnimationCurve();
						curve.AddKey(new Keyframe(0, indexAndWeight.weight));
						offClip.SetCurve(path, typeof(SkinnedMeshRenderer), blendShapeName, curve);
					}
				}

				AssetDatabase.AddObjectToAsset(offClip, controller);
				if (onClip != null) AssetDatabase.AddObjectToAsset(onClip, controller);
				toggleClips[i] = offClip;

				MAExAnimatorFactoryUtils.CreateToggleLayerToAnimatorController(controller, toggleSetData.parameterName, offClip, onClip,
					this.Target.isToggleInvert, toggleSetData.defaultValue);
			}


			return controller;
		}

		public override void PostProcess(BuildContext context) {
			GameObject targetObject = this.Target.gameObject;
			ModularAvatarMenuInstaller menuInstaller = targetObject.GetComponent<ModularAvatarMenuInstaller>();
			VRCExpressionsMenu expressionsMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
			AssetDatabase.AddObjectToAsset(expressionsMenu, context.AssetContainer);

			expressionsMenu.controls.Add(new VRCExpressionsMenu.Control {
				name = this.Target.displayName,
				icon = this.Target.menuIcon,
				type = VRCExpressionsMenu.Control.ControlType.SubMenu,
				subMenu = CreatePageMenu(expressionsMenu)
			});

			if (this.Target.toggleSetData.Count != 0) {
				expressionsMenu.controls.Add(new VRCExpressionsMenu.Control {
					name = "衣装ON/OFF",
					icon = _folderIcon,
					type = VRCExpressionsMenu.Control.ControlType.SubMenu,
					subMenu = CreateToggleMenu(expressionsMenu)
				});
			}

			menuInstaller.menuToAppend = expressionsMenu;

			ModularAvatarParameters parameters = targetObject.GetComponent<ModularAvatarParameters>();
			parameters.parameters.Add(new ParameterConfig {
				nameOrPrefix = this.Target.parameterName,
				internalParameter = this.Target.isInternal,
				isPrefix = false,
				syncType = ParameterSyncType.Int,
				saved = this.Target.saved,
				defaultValue = this.Target.defaultPreset
			});

			foreach (MAExObjectPresetAnimatorGenerator.ToggleSetData toggleSetData in this.Target.toggleSetData) {
				parameters.parameters.Add(new ParameterConfig {
					nameOrPrefix = toggleSetData.parameterName,
					internalParameter = toggleSetData.isInternal,
					isPrefix = false,
					syncType = ParameterSyncType.Bool,
					saved = toggleSetData.saved,
					defaultValue = toggleSetData.defaultValue ? 1 : 0
				});
			}

			// デフォルトの状態に設定(Animatorをブロックされている際にもデフォルト状態で表示されるように)
			// プリセット
			// オブジェクト
			MAExObjectPresetAnimatorGenerator.PresetData defaultPreset = this.Target.presetData[this.Target.defaultPreset];
			foreach (GameObject usedObject in this._presetObjects) {
				usedObject.SetActive(defaultPreset.enableObjects.Contains(usedObject));
			}

			// ブレンドシェイプ
			foreach (SkinnedMeshRenderer renderer in BlendShapeData.GetAllSkinnedMeshRenderer(defaultPreset.blendShapes)) {
				foreach (BlendShapeData.BlendShapeIndexAndWeight indexAndWeight in BlendShapeData.GetAllIndexAndWeight(defaultPreset.blendShapes, renderer)) {
					renderer.SetBlendShapeWeight(indexAndWeight.index, indexAndWeight.weight);
				}
			}

			// トグル
			foreach (MAExObjectPresetAnimatorGenerator.ToggleSetData toggleSetData in
			         this.Target.toggleSetData.Where(data => this.Target.isToggleInvert == data.defaultValue)) {
				// オブジェクト
				foreach (GameObject toggleObject in toggleSetData.toggleObjects) {
					toggleObject.SetActive(false);
				}

				// ブレンドシェイプ
				foreach (SkinnedMeshRenderer renderer in BlendShapeData.GetAllSkinnedMeshRenderer(toggleSetData.blendShapes)) {
					foreach (BlendShapeData.BlendShapeIndexAndWeight indexAndWeight in
					         BlendShapeData.GetAllIndexAndWeight(toggleSetData.blendShapes, renderer)) {
						renderer.SetBlendShapeWeight(indexAndWeight.index, indexAndWeight.weight);
					}
				}
			}
		}



		private VRCExpressionsMenu CreatePageMenu(VRCExpressionsMenu parent) {
			VRCExpressionsMenu pageMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
			pageMenu.name = "pageMenu";
			AssetDatabase.AddObjectToAsset(pageMenu, parent);

			int remainingPageCount = Mathf.CeilToInt(this.Target.presetData.Count / 8.0f);
			int pageIndex = 0;
			int currentPageControlsCount = 0;
			VRCExpressionsMenu currentMenu = pageMenu;
			while (remainingPageCount > 0) {
				currentMenu.controls.Add(new VRCExpressionsMenu.Control {
					name = $"ページ:{pageIndex}",
					icon = _folderIcon,
					type = VRCExpressionsMenu.Control.ControlType.SubMenu,
					subMenu = CreateSelectMenu(currentMenu, pageIndex * 8)
				});

				currentPageControlsCount++;
				pageIndex++;
				remainingPageCount--;

				if (currentPageControlsCount < 7) continue;
				if (remainingPageCount <= 1) continue;
				VRCExpressionsMenu nextMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
				nextMenu.name = $"pageMenu_{pageIndex}";
				currentMenu.controls.Add(new VRCExpressionsMenu.Control {
					name = "次へ",
					icon = _moreIcon,
					type = VRCExpressionsMenu.Control.ControlType.SubMenu,
					subMenu = nextMenu
				});
				AssetDatabase.AddObjectToAsset(nextMenu, parent);
				currentMenu = nextMenu;
				currentPageControlsCount = 0;
			}

			return pageMenu;
		}

		private VRCExpressionsMenu CreateSelectMenu(VRCExpressionsMenu parent, int startIndex) {
			VRCExpressionsMenu selectMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
			selectMenu.name = $"selectMenu_{startIndex}";
			AssetDatabase.AddObjectToAsset(selectMenu, parent);

			for (int i = startIndex; i < startIndex + 8; i++) {
				if (i >= this.Target.presetData.Count) break;
				selectMenu.controls.Add(new VRCExpressionsMenu.Control {
					name = this.Target.presetData[i].displayName,
					icon = this.Target.presetData[i].menuIcon,
					type = VRCExpressionsMenu.Control.ControlType.Toggle,
					parameter = new VRCExpressionsMenu.Control.Parameter { name = this.Target.parameterName },
					value = i
				});
			}

			return selectMenu;
		}

		private VRCExpressionsMenu CreateToggleMenu(VRCExpressionsMenu parent) {
			VRCExpressionsMenu toggleMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
			toggleMenu.name = $"toggleMenu";
			AssetDatabase.AddObjectToAsset(toggleMenu, parent);

			VRCExpressionsMenu currentMenu = toggleMenu;
			int remainingToggleCount = this.Target.toggleSetData.Count;
			int pageCount = 0;
			foreach (MAExObjectPresetAnimatorGenerator.ToggleSetData toggleSetData in this.Target.toggleSetData) {
				currentMenu.controls.Add(new VRCExpressionsMenu.Control {
					name = toggleSetData.displayName,
					icon = toggleSetData.menuIcon,
					type = VRCExpressionsMenu.Control.ControlType.Toggle,
					parameter = new VRCExpressionsMenu.Control.Parameter { name = toggleSetData.parameterName }
				});
				remainingToggleCount--;

				if (currentMenu.controls.Count < 7) continue;
				if (remainingToggleCount <= 1) continue;

				VRCExpressionsMenu nextMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
				nextMenu.name = $"toggleMenu_{pageCount}";
				currentMenu.controls.Add(new VRCExpressionsMenu.Control {
					name = "次へ",
					icon = _moreIcon,
					type = VRCExpressionsMenu.Control.ControlType.SubMenu,
					subMenu = nextMenu
				});
				pageCount++;
				AssetDatabase.AddObjectToAsset(nextMenu, parent);
				currentMenu = nextMenu;
			}

			return toggleMenu;
		}
	}
}