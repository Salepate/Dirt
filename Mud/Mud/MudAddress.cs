using System;
using System.Collections.Generic;
using System.Text;

namespace Mud
{
    public struct MudAddress
    {
        public string IP;
        public int Port;

        public static bool operator ==(MudAddress l, MudAddress r)
        {
            return l.Port == r.Port && l.IP == r.IP;
        }

        public static bool operator !=(MudAddress l, MudAddress r)
        {
            return l.Port != r.Port || l.IP != r.IP;
        }

        public override int GetHashCode()
        {
            return $"{IP},{Port}".GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}
