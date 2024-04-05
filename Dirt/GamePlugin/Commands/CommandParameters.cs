using System.Collections.Generic;

namespace Dirt.GameServer.GameCommand
{
    public class CommandParameters
    {
        private Queue<string> m_Parameters;

        public CommandParameters()
        {
            m_Parameters = new Queue<string>();
        }

        public void Push(string v)
        {
            m_Parameters.Enqueue(v);
        }

        public int PopInt()
        {
            int res = 0;
            if ( m_Parameters.Count > 0 )
            {
                int.TryParse(m_Parameters.Dequeue(), out res);
            }
            return res;
        }

        public string PopString()
        {
            string res = string.Empty;
            if (m_Parameters.Count > 0)
            {
                res = m_Parameters.Dequeue();
            }
            return res;
        }
    }
}
