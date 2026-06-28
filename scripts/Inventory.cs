using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TheGoblinExam.scripts;

public partial class Inventory : Node
{
    [Signal]
    public delegate void GoldChangedEventHandler(int gold);

    public int Gold { get; private set; }
    public float Weight { get; private set; }

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
        Weight += item.Weight;
        SetUpInventorySprites();
    }

    public void RemoveAllItemsFromInventory()
    {
        RemoveInventorySprites();
        _inventory.Clear();
        Weight = 0f;
    }

    public bool CanAddItems()
    {
        return _inventory.Count < _inventorySprites.Length;
    }

    public bool IsInventoryEmpty()
    {
        return _inventory.Count == 0;
    }
    
    public int CountItemsValue()
    {
        int value = 0;

        foreach (var item in _inventory)
        {
            value += item.Value;
        }

        return value;
    }

    public void AddGoldFromTransaction(int gold)
    {
        Gold += gold;
        EmitSignal(SignalName.GoldChanged, Gold);
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

    private void RemoveInventorySprites()
    {
        for (int i = 0; i < _inventory.Count && i < _inventorySprites.Length; i++)
        {
            _inventorySprites[i].Texture = null;
        }
    }
}