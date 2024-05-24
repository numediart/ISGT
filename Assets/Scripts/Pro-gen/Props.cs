using UnityEngine;

namespace Pro_gen
{
    public class Props:MonoBehaviour
    {
        [SerializeField] private GameObject _propsPrefab;
        [SerializeField] private PropsCategory _propsCategory;
        
        public PropsCategory PropsCategory => _propsCategory;
        
        public GameObject InstantiateProp(Vector3 position, Quaternion rotation, Transform parent)
        {
            return Instantiate(_propsPrefab, position, rotation, parent);
        }
    }
}