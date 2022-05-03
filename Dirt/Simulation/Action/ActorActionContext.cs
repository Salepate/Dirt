using Dirt.Log;
using Dirt.Simulation.Context;
using Dirt.Simulation.Model;
using Dirt.Simulation.Utility;
using System.Collections.Generic;

namespace Dirt.Simulation.Action
{
    [System.Serializable]
    public class ActorActionContext : IContextItem
    {
        /// <summary>
        /// Declare in context to list available actions
        /// </summary>
        public string[] AvailableActions;

        private ActorAction[] m_Actions;

        public ActorActionContext()
        {
            AvailableActions = new string[0];
            m_Actions = new ActorAction[0];
        }

        public void CreateActionMap(AssemblyCollection coll)
        {
            Dictionary<string, System.Type> typeMap = AssemblyReflection.BuildTypeMap<ActorAction>(coll.Assemblies);

            m_Actions = new ActorAction[AvailableActions.Length];
            for (int i = 0; i < AvailableActions.Length; ++i)
            {
                if (!typeMap.TryGetValue(AvailableActions[i], out System.Type actionType))
                {
                    Console.Warning($"Action {AvailableActions[i]} was not found");
                    continue;
                }

                ActorAction action = (ActorAction)System.Activator.CreateInstance(actionType);
                m_Actions[i] = action;
            }
        }

        public bool TryGetAction(string actionName, out ActorAction actorAction)
        {
            actorAction = null;
            for (int i = 0; i < AvailableActions.Length; ++i)
            {
                if (AvailableActions[i].CompareTo(actionName) == 0)
                {
                    actorAction = m_Actions[i];
                    return true;
                }
            }
            return false;
        }

        public bool TryGetAction(int actionIndex, out ActorAction actorAction)
        {
            actorAction = null;
            if (actionIndex >= 0 && actionIndex < m_Actions.Length)
            {
                actorAction = m_Actions[actionIndex];
                return true;
            }
            return false;
        }
    }
}
