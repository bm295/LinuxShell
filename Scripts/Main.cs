using System;
using Godot;

namespace ShadowSwitch;

public partial class Main : Node2D
{
    private const float MoveSpeed = 220f;
    private const float JumpVelocity = -420f;
    private const float Gravity = 980f;
    private const float ClimbSpeed = 150f;
    private const float DarkTimeLimit = 12f;

    private CharacterBody2D _playerLight = null!;
    private CharacterBody2D _playerDark = null!;
    private Area2D _lightPlate = null!;
    private Area2D _darkSwitch = null!;
    private StaticBody2D _darkBridge = null!;
    private StaticBody2D _darkDoor = null!;
    private Area2D _goalLight = null!;
    private Area2D _goalDark = null!;
    private Label _statusLabel = null!;
    private Label _timerLabel = null!;

    private bool _isDarkActive;
    private bool _darkDoorOpen;
    private float _darkTimer = DarkTimeLimit;

    public override void _Ready()
    {
        BuildScene();
        UpdateWorldState();
        UpdateHud();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("switch_world"))
        {
            _isDarkActive = !_isDarkActive;
            UpdateWorldState();
        }

        if (_isDarkActive)
        {
            _darkTimer = Mathf.Max(0f, _darkTimer - (float)delta);
            if (_darkTimer <= 0f)
            {
                _statusLabel.Text = "Bong toi da nuot ban! Nhan R de choi lai.";
            }
        }
        else
        {
            _darkTimer = Mathf.Min(DarkTimeLimit, _darkTimer + (float)delta * 0.6f);
        }

        if (Input.IsKeyPressed(Key.R) && _darkTimer <= 0f)
        {
            GetTree().ReloadCurrentScene();
        }

