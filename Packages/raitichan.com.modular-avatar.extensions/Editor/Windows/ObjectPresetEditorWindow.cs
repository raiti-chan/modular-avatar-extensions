using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement;
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
			if (_currentWindow != null) _currentWindow.Close();
			_currentWindow = GetWindow<ObjectPresetEditorWindow>();

			VRCAvatarDescriptor avatar = RuntimeUtilHelper.FindAvatarInParents(target.transform);
			if (avatar == null) return;

			_currentWindow.Avatar = RuntimeUtilHelper.FindAvatarInParents(target.transform);
			_currentWindow.Target = target;
			_currentWindow.titleContent = new GUIContent($"Object Preset Editor : {avatar.name} : {target.name}");
			_currentWindow.Refresh();
			_currentWindow.Show();
		}

		private VRCAvatarDescriptor _avatar;

		private VRCAvatarDescriptor Avatar {
			set {
				ObjectField avatarField = this.rootVisualElement.Q<ObjectField>("AvatarField");
				avatarField.value = value;
				this._avatar = value;
			}
			get => this._avatar;
		}

		private MAExObjectPreset _target;

		private MAExObjectPreset Target {
			set {
				ObjectField presetField = this.rootVisualElement.Q<ObjectField>("PresetField");
				presetField.value = value;
				this._target = value;
				this.SerializedObject = value != null ? new SerializedObject(value) : null;
			}
			get => this._target;
		}

		private SerializedObject _serializedObject;

		public SerializedObject SerializedObject {
			private set {
				if (value != null) {
					this.rootVisualElement.Q<CustomBindableElement>("Root").Bind(value);
				} else {
					this.rootVisualElement.Q<CustomBindableElement>("Root").Unbind();
				}

				this._serializedObject = value;
			}
			get => this._serializedObject;
		}

		private PresetEditorPreview _preview;

		private void Refresh() {
			if (this.Avatar == null || this.Target == null) return;
			this._preview.SetAvatar(this.Avatar);
		}

		private void OnEnable() {
			Undo.undoRedoPerformed += this.UndoRedoPerformed;
		}
		
		private void OnDisable() {
			this.rootVisualElement.Q<PresetEditorPreview>("Preview")?.Dispose();
			this.Avatar = null;
			this.Target = null;
			Undo.undoRedoPerformed -= this.UndoRedoPerformed;
		}
		
		private void UndoRedoPerformed() {
			this._preview.ProcessCommand(new PreviewReloadPresetCommand(), this.Target);
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

			this._preview = this.rootVisualElement.Q<PresetEditorPreview>("Preview");

			this.rootVisualElement.RegisterCallback<PresetChangeEvent>(this.OnPresetChangeEvent);
			
			// UnityのバグでTextFieldでIMEの入力が無効化されているため応急処置
			this.rootVisualElement.RegisterCallback<FocusInEvent>(evt => {
				if (evt.target.GetType().Name == "TextInput") Input.imeCompositionMode = IMECompositionMode.On;
			});
			this.rootVisualElement.RegisterCallback<FocusOutEvent>(evt => {
				if (evt.target.GetType().Name == "TextInput") Input.imeCompositionMode = IMECompositionMode.Auto;
			});
		}

		private void OnPresetChangeEvent(PresetChangeEvent evt) {
			if (this.Target == null) return;
			this._preview.ProcessCommand(evt.command, this.Target);
			evt.StopImmediatePropagation();
			evt.PreventDefault();
		}
	}
}