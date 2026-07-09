using Godot;
using Godot.Collections;

namespace TheGoblinExam.scripts;

public partial class EnemyAi : CharacterBody2D
{
    [Signal]
    public delegate void PlayerCaughtEventHandler();

    [Export] private float _speed = 40f;
    [Export] private float _chaseSpeed = 65f;
    [Export] private float _markerReachDistance = 6f;

    [Export] private float _sightMemoryTime = 0.20f;
    [Export] private float _loseSightTime = 1.20f;
    [Export] private float _maxChaseTime = 5.0f;
    [Export] private float _targetRefreshDistance = 8.0f;

    [Export(PropertyHint.Layers2DPhysics)] private uint _sightCollisionMask;
    [Export] private Marker2D[] _points;

    private NavigationAgent2D _agent;
    private AnimatedSprite2D _sprite;
    private Node2D _facingPivot;
    private Area2D _visionArea;
    private Timer _patrolTimer;
    private Timer _loseSightTimer;
    private Timer _maxChaseTimer;

    private int _currentIndex;
    private Node2D _player;
    private bool _playerInRange;
    private bool _hasConfirmedSight;
    private bool _isWaitingAtPoint;
    private bool _isSearchingLastSeenPosition;
    private bool _hasReachedMaxChaseTime;
    private bool _hasCaughtPlayer;

    private float _timeSinceLastVisible = float.MaxValue;

    private Vector2 _facingDirection = Vector2.Right;
    private Vector2 _lastSeenPlayerPosition = Vector2.Zero;
    private Vector2 _currentTargetPosition = Vector2.Zero;

    public override void _Ready()
    {
        MotionMode = MotionModeEnum.Floating;

        _agent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        _sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        _facingPivot = GetNodeOrNull<Node2D>("FacingPivot");
        _visionArea = GetNodeOrNull<Area2D>("FacingPivot/VisionArea");
        _patrolTimer = GetNodeOrNull<Timer>("PatrolTimer");
        _loseSightTimer = GetNodeOrNull<Timer>("LoseSightTimer");
        _maxChaseTimer = GetNodeOrNull<Timer>("MaxChaseTimer");

        if (_agent == null || _facingPivot == null || _visionArea == null || _patrolTimer == null ||
            _loseSightTimer == null || _maxChaseTimer == null)
        {
            GD.PrintErr("EnemyAi scene is missing required nodes.");
            return;
        }

        if (_points == null || _points.Length == 0)
        {
            GD.PrintErr("No patrol points assigned.");
            return;
        }

        foreach (var point in _points)
        {
            if (point == null)
            {
                GD.PrintErr("One of the patrol points is null.");
                return;
            }
        }

        _agent.PathDesiredDistance = 6.0f;
        _agent.TargetDesiredDistance = 6.0f;

        _patrolTimer.OneShot = true;

        _loseSightTimer.OneShot = true;
        _loseSightTimer.WaitTime = _loseSightTime;

        _maxChaseTimer.OneShot = true;
        _maxChaseTimer.WaitTime = _maxChaseTime;

        _visionArea.BodyEntered += OnVisionBodyEntered;
        _visionArea.BodyExited += OnVisionBodyExited;
        _patrolTimer.Timeout += OnPatrolTimerTimeout;
        _loseSightTimer.Timeout += OnLoseSightTimerTimeout;
        _maxChaseTimer.Timeout += OnMaxChaseTimerTimeout;

        _currentIndex = GetClosestPatrolPointIndex();
        SetAgentTarget(_points[_currentIndex].GlobalPosition);
        UpdateFacingVisuals();
    }

    public override void _ExitTree()
    {
        if (_visionArea != null)
        {
            _visionArea.BodyEntered -= OnVisionBodyEntered;
            _visionArea.BodyExited -= OnVisionBodyExited;
        }

        if (_patrolTimer != null)
            _patrolTimer.Timeout -= OnPatrolTimerTimeout;

        if (_loseSightTimer != null)
            _loseSightTimer.Timeout -= OnLoseSightTimerTimeout;

        if (_maxChaseTimer != null)
            _maxChaseTimer.Timeout -= OnMaxChaseTimerTimeout;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_hasCaughtPlayer)
            return;

        if (_agent == null || _points == null || _points.Length == 0)
            return;

        if (NavigationServer2D.MapGetIterationId(_agent.GetNavigationMap()) == 0)
            return;

        if (_player != null && !IsInstanceValid(_player))
            ClearPlayerTracking();

