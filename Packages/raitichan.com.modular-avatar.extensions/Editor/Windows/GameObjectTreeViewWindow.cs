using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace raitichan.com.modular_avatar.extensions.Editor.Windows {
	public class GameObjectTreeViewWindow : EditorWindow {
		public static IEnumerable<GameObject> ShowModalWindow(Transform root) {
			GameObjectTreeViewWindow window = CreateInstance<GameObjectTreeViewWindow>();
			window.titleContent = new GUIContent("Select add object");
			window._treeView = new GameObjectTreeView(new TreeViewState { expandedIDs = new List<int> { 0 } }) {
				RootTransform = root,
				onDoubleClickSelect = () => {
					window._isCancel = false;
					window.Close();
				}
			};
			window.ShowModalUtility();

			return window._isCancel ? Enumerable.Empty<GameObject>() : window._treeView.SelectObjects.Select(transform => transform.gameObject);
		}

		private GameObjectTreeView _treeView;

		private bool _isCancel = true;

		private void OnGUI() {
			this._treeView.OnGUI(new Rect(0, 0, this.position.width, this.position.height - 20));
			if (GUI.Button(new Rect(this.position.width - 140, position.height - 20, 70, 20), "Add")) {
				this._isCancel = false;
				this.Close();
				return;
			}

			if (GUI.Button(new Rect(this.position.width - 70, position.height - 20, 70, 20), "Cancel")) {
				this._isCancel = true;
				this.Close();
				return;
			}

			if (Event.current.type != EventType.KeyUp) return;
			// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
			switch (Event.current.keyCode) {
				case KeyCode.Return:
					this._isCancel = false;
					this.Close();
					break;
				case KeyCode.Escape:
					this._isCancel = true;
					this.Close();
					break;
			}
		}

		private class GameObjectTreeView : TreeView {
			private Transform _rootTransform;

			public Transform RootTransform {
				set {
					this._rootTransform = value;
					this.Reload();
				}
			}

			public Action onDoubleClickSelect;

			public IEnumerable<Transform> SelectObjects => this.selectIndexes.Where(id => id != 0).Select(id => this._transforms[id]);

			public GameObjectTreeView(TreeViewState state) : base(state) { }

			private IList<int> selectIndexes = new List<int>();

			private readonly List<Transform> _transforms = new List<Transform>();

			protected override void DoubleClickedItem(int id) {
				if (id == 0) return;
				this.selectIndexes = new List<int> { id };
				this.onDoubleClickSelect.Invoke();
			}

			protected override void SelectionChanged(IList<int> selectedIds) {
				this.selectIndexes = selectedIds;
			}

			protected override TreeViewItem BuildRoot() {
				this._transforms.Clear();

				TreeViewItem root = new TreeViewItem(-1, -1, "<root>");

				List<TreeViewItem> treeItems = new List<TreeViewItem>();
				this.TraverseTransform(0, treeItems, this._rootTransform);

				SetupParentsAndChildrenFromDepths(root, treeItems);
				return root;
			}

			private void TraverseTransform(int depth, List<TreeViewItem> items, Transform transform) {
				items.Add(new TreeViewItem(items.Count, depth, transform.name));
				this._transforms.Add(transform);

				foreach (Transform child in transform) {
					this.TraverseTransform(depth + 1, items, child);
				}
			}
		}
	}
}