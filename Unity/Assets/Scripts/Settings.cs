using UnityEngine;
using System.Collections;

public class ControlSettings
{
	public readonly KeyCode Fire = KeyCode.V;
}

public static class Settings
{
	public static ControlSettings Controls = new ControlSettings();
	
	public static int LaserTargetDistance = 5;
}