        var playerVisible =
            !_hasReachedMaxChaseTime &&
            _player != null &&
            _playerInRange &&
            HasLineOfSightToPlayer();

        if (playerVisible)
        {
            _hasConfirmedSight = true;
            _timeSinceLastVisible = 0f;
            _lastSeenPlayerPosition = GetReachableSearchPosition(_player.GlobalPosition);
            _loseSightTimer.Stop();
            ChaseLastSeenPosition();
            return;
        }

        _timeSinceLastVisible += (float)delta;

        var keepVisualContact =
            _hasConfirmedSight &&
            _timeSinceLastVisible <= _sightMemoryTime;

        if (keepVisualContact)
        {
            ChaseLastSeenPosition();
            return;
        }

        if (_hasConfirmedSight && _player != null)
        {
            if (_loseSightTimer.IsStopped() && _loseSightTimer.IsInsideTree())
                _loseSightTimer.Start();

            _isSearchingLastSeenPosition = true;
        }

        if (_isSearchingLastSeenPosition)
        {
            SearchLastSeenPosition();
            return;
        }

        Patrol();
    }

    private void CheckPlayerCollision()
    {
        if (_hasCaughtPlayer)
            return;

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);
            var collider = collision.GetCollider() as Node;

            if (collider != null && collider.IsInGroup("player"))
            {
                _hasCaughtPlayer = true;
                Velocity = Vector2.Zero;
                EmitSignal(SignalName.PlayerCaught);
                return;
            }
        }
    }

    private void ChaseLastSeenPosition()
    {
        _isWaitingAtPoint = false;
        _isSearchingLastSeenPosition = true;
        _patrolTimer.Stop();
        _loseSightTimer.Stop();

        if (_maxChaseTimer.IsStopped() && _maxChaseTimer.IsInsideTree())
            _maxChaseTimer.Start();

        if (_currentTargetPosition.DistanceTo(_lastSeenPlayerPosition) >= _targetRefreshDistance)
            SetAgentTarget(_lastSeenPlayerPosition);

        MoveToCurrentTarget(_chaseSpeed);
    }

    private void SearchLastSeenPosition()
    {
        _isWaitingAtPoint = false;
        _patrolTimer.Stop();

        if (_maxChaseTimer.IsStopped() && !_hasReachedMaxChaseTime && _maxChaseTimer.IsInsideTree())
            _maxChaseTimer.Start();

        SetAgentTarget(_lastSeenPlayerPosition);

        if (_agent.IsNavigationFinished() || GlobalPosition.DistanceTo(_lastSeenPlayerPosition) <= _markerReachDistance)
        {
            EndSearchAndResumePatrol();
            return;
        }

        MoveToCurrentTarget(_chaseSpeed);
    }

    private void Patrol()
    {
        if (_isWaitingAtPoint)
        {
            Velocity = Vector2.Zero;
            UpdateFacingVisuals();
            MoveAndSlide();
            CheckPlayerCollision();
            return;
        }

        var patrolTarget = _points[_currentIndex].GlobalPosition;
        SetAgentTarget(patrolTarget);

        if (_agent.IsNavigationFinished() || GlobalPosition.DistanceTo(patrolTarget) <= _markerReachDistance)
        {
            StartPatrolPause();
            return;
        }

        MoveToCurrentTarget(_speed);
    }

    private void StartPatrolPause()
    {
        if (_isWaitingAtPoint)
            return;

        _isWaitingAtPoint = true;
        Velocity = Vector2.Zero;
        UpdateFacingVisuals();
        MoveAndSlide();
        CheckPlayerCollision();

        if (_patrolTimer.IsInsideTree())
            _patrolTimer.Start();
    }

    private void EndSearchAndResumePatrol()
    {
        _isSearchingLastSeenPosition = false;
        _hasConfirmedSight = false;
        _timeSinceLastVisible = float.MaxValue;
        _loseSightTimer.Stop();
        _maxChaseTimer.Stop();

        _currentIndex = GetClosestPatrolPointIndex();
        SetAgentTarget(_points[_currentIndex].GlobalPosition);

        StartPatrolPause();
    }

    private void ClearPlayerTracking()
    {
        _player = null;
        _playerInRange = false;
        _hasConfirmedSight = false;
        _timeSinceLastVisible = float.MaxValue;
    }

    private int GetClosestPatrolPointIndex()
    {
        if (_points == null || _points.Length == 0)
            return 0;

        var closestIndex = 0;
        var closestDistanceSquared = GlobalPosition.DistanceSquaredTo(_points[0].GlobalPosition);

        for (var i = 1; i < _points.Length; i++)
        {
            var distanceSquared = GlobalPosition.DistanceSquaredTo(_points[i].GlobalPosition);

            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void OnPatrolTimerTimeout()
    {
        _isWaitingAtPoint = false;
        _hasReachedMaxChaseTime = false;
        _currentIndex = (_currentIndex + 1) % _points.Length;
        SetAgentTarget(_points[_currentIndex].GlobalPosition);
    }

    private void OnLoseSightTimerTimeout()
    {
        _isSearchingLastSeenPosition = false;
        _hasConfirmedSight = false;
        _timeSinceLastVisible = float.MaxValue;
        _maxChaseTimer.Stop();

        _currentIndex = GetClosestPatrolPointIndex();
        SetAgentTarget(_points[_currentIndex].GlobalPosition);

        StartPatrolPause();
    }

    private void OnMaxChaseTimerTimeout()
    {
        _hasReachedMaxChaseTime = true;
        _isSearchingLastSeenPosition = false;
        _hasConfirmedSight = false;
        _timeSinceLastVisible = float.MaxValue;
        _loseSightTimer.Stop();

        _currentIndex = GetClosestPatrolPointIndex();
        SetAgentTarget(_points[_currentIndex].GlobalPosition);

        StartPatrolPause();
    }

    private void MoveToCurrentTarget(float maxSpeed)
    {
        if (_agent.IsNavigationFinished())
        {
            Velocity = Vector2.Zero;
            UpdateFacingVisuals();
            MoveAndSlide();
            CheckPlayerCollision();
            return;
        }

        var nextPoint = _agent.GetNextPathPosition();
        var direction = GlobalPosition.DirectionTo(nextPoint);
        Velocity = direction * maxSpeed;

        if (Velocity.Length() > 0.1f)
            _facingDirection = Velocity.Normalized();

        UpdateFacingVisuals();
        MoveAndSlide();
        CheckPlayerCollision();
    }

    private void UpdateFacingVisuals()
    {
        if (_sprite != null)
        {
            if (Velocity.X < 0)
                _sprite.FlipH = true;
            else if (Velocity.X > 0)
                _sprite.FlipH = false;

            var animationName = Velocity.Length() > 0.1f ? "walk" : "idle";
            if (_sprite.Animation != animationName)
                _sprite.Play(animationName);
        }

        if (_facingPivot != null)
            _facingPivot.Rotation = _facingDirection.Angle();
    }

    private void SetAgentTarget(Vector2 target)
    {
        _currentTargetPosition = target;
        _agent.TargetPosition = target;
    }

    private Vector2 GetReachableSearchPosition(Vector2 desiredPosition)
    {
        return NavigationServer2D.MapGetClosestPoint(_agent.GetNavigationMap(), desiredPosition);
    }

    private bool HasLineOfSightToPlayer()
    {
        if (_player == null || !IsInstanceValid(_player))
            return false;

        return HasClearRayTo(_player.GlobalPosition);
    }

    private bool HasClearRayTo(Vector2 targetPoint)
    {
        var from = GetSightOrigin();

        var query = PhysicsRayQueryParameters2D.Create(from, targetPoint, _sightCollisionMask);
        query.CollideWithBodies = true;
        query.CollideWithAreas = false;
        query.HitFromInside = false;
        query.Exclude = new Array<Rid> { GetRid() };

        var result = GetWorld2D().DirectSpaceState.IntersectRay(query);

        if (result.Count == 0 || !result.ContainsKey("collider"))
            return false;

        var collider = result["collider"].AsGodotObject();
        return collider == _player;
    }

    private Vector2 GetSightOrigin()
    {
        return _facingPivot != null ? _facingPivot.GlobalPosition : GlobalPosition;
    }

    private void OnVisionBodyEntered(Node2D body)
    {
        if (_hasReachedMaxChaseTime || _hasCaughtPlayer)
            return;

        if (!body.IsInGroup("player"))
            return;

        _player = body;
        _playerInRange = true;
    }

    private void OnVisionBodyExited(Node2D body)
    {
        if (!IsInsideTree())
            return;

        if (_loseSightTimer == null || !_loseSightTimer.IsInsideTree())
            return;

        if (body != _player)
            return;

        _playerInRange = false;

        if (_hasConfirmedSight)
        {
            _lastSeenPlayerPosition = GetReachableSearchPosition(body.GlobalPosition);
            _isSearchingLastSeenPosition = true;

            if (_loseSightTimer.IsStopped())
                _loseSightTimer.Start();
        }

        _player = null;
    }
}