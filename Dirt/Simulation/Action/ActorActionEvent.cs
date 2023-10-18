using Newtonsoft.Json;

namespace Dirt.Simulation.Action
{
    public class ActorActionEvent : SimulationEvent
    {
        public static readonly ActionParameter[] s_EmptyList = new ActionParameter[0];
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

        public ActorActionEvent(int actorID, int idx, params object[] parameters)
        {
            SourceActor = actorID;
            ActionIndex = idx;

            if ( parameters.Length > 0 )
            {
                ActionParameter[] convertedParameters = new ActionParameter[parameters.Length];
                for(int i = 0; i < convertedParameters.Length; ++i)
                {
                    System.Type objType = parameters[i].GetType();
                    if ( objType == typeof(int) )
                    {
                        convertedParameters[i] = new ActionParameter() { intValue = (int)parameters[i] };
                    }
                    else if (objType == typeof(float) )
                    {
                        convertedParameters[i] = new ActionParameter() { floatValue = (float)parameters[i] };
                    }
                }
            }
            else
            {
                Parameters = s_EmptyList;
            }
        }
    }
}
