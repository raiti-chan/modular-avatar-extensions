using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Editor.MAAccessHelpers;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using static raitichan.com.modular_avatar.extensions.Modules.MAExObjectPresetAnimatorGenerator.PresetData;
using static raitichan.com.modular_avatar.extensions.Modules.MAExObjectPresetAnimatorGenerator.PresetData.BlendShapeData;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class ObjectPresetAnimatorFactory : IRuntimeAnimatorFactory<MAExObjectPresetAnimatorGenerator> {
		private static readonly Texture2D _moreIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
			"Packages/nadena.dev.modular-avatar/Runtime/Icons/Icon_More_A.png"
		);

		private static readonly Texture2D _folderIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
			"Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Expressions Menu/Icons/item_folder.png"
		);

		public MAExObjectPresetAnimatorGenerator Target { get; set; }

		public void PreProcess(GameObject avatarGameObject) { }

		public RuntimeAnimatorController CreateController(GameObject avatarGameObject) {
			AnimatorController controller = UtilHelper.CreateAnimator();

			AnimationClip[] clips = new AnimationClip[this.Target.presetData.Count];
			HashSet<GameObject> allEnableObjectSet = new HashSet<GameObject>();
			foreach (GameObject enableObject in this.Target.presetData.SelectMany(presetData => presetData.enableObjects)) {
				allEnableObjectSet.Add(enableObject);
			}

			Dictionary<SkinnedMeshRenderer, HashSet<int>> defaultWeightsDictionary = new Dictionary<SkinnedMeshRenderer, HashSet<int>>();
			foreach (BlendShapeData blendShapeData in this.Target.presetData.SelectMany(presetData => presetData.blendShapes)) {
				if (blendShapeData.skinnedMeshRenderer == null) continue;
				if (!defaultWeightsDictionary.TryGetValue(blendShapeData.skinnedMeshRenderer, out HashSet<int> blendShapeIndexSet)) {
					blendShapeIndexSet = new HashSet<int>();
					defaultWeightsDictionary[blendShapeData.skinnedMeshRenderer] = blendShapeIndexSet;
				}

				foreach (BlendShapeIndexAndWeight blendShapeIndexAndWeight in blendShapeData.BlendShapeIndexAndWeights) {
					blendShapeIndexSet.Add(blendShapeIndexAndWeight.index);
				}
			}


			for (int i = 0; i < clips.Length; i++) {
				AnimationClip clip = new AnimationClip() {
					name = this.Target.presetData[i].displayName
				};
				foreach (GameObject allObject in allEnableObjectSet) {
					string path = MAExAnimatorFactoryUtils.GetBindingPath(allObject.transform);
					AnimationCurve curve = new AnimationCurve();
					curve.AddKey(new Keyframe(0, this.Target.presetData[i].enableObjects.Contains(allObject) ? 1 : 0));
					clip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
				}

				foreach (KeyValuePair<SkinnedMeshRenderer, HashSet<int>> writeBlendShapeIndexSet in defaultWeightsDictionary) {
					string path = MAExAnimatorFactoryUtils.GetBindingPath(writeBlendShapeIndexSet.Key.transform);
					Dictionary<int, float> weightDictionary = this.Target.presetData[i].GetAllBlendShapeIndexAndWeight(writeBlendShapeIndexSet.Key);
					foreach (int blendShapeIndex in writeBlendShapeIndexSet.Value) {
						string blendShapeName = $"blendShape.{writeBlendShapeIndexSet.Key.sharedMesh.GetBlendShapeName(blendShapeIndex)}";
						AnimationCurve curve = new AnimationCurve();
						if (!weightDictionary.TryGetValue(blendShapeIndex, out float weight)) {
							weight = writeBlendShapeIndexSet.Key.GetBlendShapeWeight(blendShapeIndex);
						}

						curve.AddKey(new Keyframe(0, weight));
						clip.SetCurve(path, typeof(SkinnedMeshRenderer), blendShapeName, curve);
					}
				}

				AssetDatabase.AddObjectToAsset(clip, controller);
				clips[i] = clip;
			}

			MAExAnimatorFactoryUtils.CreateSelectStateLayerToAnimatorController(controller, this.Target.parameterName, clips);

			return controller;
		}

		public void PostProcess(GameObject avatarGameObject) {
			GameObject targetObject = this.Target.gameObject;
			ModularAvatarMenuInstaller menuInstaller = targetObject.GetComponent<ModularAvatarMenuInstaller>();
			VRCExpressionsMenu expressionsMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
			AssetDatabase.CreateAsset(expressionsMenu, UtilHelper.GenerateAssetPath());

			expressionsMenu.controls.Add(new VRCExpressionsMenu.Control {
				name = this.Target.displayName,
				icon = this.Target.menuIcon,
				type = VRCExpressionsMenu.Control.ControlType.SubMenu,
				subMenu = CreatePageMenu(expressionsMenu)
			});

			menuInstaller.menuToAppend = expressionsMenu;

			ModularAvatarParameters parameters = targetObject.GetComponent<ModularAvatarParameters>();
			parameters.parameters.Add(new ParameterConfig {
				nameOrPrefix = this.Target.parameterName,
				internalParameter = this.Target.isInternal,
				isPrefix = false,
				syncType = ParameterSyncType.Int,
				saved = this.Target.saved,
				defaultValue = 0
			});
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
	}
}