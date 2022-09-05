using System;
using System.Collections.Generic;
using UnityEngine;

public class GrabBag<T>
{
    public Queue<T> queue;

    private T[] startingValues;

    public GrabBag(T[] startingValues)
    {
        this.startingValues = startingValues;
        Refill();
        throw new NotImplementedException();
    }

    public T Grab()
    {
        return queue.Dequeue();
    }

    public void Refill()
    {
        queue.Clear();
        foreach (T t in ShuffledValues())
        {
            queue.Enqueue(t);
        }
    }

    private T[] ShuffledValues()
    {
        T[] temp = (T[])startingValues.Clone();
        return ShuffledValues(temp);
    }

    private T[] ShuffledValues(T[] values)
    {
        for (int x = values.Length; x > 0; x--)
        {
            var swap = values[0];
            var index = UnityEngine.Random.Range(0, x);
            values[0] = values[index];
            values[index] = swap;
        }
        return values;
    }
}
