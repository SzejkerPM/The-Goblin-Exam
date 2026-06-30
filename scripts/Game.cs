using System;
using System.Collections.Generic;
using Godot;
using TheGoblinExam.scripts;

namespace TheGoblinExam.Scripts;

public partial class Game : Node2D
{
    [Export] private int _goldNeededForNextLevel = 100;

    [Export] private GameUi _gameUi;
    [Export] private Player _player;
    [Export] private ShopkeeperSpawner _shopkeeperSpawner;
    [Export] private CollectibleSpawner _collectibleSpawner;

    [Export(PropertyHint.File, "*.tscn")] private string _nextLevelPath;

    private Stamina _stamina;
    private Inventory _inventory;
    private Shopkeeper _shopkeeper;
    private Door _door;

    private bool _isGameOver;
    private readonly List<EnemyAi> _enemies = new();

    public override void _Ready()
    {
        if (!ValidateReferences())
            return;

        _stamina = _player.GetNodeOrNull<Stamina>("Stamina");
        _inventory = _player.GetNodeOrNull<Inventory>("Inventory");

        if (_stamina == null || _inventory == null)
        {
            GD.PushError("Game.cs: Player is missing Stamina or Inventory child node.");
            return;
        }

        _gameUi.SetPlayer(_player);
        _collectibleSpawner.GameUi = _gameUi;

        FindDoorOnTree();
        CheckNextLevelPath();
        InitializeConnections();
        ConnectEnemies();

        _shopkeeperSpawner.ShopkeeperSpawned += OnShopkeeperSpawned;
        _shopkeeperSpawner.SpawnShopkeeper(_goldNeededForNextLevel);
    }

    public override void _ExitTree()
    {
        if (_shopkeeperSpawner != null)
            _shopkeeperSpawner.ShopkeeperSpawned -= OnShopkeeperSpawned;

        if (_shopkeeper != null && GodotObject.IsInstanceValid(_shopkeeper) && _gameUi != null)
            _shopkeeper.MinigameRequested -= _gameUi.ShowMinigame;

        if (_stamina != null && _gameUi != null)
            _stamina.StaminaChanged -= _gameUi.OnStaminaChanged;

        if (_player != null && _gameUi != null)
        {
            _player.InteractionAreaEntered -= _gameUi.ShowEKeyContainer;
            _player.InteractionAreaExited -= _gameUi.HideEKeyContainer;
        }

        if (_inventory != null)
            _inventory.GoldChanged -= OnGoldChanged;

        if (_door != null && GodotObject.IsInstanceValid(_door))
            _door.PlayerEnterDoor -= LoadNextLevel;

        foreach (var enemy in _enemies)
        {
            if (enemy != null && GodotObject.IsInstanceValid(enemy))
                enemy.PlayerCaught -= OnPlayerCaught;
        }

        _enemies.Clear();
    }

    private bool ValidateReferences()
    {
        if (_gameUi == null)
        {
            GD.PushError("Game.cs: _gameUi is not assigned in the Inspector.");
            return false;
        }

        if (_player == null)
        {
            GD.PushError("Game.cs: _player is not assigned in the Inspector.");
            return false;
        }

        if (_shopkeeperSpawner == null)
        {
            GD.PushError("Game.cs: _shopkeeperSpawner is not assigned in the Inspector.");
            return false;
        }

        if (_collectibleSpawner == null)
        {
            GD.PushError("Game.cs: _collectibleSpawner is not assigned in the Inspector.");
            return false;
        }

        return true;
    }

    private void OnShopkeeperSpawned(Shopkeeper shopkeeper)
    {
        if (shopkeeper == null)
        {
            GD.PushError("Game.cs: Spawned shopkeeper is null.");
            return;
        }

        if (_shopkeeper != null && GodotObject.IsInstanceValid(_shopkeeper) && _gameUi != null)
            _shopkeeper.MinigameRequested -= _gameUi.ShowMinigame;

        _shopkeeper = shopkeeper;
        _shopkeeper.Initialize(_goldNeededForNextLevel);

        if (_gameUi != null)
            _shopkeeper.MinigameRequested += _gameUi.ShowMinigame;
    }

    private void OnGoldChanged(int gold)
    {
        if (_door == null || !GodotObject.IsInstanceValid(_door))
            return;

        if (gold >= _goldNeededForNextLevel)
            _door.Unlock();
    }

    private void LoadNextLevel()
    {
        if (string.IsNullOrWhiteSpace(_nextLevelPath))
        {
            GD.PushError("Game.cs: Cannot load next level because _nextLevelPath is empty.");
            return;
        }

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
            throw new NullReferenceException("Game.cs: Failed to find Door scene in group 'door'.");
    }

    private void CheckNextLevelPath()
    {
        if (string.IsNullOrWhiteSpace(_nextLevelPath))
            throw new InvalidOperationException("Game.cs: Next Level Path is missing in Inspector.");
    }

    private void ConnectEnemies()
    {
        foreach (var child in GetTree().GetNodesInGroup("enemy"))
        {
            if (child is EnemyAi enemy)
            {
                enemy.PlayerCaught += OnPlayerCaught;
                _enemies.Add(enemy);
            }
        }
    }

    private void OnPlayerCaught()
    {
        if (_isGameOver)
            return;

        _isGameOver = true;
        GetTree().Paused = true;
        _gameUi?.ShowDeathScreen();
    }
}