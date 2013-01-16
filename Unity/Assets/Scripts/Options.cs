using UnityEngine;
using System.Collections;

public class ControlSettings
{
	public KeyCode Fire = KeyCode.Mouse0;
    public KeyCode Forward = KeyCode.W;
    public KeyCode Backward = KeyCode.S;
    public KeyCode StrafeLeft = KeyCode.A;
    public KeyCode StrafeRight = KeyCode.D;
}

public static class Options
{
	public static readonly char NewLineChar = '\n';
	public static ControlSettings Controls = new ControlSettings();
	public static int LaserTargetDistance = 5;
	public static int StartingHealth = 100;
    public static int SongIndex = 0;
}
