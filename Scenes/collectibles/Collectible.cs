using Godot;
using TheGoblinExam.scripts;

[GlobalClass]
public partial class Collectible : Node2D, IInteractable
{
	public CollectibleResource CollectibleResource { get; private set; }
	
	private Sprite2D _collectibleSprite;
	
	public void Init(CollectibleResource resource)
	{
		CollectibleResource = resource;
		_collectibleSprite = GetNode<Sprite2D>("Sprite");
		_collectibleSprite.Texture = resource.Texture;
	}

	public void Interact()
	{
		QueueFree();
	}

	public void Highlight(bool enabled)
	{
		if (enabled)
		{
			_collectibleSprite.SelfModulate = new Color(1.5f, 1.5f, 1.5f);
		}
		else
		{
			_collectibleSprite.SelfModulate = Colors.White;
		}
	}
}