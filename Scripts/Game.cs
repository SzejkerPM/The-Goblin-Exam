using Godot;

namespace TheGoblinExam.Scripts;

public partial class Game : Node2D
{
    [Export] private Stamina _stamina;
    [Export] private GameUi _gameUi;

    public override void _Ready()
    {
        InitializeConnections();
    }

    public override void _ExitTree()
    {
        if (_stamina != null && _gameUi != null)
        {
            _stamina.StaminaChanged -= _gameUi.OnStaminaChanged;
        }
        else
        {
            GD.PushWarning("Game.cs: Failed to disconnect events between Stamina and GameUI (null reference).");
        }
    }

    private void InitializeConnections()
    {
        _stamina.StaminaChanged += _gameUi.OnStaminaChanged;
    }
}