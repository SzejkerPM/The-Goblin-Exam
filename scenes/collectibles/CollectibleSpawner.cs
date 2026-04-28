using Godot;
using Godot.Collections;

[GlobalClass]
public partial class CollectibleSpawner : Node2D
{

    [Export]
    private Node MarkersParent { get; set; }
    
    [Export]
    private Node SpawnParent { get; set; }
    
    [Export]
    private Array<CollectibleResource> Collectibles { get; set; } = new();
    
    private PackedScene _collectibleScene = GD.Load<PackedScene>("res://scenes/collectibles/Collectible.tscn");

    public override void _Ready()
    {
        SpawnCollectibles();
    }
    
    public void SpawnCollectibles()
    {
        var markers = getMarkers();
        var collectibles = new Array<CollectibleResource>(Collectibles);

        markers.Shuffle();
        collectibles.Shuffle();

        var count = Mathf.Min(markers.Count, collectibles.Count);
        
        GD.Print($"Spawning {count} collectibles...");

        for (int i = 0; i < count; i++)
        {
            var marker = markers[i];
            var resource = collectibles[i];
            SpawnCollectible(marker, resource);
        }
        
        HideMarkers(markers);
    }

    private void SpawnCollectible(CollectibleMarker marker, CollectibleResource resource)
    {
        GD.Print($"Spawning {resource.Name} {marker.GlobalPosition}");
        
        var collectible = _collectibleScene.Instantiate<Collectible>();
        SpawnParent.AddChild(collectible);
        collectible.GlobalPosition = marker.GlobalPosition;
        collectible.Init(resource);
    }

    private Array<CollectibleMarker> getMarkers()
    {
        var markers = new Array<CollectibleMarker>();
        
        foreach (var child in MarkersParent.GetChildren())
        {
            if (child is CollectibleMarker marker)
            {
                markers.Add(marker);
            }
        }
        return markers;
    }

    private void HideMarkers(Array<CollectibleMarker> markers)
    {
        foreach (var marker in markers)
        {
            marker.Visible = false;
        }
    }
}
