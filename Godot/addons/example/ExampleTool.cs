using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

[Tool]
public partial class ExampleTool : EditorPlugin
{
	[ExportToolButton("Button")]
	public Callable InitToolButton => Callable.From(UseButton);

	private EditorDock dock;
	private Node activeRoot;
	private TileToolUI dockUI;

	public override void _EnterTree()
	{
		dock = new EditorDock();
		dock.Title = "TileTool";
		dock.DefaultSlot = EditorDock.DockSlot.RightUl;
		dockUI = GD.Load<PackedScene>("res://addons/example/example_tool_ui.tscn").Instantiate<TileToolUI>();
		dockUI.exampleTool = this;
		dock.AddChild(dockUI);
		AddDock(dock);
		GD.Print("Plugin enabled");
	}

	public override void _ExitTree()
	{
		RemoveDock(dock);
		dock.QueueFree();
		GD.Print("Plugin disabled");
	}

	public void UseButton()
	{

		activeRoot = EditorInterface.Singleton.GetEditedSceneRoot();

		// Do something. 
	}


}
