using System.Collections.Generic;

namespace TowerDefense.Interface
{
    public interface ITower
    {
        public void Bauen(List<int> positions);
        public void Abbrechen();
        public void Upgrade();
    }
}