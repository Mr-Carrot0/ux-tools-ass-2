using Godot;
using System;
using System.Security.Cryptography.X509Certificates;


[Tool]
public partial class TileTool : Node
{

	[ExportToolButton("GenerateGrid")]
    public Callable ClickMeButton => Callable.From(GenerateGrid);

	[Export]
    public bool toolActive = false;

    public MeshInstance3D[,] tileGrid;

    private int[,] selectionGrid; // integer for flexibility. Could go byte alloc for efficiency. 


    public override void _Process(double delta)
	{
        // raycast here later.. 
        // 
	}

	public void SetTileGraphics(){
        // https://www.boristhebrave.com/permanent/24/06/cr31/stagecast/wang/intro.html
        // the math part. 
	}

    public void DebugDrawGrid(){

        if(selectionGrid == null){
            return;
        }

        for (int y = 0; y < selectionGrid.GetLength(1); y++)
        {
            for (int x = 0; x < selectionGrid.GetLength(0); x++)
            {
                DebugDraw3D.DrawSphere(new Vector3(x, 0, y), 0.1f, Colors.Red);
            }
        }
    }

	public void GenerateGrid(){
        tileGrid = new MeshInstance3D[20, 20];
        selectionGrid = new int[tileGrid.GetLength(0) -2, tileGrid.GetLength(1) -2];
        for (int y = 0; y < selectionGrid.GetLength(1); y++)
        {
            for (int x = 0; x < selectionGrid.GetLength(0); x++)
            {
                selectionGrid[x, y] = 0; // set initial state. 
                DebugDraw3D.DrawSphere(new Vector3(x, 0, y), 0.1f, Colors.Red, 2);
            }
        }
    }
}
