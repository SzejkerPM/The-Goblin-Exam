using Godot;
using TheGoblinExam.scripts;

[GlobalClass]
public partial class Openable : Area2D, IInteractable
{
    [Signal]
    public delegate void OpenedEventHandler(Openable openable);
    
    public string InteractionPrompt => "Open";

    private OpenableResource _resource;
    private Sprite2D _sprite;

    public void Init(OpenableResource resource)
    {
        _resource = resource;
        _sprite = GetNode<Sprite2D>("Sprite");
        _sprite.Texture = resource.Texture;
        
    }

    public OpenableResource GetResource() => _resource;

    public void Interact()
    {
        GD.Print("Opening...");
        EmitSignal(SignalName.Opened, this);
        QueueFree();
    }

    public void Highlight(bool enabled)
    {
        if (_sprite == null) return;
        
        if (enabled)
        {
            _sprite.SelfModulate = new Color(1.5f, 1.5f, 1.5f);
        }
        else
        {
            _sprite.SelfModulate = Colors.White;
        }
    }
}
