using System.Collections.Generic;
using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Controller
{
    public class TowerPrefabGenerator : MonoBehaviour
    {
        [SerializeField] private TowerController _prefab;
        [SerializeField] private GameObject _cube;
        [SerializeField] private float _cubeSize = .7f;
        public List<TowerController> Towers;

        public void GenerateTowerPrefabs(List<TowerModel> models)
        {
            Towers = new List<TowerController>();
            float s = _cubeSize;

            foreach (TowerModel model in models)
            {
                GameObject towerGO = Instantiate(_prefab.gameObject, transform);
                towerGO.name = $"Turm_{model.Name}";
                TowerController towerController = towerGO.GetComponent<TowerController>();
                towerController.Initialize(model);
                towerGO.SetActive(false);

                int rows = model.Rows;
                int cols = model.Cols;

                // Precompute shifts
                float halfShiftX = (cols - 1) * s * 0.5f;
                float halfShiftZ = (rows - 1) * s * 0.5f;
                float centerOffsetX = cols % 2 == 0 ? 0f : cols == 1 ? -0.5f : 0.5f;
                float centerOffsetZ = rows % 2 == 0 ? 0f : rows == 1 ? -0.5f : 0.5f;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (model.GetCell(r, c) == 0)
                            continue;

                        float x = c * s - halfShiftX + centerOffsetX;
                        float z = r * s - halfShiftZ + centerOffsetZ;

                        GameObject cube = Instantiate(_cube, towerGO.transform);
                        cube.transform.localPosition = new Vector3(x, 0f, z);
                        cube.transform.localScale = Vector3.one * s;
                        cube.name = $"Cube_{r}_{c}";
                        cube.SetActive(true);
                    }
                }

                Towers.Add(towerController);
            }
        }
    }
}