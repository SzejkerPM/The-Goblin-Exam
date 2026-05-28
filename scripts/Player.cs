using Godot;
using TheGoblinExam.scripts;

namespace TheGoblinExam.Scripts;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void InteractionAreaEnteredEventHandler(string interactionPrompt);

    [Signal]
    public delegate void InteractionAreaExitedEventHandler();

    [Export] private float _playerSpeed = 75f;
    [Export] private float _playerSprintBonusSpeed = 25f;
    [Export] private float _sprintAnimationScale = 1.35f;

    private Stamina _stamina;
    private Inventory _inventory;
    private AnimatedSprite2D _animatedSprite;

    private IInteractable _currentInteractable;

    public override void _Ready()
    {
        _stamina = GetNode<Stamina>("Stamina");
        _inventory = GetNode<Inventory>("Inventory");
        _animatedSprite = GetNode<AnimatedSprite2D>("%AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        var direction = GetDirectionFromInput();

        var speed = GetMovementSpeed(direction, delta);

        MovePlayer(direction, speed);

        UpdateAnimation(direction);

        InteractWithInteractable();
    }

    private void OnInteractionZoneAreaEntered(Node2D node)
    {
        if (node is IInteractable interactable)
        {
            if (node is Collectible && !_inventory.CanAddItems()) return;
            if (node is Shopkeeper && _inventory.IsInventoryEmpty()) return;

            _currentInteractable = interactable;
            interactable.Highlight(true);
            EmitSignal(SignalName.InteractionAreaEntered, interactable.InteractionPrompt);
        }
    }

    private void OnInteractionZoneAreaExited(Node2D node)
    {
        if (node is IInteractable interactable)
        {
            if (_currentInteractable == interactable)
            {
                _currentInteractable = null;
            }

            interactable.Highlight(false);
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

        _animatedSprite.SpeedScale = 1.0f;

        if (Input.IsActionPressed("sprint") && direction != Vector2.Zero && _stamina.CanUseStamina())
        {
            speed += _playerSprintBonusSpeed;
            _stamina.ConsumeStamina(delta);
            _animatedSprite.SpeedScale = _sprintAnimationScale;
        }

        return speed;
    }

    private void MovePlayer(Vector2 direction, float speed)
    {
        Velocity = direction * speed;
        MoveAndSlide();
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (direction.X < 0)
        {
            _animatedSprite.FlipH = true;
        }
        else if (direction.X > 0)
        {
            _animatedSprite.FlipH = false;
        }

        if (direction == Vector2.Zero)
        {
            _animatedSprite.Play("idle");
        }
        else
        {
            _animatedSprite.Play("running");
        }
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
            else if (_currentInteractable is Shopkeeper shopkeeper)
            {
                var itemsValue = _inventory.CountItemsValue();

                if (shopkeeper.Gold >= itemsValue)
                {
                    shopkeeper.ReduceGold(itemsValue);
                    _inventory.RemoveAllItemsFromInventory();
                    _inventory.AddGoldFromTransaction(itemsValue);
                    shopkeeper.UpdatePlayerGold(_inventory.Gold);
                    _currentInteractable?.Interact();
                }
            }
        }
    }
}