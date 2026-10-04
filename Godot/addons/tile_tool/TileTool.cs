using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

[Tool]
public partial class TileTool : EditorPlugin
{
	[ExportToolButton("Init Tool")]
	public Callable InitToolButton => Callable.From(Init);

	[Export]
	public bool toolActive = true;

	[Export]
	public string tileFolderPath = "tiles/demo/";

	public Vector2I gridSize = new Vector2I(10, 10);
	private bool[,] primaryBlockGrid;
	private Node3D[,] meshGrid;

	Dictionary<string, string> fileReference = [];

	Dictionary<byte, int> tileRotation = new()
    {
		{ 0, 0 },
		{ 1, 0 }, { 2, 90 }, { 4, 180 }, { 8, 270 },
		{ 3, 90 }, { 6, 180 }, { 9, 0 }, { 12, 270 },
		{ 5, 0 }, { 10, 90 },
		{ 7, 180 }, { 11, 90 }, { 13, 0 }, { 14, 270 },
		{ 15, 0 },
	};

	Dictionary<byte, string> tileMapDictionary = new()
    {
		{ 0, "0" },
		{ 1, "1" }, { 2, "1" }, { 4, "1" }, { 8, "1" },
		{ 3, "3" }, { 6, "3" }, { 9, "3" }, { 12, "3" },
		{ 5, "5" }, { 10, "5" },
		{ 7, "7" }, { 11, "7" }, { 13, "7" }, { 14, "7" },
		{ 15, "15" },
	};

	[Flags]
	public enum NeighborBitMask : byte
	{
		NorthEast = 1 << 0,
		SouthEast = 1 << 1,
		SouthWest = 1 << 2,
		NorthWest = 1 << 3,
	}

	public NeighborBitMask[] bitmasks =
    [
        NeighborBitMask.NorthEast,
		NeighborBitMask.SouthEast,
		NeighborBitMask.SouthWest,
		NeighborBitMask.NorthWest,
	];

	// Primary grid offset.
	public Vector2I[] NeighborCoords =
    [
        new (0, 0),
		new (0, -1),
		new (-1, -1),
		new (-1, 0),
	];

	bool init = false;
	Vector3 pointerGridPos = Vector3.Zero;
	private EditorDock dock;
	private Node activeRoot;
	private TileToolUI dockUI;

	public override void _EnterTree()
	{
        dock = new EditorDock
        {
            Title = "TileTool",
            DefaultSlot = EditorDock.DockSlot.RightUl
        };
        dockUI = GD.Load<PackedScene>("res://addons/tile_tool/tile_tool_ui.tscn").Instantiate<TileToolUI>();
		dockUI.tileTool = this;
		dock.AddChild(dockUI);
		AddDock(dock);
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
		if (!init)
		{
			return;
		}

		for (int x = 0; x < gridSize.X; x++)
		{
			for (int y = 0; y < gridSize.Y; y++)
			{
				if (primaryBlockGrid[x, y])
				{
					DebugDraw3D.DrawSphere(new Vector3(x, 0, y), 0.03f, Colors.Green, 0.1f);
				}
				else
				{
					DebugDraw3D.DrawSphere(new Vector3(x, 0, y), 0.03f, Colors.Red, 0.1f);
				}
			}
		}
		DebugDraw3D.DrawSphere(new Vector3(pointerGridPos.X, 0, pointerGridPos.Z), 0.04f, Colors.Yellow, 0.1f);
	}

	public void Init()
	{
		fileReference.Clear();
		SetCurrentScene();
		DirContents(tileFolderPath);
		primaryBlockGrid = new bool[gridSize.X, gridSize.Y];
		meshGrid = new Node3D[gridSize.X - 1, gridSize.Y - 1];
		for (int y = 0; y < gridSize.Y - 1; y++)
		{
			for (int x = 0; x < gridSize.X - 1; x++)
			{
				primaryBlockGrid[x, y] = false;
				SetTileMesh("0", x, y, 0);
			}
		}
		init = true;
		toolActive = true;
	}

	public void SetCurrentScene()
	{
		activeRoot = EditorInterface.Singleton.GetEditedSceneRoot();
	}

