
using Dirt.Log;
using System.Collections.Generic;

namespace Dirt.Simulation.Action
{
    public static partial class ActionHelper
    {
        public static void RequestAction(this GameSimulation simulation, GameActor sourceActor, int actionIndex)
        {
            simulation.Events.Enqueue(new ActorActionEvent(sourceActor.ID, actionIndex));
        }
        public static void RequestAction(this GameSimulation simulation, GameActor sourceActor, int actionIndex, object[] parameters)
        {
            simulation.Events.Enqueue(new ActorActionEvent(sourceActor.ID, actionIndex, parameters));
        }

        public static void ConvertParameters(IList<ActionParameter> to, params object[] from)
        {
            if ( to.Count != from.Length )
            {
                Console.Error($"Array mismatch in ActionHelper.ConvertParameters: {to.Count} != {from.Length}");
                return;
            }

            if (from.Length > 0)
            {
                for (int i = 0; i < to.Count; ++i)
                {
                    System.Type objType = from[i].GetType();
                    if (objType == typeof(int))
                    {
                        to[i] = new ActionParameter() { intValue = (int)from[i] };
                    }
                    else if (objType == typeof(float))
                    {
                        to[i] = new ActionParameter() { floatValue = (float)from[i] };
                    }
                }
            }
        }
    }
}