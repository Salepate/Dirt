namespace Dirt.Simulation.Actor
{
    public class ActorTuple<C1> : System.Tuple<GameActor, C1> 
        where C1 : IComponent
    {
        public GameActor Actor => Item1;
        public int ActorID => Item1.ID;
        public C1 Comp1 => Item2;

        public ActorTuple(GameActor actor, C1 comp1) : base(actor, comp1) { }
    }

    public class ActorTuple<C1, C2> : System.Tuple<GameActor, C1, C2>
        where C1 : IComponent
        where C2 : IComponent
    {
        public GameActor Actor => Item1;
        public int ActorID => Item1.ID;
        public C1 Comp1 => Item2;
        public C2 Comp2 => Item3;

        public ActorTuple(GameActor actor, C1 comp1, C2 comp2) : base(actor, comp1, comp2) { }
    }

    public class ActorTuple<C1, C2, C3> : System.Tuple<GameActor, C1, C2, C3>
        where C1 : IComponent
        where C2 : IComponent
        where C3 : IComponent
    {
        public GameActor Actor => Item1;
        public int ActorID => Item1.ID;
        public C1 Comp1 => Item2;
        public C2 Comp2 => Item3;
        public C3 Comp3 => Item4;

        public ActorTuple(GameActor actor, C1 comp1, C2 comp2, C3 comp3) : base(actor, comp1, comp2, comp3) { }
    }
}