	public void CommitTiles()
	{
		// Loop all tileInstance.Owner = activeRoot;
		// Move all tile instances to current active root as well.
	}

	public void DiscardTiles()
	{
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
		activeRoot.AddChild(tileInstance);
		tileInstance.Owner = activeRoot;
		tileInstance.GlobalPosition = new Vector3(x + 0.5f, 0, y + 0.5f);
		tileInstance.Scale = Vector3.One;
		tileInstance.RotateY(Mathf.DegToRad(rotation));
		meshGrid[x, y] = tileInstance;
		tileInstance.Name = $"tile_x{x}y{y}";
	}

	public override int _Forward3DGuiInput(Camera3D camera, InputEvent @event)
	{
		if (!toolActive)
		{
			return (int)EditorPlugin.AfterGuiInput.Pass;
		}

		if (@event is InputEventMouseMotion motion)
		{
			UpdatePointerPosition(camera, motion.Position);
			return (int)EditorPlugin.AfterGuiInput.Pass;
		}

		if (@event is InputEventMouseButton mouse &&
			mouse.Pressed &&
			(mouse.ButtonIndex == MouseButton.Left || mouse.ButtonIndex == MouseButton.Right))
		{
			Vector2I gridPoint = RayToGridPoint(camera, mouse.Position);
			if (CheckIfValidPrimaryGrid(gridPoint))
			{
				SetBlockState(gridPoint, mouse.ButtonIndex == MouseButton.Left);
				pointerGridPos = new Vector3(gridPoint.X, 0, gridPoint.Y);
			}

			return (int)EditorPlugin.AfterGuiInput.Stop;
		}

		return (int)EditorPlugin.AfterGuiInput.Pass;
	}

	private Vector2I RayToGridPoint(Camera3D camera, Vector2 screenPos)
	{
		Vector3 rayOrigin = camera.ProjectRayOrigin(screenPos);
		Vector3 rayDir = camera.ProjectRayNormal(screenPos);

		Plane plane = new Plane(Vector3.Up, 0);
		Vector3? hit = plane.IntersectsRay(rayOrigin, rayDir);
		if (hit == null)
		{
			return new Vector2I(-1, -1);
		}

		return new Vector2I(
			Mathf.RoundToInt(hit.Value.X),
			Mathf.RoundToInt(hit.Value.Z)
		);
	}

	private void UpdatePointerPosition(Camera3D camera, Vector2 screenPos)
	{
		Vector3 rayOrigin = camera.ProjectRayOrigin(screenPos);
		Vector3 rayDir = camera.ProjectRayNormal(screenPos);

		Plane plane = new Plane(Vector3.Up, 0);
		Vector3? hit = plane.IntersectsRay(rayOrigin, rayDir);
		if (hit == null)
		{
			return;
		}

		pointerGridPos = new Vector3(
			Mathf.RoundToInt(hit.Value.X),
			0,
			Mathf.RoundToInt(hit.Value.Z)
		);
	}

	public void SetBlockState(Vector2I pos, bool state)
	{
		if (!CheckIfValidPrimaryGrid(pos))
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
			if (CheckIfValidDualGrid(dualGridPoint))
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
		for (int i = 0; i < NeighborCoords.Length; i++)
		{
			Vector2I primaryGridPoint = dualGridPoint - NeighborCoords[i];
			if (CheckIfValidPrimaryGrid(primaryGridPoint) && primaryBlockGrid[primaryGridPoint.X, primaryGridPoint.Y])
			{
				mask |= (byte)bitmasks[i];
			}
		}
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
				if (!dir.CurrentIsDir())
				{
					if (!fileName.Contains("import"))
					{
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

	private bool CheckIfValidPrimaryGrid(Vector2I gridPos)
	{
		return gridPos.X >= 0 && gridPos.X < gridSize.X && gridPos.Y >= 0 && gridPos.Y < gridSize.Y;
	}

	private bool CheckIfValidDualGrid(Vector2I gridPos)
	{
		return gridPos.X >= 0 && gridPos.X < gridSize.X - 1 && gridPos.Y >= 0 && gridPos.Y < gridSize.Y - 1;
	}
}
