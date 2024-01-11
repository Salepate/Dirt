namespace Dirt.Simulation.Actor.Components
{
    /// <summary>
    /// Tag actors with this to destroy them
    /// </summary>
    [System.Serializable]
    public struct Destroy : IComponent
    {
        public int Reason;
    }
}
