using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;


[Tool]
public partial class TileTool : EditorPlugin
{
	[ExportToolButton("Init Tool")]
    public Callable ClickMeButton => Callable.From(Init);

	[Export]
    public bool toolActive = false;

	[Export]
	public string tileFolderPath = "tiles/demo/";
	
	public Vector2I gridSize = new Vector2I(10,10);

	private bool[,] primaryBlockGrid;
	private Node3D[,] meshGrid;

	[Export]
	public Camera3D mainCamera;

	Dictionary<string, string> fileReference = new Dictionary<string, string>();

	Dictionary<byte, int> tileRotation = new Dictionary<byte, int>
	{
		{0, 0},
		{1,0},{2,90},{4,180},{8,270},
		{3,90},{6,180},{9,0},{12,270},
		{5,0},{10,90},
		{7,180},{11,90},{13,0},{14,270},
		{15,0},
	};

	Dictionary<byte, string> tileMapDictionary = new Dictionary<byte, string>
	{
		{0,"0"},
		{1,"1"},{2,"1"},{4,"1"},{8,"1"},
		{3,"3"},{6,"3"},{9,"3"},{12,"3"},
		{5,"5"},{10,"5"},
		{7,"7"},{11,"7"},{13,"7"},{14,"7"},
		{15,"15"},
	};


	[Flags]
	public enum NeighborBitMask : byte
    {
        NorthEast = 1 << 0,
        SouthEast = 1 << 1,
        SouthWest = 1 << 2,
        NorthWest = 1 << 3,
    }

	public NeighborBitMask[] bitmasks = new NeighborBitMask[]
    {
		NeighborBitMask.NorthEast,
        NeighborBitMask.SouthEast,
        NeighborBitMask.SouthWest,
		NeighborBitMask.NorthWest
    };
	
	public Vector2I[] NeighborCoords = new Vector2I[]{ // primary grid offset.
		new Vector2I(0,0),
		new Vector2I(0,-1),
		new Vector2I(-1,-1),
		new Vector2I(-1,0),
	};

	bool init = false;
	Vector3 pointerGridPos = Vector3.Zero;
	private EditorDock dock;
    private Node activeRoot;

    private TileToolUI dockUI;
    public override void _EnterTree()
    {
        dock = new EditorDock();
        dock.Title = "TileTool";
        dock.DefaultSlot = EditorDock.DockSlot.RightUl;
        dockUI = GD.Load<PackedScene>("res://addons/tile_tool/tile_tool_ui.tscn").Instantiate<TileToolUI>();
        dockUI.tileTool = this;
        dock.AddChild(dockUI); 
		// if i want to know the class on the UI, for callback i do: .Instantiate<ClassType>()
		// i could also just instantiate the buttons... 
        AddDock(dock);
        //_dock.Setup();

        SetInputEventForwardingAlwaysEnabled(); // Required for raycasting in Scene. 

        GD.Print("Plugin enabled");
    }

    public override void _ExitTree()
    {
        RemoveDock(dock);
        dock.QueueFree();
        GD.Print("Plugin disabled");
    }



    public override void _Process(double delta)
    {
		if(!init){
			return;
		}

		for(int x = 0; x < gridSize.X; x++){
			for(int y = 0; y < gridSize.Y; y++){
				if(primaryBlockGrid[x,y]){
					DebugDraw3D.DrawSphere(new Vector3(x,0,y), 0.03f,Colors.Green,0.1f);
				}
				else{
					DebugDraw3D.DrawSphere(new Vector3(x,0,y), 0.03f,Colors.Red,0.1f);
				}
			}
		}
		DebugDraw3D.DrawSphere(new Vector3(pointerGridPos.X,0,pointerGridPos.Z), 0.04f,Colors.Yellow,0.1f);
    }

	public void Init(){

        fileReference.Clear();
        SetCurrentScene();
        DirContents(tileFolderPath);
		primaryBlockGrid = new bool[gridSize.X,gridSize.Y];
		meshGrid = new Node3D[gridSize.X -1,gridSize.Y -1];
		for(int y = 0; y < gridSize.Y -1; y++){
			for(int x = 0; x < gridSize.X -1; x++){
				primaryBlockGrid[x,y] = false;
				SetTileMesh("0",x,y, 0);
			}
		}
	}
	public void SetCurrentScene(){
        activeRoot = EditorInterface.Singleton.GetEditedSceneRoot();
	}

	public void CommitTiles(){
		//loop all tileInstance.Owner = activeRoot;
		//move all tile instances to current active root as well..
	}

	public void DiscardTiles(){

	}

	public void SetTileMesh(string tile, int x, int y, int rotation)
	{

        if (meshGrid[x, y] != null)
		{
			meshGrid[x, y].QueueFree();
			meshGrid[x, y] = null;
		}

        string resourcePath = fileReference[tile];
		var asset = GD.Load<PackedScene>(resourcePath);
		var tileInstance = (Node3D)asset.Instantiate();
		this.AddChild(tileInstance);
		tileInstance.GlobalPosition = new Vector3(x + 0.5f, 0, y + 0.5f);
		tileInstance.Scale = Vector3.One;
		tileInstance.RotateY(Mathf.DegToRad(rotation));
		meshGrid[x, y] = tileInstance;
        tileInstance.Name = "tile_x" + x.ToString() + "y" + y.ToString();
        //GD.Print(resourcePath + " rot: " + rotation);
	}


