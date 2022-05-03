using Dirt.Log;
using Dirt.Simulation.Action;
using Dirt.Simulation.Context;
using Dirt.Simulation.Model;
using Dirt.Simulation.SystemHelper;

namespace Dirt.Simulation.Systems
{
    public class ActorActionInterpreter : ISimulationSystem, IContextReader, IEventReader
    {
        private ActorActionContext m_Context;
        private GameSimulation m_Simulation;
        public void Initialize(GameSimulation sim)
        {
            m_Simulation = sim;
        }

        public void SetContext(SimulationContext context)
        {
            m_Context = context.GetContext<ActorActionContext>();
            if (m_Context == null)
            {
                Console.Warning($"Actor Action context is missing");
            }
            else
            {
                m_Context.CreateActionMap(context.GetContext<AssemblyCollection>());
            }
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

            ActorAction.ExecutionContext actionCtx = new ActorAction.ExecutionContext()
            {
                SourceActor = actor
            };

            if (!m_Context.TryGetAction(actionEvent.ActionIndex, out ActorAction action))
            {
                Console.Warning($"Invalid Action {actionEvent.ActionIndex}");
            }

            Console.Message($"Perform action {action.GetType().Name}");
            action.PerformAction(m_Simulation, actionCtx);
        }
    }
}