        UpdatePuzzleState();
        CheckWin();
        UpdateHud();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_darkTimer <= 0f)
        {
            return;
        }

        SimulatePlayer(_playerLight, delta, 0);
        SimulatePlayer(_playerDark, delta, 1);
    }

    private void SimulatePlayer(CharacterBody2D player, double delta, uint worldLayer)
    {
        var velocity = player.Velocity;
        float direction = Input.GetAxis("move_left", "move_right");
        velocity.X = direction * MoveSpeed;

        bool isClimbing = false;
        foreach (var area in player.GetOverlappingAreas())
        {
            if (area is Area2D ladder && ladder.IsInGroup("ladder"))
            {
                isClimbing = true;
                break;
            }
        }

        if (isClimbing)
        {
            float climbDir = Input.GetAxis("climb_up", "climb_down");
            velocity.Y = climbDir * ClimbSpeed;
            if (Input.IsActionJustPressed("jump"))
            {
                velocity.Y = JumpVelocity * 0.8f;
            }
        }
        else
        {
            velocity.Y += Gravity * (float)delta;
            if (player.IsOnFloor() && Input.IsActionJustPressed("jump"))
            {
                velocity.Y = JumpVelocity;
            }
        }

        player.Velocity = velocity;
        player.MoveAndSlide();

        // Keep players inside their world section.
        var minX = worldLayer == 0 ? 60f : 700f;
        var maxX = worldLayer == 0 ? 620f : 1260f;
        player.Position = new Vector2(Mathf.Clamp(player.Position.X, minX, maxX), Mathf.Min(player.Position.Y, 650f));
    }

    private void UpdatePuzzleState()
    {
        bool platePressed = _lightPlate.OverlapsBody(_playerLight);
        SetBridgeEnabled(platePressed);

        bool canToggleDoor = _darkSwitch.OverlapsBody(_playerDark) && Input.IsActionJustPressed("interact");
        if (canToggleDoor)
        {
            _darkDoorOpen = !_darkDoorOpen;
            _darkDoor.ProcessMode = _darkDoorOpen ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit;
            _darkDoor.CollisionLayer = _darkDoorOpen ? 0u : 2u;
            _darkDoor.CollisionMask = _darkDoorOpen ? 0u : 8u;
            _statusLabel.Text = _darkDoorOpen
                ? "Cong bong toi dang mo!"
                : "Cong bong toi dong lai!";
        }
    }

    private void SetBridgeEnabled(bool enabled)
    {
        _darkBridge.ProcessMode = enabled ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        _darkBridge.CollisionLayer = enabled ? 2u : 0u;
        _darkBridge.CollisionMask = enabled ? 8u : 0u;
    }

    private void CheckWin()
    {
        if (_goalLight.OverlapsBody(_playerLight) && _goalDark.OverlapsBody(_playerDark))
        {
            _statusLabel.Text = "Ban da dua ca hai nhan vat den dich. Chien thang!";
        }
    }

    private void UpdateWorldState()
    {
        Modulate = _isDarkActive ? new Color(0.82f, 0.82f, 1f) : Colors.White;
        _statusLabel.Text = _isDarkActive
            ? "Dang o THE GIOI BONG TOI (gioi han thoi gian)."
            : "Dang o THE GIOI ANH SANG.";
    }

    private void UpdateHud()
    {
        _timerLabel.Text = $"Dark Timer: {Mathf.Ceil(_darkTimer):0}s";
    }

    private void BuildScene()
    {
        var bg = new ColorRect
        {
            Color = new Color(0.07f, 0.08f, 0.12f),
            Size = new Vector2(1280, 720)
        };
        AddChild(bg);

        CreateWorldFrame(new Vector2(40, 70), new Color(0.2f, 0.2f, 0.35f), "ANH SANG");
        CreateWorldFrame(new Vector2(680, 70), new Color(0.14f, 0.1f, 0.2f), "BONG TOI");

        BuildLightWorld();
        BuildDarkWorld();
        BuildHud();
    }

    private void BuildLightWorld()
    {
        CreatePlatform(new Vector2(320, 640), new Vector2(520, 32), 1, new Color(0.75f, 0.78f, 0.9f));
        CreatePlatform(new Vector2(180, 520), new Vector2(170, 24), 1, new Color(0.85f, 0.9f, 1f));
        CreatePlatform(new Vector2(410, 450), new Vector2(140, 24), 1, new Color(0.85f, 0.9f, 1f));
        CreateLadder(new Vector2(510, 545), new Vector2(24, 190), new Color(0.5f, 0.8f, 1f));

        _playerLight = CreatePlayer(new Vector2(120, 590), new Color(0.98f, 0.95f, 0.5f), 4u, 1u, "PlayerLight");
        _lightPlate = CreateTrigger(new Vector2(430, 618), new Vector2(56, 12), new Color(0.3f, 1f, 0.5f), "Nut giu cau cho bong toi");
        _goalLight = CreateGoal(new Vector2(560, 407), new Color(0.7f, 1f, 0.7f));
    }

    private void BuildDarkWorld()
    {
        CreatePlatform(new Vector2(960, 640), new Vector2(520, 32), 2, new Color(0.3f, 0.26f, 0.4f));
        CreatePlatform(new Vector2(810, 530), new Vector2(140, 24), 2, new Color(0.4f, 0.35f, 0.5f));
        CreatePlatform(new Vector2(1110, 470), new Vector2(140, 24), 2, new Color(0.4f, 0.35f, 0.5f));
        CreateLadder(new Vector2(1020, 550), new Vector2(24, 180), new Color(0.7f, 0.4f, 1f));

        _darkBridge = CreatePlatform(new Vector2(980, 390), new Vector2(180, 20), 2, new Color(0.6f, 0.45f, 0.95f));
        SetBridgeEnabled(false);

        _darkDoor = CreatePlatform(new Vector2(1160, 565), new Vector2(28, 130), 2, new Color(0.8f, 0.2f, 0.35f));
        _darkSwitch = CreateTrigger(new Vector2(835, 505), new Vector2(46, 14), new Color(0.9f, 0.5f, 0.2f), "Nhan E de mo cong");

        _playerDark = CreatePlayer(new Vector2(760, 590), new Color(0.75f, 0.55f, 1f), 8u, 2u, "PlayerDark");
        _goalDark = CreateGoal(new Vector2(1160, 370), new Color(0.75f, 0.95f, 1f));
    }

    private void BuildHud()
    {
        var ui = new CanvasLayer();
        AddChild(ui);

        var title = new Label
        {
            Text = "SHADOW SWITCH - WASD/Arrow di chuyen, Space switch, E tuong tac",
            Position = new Vector2(20, 15),
            Modulate = new Color(0.95f, 0.96f, 1f)
        };
        ui.AddChild(title);

        _statusLabel = new Label
        {
            Position = new Vector2(20, 40),
            Modulate = new Color(0.8f, 0.9f, 1f)
        };
        ui.AddChild(_statusLabel);

        _timerLabel = new Label
        {
            Position = new Vector2(20, 62),
            Modulate = new Color(1f, 0.84f, 0.84f)
        };
        ui.AddChild(_timerLabel);
    }

    private void CreateWorldFrame(Vector2 position, Color tint, string name)
    {
        var panel = new ColorRect
        {
            Position = position,
            Size = new Vector2(560, 620),
            Color = tint
        };
        AddChild(panel);

        var label = new Label
        {
            Text = name,
            Position = position + new Vector2(10, 8),
            Modulate = Colors.White
        };
        AddChild(label);
    }

    private StaticBody2D CreatePlatform(Vector2 center, Vector2 size, uint collisionLayer, Color color)
    {
        var body = new StaticBody2D
        {
            CollisionLayer = collisionLayer,
            CollisionMask = collisionLayer == 1 ? 4u : 8u,
            Position = center
        };

        var collider = new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = size }
        };
        body.AddChild(collider);

        var visual = new Polygon2D
        {
            Polygon = new[]
            {
                new Vector2(-size.X / 2f, -size.Y / 2f),
                new Vector2(size.X / 2f, -size.Y / 2f),
                new Vector2(size.X / 2f, size.Y / 2f),
                new Vector2(-size.X / 2f, size.Y / 2f)
            },
            Color = color
        };
        body.AddChild(visual);
        AddChild(body);
        return body;
    }

    private CharacterBody2D CreatePlayer(Vector2 position, Color color, uint layer, uint mask, string nodeName)
    {
        var player = new CharacterBody2D
        {
            Name = nodeName,
            Position = position,
            CollisionLayer = layer,
            CollisionMask = mask
        };

        var collider = new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = new Vector2(28, 44) }
        };
        player.AddChild(collider);

        var visual = new Polygon2D
        {
            Polygon = new[]
            {
                new Vector2(-14, -22),
                new Vector2(14, -22),
                new Vector2(14, 22),
                new Vector2(-14, 22)
            },
            Color = color
        };
        player.AddChild(visual);

        player.SetMaxSlides(4);
        player.SetSafeMargin(0.08f);
        AddChild(player);
        return player;
    }

    private Area2D CreateLadder(Vector2 center, Vector2 size, Color color)
    {
        var ladder = new Area2D
        {
            CollisionLayer = 0,
            CollisionMask = 12,
            Position = center,
            Monitoring = true,
            Monitorable = true
        };
        ladder.AddToGroup("ladder");

        var collider = new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = size }
        };
        ladder.AddChild(collider);

        var visual = new Polygon2D
        {
            Polygon = new[]
            {
                new Vector2(-size.X / 2f, -size.Y / 2f),
                new Vector2(size.X / 2f, -size.Y / 2f),
                new Vector2(size.X / 2f, size.Y / 2f),
                new Vector2(-size.X / 2f, size.Y / 2f)
            },
            Color = color with { A = 0.6f }
        };
        ladder.AddChild(visual);

        AddChild(ladder);
        return ladder;
    }

    private Area2D CreateTrigger(Vector2 center, Vector2 size, Color color, string hint)
    {
        var trigger = new Area2D
        {
            Position = center,
            CollisionLayer = 0,
            CollisionMask = 12,
            Monitoring = true,
            Monitorable = true
        };
        var collider = new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = size }
        };
        trigger.AddChild(collider);

        var visual = new Polygon2D
        {
            Polygon = new[]
            {
                new Vector2(-size.X / 2f, -size.Y / 2f),
                new Vector2(size.X / 2f, -size.Y / 2f),
                new Vector2(size.X / 2f, size.Y / 2f),
                new Vector2(-size.X / 2f, size.Y / 2f)
            },
            Color = color
        };
        trigger.AddChild(visual);

        var label = new Label
        {
            Text = hint,
            Position = center + new Vector2(-50, -28),
            Modulate = Colors.White
        };
        AddChild(label);

        AddChild(trigger);
        return trigger;
    }

    private Area2D CreateGoal(Vector2 center, Color color)
    {
        var goal = new Area2D
        {
            Position = center,
            CollisionLayer = 0,
            CollisionMask = 12,
            Monitoring = true,
            Monitorable = true
        };
        var collider = new CollisionShape2D
        {
            Shape = new RectangleShape2D { Size = new Vector2(54, 54) }
        };
        goal.AddChild(collider);

        var visual = new Polygon2D
        {
            Polygon = new[]
            {
                new Vector2(-27, -27),
                new Vector2(27, -27),
                new Vector2(27, 27),
                new Vector2(-27, 27)
            },
            Color = color with { A = 0.7f }
        };
        goal.AddChild(visual);

        AddChild(goal);
        return goal;
    }
}
