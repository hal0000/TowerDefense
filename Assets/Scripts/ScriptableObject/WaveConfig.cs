using System.Collections.Generic;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.ScriptableObject
{
    [CreateAssetMenu(menuName = "TowerDefense/WaveConfig")]
    public class WaveConfig : UnityEngine.ScriptableObject
    {
        public List<WaveModel> Waves;
    }
}