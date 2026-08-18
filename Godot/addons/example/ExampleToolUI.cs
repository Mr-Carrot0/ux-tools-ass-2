using Godot;
using System;

[Tool]
public partial class TileToolUI : Control
{
    public ExampleTool exampleTool;

    [Export]
    Button exampleButton;

    public override void _EnterTree()
    {
        exampleButton.Pressed += ButtonPress;
    }

	public void ButtonPress(){
        exampleTool.UseButton();
    }

}
