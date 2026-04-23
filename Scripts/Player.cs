using Godot;

namespace TheGoblinExam.Scripts;

public partial class Player : CharacterBody2D
{
    
    [Export] private float _playerSpeed = 75f;
    
    public override void _PhysicsProcess(double delta)
    {
        var direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        
        Velocity = direction * _playerSpeed;

        MoveAndSlide();
    }
}