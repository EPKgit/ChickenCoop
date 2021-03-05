[System.Serializable]
public struct StatInspectorValue
{
	public StatName name;
	public float value;
	public override string ToString()
	{
		return name.ToString() + ": " + value;
	}
}