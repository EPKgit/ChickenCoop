﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroData : IEquatable<AggroData>
{
	public GameObject source;
	public float value;

	public AggroData(GameObject g, float f)
	{
		source = g;
		value = f;
	}

	public AggroData(HealthChangeData hcd)
	{
		source = hcd.BilateralData.OverallSource;
        float aggroValue = StatBlockComponent.GetValue(source, StatName.AggroPercentage);
		value = -hcd.Delta * aggroValue;
	}

	public bool Equals(AggroData other)
	{
		return source == other.source;
	}
}

public class MaxAggroComparator : IComparer<AggroData>
{
	float val;
	public int Compare(AggroData ag1, AggroData ag2)
	{
		val = ag1.value - ag2.value;
		return (val == 0) ? 0 : (val < 0) ? -1 : 1;
	}
}

public class MinAggroComparator : IComparer<AggroData>
{
	float val;
	public int Compare(AggroData ag1, AggroData ag2)
	{
		val = ag2.value - ag1.value;
		return (val == 0) ? 0 : (val < 0) ? -1 : 1;
	}
}