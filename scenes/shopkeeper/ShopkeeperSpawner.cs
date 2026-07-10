using Godot;
using Godot.Collections;
using TheGoblinExam.scripts;

namespace TheGoblinExam.scenes.shopkeeper;

public partial class ShopkeeperSpawner : Node2D
{
    [Export] private Node SpawnParent { get; set; }
    [Export] private PackedScene _shopkeeperScene;
    
    private Array<ShopkeeperMarker> _spawnPositions = [];

    [Signal]
    public delegate void ShopkeeperSpawnedEventHandler(Shopkeeper shopkeeper);

    public override void _Ready()
    {
        foreach (var child in GetChildren())
        {
            if (child is ShopkeeperMarker marker)
            {
                _spawnPositions.Add(marker);
            }
        }
    }

    public void SpawnShopkeeper(int goldNeeded)
    {
        GD.Print($"Spawning shopkeeper with {goldNeeded} gold...");
        
        if (_spawnPositions.Count == 0)
        {
            GD.PrintErr("ShopkeeperSpawner: No ShopkeeperMarker children found!");
            return;
        }

        var random = GD.RandRange(0, _spawnPositions.Count - 1);
        var spawnPoint = _spawnPositions[random];

        var shopkeeper = _shopkeeperScene.Instantiate<Shopkeeper>();
        
        SpawnParent.AddChild(shopkeeper);
        shopkeeper.GlobalPosition = spawnPoint.GlobalPosition;
        shopkeeper.Initialize(goldNeeded);

        GD.Print($"Shopkeeper spawned at {spawnPoint.Name} ({spawnPoint.GlobalPosition})");
    }
}
