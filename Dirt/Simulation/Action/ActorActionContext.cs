using Dirt.Game;
using Dirt.Game.Content;
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

        private AssemblyCollection m_Assemblies;
        private ActorAction[] m_Actions;

        public ActorActionContext()
        {
            AvailableActions = new string[0];
            m_Actions = new ActorAction[0];
        }

        public void CreateActionMap(GameSimulation sim, SimulationContext simContext, IManagerProvider managers, IContentProvider content)
        {
            Dictionary<string, System.Type> typeMap = AssemblyReflection.BuildTypeMap<ActorAction>(m_Assemblies.Assemblies);

            m_Actions = new ActorAction[AvailableActions.Length];
            for (int i = 0; i < AvailableActions.Length; ++i)
            {
                if (!typeMap.TryGetValue(AvailableActions[i], out System.Type actionType))
                {
                    Console.Warning($"Action {AvailableActions[i]} was not found");
                    continue;
                }

                ActorAction action = (ActorAction)System.Activator.CreateInstance(actionType);
                action.SetIndex(i);
                action.Initialize(sim, simContext, managers, content);
                m_Actions[i] = action;
            }
        }

        internal void SetAssemblies(AssemblyCollection assemblyCollection)
        {
            m_Assemblies = assemblyCollection;
        }

        public int GetActionIndex<T>() where T : ActorAction
        {
            for (int i = 0; i < m_Actions.Length; ++i)
            {
                if (m_Actions[i] is T)
                {
                    return i;
                }
            }
            return -1;
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
