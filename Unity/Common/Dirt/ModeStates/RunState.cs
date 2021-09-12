using Framework;

namespace Dirt.States
{
    public class RunState : State<DirtMode>
    {
        public override void OnEnter()
        {
            controller.OnModeReady();
        }

        public override void Update()
        {
            controller.UpdateSystems.ForEach(sys => sys.Update());
        }

        public override void LateUpdate()
        {
            controller.LateUpdateSystems.ForEach(sys => sys.LateUpdate());
        }

        public override void FixedUpdate()
        {
            controller.FixedUpdateSystems.ForEach(sys => sys.FixedUpdate());
        }
    }
}