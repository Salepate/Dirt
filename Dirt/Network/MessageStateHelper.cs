using System.Collections.Generic;

namespace Dirt.Network
{
    public static class MessageStateHelper
    {
        public static void MergeState(MessageHeader sourceState, MessageHeader destinationState)
        {
            List<int> fields = new List<int>(destinationState.FieldIndex);
            List<object> values = new List<object>(destinationState.FieldValue);

            for (int i = 0; i < sourceState.FieldIndex.Length; ++i)
            {
                int idx = fields.FindIndex(fieldIndex => fieldIndex == sourceState.FieldIndex[i]);

                if (idx == -1)
                {
                    fields.Add(sourceState.FieldIndex[i]);
                    values.Add(sourceState.FieldValue[i]);
                }
                else
                {
                    values[idx] = sourceState.FieldValue[i];
                }
            }

            destinationState.FieldIndex = fields.ToArray();
            destinationState.FieldValue = values.ToArray();
        }
    }
}
