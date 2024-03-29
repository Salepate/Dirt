﻿using Dirt.Game.Metrics;

namespace Dirt.Simulation.Model
{
    [System.Serializable]
    public struct SystemMetric : MetricObject
    {
        public string Name;
        public float AveragedCost;
        public float AveragedMin;
        public float AveragedMax;
    }
}
