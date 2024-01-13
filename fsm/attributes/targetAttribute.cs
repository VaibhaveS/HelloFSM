using System;
using System.Collections.Generic;

namespace FSM.Attributes
{    
    [AttributeUsage(AttributeTargets.Method)]
    public class TargetAttribute : Attribute
    {
        public List<Enum> States { get; set; }

        public TargetAttribute(params object[] states)
        {
            States = new List<Enum>(states.Cast<Enum>());
        }
    }
}
