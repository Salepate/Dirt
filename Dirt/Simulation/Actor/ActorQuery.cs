namespace Dirt.Simulation.Actor
{
    internal class ActorQuery
    {
        public int[] Indices;
        public int Count { get; private set; }
        public ActorQuery(int items)
        {
            Indices = new int[items];
            Count = 0;
        }

        public void Reset()
        {
            Count = 0;
        }

        public void Add(int actorIdx)
        {
            Indices[Count++] = actorIdx;
        }
    }
}
