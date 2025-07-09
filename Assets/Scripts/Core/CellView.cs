using TowerDefense.Model;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Handles a cell’s visual state based on its CellModel,
    /// having the per‐instance color in an instanced shader. my personal preference for not breaking the batch
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class CellView : MonoBehaviour
    {
        public CellModel Model { get; private set; }

        [Header("Tile Colors")]
        [SerializeField] private Color _oddColor = new Color(60 / 255f, 147 / 255f, 45 / 255f, 1f);
        [SerializeField] private Color _evenColor = new Color(29 / 255f, 103 / 255f, 16 / 255f, 1f);
        [SerializeField] private Color _pathColor = new Color(217 / 255f, 162 / 255f, 0 / 255f, 1f);
        [SerializeField] private Color _occupiedColor = Color.red;
        [SerializeField] private Color _availableColor = Color.green;

        private Color _defaultColor;
        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;

        // must match the instanced shader property name:
        private static readonly int InstanceColorID = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Initialize with a CellModel, set default color by parity or path flag.
        /// </summary>
        public void Initialize(CellModel model)
        {
            Model = model;

            if (Model.IsPath)
                _defaultColor = _pathColor;
            else if (((Model.X + Model.Y) & 1) == 0)
                _defaultColor = _evenColor;
            else
                _defaultColor = _oddColor;

            ApplyColor(_defaultColor);
        }

        /// <summary>
        /// Highlight this cell: green if valid placement, red otherwise.
        /// </summary>
        public void Highlight(bool valid)
        {
            ApplyColor(valid ? _availableColor : _occupiedColor);
        }

        /// <summary>
        /// Restore the cell back to its default state.
        /// </summary>
        public void ResetHighlight()
        {
            ApplyColor(_defaultColor);
        }

        /// <summary>
        /// Applies a tint color via MaterialPropertyBlock to preserve instancing.
        /// </summary>
        private void ApplyColor(Color c)
        {
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(InstanceColorID, c);
            _renderer.SetPropertyBlock(_mpb);
        } 
    }
}