using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GrabBag<T> where T : IComparable<T>
{
    public Queue<T> queue;

    private T[] startingValues;
    private bool preventRepeats = false;

    public GrabBag(T[] startingValues, bool preventRepeatOnShuffler)
    {
        if(startingValues == null || startingValues.Length <= 1)
        {
            throw new ArgumentException("ERROR: grab bag made with less than 2 elements");
        }
        queue = new Queue<T>();
        this.startingValues = startingValues;
        this.preventRepeats = preventRepeatOnShuffler;
        Refill();
    }

    public T Grab()
    {
        T grabbed = queue.Dequeue();
        if(queue.Count == 0)
        {
            if (preventRepeats)
            {
                Refill(grabbed);
            }
            else
            {
                Refill();
            }
        }
        return grabbed;
    }

    public void Refill()
    {
        queue.Clear();
        T[] shuffled = ShuffledValues();
        foreach (T t in shuffled)
        {
            queue.Enqueue(t);
        }
    }

    public void Refill(T lastUsed)
    {
        T[] shuffled = ShuffledValues();
        while (lastUsed.CompareTo(shuffled[0]) == 0)
        {
            var swap = shuffled[0];
            var index = UnityEngine.Random.Range(0, shuffled.Length - 1);
            shuffled[0] = shuffled[index];
            shuffled[index] = swap;
        }

        foreach (T t in shuffled)
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
