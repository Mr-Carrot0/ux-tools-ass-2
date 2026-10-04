using Godot;
using System;
using System.IO;

[Tool]
public partial class ExampleToolUI : Control
{
    public ExampleTool exampleTool;
    [Export] EditorFileDialog fileDialog;
    [Export] LineEdit filePathEdit;
    [Export] Button BrowseButton;

    public override void _EnterTree()
    {
        BrowseButton.Pressed += OpenFileDialoge;
        fileDialog.FileSelected += FillFile;
    }

    public void OpenFileDialoge()
    {
        fileDialog.Visible = true;
        exampleTool.OnFileDialogeOpen();
    }
    public void FillFile(string file)
    {
        GD.Print("file selected: ", file);
        filePathEdit.Text = file;

		exampleTool.OnFileSelected(file);
    }
}
