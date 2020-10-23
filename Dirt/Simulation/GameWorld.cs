using System.Collections.Generic;

namespace Dirt.Simulation
{
    [System.Serializable]
    public class GameWorld
    {
        public List<GameActor> Actors;

        public GameWorld()
        {
            Actors = new List<GameActor>();
        }
    }
}
