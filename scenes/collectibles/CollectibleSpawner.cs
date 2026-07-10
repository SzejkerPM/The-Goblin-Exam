using Godot;
using Godot.Collections;
using TheGoblinExam.Resources;
using TheGoblinExam.Resources.Collectibles;
using TheGoblinExam.Resources.Openables;
using TheGoblinExam.Scripts;

namespace TheGoblinExam.scenes.collectibles;

[GlobalClass]
public partial class CollectibleSpawner : Node2D
{
    [Export] private Node MarkersParent { get; set; }

    [Export] private Node SpawnParent { get; set; }

    [Export] public GameUi GameUi { get; set; }

    [Export] private Array<InteractableResource> Collectibles { get; set; } = new();

    private PackedScene _collectibleScene = GD.Load<PackedScene>("res://scenes/collectibles/Collectible.tscn");
    private PackedScene _openableScene = GD.Load<PackedScene>("res://scenes/collectibles/Openable.tscn");

    public override void _Ready()
    {
        SpawnCollectibles();
    }

    private void SpawnCollectibles()
    {
        var markers = GetMarkers();
        var interactables = new Array<InteractableResource>(Collectibles);

        markers.Shuffle();
        interactables.Shuffle();

        var count = Mathf.Min(markers.Count, interactables.Count);

        GD.Print($"Spawning {count} interactables...");

        for (int i = 0; i < count; i++)
        {
            var marker = markers[i];
            var resource = interactables[i];
            SpawnInteractable(marker, resource);
        }

        HideMarkers(markers);
    }

    private Collectible SpawnCollectible(Vector2 globalPosition, CollectibleResource resource)
    {
        GD.Print($"Spawning collectible {resource.Name} at {globalPosition}");
        var collectible = _collectibleScene.Instantiate<Collectible>();
        SpawnParent.AddChild(collectible);
        collectible.GlobalPosition = globalPosition;
        collectible.Init(resource);
        return collectible;
    }

    private void SpawnOpenable(Vector2 globalPosition, OpenableResource resource)
    {
        GD.Print($"Spawning openable at {globalPosition}");
        var openable = _openableScene.Instantiate<Openable>();
        SpawnParent.AddChild(openable);
        openable.GlobalPosition = globalPosition;
        openable.Init(resource);
        openable.Opened += OnOpenableOpened;
        openable.MinigameRequested += GameUi.ShowMinigameForOpenable;
        
    }

    private void OnOpenableOpened(Openable openable)
    {
        var resource = openable.GetResource();

        var random = new RandomNumberGenerator();
        random.Randomize();

        foreach (var collectibleResource in resource.Collectibles)
        {
            var collectible = SpawnCollectible(openable.GlobalPosition, collectibleResource);
            var forceDirection = Vector2.Up.Rotated(random.RandfRange(-Mathf.Pi / 2, Mathf.Pi / 2));
            var forceMagnitude = random.RandfRange(70f, 150f);

            collectible.ApplyImpulse(forceDirection * forceMagnitude);
        }

        openable.Opened -= OnOpenableOpened;
        openable.MinigameRequested -= GameUi.ShowMinigameForOpenable;
    }

    private void SpawnInteractable(CollectibleMarker marker, InteractableResource resource)
    {
        if (resource is CollectibleResource collectibleResource)
        {
            SpawnCollectible(marker.GlobalPosition, collectibleResource);
        }
        else if (resource is OpenableResource openableResource)
        {
            SpawnOpenable(marker.GlobalPosition, openableResource);
        }
    }

    private Array<CollectibleMarker> GetMarkers()
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