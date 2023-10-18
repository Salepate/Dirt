using Dirt.Game.Math;
using System.Collections.Generic;

namespace Dirt.Simulation.Action
{
    public static class ActionParameterHelper
    {
        public static float getFloat(this ActionExecutionData ctx, int index)
        {
            return ctx.Parameters[index].floatValue;
        }

        public static int getInt(this ActionExecutionData ctx, int index)
        {
            return ctx.Parameters[index].intValue;
        }
        public static float3 getFloat3(this ActionExecutionData ctx, int startIndex)
        {
            return new float3(ctx.Parameters[startIndex].floatValue,
                ctx.Parameters[startIndex + 1].floatValue,
                ctx.Parameters[startIndex + 2].floatValue);
        }

        public static void AddFloat3(this List<ActionParameter> actionParams, in float3 value)
        {
            actionParams.Add(ActionParameter.Create(value.x));
            actionParams.Add(ActionParameter.Create(value.y));
            actionParams.Add(ActionParameter.Create(value.z));
        }
    }
}
