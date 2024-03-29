﻿namespace Dirt.Simulation.Model
{
    [System.Serializable]
    public class SimulationArchetype
    {
        public string[] Packs;
        public string Context;
        public int MaximumActors;
        public int MaximumQueries;
        /// <summary>
        /// if true, only components specified in LimitedComponents will be pooled and allowed.
        /// </summary>
        public bool LimitComponents;
        public string[] LimitedComponents;
    }
}