// Copyright (c) 2020 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

#nullable disable

namespace ICSharpCode.ILSpyX.TreeView
{
	public sealed class TreeFlattener : IList, INotifyCollectionChanged
	{
		/// <summary>
		/// The root node of the flat list tree.
		/// Tjis is not necessarily the root of the model!
		/// </summary>
		internal SharpTreeNode root;
		readonly bool includeRoot;
		readonly object syncRoot = new object();

		public TreeFlattener(SharpTreeNode modelRoot, bool includeRoot)
		{
			this.root = modelRoot;
			while (root.listParent != null)
				root = root.listParent;
			root.treeFlattener = this;
			this.includeRoot = includeRoot;
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}

		// A node plus its visible descendants form one contiguous run in the flattened list, and the
		// underlying tree's Count is updated by the WHOLE run before these fire, so the notification
		// has to describe the run as one step: Count and indices must stay in agreement for a consumer
		// that reconciles in one pass. Per-node events instead leave the source already fully resized
		// while the consumer is still mid-sequence, so one that re-indexes during reconciliation
		// (Avalonia's SelectionModel re-reading SelectedItems) reads past the end.
		//
		// A multi-item Add/Remove ("range action") cannot be used for that, though: WPF's
		// ListCollectionView.ValidateCollectionChangedEventArgs rejects any Add/Remove whose item
		// count != 1 with NotSupportedException("Range actions are not supported.") - stock WPF
		// behaviour, not a LibreWPF quirk. That threw straight out of SharpTreeNodeCollection.
		// RemoveAll while the workbench closed a solution (WpfWorkbench.OnClosing -> CloseSolution),
		// and because the close runs inside the native window-close callback the escaping exception
		// aborted the process (SIGABRT) instead of surfacing as an error.
		//
		// Reset satisfies both constraints: it is exempt from that validation, and it tells the
		// consumer to re-read everything *after* the source is fully resized - which is exactly the
		// "reconcile in one step" invariant above, so the Avalonia re-indexing bug stays fixed.
		// Single-item changes still raise the precise event, keeping the common case incremental.
		static NotifyCollectionChangedEventArgs CreateRunChangedArgs(NotifyCollectionChangedAction action, IList list, int index)
		{
			if (list.Count == 1)
				return new NotifyCollectionChangedEventArgs(action, list[0], index);
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
		}

		public void NodesInserted(int index, IEnumerable<SharpTreeNode> nodes)
		{
			if (!includeRoot)
				index--;
			IList list = nodes as IList ?? new List<SharpTreeNode>(nodes);
			if (list.Count > 0)
				RaiseCollectionChanged(CreateRunChangedArgs(NotifyCollectionChangedAction.Add, list, index));
		}

		public void NodesRemoved(int index, IEnumerable<SharpTreeNode> nodes)
		{
			if (!includeRoot)
				index--;
			IList list = nodes as IList ?? new List<SharpTreeNode>(nodes);
			if (list.Count > 0)
				RaiseCollectionChanged(CreateRunChangedArgs(NotifyCollectionChangedAction.Remove, list, index));
		}

		public void Stop()
		{
			Debug.Assert(root.treeFlattener == this);
			root.treeFlattener = null;
		}

		public object this[int index] {
			get {
				if (index < 0 || index >= this.Count)
					throw new ArgumentOutOfRangeException();
				return SharpTreeNode.GetNodeByVisibleIndex(root, includeRoot ? index : index + 1);
			}
			set {
				throw new NotSupportedException();
			}
		}

		public int Count {
			get {
				return includeRoot ? root.GetTotalListLength() : root.GetTotalListLength() - 1;
			}
		}

		public int IndexOf(object item)
		{
			SharpTreeNode node = item as SharpTreeNode;
			if (node != null && node.IsVisible && node.GetListRoot() == root)
			{
				if (includeRoot)
					return SharpTreeNode.GetVisibleIndexForNode(node);
				else
					return SharpTreeNode.GetVisibleIndexForNode(node) - 1;
			}
			else
			{
				return -1;
			}
		}

		bool IList.IsReadOnly {
			get { return true; }
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get {
				return syncRoot;
			}
		}

		void IList.Insert(int index, object item)
		{
			throw new NotSupportedException();
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		int IList.Add(object item)
		{
			throw new NotSupportedException();
		}

		void IList.Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(object item)
		{
			return IndexOf(item) >= 0;
		}

		public void CopyTo(Array array, int arrayIndex)
		{
			foreach (object item in this)
				array.SetValue(item, arrayIndex++);
		}

		void IList.Remove(object item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator GetEnumerator()
		{
			for (int i = 0; i < this.Count; i++)
			{
				yield return this[i];
			}
		}
	}
}
