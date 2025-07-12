using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Interface
{
    public interface ITower
    {
        public void Bauen(List<int> positions);
        public void Abbrechen();
    }
}