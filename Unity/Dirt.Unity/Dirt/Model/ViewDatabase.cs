using UnityEngine;

namespace Dirt.Model
{
    [CreateAssetMenu(fileName = "gameviews", menuName = "Dirt/Simulation View Database")]
    public class ViewDatabase : DirtSystemContent
    {
        public ViewDefinition[] Definitions;
    }
}