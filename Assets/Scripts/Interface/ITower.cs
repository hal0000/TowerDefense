using System.Collections.Generic;

namespace TowerDefense.Interface
{
    public interface ITower
    {
        public void Bauen();
        public void Abbrechen();
        public void Upgrade();
    }
}