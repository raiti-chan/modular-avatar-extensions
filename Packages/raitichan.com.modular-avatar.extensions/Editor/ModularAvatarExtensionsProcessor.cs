using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using nadena.dev.modular_avatar.core.editor;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.Editor {
	[InitializeOnLoad]
	public class ModularAvatarExtensionsProcessor {
		static ModularAvatarExtensionsProcessor() {
			MAPatcher.PreProcess += ProcessAvatar;
		}

		private static MeshRendererOverrideHook _meshRendererOverrideHook;
		private static AnimatorGeneratorHook _animatorGeneratorHook;

		private static void ProcessAvatar(GameObject avatarGameObject) {
			_meshRendererOverrideHook = new MeshRendererOverrideHook();
			_meshRendererOverrideHook.OnProcessAvatar(avatarGameObject);

			_animatorGeneratorHook = new AnimatorGeneratorHook();
			_animatorGeneratorHook.OnProcessAvatar(avatarGameObject);
		}
	}

	[InitializeOnLoad]
	public class MAPatcher {
		static MAPatcher() {
			Harmony harmonyInstance = new Harmony("raitichan.com.modular_avatar.extensions.Editor");
			harmonyInstance.UnpatchAll();

			MethodInfo original = typeof(AvatarProcessor).GetMethod(nameof(AvatarProcessor.ProcessAvatar));
			MethodInfo transpiler = typeof(MAPatcher).GetMethod(nameof(ProcessAvatarTranspiler));
			harmonyInstance.Patch(original, transpiler: new HarmonyMethod(transpiler));
		}

		private static readonly ConstructorInfo buildContextConstructor = AccessTools.Constructor(AccessTools.TypeByName("BuildContext"), new[] { typeof(VRCAvatarDescriptor) });
		private static bool isBuildContextConstructor;
		private static bool isInsertPosition;

		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> ProcessAvatarTranspiler(IEnumerable<CodeInstruction> instructions) {
			foreach (CodeInstruction instruction in instructions) {
				if (isInsertPosition) {
					isInsertPosition = false;
					yield return new CodeInstruction(OpCodes.Ldloc_0);
					yield return CodeInstruction.Call(typeof(MAPatcher), nameof(InvokePreProcess), new []{typeof(VRCAvatarDescriptor)});
				}
				
				if (isBuildContextConstructor && instruction.IsStloc()) {
					isInsertPosition = true;
					isBuildContextConstructor = false;
				} else {
					isBuildContextConstructor = false;
				}
				
				if (instruction.Is(OpCodes.Newobj, buildContextConstructor)) {
					isBuildContextConstructor = true;
				}


				yield return instruction;
			}
		}

		public static event Action<GameObject> PreProcess;

		public static void InvokePreProcess(VRCAvatarDescriptor avatar) {
			PreProcess?.Invoke(avatar.gameObject);
		}
	}
}