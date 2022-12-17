using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRender : MonoBehaviour
{
    public float highlightRotationSpeed = 10.0f;

    private PlayerInput playerInput;
	private SpriteRenderer sprite;
    private Material highlightMaterial;
    private float rotationDegrees;
    IEnumerator Start()
	{
		sprite = transform.Find("Render").GetComponent<SpriteRenderer>();
		playerInput = GetComponent<PlayerInput>();
        enabled = false;
		while(playerInput.playerID == -1)
		{
            yield return null;
        }
        // sprite.color = Lib.GetPlayerColorByIndex(playerInput.playerID);
        enabled = true;
        highlightMaterial = sprite.transform.Find("Highlight").gameObject.GetComponent<SpriteRenderer>().material;
        rotationDegrees = 0;
    }

	void Update()
	{
        rotationDegrees += (Time.deltaTime * highlightRotationSpeed) % 360;
        highlightMaterial.SetFloat("_Rotation", rotationDegrees);
    }

	public void ReplaceModel(GameObject g)
	{
		Transform temp;
		if( (temp = transform.Find("Render").Find("Model")) != null)
		{
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.RENDER, "killing old model");
			Destroy(temp.gameObject);
		}
		GameObject newModel = Instantiate(g);
		newModel.transform.position = Vector3.zero;
		newModel.transform.SetParent(transform.Find("Render"), false);
	}
}
