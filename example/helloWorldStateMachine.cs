using System;
using FSM.Attributes;

[Table("hello_world")]
public class HelloWorldStateMachine : FiniteStateMachine
{

    // Define the properties of the FSM
    [Property()]
    public string Name { get; set; }

    [Property()]
    public int Age { get; set; }

    // Define the Statess of the FSM
    private enum States
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

    // Define the States transition functions
    [Action(States.Start)]
    [Target(States.Greet, States.Ignore)]
    public void Start(Outcome outcome)
    {
        Console.WriteLine("I am in start function!");

        Random random = new Random();

        if (random.Next(0, 100) % 2 == 0)
        {
            outcome.TargetState = (int)States.Greet;
        }
        else
        {
            outcome.TargetState = (int)States.Ignore;
        }
    }

    [Action(States.Greet)]
    [Target(States.Finish)]
    public void Greet(Outcome outcome)
    {
        Console.WriteLine("I am in Greet function!");
        outcome.TargetState = (int)States.Finish;
    }

    [Action(States.Ignore)]
    [Target(States.Finish)]
    public void Ignore(Outcome outcome)
    {
        Console.WriteLine("I am in Ignore function!");
        outcome.TargetState = (int)States.Finish;
    }
}
