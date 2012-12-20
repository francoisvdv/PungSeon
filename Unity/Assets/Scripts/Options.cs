using UnityEngine;
using System.Collections;

public class ControlSettings
{
	public KeyCode Fire = KeyCode.V;
}

public static class Options
{
	public static readonly char NewLineChar = '\n';
	public static ControlSettings Controls = new ControlSettings();
	public static int LaserTargetDistance = 5;
	public static int StartingHealth = 100;
}
