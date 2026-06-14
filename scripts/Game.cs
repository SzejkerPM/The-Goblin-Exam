using Godot;
using TheGoblinExam.scripts;

namespace TheGoblinExam.Scripts;

public partial class Game : Node2D
{
    [Export] private int _goldNeededForNextLevel = 100;

    [Export] private GameUi _gameUi;
    [Export] private Player _player;
    [Export] private Shopkeeper _shopkeeper;

    private Stamina _stamina;
    private Inventory _inventory;

    private bool _doorOpen;

    public override void _Ready()
    {
        _stamina = _player.GetNode<Stamina>("Stamina");
        _inventory = _player.GetNode<Inventory>("Inventory");
        InitializeConnections();
        _shopkeeper.Initialize(_goldNeededForNextLevel);
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

        if (_player != null && _gameUi != null)
        {
            _player.InteractionAreaEntered -= _gameUi.ShowEKeyContainer;
            _player.InteractionAreaExited -= _gameUi.HideEKeyContainer;
        }
        else
        {
            GD.PushWarning("Game.cs: Failed to disconnect events between Player and GameUI (null reference).");
        }

        if (_inventory != null)
        {
            _inventory.GoldChanged -= OnGoldChanged;
        }
        else
        {
            GD.PushWarning("Game.cs: Failed to disconnect events between Inventory and Game (null reference).");
        }
    }

    private void OnGoldChanged(int gold)
    {
        if (gold >= _goldNeededForNextLevel)
        {
            _doorOpen = true;
        }
    }

    private void InitializeConnections()
    {
        _stamina.StaminaChanged += _gameUi.OnStaminaChanged;
        _player.InteractionAreaEntered += _gameUi.ShowEKeyContainer;
        _player.InteractionAreaExited += _gameUi.HideEKeyContainer;
        _inventory.GoldChanged += OnGoldChanged;
    }
}