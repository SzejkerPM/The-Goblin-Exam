using Godot;

namespace TheGoblinExam.Scripts;

public partial class Player : CharacterBody2D
{
    
    [Export] private float _playerSpeed = 75f;
    [Export] private float _playerSprintSpeed = 25f;
    
    private Stamina _stamina;

    public override void _Ready()
    {
        _stamina = GetNode<Stamina>("Stamina");
    }

    public override void _PhysicsProcess(double delta)
    {
        var direction = GetDirectionFromInput();
        
        var speed = GetMovementSpeed(direction, delta);
        
        MovePlayer(direction, speed);
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
            speed += _playerSprintSpeed;
            _stamina.ConsumeStamina(delta);
        }
        
        return speed;
    }

    private void MovePlayer(Vector2 direction, float speed)
    {
        Velocity = direction * speed;
        MoveAndSlide();
    }
}