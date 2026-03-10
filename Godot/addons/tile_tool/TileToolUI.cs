using Godot;
using System;

[Tool]
public partial class TileToolUI : Control
{
    public TileTool tileTool;

    [Export]
    Button initButton;

    public override void _EnterTree()
    {
        initButton.Pressed += Init;
    }

	public void Init(){
        tileTool.Init();
    }

}
