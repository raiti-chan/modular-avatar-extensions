using raitichan.com.modular_avatar.extensions.Editor.Views;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class PresetEditorPreview : IMGUIContainer {
		private AvatarPreviewView _previewView;
		private PreviewAvatarController _previewAvatarController;
		private PresetPreviewContext _context;

		public PresetEditorPreview() {
			this.onGUIHandler = this.OnGUI;
		}

		public void SetAvatar(VRCAvatarDescriptor avatar) {
			this._previewView?.Dispose();
			if (avatar == null) {
				this._previewView = null;
				return;
			}
			this._previewView = new AvatarPreviewView(avatar);
			this._previewView.ResetCameraTransform(AvatarPreviewView.CameraAnchor.Body);
			this._previewAvatarController = new PreviewAvatarController(avatar, this._previewView.Descriptor);
			this._context = new PresetPreviewContext();
		}

		public void ProcessCommand(IPresetEditorPreviewCommand command, MAExObjectPreset data) {
			Debug.Log(command);
			command.Process(data, this._previewAvatarController, this._context);
		}

		private void OnGUI() {
			if (this._previewView == null) return;
			if (this._previewView.OnGUI(this.contentContainer.contentRect)) {
				this.MarkDirtyRepaint();
			}
		}

		protected override void Dispose(bool disposeManaged) {
			base.Dispose(disposeManaged);
			this._previewView?.Dispose();
		}

		public new class UxmlFactory : UxmlFactory<PresetEditorPreview, UxmlTraits> { }
	}
	
}