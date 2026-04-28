using Godot;

[GlobalClass]
public partial class CollectibleResource : Resource
{
    [Export] public Texture2D Texture { get; private set; }
    [Export] public string Name { get; private set; }
    [Export] public int Value { get; private set; }
    [Export] public float Weight { get; private set; }
}
