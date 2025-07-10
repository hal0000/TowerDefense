using System.Collections.Generic;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Network
{
    /// <summary>
    /// maybe mockup api call simulation?
    /// </summary>
    public class Api
    {
        public List<TowerModel> GetBuildingTypes()
        {
            var wrapper = JsonUtility.FromJson<TowerModelList>(EmbeddedBuildingData.TowerJson);
            return new List<TowerModel>(wrapper.Templates);
        }
        public List<EnemyModel> GetEnemyTypes()
        {
            var wrapper = JsonUtility.FromJson<EnemyModelList>(EmbeddedBuildingData.EnemyJson);
            return new List<EnemyModel>(wrapper.Enemies);
        }
    }
}