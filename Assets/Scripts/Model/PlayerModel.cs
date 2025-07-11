using TowerDefense.UI.Binding;

namespace TowerDefense.Model
{
    public class PlayerModel
    {
        private uint _data;//write to storage?

        // JIT Compile-time constants for mask & shift (all inlined)
        private const int GoldShift = 0;
        private const uint GoldMask = (1u << 19) - 1; // 19 bits

        private const int LevelShift = 19;
        private const uint LevelMask = (1u << 9) - 1; // 9 bits

        private const int HealthShift = 28;
        private const uint HealthMask = (1u << 4) - 1; // 4 bits
        
        public int GoldData
        {
            get => (int)((_data >> GoldShift) & GoldMask);
            set => _data = (_data & ~(GoldMask << GoldShift)) | ((uint)value & GoldMask) << GoldShift;
        }

        public int LevelData
        {
            get => (int)((_data >> LevelShift) & LevelMask);
            set => _data = (_data & ~(LevelMask << LevelShift)) | ((uint)value & LevelMask) << LevelShift;
        }

        public int DefaultGold;
        public int DefaulLevel;
        public int DefaultHealth;

        public int HealthData
        {
            get => (int)((_data >> HealthShift) & HealthMask);
            set => _data = (_data & ~(HealthMask << HealthShift)) | ((uint)value & HealthMask) << HealthShift;
        }

        public Bindable<int> Health { get; private set; }
        public Bindable<int> Level { get; private set; }
        public Bindable<int> Gold { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gold">MAX = 524_287</param>
        /// <param name="level">MAX = 511</param>
        /// <param name="health">MAX = 15</param>
        public PlayerModel(int gold, int level, int health)
        {
            _data = 0;
            DefaulLevel = level;
            DefaultGold = gold;
            DefaultHealth = health;
            GoldData = gold;
            LevelData = level;
            HealthData = health;
            Gold = new Bindable<int>(GoldData);
            Level = new Bindable<int>(LevelData);
            Health = new Bindable<int>(HealthData);
        }
    }
}