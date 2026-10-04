using System;
// using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

[Tool]
public partial class ExampleTool : EditorPlugin
{
    private EditorDock dock;
    private Node activeRoot;
    private ExampleToolUI dockUI;
    private string PrefabFilePath;

    public StringName Name = "Example Tool";
    public override void _EnterTree()
    {
        dock = new EditorDock();
        dock.Title = "ExampleTool";

        dock.DefaultSlot = EditorDock.DockSlot.RightUl;
        dockUI = GD.Load<PackedScene>("res://addons/example/example_tool_ui.tscn").Instantiate<ExampleToolUI>();
        dockUI.exampleTool = this;
        dock.AddChild(dockUI);

        AddDock(dock);
        GD.Print("Plugin enabled: ", Name);
    }

    public override void _ExitTree()
    {
        RemoveDock(dock);
        dock.QueueFree();
        GD.Print("Plugin disabled: ", Name);
    }

    public void OnFileDialogeOpen()
    {
		// Regex e = new("mues");
    }
    public void OnFileSelected(string file)
    {
        PrefabFilePath = file;
    }
}
