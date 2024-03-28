using Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Dirt.DirtSystem;

namespace Dirt.States
{
    public class BootState : State<DirtMode>
    {
        override public void Update()
        {
            fsm.SetState(DirtMode.Load);
        }
    }
}