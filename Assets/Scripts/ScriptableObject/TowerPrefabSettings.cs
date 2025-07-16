using UnityEngine;

namespace TowerDefense.ScriptableObject
{
    [CreateAssetMenu(fileName = "TowerPrefabReference", menuName = "TowerDefense/TowerPrefabReference")]
    public class TowerPrefabSettings : UnityEngine.ScriptableObject
    {
        public GameObject CubePrefab;
    }
}