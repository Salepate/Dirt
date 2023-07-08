namespace Dirt.Simulation.Actor
{

    public struct ActorTuple<C1> 
        where C1 : struct
    {
        public GameActor Actor { get; private set; }

        private ComponentArray<C1> m_C1Array;
        private int m_C1Index;
        public ref C1 Get() => ref m_C1Array.Components[m_C1Index];
        public ActorTuple(GameActor actor)
        {
            Actor = actor;
            m_C1Array = null;
            m_C1Index = -1;
        }

        public void SetC1(ComponentArray<C1> comps, int idx)
        {
            m_C1Array = comps;
            m_C1Index = idx;
        }
    }

    public struct ActorTuple<C1, C2>
    where C1 : struct
    where C2 : struct
    {
        public GameActor Actor { get; private set; }

        private ComponentArray<C1> m_C1Array;
        private int m_C1Index;

        private ComponentArray<C2> m_C2Array;
        private int m_C2Index;
        public ref C1 GetC1() => ref m_C1Array.Components[m_C1Index];
        public ref C2 GetC2() => ref m_C2Array.Components[m_C2Index];
        public ActorTuple(GameActor actor)
        {
            Actor = actor;
            m_C1Array = null;
            m_C1Index = -1;
            m_C2Array = null;
            m_C2Index = -1;
        }

        public void SetC1(ComponentArray<C1> comps, int idx)
        {
            m_C1Array = comps;
            m_C1Index = idx;
        }

        public void SetC2(ComponentArray<C2> comps, int idx)
        {
            m_C2Array = comps;
            m_C2Index = idx;
        }
    }

    public struct ActorTuple<C1, C2, C3>
      where C1 : struct
      where C2 : struct
      where C3 : struct
    {
        public GameActor Actor { get; private set; }

        private ComponentArray<C1> m_C1Array;
        private int m_C1Index;

        private ComponentArray<C2> m_C2Array;
        private int m_C2Index;

        private ComponentArray<C3> m_C3Array;
        private int m_C3Index;
        public ref C1 GetC1() => ref m_C1Array.Components[m_C1Index];
        public ref C2 GetC2() => ref m_C2Array.Components[m_C2Index];
        public ref C3 GetC3() => ref m_C3Array.Components[m_C3Index];
        public ActorTuple(GameActor actor)
        {
            Actor = actor;
            m_C1Array = null;
            m_C1Index = -1;
            m_C2Array = null;
            m_C2Index = -1;
            m_C3Array = null;
            m_C3Index = -1;
        }

        public void SetC1(ComponentArray<C1> comps, int idx)
        {
            m_C1Array = comps;
            m_C1Index = idx;
        }

        public void SetC2(ComponentArray<C2> comps, int idx)
        {
            m_C2Array = comps;
            m_C2Index = idx;
        }

        public void SetC3(ComponentArray<C3> comps, int idx)
        {
            m_C3Array = comps;
            m_C3Index = idx;
        }
    }

}
