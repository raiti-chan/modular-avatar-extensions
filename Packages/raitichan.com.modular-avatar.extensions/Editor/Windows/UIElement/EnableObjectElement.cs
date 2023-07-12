using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.Modules;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class EnableObjectElement : CustomBindableElement {
		private const string UXML_GUID = "f454c40251d1f1c4e81270e85b44a2c1";

		public EnableObjectElement() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this.Q<Toggle>("EnableToggle").RegisterValueChangedCallback(this.OnEnableChanged);
			this.Q<ReadOnlyObjectField>("ObjectField").objectType = typeof(GameObject);
		}

		private void OnEnableChanged(ChangeEvent<bool> evt) {
			if (this.BindingProperty == null) return;
			if (!this.IsBound) return;
			GameObject gameObject = this.BindingProperty.FindPropertyRelative(nameof(MAExObjectPreset.EnableObject.gameObject)).objectReferenceValue as GameObject;
			using (EnableObjectChangeEvent pooled = EnableObjectChangeEvent.GetPooled(gameObject, evt.newValue)) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
			
		}
	}

	public class EnableObjectChangeEvent : EventBase<EnableObjectChangeEvent> {
		public GameObject GameObject { get; private set; }
		public bool Enable { get; private set; }
		
		protected override void Init() {
			base.Init();
			this.LocalInit();
		}

		private void LocalInit() {
			EventBaseHelper.SetPropagation(this, EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable);
		}

		public EnableObjectChangeEvent() {
			this.LocalInit();
		}

		public static EnableObjectChangeEvent GetPooled(GameObject gameObject, bool enable) {
			EnableObjectChangeEvent evt = EventBase<EnableObjectChangeEvent>.GetPooled();
			evt.GameObject = gameObject;
			evt.Enable = enable;
			return evt;
		}
	}
}