	private const float RayLength = 1000.0f;

	/*
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouse eventMouse)
		{
			if(!toolActive){
                return;
            }

			var spaceState = GetWorld3D().DirectSpaceState;
			var from = mainCamera.ProjectRayOrigin(eventMouse.Position);
			var to = from + mainCamera.ProjectRayNormal(eventMouse.Position) * RayLength;

			var query = PhysicsRayQueryParameters3D.Create(from, to);
    		var result = spaceState.IntersectRay(query);

            GD.Print(result);

            return;

            Vector3I gridPoint = (Vector3I)((Vector3)result["position"] + Vector3.Right *0.5f);
			// the gridpoint has to be scaled with the grid size and offsetted with the gridsize to work properly
			// nowe assume the grid is always 1 unit per cell.
			GD.Print(gridPoint);

			pointerGridPos = gridPoint;

			if(@event is InputEventMouseButton eventMouseButtonPressedLeft && eventMouseButtonPressedLeft.Pressed && eventMouseButtonPressedLeft.ButtonIndex == MouseButton.Left){
				SetBlockState(new Vector2I(gridPoint.X,gridPoint.Z), true);
			}
			else if(@event is InputEventMouseButton eventMouseButtonPressedRight && eventMouseButtonPressedRight.Pressed && eventMouseButtonPressedRight.ButtonIndex == MouseButton.Right){
				SetBlockState(new Vector2I(gridPoint.X,gridPoint.Z), false);
			}

		}
	}
	*/

    public override int _Forward3DGuiInput(Camera3D camera, InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse &&
            mouse.Pressed &&
            mouse.ButtonIndex == MouseButton.Left)
        {

            Vector3 rayOrigin = camera.ProjectRayOrigin(mouse.Position);
            Vector3 rayDir = camera.ProjectRayNormal(mouse.Position);

            Plane plane = new Plane(Vector3.Up, 0);
            Vector3? hit = plane.IntersectsRay(rayOrigin, rayDir);

            if (hit != null)
            {
                GD.Print($"Plane hit: {hit.Value}");
                DebugDraw3D.DrawSphere(hit.Value, 0.1f, Colors.Red, 3.0f);
            }

            return (int)EditorPlugin.AfterGuiInput.Stop;
        }

        return (int)EditorPlugin.AfterGuiInput.Pass;
    }


	public void SetBlockState(Vector2I pos, bool state)
	{
		if (!checkIfValidPrimaryGrid(pos))
		{
			return;
		}

		primaryBlockGrid[pos.X, pos.Y] = state;
		CalculateBitMask(pos);
	}


public void CalculateBitMask(Vector2I gridPoint)
{
    for (int i = 0; i < NeighborCoords.Length; i++)
    {
        Vector2I dualGridPoint = gridPoint + NeighborCoords[i];
        if (checkIfValidDualGrid(dualGridPoint))
        {
            UpdateDualGridTile(dualGridPoint);
        }
    }
}

private void UpdateDualGridTile(Vector2I dualGridPoint)
{
    byte mask = CalculateBitMaskForDualGrid(dualGridPoint);
    if (tileMapDictionary.ContainsKey(mask))
    {
        SetTileMesh(tileMapDictionary[mask], dualGridPoint.X, dualGridPoint.Y, tileRotation[mask]);
    }
}

private byte CalculateBitMaskForDualGrid(Vector2I dualGridPoint)
{
    byte mask = 0;
    // Check each neighbor of the dual grid point
    for (int i = 0; i < NeighborCoords.Length; i++)
    {
        Vector2I primaryGridPoint = dualGridPoint - NeighborCoords[i];
        if (checkIfValidPrimaryGrid(primaryGridPoint) && primaryBlockGrid[primaryGridPoint.X, primaryGridPoint.Y])
        {
            mask |= (byte)bitmasks[i];
        }
    }
	GD.Print(mask);
    return mask;
}



public void DirContents(string path)
{
	GD.Print(path);
	using var dir = DirAccess.Open(path);
	if (dir != null)
	{
		dir.ListDirBegin();
		string fileName = dir.GetNext();
		while (fileName != "")
		{
			if (dir.CurrentIsDir())
			{
				//GD.Print($"Found directory: {fileName}");
			}
			else
			{
				if(!fileName.Contains("import")){
					string result = Regex.Replace(fileName, "\\.glb$", "", RegexOptions.IgnoreCase);
					fileReference.Add(result, path + fileName);
					GD.Print(result + "  " + path + fileName);
				}
			}
			fileName = dir.GetNext();
		}
	}
	else
	{
		GD.Print("An error occurred when trying to access the path.");
	}
}


private bool checkIfValidPrimaryGrid(Vector2I gridPos)
{
    return gridPos.X >= 0 && gridPos.X < gridSize.X && gridPos.Y >= 0 && gridPos.Y < gridSize.Y;
}

private bool checkIfValidDualGrid(Vector2I gridPos)
{
    return gridPos.X >= 0 && gridPos.X < gridSize.X - 1 && gridPos.Y >= 0 && gridPos.Y < gridSize.Y - 1;
}


}