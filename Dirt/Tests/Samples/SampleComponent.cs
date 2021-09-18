using Dirt.Simulation;

namespace Dirt.Tests.Samples
{
    public struct SampleComponent : IComponent, IComponentAssign
    {
        public const int DefaultValue = 255;
        public int Value;
        public void Assign()
        {
            Value = DefaultValue;
        }
    }
}
