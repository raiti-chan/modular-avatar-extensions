using nadena.dev.modular_avatar.core;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[DisallowMultipleComponent]
	[AddComponentMenu("Modular Avatar/MAEx MeshRendererOverride")]
	public class MAExMeshRendererOverride : AvatarTagComponent {
		public Transform AnchorOverride;
		public Transform RootBone;
		public Bounds Bounds = new Bounds(Vector3.zero, Vector3.one * 2);
	}
}