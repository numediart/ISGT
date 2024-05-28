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
        
        const int MAX_DEPTH = 5;

        public QuadTreeNode(Bounds bounds, int depth)
        {
            this.bounds = bounds;
            _objects = new List<Props>();
            _children = null;
            _depth = depth;
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
            if (_depth < MAX_DEPTH)
            {
                Subdivide();
                //Insert the prop in the children nodes
                foreach(var child in _children)
                {
                    child.Insert(prop);
                }
            }
        }
        

        public List<QuadTreeNode> FindBiggestEmptyNodes()
        {
            List<QuadTreeNode> result = new List<QuadTreeNode>();
            int minDepth = int.MaxValue;
            FindBiggestEmptyNodesRecursive(this, ref minDepth, result);
            return result;
        }

        private void FindBiggestEmptyNodesRecursive(QuadTreeNode node, ref int minDepth, List<QuadTreeNode> result)
        {
            if (node._objects.Count == 0)
            {
                if (node._depth < minDepth)
                {
                    minDepth = node._depth;
                    result.Clear();
                    result.Add(node);
                }
                else if (node._depth == minDepth)
                {
                    result.Add(node);
                }
            }
            else if (node._children != null)
            {
                foreach (var child in node._children)
                {
                    FindBiggestEmptyNodesRecursive(child, ref minDepth, result);
                }
            }
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

            _children[0] = new QuadTreeNode(new Bounds(center + new Vector3(-size.x / 2, 0, -size.z / 2), size), _depth + 1);
            _children[1] = new QuadTreeNode(new Bounds(center + new Vector3(size.x / 2, 0, -size.z / 2), size), _depth + 1);
            _children[2] = new QuadTreeNode(new Bounds(center + new Vector3(-size.x / 2, 0, size.z / 2), size), _depth + 1);
            _children[3] = new QuadTreeNode(new Bounds(center + new Vector3(size.x / 2, 0, size.z / 2), size), _depth + 1);
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
