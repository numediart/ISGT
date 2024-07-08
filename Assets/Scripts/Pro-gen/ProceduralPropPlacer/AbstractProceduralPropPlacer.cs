using System.Collections;
using System.Collections.Generic;
using Pro_gen.QuadTree;
using UnityEngine;
using Random = System.Random;

namespace Pro_gen
{
    public abstract class AbstractProceduralPropPlacer : MonoBehaviour
    {
        protected RoomsGenerationScriptableObject _roomsGenerationData;
        protected int numberOfProps = 4;
        protected List<Props> _selectedProps;
        protected List<Bounds> _selectedPropsBounds;
        protected QuadTreeNode _quadTree;
        protected readonly int _maxAttempts = 25;
        protected List<Vector3> _propsPositions;
        protected Bounds _groundBounds;

        protected virtual void Awake()
        {
            _selectedProps = new List<Props>();
            _selectedPropsBounds = new List<Bounds>();
            _propsPositions = new List<Vector3>();
            Time.fixedDeltaTime = 0.001f;
        }

        public abstract void Init(RoomsGenerationScriptableObject roomGenerationData);

        public abstract IEnumerator PlaceProps(Random random, int area);

        protected abstract Vector3? PropsPossiblePosition(Random random, Bounds roomBounds, Bounds bounds, Transform propTransform, PropsCategory category);

        protected abstract bool CanPropsBePlaced(Transform propTransform, Bounds bounds);

        protected abstract void RotateProp(Transform propTransform, PropsCategory category);

        protected abstract GameObject findClosestWall(Transform propTransform);

        protected abstract Bounds GetGroundBounds();

        protected abstract float NextFloat(Random random, float min, float max);

        public abstract List<Bounds> GetAllEmptyQuadNodes();

        protected abstract void OnDrawGizmos();
    }
}