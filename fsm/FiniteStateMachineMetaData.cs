using System.Reflection;
using FSM.Attributes;

public class FiniteStateMachineMetaData
{
    public Dictionary<Enum, List<MethodInfo>> map;

    public FiniteStateMachineMetaData(Type stateMachineType)
    {
        Dictionary<Enum, List<MethodInfo>> map = new Dictionary<Enum, List<MethodInfo>>();

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
                map[state].Add(methodInfo);
            }
        }

        this.map = map;
    }
}
