using Godot;

namespace TheGoblinExam.scripts;

public partial class Door : Area2D, IInteractable
{
    [Signal]
    public delegate void PlayerEnterDoorEventHandler();

    public string InteractionPrompt => "Leave";

    private Sprite2D _sprite2D;
    private CollisionShape2D _interactionZone;
    private bool _isUnlocked;

    public override void _Ready()
    {
        _sprite2D = GetNode<Sprite2D>("Sprite2D");
        _interactionZone = GetNode<CollisionShape2D>("InteractionZone");
        _interactionZone.Disabled = true;
    }

    public void Unlock()
    {
        _isUnlocked = true;
        _interactionZone.Disabled = false;
    }

    public void Interact()
    {
        if (_isUnlocked)
        {
            EmitSignal(SignalName.PlayerEnterDoor);
        }
    }

    public void Highlight(bool enabled)
    {
        if (enabled)
        {
            _sprite2D.SelfModulate = new Color(1.5f, 1.5f, 1.5f);
        }
        else
        {
            _sprite2D.SelfModulate = Colors.White;
        }
    }
}