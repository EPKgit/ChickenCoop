using System;
using UnityEngine;


public class PerformableAction
{
	public GameObject source;	
	public Func<bool> IsPerformable;	
	public int priority; //
	public Action<GameObject> Action;

	/// <summary>
	/// Create an instance of a PerformableAction
	/// </summary>
	/// <param name="s">The gameobject that is providing the option of interacting</param>
	/// <param name="bd">
	/// The delegate that returns if the action can be performed, useful for if the caller wants to perform some action if the action is performable e.g. particles or an outline
	/// </param>
	/// <param name="p">The priority of the action, arbitrates which will be executed if multiple are possible. Higher means that it will override lower interactions</param>
	/// <param name="ad">The action delegate to perform</param>
	public PerformableAction(GameObject s, Func<bool> bd, int p, Action<GameObject> ad)
	{
		source = s;
		IsPerformable = bd;
		priority = p;
		Action = ad;
	}
}
