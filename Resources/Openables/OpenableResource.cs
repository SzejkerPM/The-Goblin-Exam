using Godot;
using Godot.Collections;

[GlobalClass]
public partial class OpenableResource : InteractableResource
{
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Array<CollectibleResource> Collectibles { get; private set; } = new();
}
