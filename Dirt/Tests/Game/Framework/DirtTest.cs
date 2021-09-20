using Dirt.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dirt.Tests.Framework
{
    public abstract class DirtTest
    {
        public virtual void Initialize()
        {
            Dirt.Log.Console.Logger = new BasicLogger();
        }
    }
}
