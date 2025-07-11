using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class TowerPrefabGenerator : MonoBehaviourExtra
    {
        [SerializeField] private TowerController _prefab;
        [SerializeField] private GameObject _cube;
        [SerializeField] private float _cubeSize = .7f;
        public List<TowerController> Towers;

        protected override void Tick() { /* no runtime logic */ }

        public void GenerateTowerPrefabs(List<TowerModel> models)
        {
            Towers = new List<TowerController>();
            float s = _cubeSize;

            foreach (var model in models)
            {
                var towerGO = Instantiate(_prefab.gameObject, transform);
                towerGO.name = $"Turm_{model.Name}";
                var towerController = towerGO.GetComponent<TowerController>();
                towerController.Initialize(model);
                towerGO.SetActive(false);

                int rows = model.Rows;
                int cols = model.Cols;

                // Precompute shifts
                float halfShiftX = (cols - 1) * s * 0.5f;
                float halfShiftZ = (rows - 1) * s * 0.5f;
                float centerOffsetX = (cols % 2 != 0) ? 0.5f : 0f;
                float centerOffsetZ = (rows % 2 != 0) ? 0.5f : 0f;

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (model.GetCell(r, c) == 0) 
                            continue;

                        float x = c * s - halfShiftX + centerOffsetX;
                        float z = r * s - halfShiftZ + centerOffsetZ;

                        var cube = Instantiate(_cube, towerGO.transform);
                        cube.transform.localPosition = new Vector3(x, 0f, z);
                        cube.transform.localScale = Vector3.one * s;
                        cube.name = $"Cube_{r}_{c}";
                    }
                }

                Towers.Add(towerController);
            }
        }
    }
}