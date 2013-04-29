using System;
using System.Collections.Generic;
using System.Linq;
using TamperEvidentLogs.Aggregators;

namespace TamperEvidentLogs
{
    public class HashTree
    {
        private class Node
        {
            public byte[] Data { get; set; }
            
            public byte[] Hash { get; set; }

            public int Index { get; set; }
        }

        public HashTree(Aggregator aggregator)
        {
            this.Tree = new Dictionary<int, Node>();
            this.Height = 0;
            this.LatestLeafNode = new Node();
            this.LatestLeafNode.Index = 0;
            this.Aggregator = aggregator;
        }

        #region Tree Properties

        public Aggregator Aggregator { get; private set; }

        public int Height { get; set; }

        public int NumberOfLeaves
        {
            get
            {
                return this.LatestLeafNode.Index - (1 << this.Height) + 1;
            }
        }

        public byte[] RootCommitment
        {
            get
            {
                return this.Root.Hash;
            }
        }

        public string RootCommitmentString
        {
            get
            {
                return Encoding.EncodeBytes(RootCommitment);
            }
        }

        private Dictionary<int, Node> Tree { get; set; }

        #endregion

        #region Tree Operations

        public void Append(string data)
        {
            Append(Encoding.DecodeString(data));
        }

        public void Append(byte[] data)
        {
            // Tree is full, expand it
            if (this.NumberOfLeaves >= (1 << this.Height))
            {
                GrowTree();
            }

            int index = this.LatestLeafNode.Index + 1;

            Node node = new Node();
            node.Data = data;
            node.Hash = this.Aggregator.HashLeaf(data);

            InsertNode(node, index);
        }

        public void PutHash(int index, byte[] hash)
        {
            if (this.Tree.ContainsKey(index))
            {
                throw new ArgumentException("Index in use.");
            }
            else if (Left(index) != null)
            {
                throw new ArgumentException("Left exists");
            }
            else if (Right(index) != null)
            {
                throw new ArgumentException("Right exists");
            }

            Node node = new Node();
            node.Hash = hash;

            InsertNode(node, index);
        }

        public MembershipProof GenerateMembershipProof(int leafIndex)
        {
            int index = (1 << this.Height) + leafIndex;

            if (!this.Tree.ContainsKey(index))
            {
                throw new ArgumentException("Index does not exist");
            }

            MembershipProof proof = new MembershipProof();
            proof.Commitment = this.RootCommitment;
            proof.MemberIndex = index;
            proof.AggregatorName = this.Aggregator.Name;

            List<Tuple<int, byte[]>> prunedTree = new List<Tuple<int, byte[]>>();

            Node node = this.Tree[index];
            proof.MemberData = node.Data;
            prunedTree.Add(new Tuple<int, byte[]>(index, node.Hash));

            do
            {
                Node sibling = Sibling(index);
                if (sibling != null)
                {
                    prunedTree.Add(new Tuple<int, byte[]>(sibling.Index, sibling.Hash));
                }
            } while ((index /= 2) > 0);

            proof.PrunedTree = prunedTree;

            return proof;
        }

        #endregion

        #region Private Tree Operations

        private void InsertNode(Node node, int index)
        {
            node.Index = index;
            this.Tree[index] = node;

            if (node.Index > this.LatestLeafNode.Index)
            {
                this.LatestLeafNode = node;
            }

            Node prevNode = node;
            int prevIndex = index;

            while ((index = index / 2) > 0)
            {
                Node parent = null;
                if (!this.Tree.TryGetValue(index, out parent) || parent == null)
                {
                    parent = new Node();
                    parent.Index = index;
                    this.Tree[index] = parent;
                }

                Node sibling = Sibling(prevIndex);

                if (isLeftBranch(prevIndex))
                {
                    parent.Hash = this.Aggregator.AggregateChildren(
                            prevNode.Hash,
                            (sibling != null) ? sibling.Hash : null
                            );
                }
                else
                {
                    parent.Hash = this.Aggregator.AggregateChildren(
                            (sibling != null) ? sibling.Hash : null,
                            prevNode.Hash
                            );
                }

                prevNode = parent;
                prevIndex = index;
            }

            this.Height = Log2((uint)this.LatestLeafNode.Index);
        }

        private void GrowTree()
        {
            Node oldRoot = this.Root;

            Node newRoot = new Node();
            newRoot.Data = null;
            newRoot.Hash = this.Aggregator.AggregateChildren(
                    (oldRoot != null) ? oldRoot.Hash : null,
                    null
                    );
            newRoot.Index = oldRoot.Index;

            Dictionary<int, Node> newTree = new Dictionary<int, Node>();
            newTree[newRoot.Index] = newRoot;

            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(oldRoot);

            while (queue.Count > 0)
            {
                Node node = queue.Dequeue();
                Node parent = Parent(node);

                int newIndex;
                if (parent == null)
                {
                    newIndex = 2;
                }
                else if (isLeftBranch(node))
                {
                    newIndex = parent.Index * 2;
                }
                else // must be right branch
                {
                    newIndex = parent.Index * 2 + 1;
                }

                newTree[newIndex] = node;

                Node left = Left(node);
                if (left != null)
                {
                    queue.Enqueue(left);
                }

                Node right = Right(node);
                if (right != null)
                {
                    queue.Enqueue(right);
                }

                node.Index = newIndex;
            }

            this.Tree = newTree;
        }

        #endregion

        #region TreeHelpers

        private Node LatestLeafNode { get; set; }

        private Node Root
        {
            get
            {
                Node root = null;
                this.Tree.TryGetValue(1, out root);
                return root;
            }
        }

        private Node Left(int index)
        {
            Node left = null;
            this.Tree.TryGetValue(index * 2, out left);
            return left;
        }

        private Node Left(Node node)
        {
            return Left(node.Index);
        }

        private Node Right(int index)
        {
            Node right = null;
            this.Tree.TryGetValue(index * 2 + 1, out right);
            return right;
        }

        private Node Right(Node node)
        {
            return Right(node.Index);
        }

        private Node Parent(int index)
        {
            Node parent = null;
            this.Tree.TryGetValue(index / 2, out parent);
            return parent;
        }

        private Node Parent(Node node)
        {
            return Parent(node.Index);
        }

        private Node Sibling(int index)
        {
            int parent = index / 2;

            // If left, get right
            if (index == parent * 2)
            {
                return Right(parent);
            }
            else // right, get left
            {
                return Left(parent);
            }
        }

        private bool isRoot(int index)
        {
            return index == 1;
        }

        private bool isLeftBranch(int index)
        {
            return index % 2 == 0;
        }

        private bool isLeftBranch(Node node)
        {
            return isLeftBranch(node.Index);
        }

        private bool isRightBranch(int index)
        {
            return index % 2 != 0 && !isRoot(index);
        }

        private bool isRightBranch(Node node)
        {
            return isRightBranch(node.Index);
        }

        #endregion

        #region MISC Helpers

        private static int Log2(uint bits) // returns 0 for bits=0
        {
            int log = 0;
            if ((bits & 0xffff0000) != 0) { bits >>= 16; log = 16; }
            if (bits >= 256) { bits >>= 8; log += 8; }
            if (bits >= 16) { bits >>= 4; log += 4; }
            if (bits >= 4) { bits >>= 2; log += 2; }
            return (int)(log + (bits >> 1));
        }

        #endregion
    }
}
