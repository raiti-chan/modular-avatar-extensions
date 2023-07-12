using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace raitichan.com.modular_avatar.extensions.Editor.ControllerFactories {
	// ReSharper disable once UnusedType.Global
	public class ObjectPresetFactory : IRuntimeAnimatorFactory<MAExObjectPreset> {
		private static readonly AnimationClip _EMPTY_CLIP = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath("a62a55da1646ad240a9d6c4344484333"));

		public MAExObjectPreset Target { get; set; }

		public void PreProcess(GameObject avatarGameObject) {
			if (string.IsNullOrEmpty(this.Target.parameterName)) {
				this.Target.parameterName = GUID.Generate().ToString();
			}
		}

		public RuntimeAnimatorController CreateController(GameObject avatarGameObject) {
			AnimatorController controller = MAExUtils.CreateAnimator();
			this.AddAnimatorParameter(controller);
			this.CreatePresetSelectLayer(controller);
			int useParameterCount = this.Target.GetSyncedToggleParameterUseCount();
			for (int parameterIndex = 0; parameterIndex < useParameterCount; parameterIndex++) {
				this.CreateToggleLayer(controller, parameterIndex);
			}

			return controller;
		}
		
		
		public void PostProcess(GameObject avatarGameObject) {
			this.CreateParameter();
			this.CreateMenu();
			this.ApplyDefaultPreset();
		}

		private void AddAnimatorParameter(AnimatorController controller) {
			controller.AddParameter("IsLocal", AnimatorControllerParameterType.Bool);
			controller.AddParameter(this.Target.parameterName, AnimatorControllerParameterType.Int);

			int syncParameterCount = this.Target.GetSyncedToggleParameterUseCount();
			for (int parameterIndex = 0; parameterIndex < syncParameterCount; parameterIndex++) {
				controller.AddParameter(new AnimatorControllerParameter {
					name = this.CreateSyncParameterName(parameterIndex),
					type = AnimatorControllerParameterType.Bool,
					defaultBool = true
				});
			}

			for (int presetIndex = 0; presetIndex < this.Target.presets.Count; presetIndex++) {
				MAExObjectPreset.Preset targetPreset = this.Target.presets[presetIndex];
				for (int toggleIndex = 0; toggleIndex < targetPreset.toggleSets.Count; toggleIndex++) {
					controller.AddParameter(this.CreateLocalParameterName(presetIndex, toggleIndex), AnimatorControllerParameterType.Bool);
				}
			}
		}

		private void CreatePresetSelectLayer(AnimatorController controller) {
			AnimatorStateMachine stateMachine = new AnimatorStateMachine() {
				name = $"{this.Target.parameterName}_SelectLayer",
				entryPosition = Vector3.zero,
				exitPosition = new Vector3(0, 100, 0),
				anyStatePosition = new Vector3(0, 50, 0)
			};
			AssetDatabase.AddObjectToAsset(stateMachine, controller);
			AnimatorState rootState = stateMachine.AddStateEx("Root", _EMPTY_CLIP, 500);

			for (int presetIndex = 0; presetIndex < this.Target.presets.Count; presetIndex++) {
				AnimationClip presetClip = this.CreatePresetAnimationClip(presetIndex);
				AssetDatabase.AddObjectToAsset(presetClip, controller);

				AnimatorState presetState = stateMachine.AddStateEx($"{this.Target.parameterName}_{presetIndex}", presetClip, 250, presetIndex * 50);
				rootState.AddTransitionEx(presetState).AddCondition(AnimatorConditionMode.Equals, presetIndex, this.Target.parameterName);
				presetState.AddTransitionEx(rootState).AddCondition(AnimatorConditionMode.NotEqual, presetIndex, this.Target.parameterName);

				if (presetIndex == this.Target.defaultValue) {
					stateMachine.defaultState = presetState;
				}
			}

			AnimatorControllerLayer layer = new AnimatorControllerLayer {
				name = stateMachine.name,
				avatarMask = null,
				blendingMode = AnimatorLayerBlendingMode.Override,
				defaultWeight = 1.0f,
				syncedLayerIndex = -1,
				syncedLayerAffectsTiming = false,
				iKPass = false,
				stateMachine = stateMachine
			};

			controller.AddLayer(layer);
		}

		private void CreateToggleLayer(AnimatorController controller, int parameterIndex) {
			AnimatorStateMachine stateMachine = new AnimatorStateMachine {
				name = $"{this.Target.parameterName}_{parameterIndex}_ParameterLoadLayer",
				entryPosition = Vector3.zero,
				exitPosition = new Vector3(0, 100, 0),
				anyStatePosition = new Vector3(0, 50, 0)
			};
			AssetDatabase.AddObjectToAsset(stateMachine, controller);

			AnimatorState rootState = stateMachine.AddStateEx("Root", _EMPTY_CLIP, 250);
			AnimatorState globalState = stateMachine.AddStateEx("Global", _EMPTY_CLIP, 250, -50);
			AnimatorState localState = stateMachine.AddStateEx("Local", _EMPTY_CLIP, 250, 50);

			rootState.AddTransitionEx(globalState).AddCondition(AnimatorConditionMode.IfNot, 0, "IsLocal");
			rootState.AddTransitionEx(localState).AddCondition(AnimatorConditionMode.If, 0, "IsLocal");

			List<AnimatorState> globalPresetStates = new List<AnimatorState>();
			List<AnimatorState> localPresetStates = new List<AnimatorState>();

			for (int presetIndex = 0; presetIndex < this.Target.presets.Count; presetIndex++) {
				MAExObjectPreset.Preset targetPreset = this.Target.presets[presetIndex];

				AnimatorState presetStateGlobal = stateMachine.AddStateEx($"{this.Target.parameterName}_{presetIndex}_Global", _EMPTY_CLIP, 500, -presetIndex * 100 - 50);
				globalState.AddTransitionEx(presetStateGlobal).AddCondition(AnimatorConditionMode.Equals, presetIndex, this.Target.parameterName);
				globalPresetStates.Add(presetStateGlobal);
					
				AnimatorState presetStateLocal = stateMachine.AddStateEx($"{this.Target.parameterName}_{presetIndex}_Local", _EMPTY_CLIP, 500, presetIndex * 100 + 50);
				localState.AddTransitionEx(presetStateLocal).AddCondition(AnimatorConditionMode.Equals, presetIndex, this.Target.parameterName);
				localPresetStates.Add(presetStateLocal);

				string localParameterName = this.CreateLocalParameterName(presetIndex, parameterIndex);
				string globalParameterName = this.CreateSyncParameterName(parameterIndex);
				if (targetPreset.toggleSets.Count <= parameterIndex) {
					// 避難場所を作る
					AnimatorState noneStateGlobal = stateMachine.AddStateEx($"{localParameterName}_NONE_Global", _EMPTY_CLIP, 750, -presetIndex * 100 - 50);
					presetStateGlobal.AddTransitionEx(noneStateGlobal).AddCondition(AnimatorConditionMode.Equals, presetIndex, this.Target.parameterName);
					noneStateGlobal.AddTransitionEx(presetStateGlobal).AddCondition(AnimatorConditionMode.NotEqual, presetIndex, this.Target.parameterName);
					AnimatorState noneStateLocal = stateMachine.AddStateEx($"{localParameterName}_NONE_Local", _EMPTY_CLIP, 750, presetIndex * 100 + 50);
					presetStateLocal.AddTransitionEx(noneStateLocal).AddCondition(AnimatorConditionMode.Equals, presetIndex, this.Target.parameterName);
					noneStateLocal.AddTransitionEx(presetStateLocal).AddCondition(AnimatorConditionMode.NotEqual, presetIndex, this.Target.parameterName);
					continue;
				}
				AnimationClip onClip = this.CreateToggleAnimationClip(presetIndex, parameterIndex);
				AssetDatabase.AddObjectToAsset(onClip, controller);
				if (targetPreset.toggleSets[parameterIndex].defaultValue) {
					presetStateGlobal.motion = onClip;
					presetStateLocal.motion = onClip;
					if (presetIndex == this.Target.defaultValue) {
						rootState.motion = onClip;
						globalState.motion = onClip;
						localState.motion = onClip;
					}
				}
				{
					AnimatorState onStateGlobal = stateMachine.AddStateEx($"{localParameterName}_ON_Global", onClip, 750, -presetIndex * 100 - 25);
					AnimatorState offStateGlobal = stateMachine.AddStateEx($"{localParameterName}_OFF_Global", _EMPTY_CLIP, 750, -presetIndex * 100 - 75);

					presetStateGlobal.AddTransitionEx(offStateGlobal)
						.AddConditionEx(AnimatorConditionMode.IfNot, 0, globalParameterName)
						.AddCondition(AnimatorConditionMode.Equals, presetIndex, this.Target.parameterName);
					presetStateGlobal.AddTransitionEx(onStateGlobal)
						.AddConditionEx(AnimatorConditionMode.If, 0, globalParameterName)
						.AddCondition(AnimatorConditionMode.Equals, presetIndex, this.Target.parameterName);
					
					onStateGlobal.AddTransitionEx(offStateGlobal).AddCondition(AnimatorConditionMode.IfNot, 0, globalParameterName);
					offStateGlobal.AddTransitionEx(onStateGlobal).AddCondition(AnimatorConditionMode.If, 0, globalParameterName);

					onStateGlobal.AddTransitionEx(presetStateGlobal).AddCondition(AnimatorConditionMode.NotEqual, presetIndex, this.Target.parameterName);
					offStateGlobal.AddTransitionEx(presetStateGlobal).AddCondition(AnimatorConditionMode.NotEqual, presetIndex, this.Target.parameterName);
				}
				{
					AnimatorState onStateLocal = stateMachine.AddStateEx($"{localParameterName}_ON_Local", onClip, 750, presetIndex * 100 + 25);
					AnimatorState offStateLocal = stateMachine.AddStateEx($"{localParameterName}_OFF_Local", _EMPTY_CLIP, 750, presetIndex * 100 + 75);

					presetStateLocal.AddTransitionEx(offStateLocal)
						.AddConditionEx(AnimatorConditionMode.IfNot, 0, localParameterName)
						.AddCondition(AnimatorConditionMode.Equals, presetIndex, this.Target.parameterName);
					presetStateLocal.AddTransitionEx(onStateLocal)
						.AddConditionEx(AnimatorConditionMode.If, 0, localParameterName)
						.AddCondition(AnimatorConditionMode.Equals, presetIndex, this.Target.parameterName);

					onStateLocal.AddTransitionEx(offStateLocal).AddCondition(AnimatorConditionMode.IfNot, 0, localParameterName);
					offStateLocal.AddTransitionEx(onStateLocal).AddCondition(AnimatorConditionMode.If, 0, localParameterName);

					onStateLocal.AddTransitionEx(presetStateLocal).AddCondition(AnimatorConditionMode.NotEqual, presetIndex, this.Target.parameterName);
					offStateLocal.AddTransitionEx(presetStateLocal).AddCondition(AnimatorConditionMode.NotEqual, presetIndex, this.Target.parameterName);

					VRCAvatarParameterDriver onDriver = onStateLocal.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
					onDriver.localOnly = false;
					onDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter {
						type = VRC_AvatarParameterDriver.ChangeType.Set,
						name = this.CreateSyncParameterName(parameterIndex),
						value = 1
					});

					VRCAvatarParameterDriver offDriver = offStateLocal.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
					offDriver.localOnly = false;
					offDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter {
						type = VRC_AvatarParameterDriver.ChangeType.Set,
						name = this.CreateSyncParameterName(parameterIndex),
						value = 0
					});
				}
			}

			for (int i = 0; i < globalPresetStates.Count; i++) {
				AnimatorState targetGlobal = globalPresetStates[i];
				AnimatorState targetLocal = localPresetStates[i];
				for (int j = 0; j < globalPresetStates.Count; j++) {
					if (j == i) continue;
					AnimatorState dstGlobal = globalPresetStates[j];
					AnimatorState dstLocal = localPresetStates[j];
					targetGlobal.AddTransitionEx(dstGlobal).AddCondition(AnimatorConditionMode.Equals, j, this.Target.parameterName);
					targetLocal.AddTransitionEx(dstLocal).AddCondition(AnimatorConditionMode.Equals, j, this.Target.parameterName);
					
				}
			}
			
			AnimatorControllerLayer layer = new AnimatorControllerLayer {
				name = stateMachine.name,
				avatarMask = null,
				blendingMode = AnimatorLayerBlendingMode.Override,
				defaultWeight = 1.0f,
				syncedLayerIndex = -1,
				syncedLayerAffectsTiming = false,
				iKPass = false,
				stateMachine = stateMachine
			};

			controller.AddLayer(layer);
		}

		private AnimationClip CreatePresetAnimationClip(int presetIndex) {
			MAExObjectPreset.Preset targetPreset = this.Target.presets[presetIndex];
			AnimationClip presetClip = new AnimationClip { name = $"{this.Target.parameterName}_{presetIndex}" };

			// 使用するオブジェクトのアニメーション
			foreach (GameObject hideObject in this.Target.GetDefaultHideObjects()) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(hideObject.transform);
				AnimationCurve curve = new AnimationCurve();
				curve.AddKey(new Keyframe(0, 0));
				presetClip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
			}
			
			foreach (GameObject showObject in this.Target.GetDefaultShowObjects()) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(showObject.transform);
				AnimationCurve curve = new AnimationCurve();
				curve.AddKey(new Keyframe(0, 1));
				presetClip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
			}
			
			// 使用するブレンドシェイプのデフォルトアニメーション
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.Target.GetAllReferencedSkinnedMeshRenderer()) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(skinnedMeshRenderer.transform);
				foreach (string key in this.Target.GetAllUsedBlendShapeName(skinnedMeshRenderer)) {
					int blendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(key);
					float defaultWeight = skinnedMeshRenderer.GetBlendShapeWeight(blendShapeIndex);
					string blendShapeName = $"blendShape.{key}";
					AnimationCurve curve = new AnimationCurve();
					curve.AddKey(new Keyframe(0, defaultWeight));
					presetClip.SetCurve(path, typeof(SkinnedMeshRenderer), blendShapeName, curve);
				}
			}
			
			// 使用するマテリアルのデフォルトアニメーション 
			foreach (Renderer renderer in this.Target.GetAllReferencedRenderers()) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(renderer.transform);
				foreach (int materialIndex in this.Target.GetAllUsedMaterialIndex(renderer)) {
					EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(path, typeof(Renderer), $"m_Materials.Array.data[{materialIndex}]");
					ObjectReferenceKeyframe[] keyframes = {
						new ObjectReferenceKeyframe {
							time = 0,
							value = renderer.sharedMaterials[materialIndex]
						}
					};
					AnimationUtility.SetObjectReferenceCurve(presetClip, binding, keyframes);
				}
			}
			
			
			// プリセットで表示するオブジェクトの表示アニメーション
			foreach (GameObject hideObject in targetPreset.GetHideObjects()) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(hideObject.transform);
				AnimationCurve curve = new AnimationCurve();
				curve.AddKey(new Keyframe(0, 0));
				presetClip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
			}
			
			foreach (GameObject showObject in targetPreset.GetShowObjects()) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(showObject.transform);
				AnimationCurve curve = new AnimationCurve();
				curve.AddKey(new Keyframe(0, 1));
				presetClip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
			}
			
			// プリセットで変更されるブレンドシェイプのアニメーション
			foreach (MAExObjectPreset.BlendShape presetBlendShape in targetPreset.blendShapes) {
				SkinnedMeshRenderer skinnedMeshRenderer = presetBlendShape.skinnedMeshRenderer;
				string path = MAExAnimatorFactoryUtils.GetBindingPath(skinnedMeshRenderer.transform);
				foreach (MAExObjectPreset.BlendShapeWeight weight in presetBlendShape.weights) {
					string blendShapeName = $"blendShape.{weight.key}";
					AnimationCurve curve = new AnimationCurve();
					curve.AddKey(new Keyframe(0, weight.value));
					presetClip.SetCurve(path, typeof(SkinnedMeshRenderer), blendShapeName, curve);
				}
			}
			
			// プリセットで変更されるマテリアルのアニメーション 
			foreach (MAExObjectPreset.MaterialReplace materialReplace in targetPreset.materialReplaces) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(materialReplace.renderer.transform);
				for (int materialIndex = 0; materialIndex < materialReplace.materials.Count; materialIndex++) {
					EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(path, typeof(Renderer), $"m_Materials.Array.data[{materialIndex}]");
					ObjectReferenceKeyframe[] keyframes = {
						new ObjectReferenceKeyframe {
							time = 0,
							value = materialReplace.materials[materialIndex]
						}
					};
					AnimationUtility.SetObjectReferenceCurve(presetClip, binding, keyframes);
				}
			}
			
			return presetClip;
		}


		private AnimationClip CreateToggleAnimationClip(int presetIndex, int toggleIndex) {
			AnimationClip presetClip = new AnimationClip { name = $"{this.Target.parameterName}_{presetIndex}_{toggleIndex}_ON" };
			
			foreach (MAExObjectPreset.EnableObject enableObject in this.Target.presets[presetIndex].toggleSets[toggleIndex].enableObjects) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(enableObject.gameObject.transform);
				AnimationCurve curve = new AnimationCurve();
				curve.AddKey(new Keyframe(0, enableObject.enable ? 1 : 0));
				presetClip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
			}
			
			foreach (MAExObjectPreset.BlendShape toggleBlendShape in this.Target.presets[presetIndex].toggleSets[toggleIndex].blendShapes) {
				SkinnedMeshRenderer skinnedMeshRenderer = toggleBlendShape.skinnedMeshRenderer;
				string path = MAExAnimatorFactoryUtils.GetBindingPath(skinnedMeshRenderer.transform);
				foreach (MAExObjectPreset.BlendShapeWeight weight in toggleBlendShape.weights) {
					string blendShapeName = $"blendShape.{weight.key}";
					AnimationCurve curve = new AnimationCurve();
					curve.AddKey(new Keyframe(0, weight.value));
					presetClip.SetCurve(path, typeof(SkinnedMeshRenderer), blendShapeName, curve);
				}
			}
			
			foreach (MAExObjectPreset.MaterialReplace materialReplace in this.Target.presets[presetIndex].toggleSets[toggleIndex].materialReplaces) {
				string path = MAExAnimatorFactoryUtils.GetBindingPath(materialReplace.renderer.transform);
				for (int materialIndex = 0; materialIndex < materialReplace.materials.Count; materialIndex++) {
					EditorCurveBinding binding = EditorCurveBinding.PPtrCurve(path, typeof(Renderer), $"m_Materials.Array.data[{materialIndex}]");
					ObjectReferenceKeyframe[] keyframes = {
						new ObjectReferenceKeyframe {
							time = 0,
							value = materialReplace.materials[materialIndex]
						}
					};
					AnimationUtility.SetObjectReferenceCurve(presetClip, binding, keyframes);
				}
			}

			return presetClip;
		}

		private void CreateParameter() {
			GameObject targetObject = this.Target.gameObject;
			ModularAvatarParameters parameters = targetObject.GetComponent<ModularAvatarParameters>();
			if (parameters == null) {
				parameters = targetObject.AddComponent<ModularAvatarParameters>();
			}

			parameters.parameters.Clear();
			parameters.parameters.Add(new ParameterConfig {
				nameOrPrefix = this.Target.parameterName,
				internalParameter = this.Target.isInternal,
				isPrefix = false,
				syncType = ParameterSyncType.Int,
				localOnly = false,
				saved = this.Target.saved,
				defaultValue = this.Target.defaultValue
			});

			int syncParameterCount = this.Target.GetSyncedToggleParameterUseCount();
			for (int parameterIndex = 0; parameterIndex < syncParameterCount; parameterIndex++) {
				parameters.parameters.Add(new ParameterConfig {
					nameOrPrefix = this.CreateSyncParameterName(parameterIndex),
					internalParameter = this.Target.isInternal,
					isPrefix = false,
					syncType = ParameterSyncType.Bool,
					localOnly = false,
					saved = false,
					defaultValue = 1
				});
			}

			for (int presetIndex = 0; presetIndex < this.Target.presets.Count; presetIndex++) {
				MAExObjectPreset.Preset targetPreset = this.Target.presets[presetIndex];
				for (int toggleIndex = 0; toggleIndex < targetPreset.toggleSets.Count; toggleIndex++) {
					MAExObjectPreset.ToggleSet targetToggle = targetPreset.toggleSets[toggleIndex];
					parameters.parameters.Add(new ParameterConfig {
						nameOrPrefix = this.CreateLocalParameterName(presetIndex, toggleIndex),
						internalParameter = this.Target.isInternal,
						isPrefix = false,
						syncType = ParameterSyncType.Bool,
						localOnly = true,
						saved = targetToggle.saved,
						defaultValue = targetToggle.defaultValue ? 1 : 0
					});
				}
			}
		}

		private void CreateMenu() {
			GameObject targetObject = this.Target.gameObject;
			ModularAvatarMenuInstaller menuInstaller = targetObject.GetComponent<ModularAvatarMenuInstaller>();
			if (menuInstaller == null) menuInstaller = targetObject.AddComponent<ModularAvatarMenuInstaller>();
			menuInstaller.menuToAppend = null;

			ModularAvatarMenuItem rootMenuItem = targetObject.GetComponent<ModularAvatarMenuItem>();
			if (rootMenuItem != null) {
				rootMenuItem.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
				rootMenuItem.MenuSource = SubmenuSource.Children;
				rootMenuItem.menuSource_otherObjectChildren = null;
			}

			ModularAvatarMenuGroup rootMenuGroup = targetObject.GetComponent<ModularAvatarMenuGroup>();
			if (rootMenuGroup == null && rootMenuItem == null) {
				targetObject.AddComponent<ModularAvatarMenuGroup>();
			}

			for (int presetIndex = 0; presetIndex < this.Target.presets.Count; presetIndex++) {
				MAExObjectPreset.Preset targetPreset = this.Target.presets[presetIndex];
				GameObject presetMenuItemObject = new GameObject(targetPreset.displayName);
				presetMenuItemObject.transform.SetParent(targetObject.transform);
				ModularAvatarMenuItem presetMenuItem = presetMenuItemObject.AddComponent<ModularAvatarMenuItem>();

				presetMenuItem.Control.icon = targetPreset.menuIcon;
				presetMenuItem.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
				presetMenuItem.MenuSource = SubmenuSource.Children;

				GameObject presetEnableMenuItemObject = new GameObject(targetPreset.displayName);
				presetEnableMenuItemObject.transform.SetParent(presetMenuItemObject.transform);
				ModularAvatarMenuItem presetEnableMenuItem = presetEnableMenuItemObject.AddComponent<ModularAvatarMenuItem>();
				presetEnableMenuItem.Control.icon = targetPreset.menuIcon;
				presetEnableMenuItem.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
				presetEnableMenuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter { name = this.Target.parameterName };
				presetEnableMenuItem.Control.value = presetIndex;

				for (int toggleIndex = 0; toggleIndex < targetPreset.toggleSets.Count; toggleIndex++) {
					MAExObjectPreset.ToggleSet targetToggleSet = targetPreset.toggleSets[toggleIndex];
					GameObject toggleMenuItemObject = new GameObject(targetToggleSet.displayName);
					toggleMenuItemObject.transform.SetParent(presetMenuItemObject.transform);
					ModularAvatarMenuItem toggleMenuItem = toggleMenuItemObject.AddComponent<ModularAvatarMenuItem>();
					toggleMenuItem.Control.icon = targetToggleSet.menuIcon;
					toggleMenuItem.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
					toggleMenuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter { name = this.CreateLocalParameterName(presetIndex, toggleIndex) };
				}
			}
		}

		private void ApplyDefaultPreset() {
			foreach (GameObject hideObject in this.Target.GetDefaultHideObjects()) {
				hideObject.SetActive(false);
			}
			
			foreach (GameObject showObject in this.Target.GetDefaultShowObjects()) {
				showObject.SetActive(true);
			}

			var targetPreset = this.Target.presets[this.Target.defaultValue];
			
			foreach (GameObject hideObject in targetPreset.GetHideObjects()) {
				hideObject.SetActive(false);
			}
			
			foreach (GameObject showObject in targetPreset.GetShowObjects()) {
				showObject.SetActive(true);
			}

			foreach (MAExObjectPreset.BlendShape presetBlendShape in targetPreset.blendShapes) {
				SkinnedMeshRenderer skinnedMeshRenderer = presetBlendShape.skinnedMeshRenderer;
				foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in presetBlendShape.weights) {
					int blendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeWeight.key);
					skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, blendShapeWeight.value);
				}
			}
			
			foreach (MAExObjectPreset.MaterialReplace materialReplace in targetPreset.materialReplaces) {
				Renderer renderer = materialReplace.renderer;
				Material[] materials = new Material[renderer.sharedMaterials.Length];
				for (int materialIndex = 0; materialIndex < materialReplace.materials.Count; materialIndex++) {
					Material replaceSrc = materialReplace.materials[materialIndex];
					materials[materialIndex] = replaceSrc != null ? replaceSrc : renderer.sharedMaterials[materialIndex];
				}

				renderer.materials = materials;
			}
			
			foreach (MAExObjectPreset.ToggleSet toggleSet in this.Target.presets[this.Target.defaultValue].toggleSets.Where(toggleSet => toggleSet.defaultValue)) {
				foreach (MAExObjectPreset.EnableObject enableObject in toggleSet.enableObjects) {
					enableObject.gameObject.SetActive(enableObject.enable);
				}
				
				foreach (MAExObjectPreset.BlendShape toggleSetBlendShape in toggleSet.blendShapes) {
					SkinnedMeshRenderer skinnedMeshRenderer = toggleSetBlendShape.skinnedMeshRenderer;
					foreach (MAExObjectPreset.BlendShapeWeight blendShapeWeight in toggleSetBlendShape.weights) {
						int blendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(blendShapeWeight.key);
						skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, blendShapeWeight.value);
					}
				}
				
				foreach (MAExObjectPreset.MaterialReplace materialReplace in toggleSet.materialReplaces) {
					Renderer renderer = materialReplace.renderer;
					Material[] materials = new Material[renderer.sharedMaterials.Length];
					for (int materialIndex = 0; materialIndex < materialReplace.materials.Count; materialIndex++) {
						Material replaceSrc = materialReplace.materials[materialIndex];
						materials[materialIndex] = replaceSrc != null ? replaceSrc : renderer.sharedMaterials[materialIndex];
					}

					renderer.materials = materials;
				}
			}
		}


		private string CreateSyncParameterName(int parameterIndex) => $"{this.Target.parameterName}_sync_{parameterIndex}";
		private string CreateLocalParameterName(int presetIndex, int toggleIndex) => $"{this.Target.parameterName}_local_{presetIndex}_{toggleIndex}";
	}
}