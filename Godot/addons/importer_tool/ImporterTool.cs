using Godot;
using System;

[Tool]
public partial class ImporterTool : EditorImportPlugin
{
	public override string _GetImporterName() => "my.importer";
	public override string _GetVisibleName() => "My Importer";
	public override string[] _GetRecognizedExtensions() => new[] { "glb", "gltf" };
	public override string _GetSaveExtension() => "scn";
	public override string _GetResourceType() => "PackedScene";
	public override int _GetImportOrder() => 100;

	/*

	public override int _Import(string pSourceFile, string pSavePath, Dictionary pOptions, List<string> rPlatforms, Dictionary rImportOptions)
	{
		var scene = GD.Load<PackedScene>(pSourceFile);
		if (scene == null) return (int)GodotObject.CallError.Unconfigured;

		ApplyGltfExtrasRecursive(scene.Instantiate(), scene.Instantiate());

		var packedScene = new PackedScene();
		packedScene.Pack(scene.Instantiate());
		ResourceSaver.Save(packedScene, pSavePath + ".scn");

		return (int)GodotObject.CallError.Ok;
	}

	private void ApplyGltfExtrasRecursive(Node node, Node root)
	{
		foreach (var child in node.GetChildren())
		{
			ApplyGltfExtrasRecursive(child, root);
		}

		var extras = node.GetMeta("gltf_extras") as Godot.Collections.Dictionary;
		if (extras != null)
		{
			GD.Print($"Node: {node.Name}, Extras: {extras}");
		}
	}

	*/
	
}
