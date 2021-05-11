using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EditorLib
{
    public delegate T SerializedPropertyValueDelegate<T>(SerializedProperty sp);
    public static List<T> SerializedPropertyToList<T>(SerializedProperty serializedProperty, SerializedPropertyValueDelegate<T> del)
    {
        if (serializedProperty != null && serializedProperty.isArray)
        {
            int arrayLength = 0;

            serializedProperty.Next(true); // skip generic field
            serializedProperty.Next(true); // advance to array size field

            // Get the array size
            arrayLength = serializedProperty.intValue;

            serializedProperty.Next(true); // advance to first array index

            // Write values to list
            List<T> values = new List<T>(arrayLength);
            int lastIndex = arrayLength - 1;
            for (int i = 0; i < arrayLength; i++)
            {
                T val = del(serializedProperty);
                values.Add(val); // copy the value to the list
                if (i < lastIndex) serializedProperty.Next(false); // advance without drilling into children
            }
            return values;
        }
        return null;
    }
}
