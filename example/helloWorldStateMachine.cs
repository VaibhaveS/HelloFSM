using System;
using FSM.Attributes;

public class HelloWorldStateMachine : FiniteStateMachine
{

    // Define the properties of the FSM
    [Property()]
    public string Name { get; set; }

    [Property()]
    public int Age { get; set; }

    // Define the states of the FSM
    private enum State
    {
        Start,
        Greet,
        Ignore,

        Finish
    }

    // Constructor to initialize the FSM
    public HelloWorldStateMachine(string Name, int Age)
    {
        this.Name = Name;
        this.Age = Age;
    }

    // Define the state transition functions
    [Action(State.Start)]
    [Target(State.Greet, State.Ignore)]
    public void Start(Outcome outcome)
    {
        Console.WriteLine("I am in start function!");

        Random random = new Random();

        if (random.Next(0, 100) % 2 == 0)
        {
            outcome.TargetState = (int)State.Greet;
        }
        else
        {
            outcome.TargetState = (int)State.Ignore;
        }
    }
}
