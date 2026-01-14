#nullable enable
using System;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using raitichan.com.modular_avatar.extensions.Modules;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace raitichan.com.modular_avatar.extensions.Editor {
    public class RenderQueueToggleHook {
        private readonly BuildContext _buildContext;

        public RenderQueueToggleHook(BuildContext buildContext) {
            this._buildContext = buildContext;
        }

        internal void OnProcessAvatar() {
            var renderQueueToggles = this._buildContext.AvatarRootTransform
                .GetComponentsInChildren<MAExRenderQueueToggle>(true)
                .Where(toggle => toggle.enabled);
            foreach (MAExRenderQueueToggle maExRenderQueueToggle in renderQueueToggles) {
                this.OnProcess(maExRenderQueueToggle);
                Object.DestroyImmediate(maExRenderQueueToggle);
            }
        }

        private void OnProcess(MAExRenderQueueToggle renderQueueToggle) {
            var materials = renderQueueToggle.renderers
                .Where(renderer => renderer != null)
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => !renderQueueToggle.ignoreMaterials.Contains(material))
                .ToHashSet();
            if (materials.Count == 0) return;

            int renderQueueOffset = 0;
            int prevRenderQueue = 0;
            var clonedMaterials = materials
                .OrderBy(renderer => renderer.renderQueue)
                .ToDictionary(
                    material => material,
                    material => {
                        int renderQueue;
                        switch (renderQueueToggle.multiMaterialMode) {
                            case MAExRenderQueueToggle.MultiMaterialMode.Unity:
                                renderQueue = renderQueueToggle.renderQueue;
                                break;
                            case MAExRenderQueueToggle.MultiMaterialMode.Offset:
                                if (prevRenderQueue != 0) {
                                    renderQueueOffset = material.renderQueue - prevRenderQueue;
                                }

                                renderQueue = renderQueueToggle.renderQueue + renderQueueOffset;
                                break;
                            case MAExRenderQueueToggle.MultiMaterialMode.Order:
                                if (prevRenderQueue != 0 && prevRenderQueue != material.renderQueue) {
                                    renderQueueOffset++;
                                }

                                renderQueue = renderQueueToggle.renderQueue + renderQueueOffset;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        prevRenderQueue = material.renderQueue;

                        return this.CloneMaterial(material, renderQueue);
                    });

            ModularAvatarMaterialSetter setter = renderQueueToggle.gameObject.AddComponent<ModularAvatarMaterialSetter>();
            foreach (Renderer renderer in renderQueueToggle.renderers) {
                AvatarObjectReference reference = new();
                reference.Set(renderer.gameObject);
                for (int index = 0; index < renderer.sharedMaterials.Length; index++) {
                    Material originMaterial = renderer.sharedMaterials[index];
                    if (clonedMaterials.TryGetValue(originMaterial, out Material clonedMaterial)) {
                        setter.Objects.Add(new MaterialSwitchObject {
                            Object = reference,
                            Material = clonedMaterial,
                            MaterialIndex = index
                        });
                    }
                }


            }
            
        }

        private Material CloneMaterial(Material origin, int renderQueue) {
            Material cloned = new(origin);
            AssetDatabase.AddObjectToAsset(cloned, this._buildContext.AssetContainer);
            cloned.name = cloned.name + "_" + renderQueue;
            cloned.renderQueue = renderQueue;
            return cloned;
        }
    }
}