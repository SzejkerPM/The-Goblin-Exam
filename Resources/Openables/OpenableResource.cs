using Godot;
using Godot.Collections;
using TheGoblinExam.Resources.Collectibles;

namespace TheGoblinExam.Resources.Openables;

[GlobalClass]
public partial class OpenableResource : InteractableResource
{
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Array<CollectibleResource> Collectibles { get; private set; } = new();
}