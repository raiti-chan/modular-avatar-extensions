#nullable enable
using System;
using nadena.dev.modular_avatar.core;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
    [AddComponentMenu("Modular Avatar / MAEx RenderQueue Toggle")]
    public class MAExRenderQueueToggle : AvatarTagComponent {
        // 2501 = 大体透ける, 2011 = 透けないこともある
        [Tooltip("2501 = 大体透ける, 2011 = 透けないこともある")]
        public int renderQueue = 2000;

        [Tooltip("複数マテリアルのRenderQueueの決定方法")]
        public MultiMaterialMode multiMaterialMode = MultiMaterialMode.Unity;

        public Renderer[] renderers = Array.Empty<Renderer>();
        public Material[] ignoreMaterials = Array.Empty<Material>();

        
        // ReSharper disable once Unity.RedundantEventFunction
        private void Start() {
            // チェックボックスを表示するため
        }


        public enum MultiMaterialMode {
            [InspectorName("統一")]
            Unity,

            [InspectorName("オフセット")]
            Offset,

            [InspectorName("順序")]
            Order
        }
    }
}