﻿namespace Dirt.Simulation.Action
{
    public class ActorActionEvent : SimulationEvent
    {
        private static readonly ActionParameter[] s_EmptyList = new ActionParameter[0];
        public int SourceActor { get; private set; }
        public int ActionIndex { get; private set; }

        public ActionParameter[] Parameters { get; private set; }

        public bool EmptyParameters => Parameters == null || (Parameters == s_EmptyList);

        public ActorActionEvent(int actorID, int idx, ActionParameter[] parameters = null)
        {
            SourceActor = actorID;
            ActionIndex = idx;
            Parameters = parameters ?? s_EmptyList;
        }
    }
}
