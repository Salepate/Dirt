﻿using Dirt.Game;
using Dirt.Game.Content;
using Dirt.Log;
using Dirt.Simulation.Action;
using Dirt.Simulation.Context;
using Dirt.Simulation.Model;
using Dirt.Simulation.SystemHelper;
using System.Collections.Generic;

namespace Dirt.Simulation.Systems
{
    public class ActorActionInterpreter : ISimulationSystem, IContextReader, IEventReader, IManagerAccess
    {
        private GameSimulation m_Simulation;
        private SimulationContext m_SimulationContext;
        private ActorActionContext m_ActionContext;
        private List<ActionParameter> m_ParamsBuffer;
        private IContentProvider m_Provider;
        private IManagerProvider m_Managers;
        public ActorActionContext ActionContext => m_ActionContext;
        public void Initialize(GameSimulation sim)
        {
            m_Simulation = sim;
            m_ParamsBuffer = new List<ActionParameter>(20);

            if (m_ActionContext == null)
            {
                Console.Warning($"Actor Action context is missing");
            }
            else
            {
                m_ActionContext.CreateActionMap(sim, m_SimulationContext, m_Managers, m_Provider);
            }
        }

        public void SetContext(SimulationContext context)
        {
            m_SimulationContext = context;
            m_ActionContext = context.GetContext<ActorActionContext>();
            m_ActionContext.SetAssemblies(context.GetContext<AssemblyCollection>());
        }

        public void SetManagers(IManagerProvider provider)
        {
            m_Managers = provider;
            m_Provider = provider.GetManager<IContentProvider>();
        }

        public void UpdateActors(GameSimulation sim, float deltaTime)
        {
        }

        [SimulationListener(typeof(ActorActionEvent), 0)]
        private void OnActionRequest(ActorActionEvent actionEvent)
        {
            GameActor actor = m_Simulation.Builder.GetActorByID(actionEvent.SourceActor);
            if (actor == null)
            {
                Console.Warning($"Actor {actionEvent.SourceActor} was not found");
                return;
            }

            if (!m_ActionContext.TryGetAction(actionEvent.ActionIndex, out ActorAction action))
            {
                Console.Warning($"Invalid Action {actionEvent.ActionIndex}");
            }

            //Console.Message($"Perform action {action.GetType().Name}");

            ActionParameter[] actionParams = actionEvent.Parameters;

            if (actionEvent.EmptyParameters)
            {
                m_ParamsBuffer.Clear();
                action.FetchGameData(m_Simulation, m_SimulationContext, actor, m_ParamsBuffer);
                actionParams = m_ParamsBuffer.ToArray();
            }


            ActionExecutionData execData = new ActionExecutionData()
            {
                SourceActor = actor,
                Parameters = actionParams
            };
            action.PerformAction(m_Simulation, m_SimulationContext, execData);
        }
    }
}
