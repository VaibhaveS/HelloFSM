using System.Reflection;
using FSM.Attributes;

public class FiniteStateMachineMetaData
{
    public Dictionary<int, MethodInfo> transitions;

    public FiniteStateMachineMetaData(Type stateMachineType)
    {
        Dictionary<int, MethodInfo> map = new Dictionary<int, MethodInfo>();

        foreach(MethodInfo methodInfo in stateMachineType.GetMethods()) 
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
                map[Convert.ToInt32(state)] = methodInfo;
            }
        }

        this.transitions = map;
    }
}
