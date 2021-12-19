using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Lib
{
    public static Color GetPlayerColorByIndex(int index)
    {
        switch (index)
        {
            case 1:
                return Color.blue;
            case 2:
                return Color.yellow;
            case 3:
                return new Color(107f / 255, 0, 111f / 255, 1);
            case 4:
                return new Color(222f / 255, 68f / 255, 0, 1);
            default:
                return Color.black;
        }
    }

    /// <summary>
    /// Finds the closest gameobject with the component, starting with checking the gameobjects, then a downwards search starting with the children then moving upwards starting with siblings
    /// then parents etc.
    /// </summary>
    /// <typeparam name="T">The component to search for</typeparam>
    /// <param name="start">The gameobject to start with</param>
    /// <returns>Thec closest component following the search rules</returns>
	public static T FindInHierarchy<T>(GameObject start) where T : class
    {
        T temp = LibGetComponentDownTree<T>(start);
        if (IsNotNull<T>(temp))
        {
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning " + temp);
            return temp;
        }
        Transform curr = start.transform.parent;
        Transform prev = start.transform;
        while (curr != null && curr.gameObject != null)
        {
            temp = LibGetComponentInChildren<T>(curr.gameObject, prev);
            if (IsNotNull<T>(temp))
            {
                DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning " + temp);
                return temp;
            }
            prev = curr;
            curr = curr.transform.parent;
        }
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning null");
        return null;
    }

    /// <summary>
    /// Finds the closest gameobject with the component, only searching downwards in the tree starting with direct children, then searching their children etc
    /// </summary>
    /// <typeparam name="T">The component to search for</typeparam>
    /// <param name="start">The gameobject to start with</param>
    /// <returns>Thec closest component following the search rules</returns>
    public static T FindInDownTree<T>(GameObject start) where T : class
    {
        return Lib.LibGetComponentDownTree<T>(start);
    }

    internal static bool IsNotNull<T>(T toCheck)
    {
        if (typeof(T).IsSubclassOf(typeof(Component)))
        {
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, string.Format("isComponent and toCheck==null:{0} || toCheck.Equals(null):{1}", toCheck != null, !toCheck?.Equals(null)));
            return toCheck != null && !toCheck.Equals(null);
        }
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, string.Format("isntComponent and toCheck==null:{0}", toCheck != null));
        return toCheck != null;
    }

    internal static T LibGetComponentDownTree<T>(GameObject check) where T : class
    {
        Queue<GameObject> toCheck = new Queue<GameObject>();
        toCheck.Enqueue(check);
        while (toCheck.Count > 0)
        {
            GameObject curr = toCheck.Dequeue();
            T temp = curr.GetComponent<T>();
            if (IsNotNull<T>(temp))
            {
                DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning " + temp);
                return temp;
            }
            foreach (Transform t in curr.transform)
            {
                toCheck.Enqueue(t.gameObject);
            }
        }
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning null");
        return null;
    }

    internal static T LibGetComponentInChildren<T>(GameObject check, Transform excludedChild = null) where T : class
    {
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "checking " + check.name);
        T temp = check.GetComponent<T>();
        if (IsNotNull<T>(temp))
        {
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning " + temp);
            return temp;
        }
        for (int index = 0; index < check.transform.childCount; ++index)
        {
            var t = check.transform.GetChild(index);
            if (excludedChild != null && t == excludedChild)
            {
                continue;
            }
            temp = t.gameObject.GetComponent<T>();
            if (IsNotNull<T>(temp))
            {
                DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning " + temp);
                return temp;
            }
        }
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning null");
        return null;
    }

    internal static T LibGetComponent<T>(GameObject check) where T : class
    {
        T temp = check.GetComponent<T>();
        if (IsNotNull<T>(temp))
        {
            DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning " + temp);
            return temp;
        }
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

    public static bool HasTagInHierarchyDownward(GameObject start, string tag)
    {
        DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "Starting search on " + start.name + " for " + tag);
        return Lib.TagRecursiveHelper(start, tag);
    }

    internal static bool TagRecursiveHelper(GameObject toCheck, string tag)
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

    public static Vector2 GetAimPoint(InputAction.CallbackContext ctx, GameObject user)
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
                return worldSpacePosition;
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
