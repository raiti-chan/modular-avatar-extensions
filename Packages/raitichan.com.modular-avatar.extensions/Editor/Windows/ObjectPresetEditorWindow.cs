using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.ModularAvatar;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class ObjectPresetEditorWindow : EditorWindow {
		private const string UXML_GUID = "8f9e364ed3865ea44918f0b018ed0c45";
		private static ObjectPresetEditorWindow _currentWindow;

		public static ObjectPresetEditorWindow CurrentWindow => _currentWindow;

		public static bool CanShowWindow(MAExObjectPreset target) {
			VRCAvatarDescriptor avatar = RuntimeUtilHelper.FindAvatarInParents(target.transform);
			if (avatar == null) return false;

			return !IsAlreadyShowed(avatar, target);
		}

		private static bool IsAlreadyShowed(VRCAvatarDescriptor avatar, MAExObjectPreset target) {
			if (_currentWindow == null) return false;
			if (_currentWindow._avatar != avatar) return false;
			if (_currentWindow._target != target) return false;
			return true;
		}

		public static void ShowWindow(MAExObjectPreset target) {
			_currentWindow = GetWindow<ObjectPresetEditorWindow>();

			VRCAvatarDescriptor avatar = RuntimeUtilHelper.FindAvatarInParents(target.transform);
			if (avatar == null) return;

			_currentWindow.Avatar = RuntimeUtilHelper.FindAvatarInParents(target.transform);
			_currentWindow._Target = target;
			_currentWindow.titleContent = new GUIContent($"Object Preset Editor : {avatar.name} : {target.name}");

			_currentWindow.Show();
		}

		private SerializedObject _serializedObject;

		public SerializedObject SerializedObject {
			private set {
				if (this._serializedObject == value) return;
				this.rootVisualElement.Bind(value);
				this._serializedObject = value;
			}
			get => this._serializedObject;
		}

		private VRCAvatarDescriptor _avatar;

		public VRCAvatarDescriptor Avatar {
			private set {
				if (this._avatar == value) return;
				ObjectField avatarField = this.rootVisualElement.Q<ObjectField>("AvatarField");
				avatarField.value = value;
				this._avatar = value;
			}
			get => this._avatar;
		}

		private MAExObjectPreset _target;

		private MAExObjectPreset _Target {
			set {
				ObjectField presetField = this.rootVisualElement.Q<ObjectField>("PresetField");
				presetField.value = value;
				this._target = value;
				this.SerializedObject = new SerializedObject(this._target);
			}
			get => this._target;
		}

		private void CreateGUI() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this.rootVisualElement);

			ObjectField avatarField = this.rootVisualElement.Q<ObjectField>("AvatarField");
			avatarField.objectType = typeof(VRCAvatarDescriptor);
			avatarField.SetEnabled(false);

			ObjectField presetField = this.rootVisualElement.Q<ObjectField>("PresetField");
			presetField.objectType = typeof(MAExObjectPreset);
			presetField.SetEnabled(false);
		}
	}
}