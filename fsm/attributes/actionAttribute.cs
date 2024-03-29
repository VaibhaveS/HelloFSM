namespace FSM.Attributes
{ 
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute : Attribute
    {
        public List<Enum> States { get; set; }

        public ActionAttribute(params object[] states)
        {
            States = new List<Enum>(states.Cast<Enum>());
        }
    }
}
