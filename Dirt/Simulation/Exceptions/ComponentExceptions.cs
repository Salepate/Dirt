namespace Dirt.Simulation.Exceptions
{
    using Type = System.Type;

    public class ComponentException : System.Exception
    {
        public Type Component { get; private set; }
        public ComponentException(Type component)
        {
            Component = component;
        }
    }
    public class ComponentLimitException : ComponentException
    {
        public int Limit { get; private set; }
        public ComponentLimitException(Type component, int limit) : base(component)
        {
            Limit = limit;
        }
    }

    public class ComponentNotFoundException : ComponentException 
    {
        public ComponentNotFoundException(Type component) : base(component) { }
    }

    public class QueryLimitException : System.Exception
    {
        public int MaxQueries;

    }
}
