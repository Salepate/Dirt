using Dirt.Simulation;
using System;

namespace Dirt.Network.Internal
{
    internal class ObjectFieldAccessor
    {
        public string Name;
        public Action<IComponent, object> Setter;
        public Func<IComponent, object> Getter;
    }
}
