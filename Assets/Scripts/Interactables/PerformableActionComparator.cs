using System.Collections.Generic;

public class PerformableActionComparator : IComparer<PerformableAction>
{
	public int Compare(PerformableAction pa1, PerformableAction pa2)
	{
		return pa1.priority - pa2.priority;
	}
}