using System.Reflection;
using FSM.Attributes;

public class FiniteStateMachineMetaData
{
    public Dictionary<Enum, MethodInfo> transitions;

    public FiniteStateMachineMetaData(Type stateMachineType)
    {
        Dictionary<Enum, MethodInfo> map = new Dictionary<Enum, MethodInfo>();

        foreach(MethodInfo methodInfo in stateMachineType.GetMethods(BindingFlags.Public)) 
        {
            //check if action attribute is present for this method
            ActionAttribute? actionAttribute = (ActionAttribute?) methodInfo.GetCustomAttribute(typeof(ActionAttribute));
            if (actionAttribute == null) {
                continue;
            }

            //this is a transition method store it
            foreach (Enum state in actionAttribute.States)
            {
                //store the method info for each action state
                map[state] = methodInfo;
            }
        }

        this.transitions = map;
    }
}
