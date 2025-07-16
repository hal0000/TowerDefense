using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TowerDefense.Core;
using TowerDefense.Model;
using TowerDefense.ScriptableObject;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Editor
{
    public class TowerConfiguratorWindow : EditorWindow
    {
        private const string ASSETREFERENCEPATH = "Assets/TowerPrefabReference.asset";
        private const string JSONPATH = "Resources/data.json";
        TowerPrefabSettings _settings;
        // Prefab for preview rendering
        private GameObject _cubePrefab;
        private Material _cubeMaterial;

        // Cached mesh and material for preview
        private Mesh _cubeMesh;
        private TowerModel _current;

        // Flags for editor state
        private bool _isEditing;

        // Loaded tower data from JSON
        private TowerModelList _list;
        private TowerModel _original; // for Cancel

        private GameObject _previewInstance;

        // Preview utility and instance
        private PreviewRenderUtility _previewUtil;

        // Dimensions for the footprint grid
        private int _rows = 3, _cols = 3;

        private Vector2 _scroll;
        private int _damage;
        private int _fireRate;

        private int _gold;

        // Form fields for tower properties
        private int _index;
        private int _level;
        private string _name;
        private int _range;

        private void OnEnable()
        {
            LoadJson();
            _settings = AssetDatabase.LoadAssetAtPath<TowerPrefabSettings>(ASSETREFERENCEPATH);
            if (_settings != null)
                _cubePrefab = _settings.CubePrefab;
            // Setup PreviewRenderUtility
            _previewUtil = new PreviewRenderUtility();
            _previewUtil.cameraFieldOfView = 30f;
            _previewUtil.lights[0].intensity = 1f;
            _previewUtil.lights[0].transform.rotation = Quaternion.Euler(50f, 50f, 0);
            _previewUtil.lights[1].intensity = 1f;
            CachePrefab();
        }

        private void OnDisable()
        {
            // Cleanup
            if (_previewUtil != null) _previewUtil.Cleanup();
            if (_previewInstance != null) DestroyImmediate(_previewInstance);
        }

        private void OnGUI()
        {
            // Top Bar
            EditorGUILayout.BeginHorizontal();
            bool shouldClose = GUILayout.Button("CLOSE TOWER CONFIG EDITOR", GUILayout.Width(240));
            GUILayout.FlexibleSpace();
            if (_isEditing)
            {
                if (GUILayout.Button("Save", GUILayout.Width(120))) SaveJson();
                if (GUILayout.Button("Delete", GUILayout.Width(120))) DeleteTower();
                if (GUILayout.Button("Cancel Edit", GUILayout.Width(120))) CancelEdit();
            }

            EditorGUILayout.EndHorizontal();
            if (shouldClose)
            {
                Close();
                return;
            }

            EditorGUILayout.LabelField("Tower Configurator", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            // Prefab selector for preview
            GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField("Preview Cube Prefab", _cubePrefab, typeof(GameObject), false);
            if (newPrefab != _cubePrefab)
            {
                _cubePrefab = newPrefab;
                CachePrefab();
            }

            // Form: Show only basic fields when not in edit mode
            EditorGUI.BeginDisabledGroup(!_isEditing);
            _index = EditorGUILayout.IntField("Index", _index);
            _name = EditorGUILayout.TextField("Name", _name);
            _gold = EditorGUILayout.IntField("Gold", _gold);
            _range = EditorGUILayout.IntField("Range", _range);
            _damage = EditorGUILayout.IntField("Damage", _damage);
            _level = EditorGUILayout.IntField("Level", _level);
            _fireRate = EditorGUILayout.IntField("FireRate", _fireRate);
            EditorGUILayout.Space();

            // Footprint Grid: Only drawn when in edit mode
            if (_isEditing)
            {
                EditorGUILayout.LabelField("Footprint Grid");
                EditorGUILayout.BeginHorizontal();
                int newRows = EditorGUILayout.IntField("Rows", _rows);
                int newCols = EditorGUILayout.IntField("Cols", _cols);
                if (GUILayout.Button("Resize", GUILayout.Width(60)))
                {
                    newRows = Mathf.Max(1, newRows);
                    newCols = Mathf.Max(1, newCols);
                    int[] old = _current.FootprintPacked ?? Array.Empty<int>();
                    int[] next = new int[newRows + 1];
                    next[0] = CoordPacker.Pack(newRows, newCols);
                    for (int r = 0; r < newRows; r++)
                    {
                        next[1 + r] = r + 1 < old.Length ? old[1 + r] : 0;
                    }

                    _rows = newRows;
                    _cols = newCols;
                    _current.FootprintPacked = next;
                }

                EditorGUILayout.EndHorizontal();

                // Toggle buttons for grid cells
                for (int r = 0; r < _rows; r++)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int c = 0; c < _cols; c++)
                    {
                        bool on = _current.GetCell(r, c) == 1;
                        bool newVal = GUILayout.Toggle(on, GUIContent.none, GUILayout.Width(24), GUILayout.Height(24));
                        if (newVal != on)
                            _current.SetCell(r, c, (byte)(newVal ? 1 : 0));
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Existing Towers", EditorStyles.boldLabel);
            int count = _list.Templates.Count;
            for (int i = 0; i < count; i++)
            {
                TowerModel tpl = _list.Templates[i];
                if (!_isEditing && GUILayout.Button($"{tpl.Index}: {tpl.Name}"))
                    BeginEdit(tpl);
            }

            EditorGUILayout.EndScrollView();

            if (!_isEditing && GUILayout.Button("Add New Tower"))
            {
                TowerModel blank = new TowerModel { Index = GetNextIndex() };
                AppendTemplate(blank);
                BeginEdit(blank);
            }

            // Preview area
            Rect previewRect = GUILayoutUtility.GetRect(256, 256);
            DrawPreview(previewRect);
            Repaint();
        }

        [MenuItem("Window/Tower Configurator")]
        public static void Open()
        {
            GetWindow<TowerConfiguratorWindow>("Tower Configurator").LoadJson();
        }

        private void CachePrefab()
        {
            if (_cubePrefab == null)
            {
                _cubeMesh = null;
                _cubeMaterial = null;
                return;
            }

            MeshFilter mf = _cubePrefab.GetComponentInChildren<MeshFilter>();
            Renderer rend = _cubePrefab.GetComponentInChildren<Renderer>();
            _cubeMesh = mf != null ? mf.sharedMesh : null;
            _cubeMaterial = rend != null ? rend.sharedMaterial : null;
        }

        private void LoadJson()
        {
            string path = Path.Combine(Application.dataPath, JSONPATH);
            if (File.Exists(path))
            {
                string txt = File.ReadAllText(path);
                _list = JsonUtility.FromJson<TowerModelList>(txt);
            }

            _list ??= new TowerModelList { Templates = new List<TowerModel>() };

            // Reset state
            _current = null;
            _original = null;
            _isEditing = false;
        }

        private void BeginEdit(TowerModel tpl)
        {
            // Snapshot for cancel
            _original = JsonUtility.FromJson<TowerModel>(JsonUtility.ToJson(tpl));
            _current = tpl;
            _isEditing = true;

            // Load simple fields
            _index = tpl.Index;
            _name = tpl.Name;
            _gold = tpl.Gold;
            _range = tpl.Range;
            _damage = tpl.Damage;
            _level = tpl.Level;
            _fireRate = tpl.FireRate;

            // Unpack header to rows/cols
            int header = tpl.FootprintPacked != null && tpl.FootprintPacked.Length > 0 ? tpl.FootprintPacked[0] : CoordPacker.Pack(3, 3); // default 3x3
            _rows = CoordPacker.UnpackX(header);
            _cols = CoordPacker.UnpackY(header);

            // Ensure array length = header+rows
            if (tpl.FootprintPacked != null && tpl.FootprintPacked.Length == _rows + 1) return;
            tpl.FootprintPacked = new int[_rows + 1];
            tpl.FootprintPacked[0] = header;
        }

        private void CancelEdit()
        {
            if (_current != null)
            {
                if (_original != null)  LoadTemplate(_original);
                else  _list.Templates.Remove(_current);
            }

            _isEditing = false;
            EndEdit();
            LoadJson();
        }
        private void EndEdit()
        {
            _isEditing = false;
            _current = null;
            _original = null;
            _scroll = Vector2.zero;
            _index = 0;
            _name = "";
            _gold = 0;
            _range = 0;
            _damage = 0;
            _level = 0;
            _fireRate = 0;
        }

        private void SaveJson()
        {
            // 1) Push form values into the model
            _current.Index = _index;
            _current.Name = _name;
            _current.Gold = _gold;
            _current.Range = _range;
            _current.Damage = _damage;
            _current.Level = _level;
            _current.FireRate = _fireRate;

            // 2) Update header in packed footprint
            if (_current.FootprintPacked != null && _current.FootprintPacked.Length > 0) _current.FootprintPacked[0] = CoordPacker.Pack(_rows, _cols);
            // 3) Write & refresh
            string path = Path.Combine(Application.dataPath, JSONPATH);
            string json = JsonUtility.ToJson(_list, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            LoggerExtra.Log($"[TowerConfigurator] JSON saved with {_list.Templates.Count} entries.");

            // 4) remembered which index
            int savedIndex = _current.Index;

            // 5) Clear the editing state
            EndEdit();
            Repaint();

            // 6) Reload entire list and jump back into the saved entry
            LoadJson();
            TowerModel saved = _list.Templates.FirstOrDefault(t => t.Index == savedIndex);
            if (saved != null) BeginEdit(saved);
        }

        private void DeleteTower()
        {
            if (_current == null) return;

            int delIdx = _current.Index;
            _list.Templates.RemoveAll(t => t.Index == delIdx);
            for (int i = 0; i < _list.Templates.Count; i++) if (_list.Templates[i].Index > delIdx) _list.Templates[i].Index--;
            string path = Path.Combine(Application.dataPath, JSONPATH);
            File.WriteAllText(path, JsonUtility.ToJson(_list, prettyPrint: true));
            AssetDatabase.Refresh();
            LoggerExtra.Log($"[TowerConfigurator] Deleted tower {delIdx} and reindexed above entries.");
            _isEditing = false;
            _current   = null;
            EndEdit();
            Repaint();
            LoadJson();
        }

        private int GetNextIndex()
        {
            var list = _list.Templates;
            int count = list.Count;
            if (count == 0) return 0;
            int max = list[0].Index;
            for (int i = 1; i < count; i++)
            {
                int idx = list[i].Index;
                if (idx > max) max = idx;
            }
            return max + 1;
        }

        private void AppendTemplate(TowerModel tpl)
        {
            _list.Templates.Add(tpl);
        }

        private void LoadTemplate(TowerModel tpl)
        {
            BeginEdit(tpl);
        }

        private void DrawPreview(Rect r)
        {
            if (_cubeMesh == null || _cubeMaterial == null || _current == null) return;
            int rows = _current.Rows;
            int cols = _current.Cols;
            float s = 0.7f;

            // Centering calculations
            float halfShiftX = (cols - 1) * s * 0.5f;
            float halfShiftZ = (rows - 1) * s * 0.5f;
            float centerOffsetX = cols % 2 == 0 ? 0f : cols == 1 ? -0.5f : 0.5f;
            float centerOffsetZ = rows % 2 == 0 ? 0f : rows == 1 ? -0.5f : 0.5f;
            _previewUtil.BeginPreview(r, GUIStyle.none);
            for (int rIdx = 0; rIdx < rows; rIdx++)
            for (int cIdx = 0; cIdx < cols; cIdx++)
            {
                if (_current.GetCell(rIdx, cIdx) == 0) continue;
                float x = cIdx * s - halfShiftX + centerOffsetX;
                float z = rIdx * s - halfShiftZ + centerOffsetZ;
                Matrix4x4 trs = Matrix4x4.TRS(new Vector3(x, 0, z), Quaternion.identity, Vector3.one * s);
                _previewUtil.DrawMesh(_cubeMesh, trs, _cubeMaterial, 0);
            }

            Quaternion camRot = Quaternion.Euler(30f, 45f, 0f);
            _previewUtil.camera.transform.rotation = camRot;
            float dist = Mathf.Max(rows, cols) * s * 2.5f;
            _previewUtil.camera.transform.position = camRot * new Vector3(0, 0, -dist);
            _previewUtil.camera.Render();

            GUI.DrawTexture(r, _previewUtil.EndPreview(), ScaleMode.StretchToFill, false);
        }
    }
}