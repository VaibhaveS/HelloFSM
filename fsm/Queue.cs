using System;
using System.Collections.Generic;

public class Queue<T>
{
    private readonly List<(Type stateMachineType, int id)> queue;

    public Queue()
    {
        queue = new List<(Type stateMachineType, int id)>();
    }

    public void Enqueue(Type stateMachineType, int id)
    {
        queue.Add((stateMachineType, id));
    }

    public (Type stateMachineType, int id) Dequeue()
    {
        if (queue.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        var item = queue[0];
        queue.RemoveAt(0);
        return item;
    }

    public bool IsEmpty()
    {
        return queue.Count == 0;
    }
}
