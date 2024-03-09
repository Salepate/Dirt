using Mud.Server;
using System.Collections.Generic;

namespace Mud.Server.Stream
{
    public class StreamGroupManager
    {
        public List<StreamGroup> ActiveGroups { get; private set; }

        public StreamGroupManager()
        {
            ActiveGroups = new List<StreamGroup>();
        }

        public void DestroyGroup(StreamGroup group)
        {
            ActiveGroups.Remove(group);
        }

        public StreamGroup CreateGroup()
        {
            StreamGroup grp = new StreamGroup();
            ActiveGroups.Add(grp);
            return grp;
        }
    }
}