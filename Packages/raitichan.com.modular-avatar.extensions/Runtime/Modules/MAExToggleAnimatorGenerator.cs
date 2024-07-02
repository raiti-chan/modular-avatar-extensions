using System.Collections.Generic;
using raitichan.com.modular_avatar.extensions.Serializable;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Modules {
	[DisallowMultipleComponent]
	[AddComponentMenu("Modular Avatar/MAEx Toggle Animator Generator")]
	public class MAExToggleAnimatorGenerator : MAExAnimatorGeneratorModuleBase<MAExToggleAnimatorGenerator> {
		public string displayName;
		public string parameterName;
		public Texture2D menuIcon;
		public bool isInvert;
		public bool isInternal;
		public bool saved;
		public bool defaultValue;
		// デフォルトの状態とは反対になった場合に機能するブレンドシェイプ
		public List<BlendShapeData> blendShapeDataList;
		public List<GameObject> additionalToggleObjects;
		public List<GameObject> additionalInvertToggleObjects;
	}
}