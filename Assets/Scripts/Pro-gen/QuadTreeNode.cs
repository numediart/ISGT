using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pro_gen
{
    public class QuadTreeNode 
    {
        public Bounds bounds;
        private readonly List<Props> _objects;
        private QuadTreeNode[] _children;
        private readonly int _depth;

        private bool isWallNode;
        private int max_depth;

        public QuadTreeNode(Bounds bounds, int depth, int maxDepth = 5)
        {
            this.bounds = bounds;
            _objects = new List<Props>();
            _children = null;
            _depth = depth;
            max_depth = maxDepth;
            
            //Check if the node contains a wall
            Collider[] intersectingColliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity);
            foreach (var collider in intersectingColliders)
            {
                if (collider.CompareTag("Walls") || collider.CompareTag("Door"))
                {
                    isWallNode = true;
                    break;
                }
            }
        }

        public void determineMaxDepth(int area)
        {
            max_depth = 5 + (int) (4 * (Mathf.Sqrt(area) / 40f));
        }
        

        public void Insert(Props prop)
        {
            Collider[] intersectingColliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity);
            
            
            //Check if any part of the prop is inside the bounds, collider have tag 'BoundingBox'
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
            
            
            
            //Insert the prop in the current node
            _objects.Add(prop);
            
            //Try to subdivide the node if it's not at max depth
            if (_depth < max_depth)
            {
                Subdivide();
                //Insert the prop in the children nodes
                foreach(var child in _children)
                {
                    child.Insert(prop);
                }
            }
        }
        

        public List<QuadTreeNode> FindBiggestEmptyNodes(PropsCategory category)
        {
            List<QuadTreeNode> result = new List<QuadTreeNode>();
            List<QuadTreeNode> bestResult = new List<QuadTreeNode>();
            int minDepth = int.MaxValue;
            FindBiggestEmptyNodesRecursive(this, ref minDepth, result, bestResult, category);
            if (bestResult.Count > 0)
            {
                return bestResult;
            }
            return result;
        }

        private void FindBiggestEmptyNodesRecursive(QuadTreeNode node, ref int minDepth, List<QuadTreeNode> result, List<QuadTreeNode> bestResult, PropsCategory category)
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
                    FindBiggestEmptyNodesRecursive(child, ref minDepth, result, bestResult, category);
                }
            }
        }

        private bool IsBestChoice(QuadTreeNode node, PropsCategory category)
        {
            //If the object is a sofa, a fridge, a bed or a shelf, we want a wall node
            if (category == PropsCategory.Sofa || category == PropsCategory.Fridge || category == PropsCategory.Bed)
            {
                return node.isWallNode;
            }
            return false;
        }

        

        private void Subdivide()
        {
            
            if (_children != null)
                return;
            _children = new QuadTreeNode[4];
            Vector3 size = bounds.size / 2f;
            //Conserve same height
            size.y = bounds.size.y;
            Vector3 center = bounds.center;

            _children[0] = new QuadTreeNode(new Bounds(center + new Vector3(-size.x / 2, 0, -size.z / 2), size), _depth + 1, max_depth);
            _children[1] = new QuadTreeNode(new Bounds(center + new Vector3(size.x / 2, 0, -size.z / 2), size), _depth + 1, max_depth);
            _children[2] = new QuadTreeNode(new Bounds(center + new Vector3(-size.x / 2, 0, size.z / 2), size), _depth + 1, max_depth);
            _children[3] = new QuadTreeNode(new Bounds(center + new Vector3(size.x / 2, 0, size.z / 2), size), _depth + 1, max_depth);
        }
        
        public List<Bounds> GetAllEmptyNodes()
        {
            List<Bounds> result = new List<Bounds>();
            GetAllEmptyNodesRecursive(this, result);
            return result;
        }
        
        private void GetAllEmptyNodesRecursive(QuadTreeNode node, List<Bounds> result)
        {
            if (node._objects.Count == 0)
            {
                result.Add(node.bounds);
            }
            else if (node._children != null)
            {
                foreach (var child in node._children)
                {
                    GetAllEmptyNodesRecursive(child, result);
                }
            }
        }

        public void DrawGizmo()
        {
            Gizmos.color = _objects.Count == 0 ? Color.green : Color.magenta;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
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
