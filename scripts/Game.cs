using Godot;
using TheGoblinExam.scripts;

namespace TheGoblinExam.Scripts;

public partial class Game : Node2D
{
    [Export] private int _goldNeededForNextLevel = 100;

    [Export] private GameUi _gameUi;
    [Export] private Player _player;
    [Export] private Shopkeeper _shopkeeper;

    [Export(PropertyHint.File, "*.tscn")] private string _nextLevelPath;

    private Stamina _stamina;
    private Inventory _inventory;

    private Door _door;

    public override void _Ready()
    {
        _stamina = _player.GetNode<Stamina>("Stamina");
        _inventory = _player.GetNode<Inventory>("Inventory");
        FindDoorOnTree();
        CheckNextLevelPath();
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

        if (_door != null)
        {
            _door.PlayerEnterDoor -= LoadNextLevel;
        }
        else
        {
            GD.PushWarning("Game.cs: Failed to disconnect events between Door and Game (null reference).");
        }
    }

    private void OnGoldChanged(int gold)
    {
        if (gold >= _goldNeededForNextLevel)
        {
            _door.Unlock();
        }
    }

    private void LoadNextLevel()
    {
        GetTree().ChangeSceneToFile(_nextLevelPath);
    }

    private void InitializeConnections()
    {
        _stamina.StaminaChanged += _gameUi.OnStaminaChanged;
        _player.InteractionAreaEntered += _gameUi.ShowEKeyContainer;
        _player.InteractionAreaExited += _gameUi.HideEKeyContainer;
        _inventory.GoldChanged += OnGoldChanged;
        _door.PlayerEnterDoor += LoadNextLevel;
    }

    private void FindDoorOnTree()
    {
        _door = GetTree().GetFirstNodeInGroup("door") as Door;

        if (_door == null)
        {
            throw new System.NullReferenceException("Game.cs: Failed to find Door scene on the Tree!");
        }
    }

    private void CheckNextLevelPath()
    {
        if (string.IsNullOrWhiteSpace(_nextLevelPath))
        {
            throw new System.InvalidOperationException("Game.cs: Next Level Path is missing in Inspector!");
        }
    }
}