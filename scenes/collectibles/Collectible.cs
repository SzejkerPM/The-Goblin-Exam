using Godot;
using TheGoblinExam.Resources.Collectibles;
using TheGoblinExam.scripts;

namespace TheGoblinExam.scenes.collectibles;

[GlobalClass]
public partial class Collectible : CharacterBody2D, IInteractable
{
	public CollectibleResource CollectibleResource { get; private set; }
	public string InteractionPrompt => "Pick up";
	
	private Sprite2D _collectibleSprite;
	private float _friction = 0.95f;
	
	public override void _PhysicsProcess(double delta)
	{
		if (Velocity.Length() > 0.1f)
		{
			MoveAndSlide();
			Velocity *= _friction;
		}
	}

	public void Init(CollectibleResource resource)
	{
		CollectibleResource = resource;
		_collectibleSprite = GetNode<Sprite2D>("Sprite");
		_collectibleSprite.Texture = resource.Texture;
	}

	public void ApplyImpulse(Vector2 impulse)
	{
		Velocity = impulse;
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