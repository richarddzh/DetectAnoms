using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectAnoms
{
    public class FPGrowth<T>
    {
        public FPTree CreateFPTree(IEnumerable<IEnumerable<T>> trans, IEnumerable<int> count, int minSupport)
        {
            count = count ?? Enumerable.Repeat(1, int.MaxValue);
            var tree = new FPTree();
            tree.HeaderTable = this.InitHeaderTable(trans, count, minSupport);
            tree.Root = new TreeNode()
            {
                Name = default(T),
                Count = 0,
            };

            foreach (var pair in Enumerable.Zip(trans, count, (x, y) => Tuple.Create(x, y)))
            {
                var items = pair.Item1
                    .Where(x => tree.HeaderTable.ContainsKey(x))
                    .Select(x => Tuple.Create(x, tree.HeaderTable[x].Count))
                    .OrderByDescending(x => x.Item2)
                    .ThenBy(x => x.Item1)
                    .Select(x => x.Item1)
                    .ToList();
                this.UpdateFPTree(tree, items, pair.Item2);
            }

            return tree;
        }

        private void UpdateFPTree(FPTree tree, IList<T> items, int count)
        {
            var treeNode = tree.Root;
            foreach (var item in items)
            {
                if (treeNode.Children.TryGetValue(item, out var nextTree))
                {
                    nextTree.Count += count;
                    treeNode = nextTree;
                }
                else
                {
                    var headerItem = tree.HeaderTable[item];
                    var newNode = new TreeNode()
                    {
                        Name = item,
                        Count = count,
                        Parent = treeNode,
                        NodeLink = headerItem.Node,
                    };
                    headerItem.Node = newNode;
                    treeNode.Children.Add(item, newNode);
                    treeNode = newNode;
                }
            }
        }

        private Dictionary<T, HeaderItem> InitHeaderTable(IEnumerable<IEnumerable<T>> trans, IEnumerable<int> count, int minSupport)
        {
            var headerTable = new Dictionary<T, HeaderItem>();
            foreach (var pair in Enumerable.Zip(trans, count, (x, y) => Tuple.Create(x, y)))
            {
                foreach (var item in pair.Item1)
                {
                    if (headerTable.TryGetValue(item, out var headerItem))
                    {
                        headerItem.Count += pair.Item2;
                    }
                    else
                    {
                        headerTable.Add(item, new HeaderItem()
                        {
                            Count = pair.Item2,
                        });
                    }
                }
            }
            return headerTable
                .Where(x => x.Value.Count >= minSupport)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public class FPTree
        {
            public TreeNode Root { get; set; }
            public Dictionary<T, HeaderItem> HeaderTable { get; set; }
        }

        public class HeaderItem
        {
            public int Count { get; set; }
            public TreeNode Node { get; set; }
        }

        public class TreeNode
        {
            public T Name { get; set; }
            public int Count { get; set; }
            public TreeNode Parent { get; set; }
            public TreeNode NodeLink { get; set; }
            public Dictionary<T, TreeNode> Children { get; set; }

            public TreeNode()
            {
                this.Children = new Dictionary<T, TreeNode>();
            }

            internal void Dump(int depth, int tabs)
            {
                if (depth < 0)
                {
                    return;
                }

                Trace.TraceInformation("{0}{1} {2}", new string(' ', tabs), this.Name, this.Count);
                foreach (var child in this.Children)
                {
                    child.Value.Dump(depth - 1, tabs + 1);
                }
            }
        }
    }
}
