using Godot;
using System;

[Tool]
public partial class simple_tool_b : Node3D
{
    [ExportToolButton("Click me!")]
    public Callable ClickMeButton => Callable.From(ClickMe);
    public void ClickMe()
    {
        RotateY(10);
    }
}

