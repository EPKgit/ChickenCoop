using UnityEngine;

public delegate bool BoolDelegate();
public delegate void ActionDelegate(GameObject user);

public class PerformableAction
{
	public BoolDelegate IsPerformable;	
	public int priority;
	public ActionDelegate Action;

	/// <summary>
	/// Create an instance of a PerformableAction
	/// </summary>
	/// <param name="bd">
	/// The delegate that returns if the action can be performed, useful for if the caller wants to perform some action if the action is performable e.g. particles or an outline
	/// </param>
	/// <param name="p">The priority of the action, arbitrates which will be executed if multiple are possible. Higher is more likely to happen</param>
	/// <param name="ad">The action delegate to perform</param>
	public PerformableAction(BoolDelegate bd, int p, ActionDelegate ad)
	{
		IsPerformable = bd;
		priority = p;
		Action = ad;
	}
}
