using Mud.Server;
using System.Collections.Generic;

namespace Mud.Server.Stream
{
    public class StreamGroupManager
    {
        public System.Action<StreamGroup, GameClient> ClientJoined;
        public System.Action<StreamGroup, GameClient> ClientLeft;

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
            StreamGroup grp = new StreamGroup(this);
            ActiveGroups.Add(grp);
            return grp;
        }
    }
}