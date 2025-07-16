using System.Collections.Generic;
using System.Linq;
using TowerDefense.Core;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Network
{
    /// <summary>
    ///     maybe mockup api call simulation?
    /// </summary>
    public class Api
    {
        
        public List<TowerModel> GetBuildingTypes()
        {
            var textAsset = Resources.Load<TextAsset>("data");
            if (textAsset == null)
            {
                LoggerExtra.LogError("Could not load Resources/data.json");
                return new List<TowerModel>();
            }
            var wrapper = JsonUtility.FromJson<TowerModelList>(textAsset.text);
            if (wrapper?.Templates == null) return new List<TowerModel>();
            return new List<TowerModel>(wrapper.Templates);
            // TowerModelList wrapper = JsonUtility.FromJson<TowerModelList>(EmbeddedBuildingData.TowerJson);
            // return new List<TowerModel>(wrapper.Templates);
        }
    

        public List<EnemyModel> GetEnemyTypes()
        {
            EnemyModelList wrapper = JsonUtility.FromJson<EnemyModelList>(EmbeddedBuildingData.EnemyJson);
            return new List<EnemyModel>(wrapper.Enemies);
        }
    }
}