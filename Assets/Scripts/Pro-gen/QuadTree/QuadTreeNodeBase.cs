using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pro_gen
{
    public abstract class QuadTreeNodeBase
    {
        public Bounds Bounds;
        protected List<Props> _objects;
        protected QuadTreeNodeBase[] _children;
        protected int _depth;
        protected bool isWallNode;
        protected int max_depth;

        public QuadTreeNodeBase(Bounds bounds, int depth, int maxDepth = 5)
        {
            this.Bounds = bounds;
            _objects = new List<Props>();
            _children = null;
            _depth = depth;
            max_depth = maxDepth;

            // Check if the node contains a wall
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

        public abstract void Insert(Props prop);

        public abstract List<QuadTreeNodeBase> FindBiggestEmptyNodes(PropsCategory category);

        public abstract List<Bounds> GetAllEmptyNodes();

        public abstract void DrawGizmo();

        protected abstract void Subdivide();

        public void DetermineMaxDepth(int area)
        {
            max_depth = 5 + (int)(4 * (Mathf.Sqrt(area) / 40f));
        }
    }
}