using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Lib
{
	public static Color GetPlayerColorByIndex(int index)
	{
		switch(index)
		{
			case 1:
				return Color.blue;
			case 2:
				return Color.yellow;
			case 3:
				return new Color(107f / 255, 0, 111f / 255, 1);
			case 4:
				return new Color(222f / 255, 68f / 255 , 0, 1);
			default:
				return Color.black;
		}
	}

	public static T FindInHierarchy<T>(GameObject start) where T: class
	{
		GameObject top = start;
		while(top.transform.parent != null)
		{
			top = top.transform.parent.gameObject;
		}
		return Lib.ComponentRecursiveHelper<T>(top);
	}

	static bool IsNotNull<T>(T toCheck)
	{
		if(typeof(T).IsSubclassOf(typeof(Component)))
		{
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, string.Format("isComponent and toCheck==null:{0} || toCheck.Equals(null):{1}", toCheck != null, !toCheck?.Equals(null)));
			return toCheck != null && !toCheck.Equals(null);
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, string.Format("isntComponent and toCheck==null:{0}", toCheck != null));
		return toCheck != null;
	}

	static T ComponentRecursiveHelper<T>(GameObject check) where T: class
	{
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "checking " + check.name);
		T temp = check.GetComponent<T>();
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "found " + temp);
		if(IsNotNull<T>(temp))
		{
			DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning " + temp);
			return temp;
		}
		foreach(Transform t in check.transform)
		{
			temp = Lib.ComponentRecursiveHelper<T>(t.gameObject);
			if(IsNotNull<T>(temp))
			{
				DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning " + temp);
				return temp;
			}
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning null");
		return null;
	}

	public static bool HasTagInHierarchy(GameObject start, string tag)
	{
		GameObject top = start;
		while(top.transform.parent != null)
		{
			top = top.transform.parent.gameObject;
		}
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "Starting search on " + start.name + " for " + tag);
		return Lib.TagRecursiveHelper(top, tag);
	}

	static bool TagRecursiveHelper(GameObject toCheck, string tag)
	{
		bool temp = toCheck.CompareTag(tag);
		if(temp)
		{
			return temp;
		}
		foreach(Transform t in toCheck.transform)
		{
			temp = Lib.TagRecursiveHelper(t.gameObject, tag);
			if(temp)
			{
				return temp;
			}
		}
		return false;
	}

	public static bool HasParent(GameObject toCheck, GameObject potentialParent)
	{
		if(toCheck == null || potentialParent == null)
		{
			return false;
		}
		Transform parent = toCheck.transform;
		while(parent != null)
		{
			if(parent.gameObject == potentialParent)
			{
				return true;
			}
			parent = parent.parent;
		}
		return false;
	}

    //   public static Vector3 GetMouseDirection(Mouse m, GameObject from)
    //   {
    //       Vector3 val = m.position.ReadValue();
    //       Ray cameraRay = Camera.main.ScreenPointToRay(val);
    //       Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
    //       float rayLength;
    //       groundPlane.Raycast(cameraRay, out rayLength);
    //       Vector3 worldSpacePosition = cameraRay.GetPoint(rayLength);
    //       return worldSpacePosition - from.transform.position;
    //   }

    public static Vector2 GetAimDirection(InputAction.CallbackContext ctx, GameObject user)
    {
        switch(ctx.control.device.description.deviceClass)
        {
            case "Mouse":
            {
                Vector2 val = ctx.ReadValue<Vector2>();
                Ray cameraRay = Camera.main.ScreenPointToRay(val);
                Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
                float rayLength;
                groundPlane.Raycast(cameraRay, out rayLength);
                Vector3 worldSpacePosition = cameraRay.GetPoint(rayLength);
                return (worldSpacePosition - user.transform.position).normalized;
            }
        }

        return Vector2.zero;
    }

    public static Vector2 DefaultDirectionCheck(Vector2 dir)
	{
		if(dir.x == 0 && dir.y == 0)
		{
			dir = Vector2.right;
		}
		dir.Normalize();
		return dir;
	}

	public static Vector2 DefaultDirectionCheck(Vector2 dir, Rigidbody2D rb)
	{
		if(dir.x == 0 && dir.y == 0)
		{
			dir = rb.velocity;
			if(dir.x == 0 && dir.y == 0)
			{
				dir = Vector2.right;
			}
		}
		dir.Normalize();
		return dir;
	}
}
