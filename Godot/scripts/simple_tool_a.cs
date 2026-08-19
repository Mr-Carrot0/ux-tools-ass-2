using Godot;
using System;

[Tool]
public partial class simple_tool_a : Node3D
{
    public override void _Process(double delta)
	{
		// Reload scene when changing things or use ToolEnable property. 
        Vector3 v = new Vector3(Mathf.Sin(Time.GetTicksMsec()),0,0);
        Position = new Vector3(Mathf.Sin((float)Time.GetTicksMsec() * 0.001f), 0, 0);
    }
}
