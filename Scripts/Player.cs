using Godot;
using TheGoblinExam.scripts;

namespace TheGoblinExam.Scripts;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void InteractionAreaEnteredEventHandler();
    [Signal]
    public delegate void InteractionAreaExitedEventHandler();
        
    [Export] private float _playerSpeed = 75f;
    [Export] private float _playerSprintBonusSpeed = 25f;
    
    private Stamina _stamina;
    private Inventory _inventory;

    private IInteractable _currentInteractable;

    public override void _Ready()
    {
        _stamina = GetNode<Stamina>("Stamina");
        _inventory = GetNode<Inventory>("Inventory");
    }

    public override void _PhysicsProcess(double delta)
    {
        var direction = GetDirectionFromInput();
        
        var speed = GetMovementSpeed(direction, delta);
        
        MovePlayer(direction, speed);
        
        InteractWithInteractable();
    }

    private void OnInteractionZoneAreaEntered(Node2D node)
    {
        if (node is Collectible collectible && _inventory.CanAddItems())
        {
            _currentInteractable = collectible;
            collectible.Highlight(true);
            EmitSignal(SignalName.InteractionAreaEntered);
        }
    }

    private void OnInteractionZoneAreaExited(Node2D node)
    {
        if (node is Collectible collectible)
        {
            if (_currentInteractable == collectible) 
            {
                _currentInteractable = null;
            }
            
            collectible.Highlight(false);
            EmitSignal(SignalName.InteractionAreaExited);
        }
    }

    private Vector2 GetDirectionFromInput()
    {
        return Input.GetVector("move_left", "move_right", "move_up", "move_down");
    }

    private float GetMovementSpeed(Vector2 direction, double delta)
    {
        var speed = _playerSpeed;
        
        if (Input.IsActionPressed("sprint") && direction != Vector2.Zero && _stamina.CanUseStamina())
        {
            speed += _playerSprintBonusSpeed;
            _stamina.ConsumeStamina(delta);
        }
        
        return speed;
    }

    private void MovePlayer(Vector2 direction, float speed)
    {
        Velocity = direction * speed;
        MoveAndSlide();
    }

    private void InteractWithInteractable()
    {
        if (Input.IsActionJustPressed("interact"))
        {
            if (_currentInteractable is Collectible collectible)
            {
                var item = collectible.CollectibleResource;
                _currentInteractable?.Interact();
                _inventory.AddItemToInventory(item);
                _currentInteractable = null;
            }
        }
    }
}