using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using raitichan.com.modular_avatar.extensions.Editor.UnityUtils;
using raitichan.com.modular_avatar.extensions.Editor.Views;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class AnimatorControllerOverrideEditor : EditorWindow {
		private AvatarPreviewView _preview;
		private VRCAvatarDescriptor _avatar;
		private AnimatorOverrideController _overrideController;

		private SkinnedMeshRenderer _selectedSkinnedMeshRenderer;
		private SkinnedMeshRenderer _previewSkinnedMeshRenderer;

		private AnimationClip[] _overrideTargetClips;
		private SkinnedMeshRenderer[] _skinnedMeshRenderers;
		private (string blendShapeName, float value, bool enable)[] _blendShapes;
		private float[] _defaultBlendShapeValue;

		private IMGUIContainer _previewContainer;
		private ObjectField _avatarField;
		private ObjectField _overrideControllerField;
		private ListView _animationListView;
		private ListView _skinnedMeshRendererListView;
		private ListView _blendShapeListView;

		private AnimationClipListViewController _animationClipListViewController;
		private SkinnedMeshRendererListViewController _skinnedMeshRendererListViewController;
		private BlendShapeListViewController _blendShapeListViewController;


		private void OnEnable() {
			this._preview = null;
		}

		private void OnDisable() {
			this._preview?.Dispose();
			this._preview = null;
		}

		private void CreateGUI() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath("ed3c0a4ec0e50ab4eaa48eee318f6886");
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this.rootVisualElement);

			this._previewContainer = this.rootVisualElement.Q<IMGUIContainer>("Preview");
			this._previewContainer.onGUIHandler = this.OnPreview;

			this._avatarField = this.rootVisualElement.Q<ObjectField>("AvatarField");
			this._avatarField.objectType = typeof(VRCAvatarDescriptor);
			this._avatarField.RegisterValueChangedCallback(this.OnAvatarChanged);

			this._overrideControllerField = this.rootVisualElement.Q<ObjectField>("OverrideControllerField");
			this._overrideControllerField.objectType = typeof(AnimatorOverrideController);
			this._overrideControllerField.RegisterValueChangedCallback(this.OnOverrideControllerChanged);

			this._animationListView = this.rootVisualElement.Q<ListView>("AnimationList");
			this._animationClipListViewController = new AnimationClipListViewController(this._animationListView);

			this._skinnedMeshRendererListView = this.rootVisualElement.Q<ListView>("SkinnedMeshRendererList");
			this._skinnedMeshRendererListViewController = new SkinnedMeshRendererListViewController(this._skinnedMeshRendererListView);

			this._blendShapeListView = this.rootVisualElement.Q<ListView>("BlendShapeList");
			this._blendShapeListViewController = new BlendShapeListViewController(this._blendShapeListView);
		}
		
		private void OnPreview() {
			if (this._preview == null) return;
			if (this._preview.OnGUI(this._previewContainer.contentRect)) {
				this.Repaint();
			}
		}


		private void OnAvatarChanged(ChangeEvent<Object> evt) {
			if (!(evt.newValue is VRCAvatarDescriptor avatarDescriptor)) {
				this._skinnedMeshRendererListView.itemsSource = ImmutableArray<Object>.Empty;
				this._skinnedMeshRenderers = null;
				this._selectedSkinnedMeshRenderer = null;
				this.ResetBlendShapeListView();
				this._skinnedMeshRendererListViewController.ResetListView();
				return;
			}

			this._avatar = avatarDescriptor;

			this._preview?.Dispose();
			this._preview = new AvatarPreviewView(this._avatar);
			
			this.ResetBlendShapeListView();
			this._selectedSkinnedMeshRenderer = null;

			this._skinnedMeshRenderers = this._avatar.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>()
				.Where(renderer => renderer.sharedMesh != null)
				.OrderBy(renderer => renderer.name)
				.ToArray();
			this._skinnedMeshRendererListViewController.UpdateListElements(this._skinnedMeshRenderers);
			
		}

		private void OnOverrideControllerChanged(ChangeEvent<Object> evt) {
			if (!(evt.newValue is AnimatorOverrideController overrideController)) {
				this._animationListView.itemsSource = ImmutableList<Object>.Empty;
				this._overrideTargetClips = null;
				return;
			}

			this._overrideController = overrideController;
			this._overrideTargetClips = overrideController.animationClips
				.Distinct()
				.OrderBy(clip => clip.name)
				.ToArray();

			this._animationListView.itemsSource = this._overrideTargetClips;
		}
		
		
		private void OnSkinnedMeshRendererSelectionChanged(List<object> obj) {
			if (obj.Count != 1) {
				this.ResetBlendShapeListView();
				return;
			}
			
			if (!(obj[0] is SkinnedMeshRenderer skinnedMeshRenderer)) {
				this.ResetBlendShapeListView();
				return;
			}

			this._selectedSkinnedMeshRenderer = skinnedMeshRenderer;
			this._previewSkinnedMeshRenderer = this._preview.Descriptor.transform
				.Find(VRCUtils.GetPathInAvatar(this._selectedSkinnedMeshRenderer.transform))
				.GetComponent<SkinnedMeshRenderer>();

			this._blendShapes = skinnedMeshRenderer.sharedMesh.GetBlendShapeNames()
				.Select((blendShapeName, index) => (blendShapeName, skinnedMeshRenderer.GetBlendShapeWeight(index), false))
				.ToArray();
			this._defaultBlendShapeValue = this._blendShapes.Select(blendShape => blendShape.value).ToArray();
			this._blendShapeListView.itemsSource = this._blendShapes;

		}
		
		private void ResetBlendShapeListView() {
			this._blendShapes = null;
			this._blendShapeListView.itemsSource = ImmutableList<Object>.Empty;
		}


		[MenuItem("Raitichan/AnimatorController Override Editor")]
		public static void ShowWindow() {
			AnimatorControllerOverrideEditor window = GetWindow<AnimatorControllerOverrideEditor>();
			window.titleContent = new GUIContent("AnimatorControllerOverride Editor");
		}

		private class AnimationClipListViewController {
			private readonly ListView _listView;
			private AnimationClip[] _animationClips = Array.Empty<AnimationClip>();

			public AnimationClipListViewController(ListView listView) {
				this._listView = listView;
				this._listView.selectionType = SelectionType.Single;
				this._listView.makeItem += () => new Label { style = { paddingLeft = 5 } };
				this._listView.bindItem += this.BindItem;
				this._listView.itemsSource = ImmutableArray<Object>.Empty;
			}
			
			private void BindItem(VisualElement element, int index) {
				if (!(element is Label label)) return;
				label.text = this._animationClips[index].name;
			}
			
		}

		private class SkinnedMeshRendererListViewController {
			private readonly ListView _listView;
			private SkinnedMeshRenderer[] _skinnedMeshRenderers = Array.Empty<SkinnedMeshRenderer>();

			public SkinnedMeshRendererListViewController(ListView listView) {
				this._listView = listView;
				this._listView.selectionType = SelectionType.Single;
				this._listView.makeItem += () => new Label { style = { paddingLeft = 5 } };
				this._listView.bindItem += this.BindItem;
				this.ResetListView();
			}

			public void UpdateListElements(SkinnedMeshRenderer[] skinnedMeshRenderers) {
				if (skinnedMeshRenderers == null) {
					this.ResetListView();
					return;
				}

				this._skinnedMeshRenderers = skinnedMeshRenderers
					.Where(renderer => renderer.sharedMesh != null)
					.ToArray();
			}

			public void ResetListView() {
				this._listView.itemsSource = ImmutableArray<Object>.Empty;
				this._skinnedMeshRenderers = Array.Empty<SkinnedMeshRenderer>();
			}

			private void BindItem(VisualElement element, int index) {
				if (!(element is Label label)) return;
				label.text = this._skinnedMeshRenderers[index].name;
			}
		}

		private class BlendShapeListViewController {
			private readonly ListView _listView;
			private (string blendShapeName, float value, bool enable)[] _blendShapes = Array.Empty<(string blendShapeName, float value, bool enable)>();

			public BlendShapeListViewController(ListView listView) {
				this._listView = listView;
				this._listView.selectionType = SelectionType.None;
				this._listView.makeItem += this.MakeItem;
				this._listView.bindItem += this.BindItem;
				this._listView.itemsSource = ImmutableArray<Object>.Empty;
			}

			private VisualElement MakeItem() {
				BlendShapeElement blendShapeElement = new BlendShapeElement();
				blendShapeElement.onChangeValue += this.OnChangeValue;
				blendShapeElement.onChangeEnable += this.OnChangeEnable;
				return blendShapeElement;
			}

			private void OnChangeValue(BlendShapeElement element) {
				this._blendShapes[element.Index].value = element.Value;
			}

			private void OnChangeEnable(BlendShapeElement element) {
				this._blendShapes[element.Index].enable = element.Enable;
			}
			

			private void BindItem(VisualElement element, int index) {
				if (!(element is BlendShapeElement blendShapeElement)) return;
				(string blendShapeName, float value, bool enable) blendShape = this._blendShapes[index];
				blendShapeElement.Index = index;
				blendShapeElement.Text = blendShape.blendShapeName;
				blendShapeElement.SetValueWithoutNotify(blendShape.enable, blendShape.value);

			}
		}
	}
}