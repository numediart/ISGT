using UnityEngine;

namespace Pro_gen
{
    public class Props:MonoBehaviour
    {
        [SerializeField] private GameObject _propsPrefab;
        [SerializeField] private PropsCategory _propsCategory;
        [SerializeField] private BoxCollider _boxCollider;
        public PropsCategory PropsCategory => _propsCategory;
        
        public Bounds CalculateBounds()
        {
            Physics.SyncTransforms(); // Force collider update 
            Bounds combinedBounds = new Bounds(transform.position, Vector3.zero);

            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                combinedBounds.Encapsulate(collider.bounds);
            }
            
            return combinedBounds;
        }
        public BoxCollider GetBoxCollider()
        {
            return _boxCollider;
        }
    }
}