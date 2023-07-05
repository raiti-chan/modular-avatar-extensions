using raitichan.com.modular_avatar.extensions.Editor.UIElement;
using raitichan.com.modular_avatar.extensions.ReflectionHelper.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows.UIElement {
	public class MaterialReplaceElement : VisualElement {
		private const string UXML_GUID = "626c64ba67f146c40b06c70a8d26aebf";

		public int MaterialIndex { get; set; }
		public ReadOnlyObjectField OriginField { get; }
		public ObjectField ReplaceField { get; }

		public MaterialReplaceElement() {
			string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
			VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
			uxml.CloneTree(this);

			this.OriginField = this.Q<ReadOnlyObjectField>("OriginField");
			this.OriginField.objectType = typeof(Material);
			
			this.ReplaceField = this.Q<ObjectField>("ReplaceField");
			this.ReplaceField.objectType = typeof(Material);
			this.ReplaceField.RegisterValueChangedCallback(this.OnObjectChanged);
		}

		private void OnObjectChanged(ChangeEvent<Object> evt) {
			Material material = evt.newValue as Material;
			using (MaterialChangeEvent pooled = MaterialChangeEvent.GetPooled(this.MaterialIndex, material)) {
				pooled.target = this;
				this.SendEvent(pooled);
			}
			evt.StopPropagation();
		}
	}

	public class MaterialChangeEvent : EventBase<MaterialChangeEvent> {
		public int MaterialIndex { get; private set; }
		public Material Material { get; private set; }
		
		protected override void Init() {
			base.Init();
			this.LocalInit();
		}

		private void LocalInit() {
			EventBaseHelper.SetPropagation(this, EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable);
		}

		public MaterialChangeEvent() {
			this.LocalInit();
		}

		public static MaterialChangeEvent GetPooled(int materialIndex, Material material) {
			MaterialChangeEvent evt = EventBase<MaterialChangeEvent>.GetPooled();
			evt.MaterialIndex = materialIndex;
			evt.Material = material;
			return evt;
		}
		
	}
}