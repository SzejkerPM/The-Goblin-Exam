using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TheGoblinExam.scripts;

public partial class Inventory : Node
{
    private Sprite2D[] _inventorySprites;
    private readonly List<CollectibleResource> _inventory = [];

    public override void _Ready()
    {
        _inventorySprites = GetNode("%InventorySprites")
            .GetChildren()
            .OfType<Sprite2D>()
            .ToArray();

        SetUpInventorySprites();
    }

    public void AddItemToInventory(CollectibleResource item)
    {
        _inventory.Add(item);
        SetUpInventorySprites();
    }

    public bool CanAddItems()
    {
        return _inventory.Count < _inventorySprites.Length;
    }

    private void SetUpInventorySprites()
    {
        for (int i = 0; i < _inventory.Count && i < _inventorySprites.Length; i++)
        {
            if (_inventorySprites[i].Texture != _inventory[i].Texture)
            {
                _inventorySprites[i].Texture = _inventory[i].Texture;
            }
        }
    }
}