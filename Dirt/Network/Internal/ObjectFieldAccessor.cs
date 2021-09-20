using Dirt.Simulation;
using Dirt.Simulation.Actor;
using System;

namespace Dirt.Network.Internal
{
    internal class ObjectFieldAccessor
    {
        public string Name;
        public Func<IComponent, object> Getter;
        public Action<GenericArray, int, object> Setter;
    }
}
