using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class PresetPreviewContext {
		/*
		 * -3 : オブジェクトの素の状態
		 * -2 : 管理上でのデフォルト							|
		 * -1 : 指定されたプリセットの生の値					| プリセットアニメーション
		 *  0 : トグル 0  | トグル0アニメーション
		 *  1 : トグル 1  | トグル1アニメーション
		 *  ...
		 */
		
		public const int NONE_LAYER = -3;
		public const int USE_OBJECT_BLOCK_LAYER = -2;
		public const int PRESET_LAYER = -1;

		public int SelectPresetIndex { get; set; }

		private readonly Dictionary<GameObject, LayerStack<bool>> _showObjectLayerStack = new Dictionary<GameObject, LayerStack<bool>>();
		private readonly Dictionary<SkinnedMeshRenderer, Dictionary<string, LayerStack<float>>> _blendShapeLayerStack = new Dictionary<SkinnedMeshRenderer, Dictionary<string, LayerStack<float>>>();
		private readonly Dictionary<Renderer, Dictionary<int, LayerStack<Material>>> _materialLayerStack = new Dictionary<Renderer, Dictionary<int, LayerStack<Material>>>();

		public void ClearLayerStack() {
			this._showObjectLayerStack.Clear();
			this._blendShapeLayerStack.Clear();
			this._materialLayerStack.Clear();
		}

		public LayerStack<bool> GetShowObjectLayerStack(GameObject gameObject) {
			if (this._showObjectLayerStack.TryGetValue(gameObject, out LayerStack<bool> layerStack)) return layerStack;
			
			layerStack = new LayerStack<bool>();
			this._showObjectLayerStack[gameObject] = layerStack;
			return layerStack;
		}

		public LayerStack<float> GetBlendShapeLayerStack(SkinnedMeshRenderer skinnedMeshRenderer, string blendShapeName) {
			if (!this._blendShapeLayerStack.TryGetValue(skinnedMeshRenderer, out Dictionary<string, LayerStack<float>> blendShapeDictionary)) {
				blendShapeDictionary = new Dictionary<string, LayerStack<float>>();
				this._blendShapeLayerStack[skinnedMeshRenderer] = blendShapeDictionary;
			}

			if (blendShapeDictionary.TryGetValue(blendShapeName, out LayerStack<float> layerStack)) return layerStack;
			
			layerStack = new LayerStack<float>();
			blendShapeDictionary[blendShapeName] = layerStack;
			return layerStack;
		}

		public LayerStack<Material> GetMaterialLayerStack(Renderer renderer, int materialIndex) {
			if (!this._materialLayerStack.TryGetValue(renderer, out Dictionary<int, LayerStack<Material>> materialDictionary)) {
				materialDictionary = new Dictionary<int, LayerStack<Material>>();
				this._materialLayerStack[renderer] = materialDictionary;
			}

			if (materialDictionary.TryGetValue(materialIndex, out LayerStack<Material> layerStack)) return layerStack;

			layerStack = new LayerStack<Material>();
			materialDictionary[materialIndex] = layerStack;
			return layerStack;
		}

		public class LayerStack<TValue> {
			private readonly Stack<LayerValue<TValue>> _stack = new Stack<LayerValue<TValue>>();

			public void AddLayer(int layer, TValue value) {
				if (this._stack.Count <= 0) {
					this._stack.Push(new LayerValue<TValue>(layer, value));
					return;
				}

				Stack<LayerValue<TValue>> temp = new Stack<LayerValue<TValue>>();
				while (this._stack.Count > 0) {
					int current = this._stack.Peek().Layer;
					if (current == layer) {
						this._stack.Pop();
						this._stack.Push(new LayerValue<TValue>(layer, value));
						break;
					}

					if (current < layer) {
						this._stack.Push(new LayerValue<TValue>(layer, value));
						break;
					}

					temp.Push(this._stack.Pop());
				}

				while (temp.Count > 0) {
					this._stack.Push(temp.Pop());
				}
			}

			public void RemoveLayer(int layer) {
				Stack<LayerValue<TValue>> temp = new Stack<LayerValue<TValue>>();
				while (this._stack.Count > 0) {
					int current = this._stack.Peek().Layer;
					if (current == layer) {
						this._stack.Pop();
						break;
					}

					if (current < layer) {
						break;
					}

					temp.Push(this._stack.Pop());
				}

				while (temp.Count > 0) {
					this._stack.Push(temp.Pop());
				}
			}

			public bool ContainsLayer(int layer) {
				Stack<LayerValue<TValue>> temp = new Stack<LayerValue<TValue>>(this._stack);
				while (temp.Count > 0) {
					if (temp.Pop().Layer == layer) return true;
				}

				return false;
			}
			
			

			public LayerValue<TValue> GetTopLayer() {
				if (this._stack.Count > 0) {
					return this._stack.Peek();
				}

				return new LayerValue<TValue>(NONE_LAYER, default);
			}

			public override string ToString() {
				StringBuilder builder = new StringBuilder();
				Stack<LayerValue<TValue>> clonedStack = new Stack<LayerValue<TValue>>(this._stack);
				while (clonedStack.Count > 0) {
					builder.AppendLine(clonedStack.Pop().ToString());
				}
				return builder.ToString();
			}
		}

		public readonly struct LayerValue<TValue> {
			public readonly int Layer;
			public readonly TValue Value;

			public LayerValue(int layer, TValue value) {
				this.Layer = layer;
				this.Value = value;
			}

			public override string ToString() {
				switch (this.Layer) {
					case NONE_LAYER:
						return "NONE_LAYER : NONE";
					case USE_OBJECT_BLOCK_LAYER:
						return $"USE_OBJECT_BLOCK_LAYER : {this.Value}";
					case PRESET_LAYER:
						return $"PRESET_LAYER : {this.Value}";
					default:
						return $"TOGGLE_{this.Layer} : {this.Value}";
				}
			}
		}
	}
}