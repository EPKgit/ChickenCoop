using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T> where T : class
{
	private IComparer<T> comparator;

	private delegate bool PushDelegate(T add);
	private PushDelegate PushDel;

	private T[] items;
	private int size;
	private int capacity;

	public PriorityQueue(int i = 1, IComparer<T> c = null)
	{
		capacity = 1;
		size = 0;
		items = new T[capacity];
		if(c == null)
		{
			if(!typeof(IComparable).IsAssignableFrom(typeof(T)))
			{
				throw new System.Exception("Generic type must have a comparator passed in or implement the IComparable interface.");
			}
			PushDel = PushWithoutComparator;
		}
		else
		{
			PushDel = PushWithComparator;
		}
		comparator = c;
	}

	public T Peek()
	{
		return !IsEmpty() ? items[0] : null;
	}

	public T Pop()
	{
		if(size == 0)
		{
			return null;
		}
		T temp = items[0];
		for(int x = 0; x < size; ++x)
		{
			items[x] = items[x + 1];
		}
		--size;
		return temp;
	}

	public bool Push(T add)
	{
		return PushDel(add);
	}
	
	private bool PushWithComparator(T add)
	{
		if(++size >= capacity)
		{
			Resize();
		}
		for(int x = 0; x < size - 1; ++x)
		{
			if(comparator.Compare(add, items[x]) < 0)
			{
				InsertAt(add, x);
				return true;
			}
		}
		InsertAt(add, size - 1);
		return true;
	}

	private bool PushWithoutComparator(T add)
	{
		if(++size >= capacity)
		{
			Resize();
		}
		IComparable temp = (IComparable)add;
		for(int x = 0; x < size - 1; ++x)
		{
			if(temp.CompareTo(items[x]) < 0)
			{
				InsertAt(add, x);
				return true;
			}
		}
		InsertAt(add, size - 1);
		return true;
	}

	public bool IsEmpty()
	{
		return size == 0;
	}

	public void Print()
	{
		for(int x = 0; x < size; ++x)
		{
			Debug.Log(items[x].ToString());
		}
	}

	public T GetValue(Predicate<T> match)
	{
		for(int x = 0; x < size; ++x)
		{
			if(match(items[x]))
			{
				return items[x];
			}
		}
		return null;
	}

	public bool UpdateValue(Predicate<T> match, T newValue)
	{
		for(int x = 0; x < size; ++x)
		{
			if(match(items[x]))
			{
				items[x] = newValue;
				return true;
			}
		}
		return false;
	}

	private void InsertAt(T item, int index)
	{
		for(int x = index + 1; x < size + 1; ++x)
		{
			items[x] = items[x - 1];
		}
		items[index] = item;
	}

	private void Resize()
	{
		capacity *= 2;
		++capacity;
		T[] temp = new T[capacity];
		for(int x = 0; x < size; ++x)
		{
			temp[x] = items[x];
		}
		items = temp;
	}

	public T[] ToArray()
	{
		T[] temp = new T[size];
		for(int x = 0; x < size; ++x)
		{
			temp[x] = items[x];
		}
		return temp;
	}
}
