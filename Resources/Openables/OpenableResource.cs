using Godot;
using Godot.Collections;

namespace TheGoblinExam.Resources.Openables;

[GlobalClass]
public partial class OpenableResource : InteractableResource
{
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Array<Collectibles.CollectibleResource> Collectibles { get; private set; } = new();
}