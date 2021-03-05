using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRender : MonoBehaviour
{
	private PlayerInput playerInput;
	private SpriteRenderer sprite;
    IEnumerator Start()
	{
		sprite = transform.Find("Render").GetComponent<SpriteRenderer>();
		playerInput = GetComponent<PlayerInput>();
		yield return new WaitUntil( () => playerInput.playerID != -1);
		sprite.color = Lib.GetPlayerColorByIndex(playerInput.playerID);
	}

	public void ReplaceModel(GameObject g)
	{
		Transform temp;
		if( (temp = transform.Find("Render").Find("Model")) != null)
		{
			Debug.Log("killing old model");
			Destroy(temp.gameObject);
		}
		GameObject newModel = Instantiate(g);
		newModel.transform.position = Vector3.zero;
		newModel.transform.SetParent(transform.Find("Render"), false);
	}
}
