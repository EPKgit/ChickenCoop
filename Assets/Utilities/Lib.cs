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
    /// VERY DANGEROUS: THIS WILL FIND A COMPONENT OF THE TYPE YOU'RE LOOKING FOR BUT IT MIGHT NOT BE THE ONE YOU WANT
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
    public static T FindDownwardsInTree<T>(GameObject start) where T : class
    {
        return Lib.LibGetComponentDownTree<T>(start);
    }

    /// <summary>
    /// Finds the closest component of type T upwards from a gameobject
    /// </summary>
    /// <typeparam name="T">The type of the component</typeparam>
    /// <param name="start">The gameobject to start searching from</param>
    /// <returns>Returns the first found component if there is one, null otherwise</returns>
    public static T FindUpwardsInTree<T>(GameObject start) where T : class
    {
        Transform curr = start.transform;
        while(curr != null)
        {
            T temp = curr.GetComponent<T>();
            if (IsNotNull<T>(temp))
            {
                DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "returning " + temp);
                return temp;
            }
			curr = curr.parent;
        }
        return null;
    }

    /// <summary>
    /// Conglomorated null check for reference types and unity components which have a strange tendency to break
    /// </summary>
    /// <typeparam name="T">The type to check the nullness of</typeparam>
    /// <param name="toCheck">The object to check if it's null</param>
    /// <returns>True if the object is truly not null</returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="check"></param>
    /// <param name="excludedChild"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Check a gameobject for a given component
    /// </summary>
    /// <typeparam name="T">The type of the gameobject to look for</typeparam>
    /// <param name="check">The gameobject to search</param>
    /// <returns>The component of type T if present, null otherwise</returns>
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

    public static bool HasTagInParents(GameObject start, string tag)
	{
		Transform curr = start?.transform;
        bool found = false;
        while(curr != null)
		{
    		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "Checking for tag " + tag + " on " + curr.name );
            found = curr.CompareTag(tag);
            if(found)
            {
                break;
            }
			curr = curr.parent;
        }
		DEBUGFLAGS.Log(DEBUGFLAGS.FLAGS.LIB, "Ending search for tag " + tag + " with result " + found);
		return found;
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

    public static Vector2 GetAimPoint(InputAction.CallbackContext ctx)
    {
        switch(ctx.control.device.description.deviceClass)
        {
            case "Mouse":
            {
                return GetAimPoint(ctx.ReadValue<Vector2>());
            }
        }

        return Vector2.zero;
    }

    public static Vector2 GetAimPoint(Vector2 val)
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(val);
        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
        float rayLength;
        groundPlane.Raycast(cameraRay, out rayLength);
        Vector3 worldSpacePosition = cameraRay.GetPoint(rayLength);
        return worldSpacePosition;
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
