using Dirt.Simulation;
using Dirt.Simulation.Action;

namespace Dirt.Network.Simulation.Events
{
    public class RemoteActionRequestEvent : SimulationEvent
    {
        public ActionParameter[] Parameters { get; private set; }
        public int SourceActor { get; private set; }
        public int ActionIndex { get; private set; }
        public RemoteActionRequestEvent(int actorID, int idx, params object[] parameters)
        {
            SourceActor = actorID;
            ActionIndex = idx;

            if (parameters.Length > 0)
            {
                ActionParameter[] convertedParameters = new ActionParameter[parameters.Length];
                for (int i = 0; i < convertedParameters.Length; ++i)
                {
                    System.Type objType = parameters[i].GetType();
                    if (objType == typeof(int))
                    {
                        convertedParameters[i] = new ActionParameter() { intValue = (int)parameters[i] };
                    }
                    else if (objType == typeof(float))
                    {
                        convertedParameters[i] = new ActionParameter() { floatValue = (float)parameters[i] };
                    }
                }
                Parameters = convertedParameters;
            }
            else
            {
                Parameters =  ActorActionEvent.s_EmptyList;
            }
        }
    }
}
