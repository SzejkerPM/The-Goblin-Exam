using Godot;

[GlobalClass]
public partial class Collectible : Node2D
{
	private CollectibleResource collectibleResource;
	
	public void Init(CollectibleResource resource)
	{
		collectibleResource = resource;
		GetNode<Sprite2D>("Sprite").Texture = resource.Texture;
	}

}
