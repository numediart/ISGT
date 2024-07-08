using System.Collections.Generic;
using UnityEngine;

namespace Pro_gen.QuadTree
{
    /// <summary>
    ///  Quad tree node class, used to subdivide the space in a quad tree structure to optimize the placement of props
    /// </summary>
    public class QuadTreeNode : QuadTreeNodeBase
    {
        public QuadTreeNode(Bounds bounds, int depth, int maxDepth = 5)
            : base(bounds, depth, maxDepth)
        {
        }

        /// <summary>
        /// Insert a prop in the tree if it's inside the bounds of the node and the prop is not already in the tree
        /// </summary>
        /// <param name="prop"></param>
        public override void Insert(Props prop)
        {
            Collider[] intersectingColliders = Physics.OverlapBox(Bounds.center, Bounds.extents, Quaternion.identity);

            // Check if any part of the prop is inside the bounds, collider have tag 'BoundingBox'
            bool isInside = false;
            foreach (var collider in intersectingColliders)
            {
                if (collider.CompareTag("BoundingBox"))
                {
                    isInside = true;
                    break;
                }
            }

            if (!isInside)
                return;

            // Insert the prop in the current node
            _objects.Add(prop);

            // Try to subdivide the node if it's not at max depth
            if (_depth < max_depth)
            {
                Subdivide();
                // Insert the prop in the children nodes
                foreach (var child in _children)
                {
                    child.Insert(prop);
                }
            }
        }

        /// <summary>
        ///  Find the biggest empty nodes in the tree that can fit the prop
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public override List<QuadTreeNodeBase> FindBiggestEmptyNodes(PropsCategory category)
        {
            List<QuadTreeNodeBase> result = new List<QuadTreeNodeBase>();
            List<QuadTreeNodeBase> bestResult = new List<QuadTreeNodeBase>();
            int minDepth = int.MaxValue;
            FindBiggestEmptyNodesRecursive(this, ref minDepth, result, bestResult, category);
            if (bestResult.Count > 0)
            {
                return bestResult;
            }

            return result;
        }

        /// <summary>
        /// Find the biggest empty nodes in the tree that can fit the prop, recursively
        /// </summary>
        /// <param name="node"></param>
        /// <param name="minDepth"></param>
        /// <param name="result"></param>
        /// <param name="bestResult"></param>
        /// <param name="category"></param>
        private void FindBiggestEmptyNodesRecursive(QuadTreeNode node, ref int minDepth, List<QuadTreeNodeBase> result,
            List<QuadTreeNodeBase> bestResult, PropsCategory category)
        {
            if (node._objects.Count == 0)
            {
                if (node._depth < minDepth)
                {
                    minDepth = node._depth;
                    result.Clear();
                    bestResult.Clear();
                    result.Add(node);
                    if (IsBestChoice(node, category))
                    {
                        bestResult.Add(node);
                    }
                }
                else if (node._depth == minDepth)
                {
                    result.Add(node);
                    if (IsBestChoice(node, category))
                    {
                        bestResult.Add(node);
                    }
                }
            }
            else if (node._children != null)
            {
                foreach (var child in node._children)
                {
                    FindBiggestEmptyNodesRecursive((QuadTreeNode)child, ref minDepth, result, bestResult, category);
                }
            }
        }

        /// <summary>
        ///  Check if the node is the best choice for the prop category to be placed
        /// </summary>
        /// <param name="node"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        private bool IsBestChoice(QuadTreeNode node, PropsCategory category)
        {
            // If the object is a sofa, a fridge, a bed or a shelf, we want a wall node
            if (category == PropsCategory.Sofa || category == PropsCategory.Fridge || category == PropsCategory.Bed)
            {
                return node.isWallNode;
            }

            return false;
        }

        /// <summary>
        ///  Subdivide the node into 4 children
        /// </summary>
        protected override void Subdivide()
        {
            if (_children != null)
                return;
            _children = new QuadTreeNodeBase[4];
            Vector3 size = Bounds.size / 2f;
            // Conserve same height
            size.y = Bounds.size.y;
            Vector3 center = Bounds.center;

            _children[0] = new QuadTreeNode(new Bounds(center + new Vector3(-size.x / 2, 0, -size.z / 2), size),
                _depth + 1, max_depth);
            _children[1] = new QuadTreeNode(new Bounds(center + new Vector3(size.x / 2, 0, -size.z / 2), size),
                _depth + 1, max_depth);
            _children[2] = new QuadTreeNode(new Bounds(center + new Vector3(-size.x / 2, 0, size.z / 2), size),
                _depth + 1, max_depth);
            _children[3] = new QuadTreeNode(new Bounds(center + new Vector3(size.x / 2, 0, size.z / 2), size),
                _depth + 1, max_depth);
        }

        /// <summary>
        ///  Get all empty nodes in the tree
        /// </summary>
        /// <returns></returns>
        public override List<Bounds> GetAllEmptyNodes()
        {
            List<Bounds> result = new List<Bounds>();
            GetAllEmptyNodesRecursive(this, result);

            if (result.Count == 0) // if all nodes are occupied, return the biggest one i.e. the entire room
            {
                result.Add(Bounds);
            }

            return result;
        }

        /// <summary>
        ///  Get all empty nodes in the tree
        /// </summary>
        /// <param name="node"></param>
        /// <param name="result"></param>
        private void GetAllEmptyNodesRecursive(QuadTreeNode node, List<Bounds> result)
        {
            if (node._objects.Count == 0)
            {
                result.Add(node.Bounds);
            }
            else if (node._children != null)
            {
                foreach (var child in node._children)
                {
                    GetAllEmptyNodesRecursive((QuadTreeNode)child, result);
                }
            }
        }

        /// <summary>
        /// Draw the gizmo of the node
        /// </summary>
        public override void DrawGizmo()
        {
            Gizmos.color = _objects.Count == 0 ? Color.green : Color.magenta;
            Gizmos.DrawWireCube(Bounds.center, Bounds.size);
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    child.DrawGizmo();
                }
            }
        }
    }
}