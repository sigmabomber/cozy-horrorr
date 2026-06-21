# Network Game Event System - Simple Guide
## What Are Network Events?

### Network events let different parts of your multiplayer game communicate across the network without tightly coupling your systems.

Think of it like a walkie-talkie:

- **Publisher** = Player sending an action

- **Event** = The message being sent

- **Subscriber** = Other players or server listening

### The sender doesn’t need to know who receives it. Listeners don’t need to know who sent it. They just respond to the event.

## The Problem Network Events Solve

### Without Network Events (Messy):

```csharp
public class Player : MonoBehaviour
{
    public GameManager gameManager;
    public NetworkManager networkManager;
    public UIManager uiManager;

    void Shoot()
    {
        networkManager.SendShootCommand();
        gameManager.RegisterShot();
        uiManager.UpdateAmmoDisplay();
        // If a new system needs to know about shooting, we must edit this
    }
}
```

**Problems**:

### Player must know every system affected

### Hard to extend

### Hard to debug and maintain

```csharp
// With Network Events (Clean):
public class Player : MonoBehaviour
{
    void Shoot()
    {
        // Just announce the event
        NetworkEvents.Publish(new PlayerShotEvent
        {
            Position = transform.position,
            Direction = transform.forward
        });
    }
}

```
**Benefits**:
- Player doesn’t care who is listening
- Easy to add new systems (just listen)
- Clean and decoupled

# How To Use The Network Event System

## Step 1: Define Your Event

### Create a simple class with the data you want to share:

```csharp
public class PlayerShotEvent
{
    public Vector3 Position;
    public Vector3 Direction;
    public string WeaponName;
}

```
No inheritance, no interfaces—just a plain class.

Step 2: Publish The Event

Send the event when something happens:
```csharp
public class Player : MonoBehaviour
{
    void Shoot()
    {
        NetworkEvents.Publish(new PlayerShotEvent
        {
            Position = transform.position,
            Direction = transform.forward,
            WeaponName = "Rifle"
        });
    }
}
```
## Step 3: Listen For The Event

##3 Any system can listen. Just inherit from NetworkEventListener:

```csharp
public class NetworkGameManager : NetworkEventListener
{
    void Start()
    {
        Listen<PlayerShotEvent>(OnPlayerShot);
    }

    void OnPlayerShot(PlayerShotEvent evt)
    {
        Debug.Log($"Player shot from {evt.Position} towards {evt.Direction}");
        SpawnBullet(evt.Position, evt.Direction, evt.WeaponName);
    }
}
```

### Complete Example: Multiplayer Coin Collection
## 1. Define the event:

```csharp
public class CoinCollectedNetworkEvent
{
    public int Amount;
    public Vector3 Position;
    public string PlayerID;
}
```

## 2. Publish the event:

```csharp
public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkEvents.Publish(new CoinCollectedNetworkEvent
            {
                Amount = coinValue,
                Position = transform.position,
                PlayerID = other.name
            });

            Destroy(gameObject);
        }
    }
}
```

## 3. Systems listen independently:

```csharp
// Update UI for the player
public class UIManager : NetworkEventListener
{
    void Start()
    {
        Listen<CoinCollectedNetworkEvent>(OnCoinCollected);
    }

    void OnCoinCollected(CoinCollectedNetworkEvent evt)
    {
        if (evt.PlayerID == "Player1")
        {
            playerCoinText.text = evt.Amount.ToString();
        }
    }
}

// Spawn particle effects for all players
public class VFXManager : NetworkEventListener
{
    void Start()
    {
        Listen<CoinCollectedNetworkEvent>(evt =>
        {
            Instantiate(sparklesPrefab, evt.Position, Quaternion.identity);
        });
    }
}

// Update server-side coin count
public class ServerCoinManager : NetworkEventListener
{
    void Start()
    {
        Listen<CoinCollectedNetworkEvent>(evt =>
        {
            totalCoinsCollected += evt.Amount;
        });
    }
}
```

## Notice:

### Coin doesn’t know about any system

### Each system only reacts to the event

### Adding or removing systems doesn’t require changing Coin.cs

### Common Network Event Examples

```csharp
// Player events
public class PlayerJoinedEvent { public string PlayerID; }
public class PlayerLeftEvent { public string PlayerID; }

// Combat events
public class PlayerShotEvent 
{ 
    public Vector3 Position; 
    public Vector3 Direction; 
    public string WeaponName; 
}

public class DamageReceivedEvent 
{ 
    public string PlayerID; 
    public int Damage; 
}

// Game events
public class LevelStartEvent { public int LevelNumber; }
public class LevelEndEvent { public int LevelNumber; public float Time; }

// UI events
public class ScoreUpdatedEvent { public string PlayerID; public int NewScore; }
public class ChatMessageEvent { public string PlayerID; public string Message; }
```

# Important Rules
## DO:

### Inherit from NetworkEventListener for automatic cleanup

```csharp
public class MySystem : NetworkEventListener


Use Listen<T>() in Start() to subscribe

void Start()
{
    Listen<MyEvent>(OnMyEvent);
}
```

### Use NetworkEvents.Publish() to send events

### NetworkEvents.Publish(new MyEvent { Data = value });

# DON’T:

## Forget to inherit NetworkEventListener

```csharp
// BAD - Will leak!
public class MySystem : MonoBehaviour

// GOOD
public class MySystem : NetworkEventListener
```

Publish null events

```csharp
// BAD
NetworkEvents.Publish(null);

// GOOD
NetworkEvents.Publish(new MyEvent());

```

## Store events as state

```csharp
// BAD
private PlayerShotEvent lastShot;

// GOOD
private int totalShotsFired;
```

# Advanced: Manual Subscription


## If you can’t inherit from NetworkEventListener:

```csharp
public class MySystem : MonoBehaviour
{
    private IDisposable subscription;

    void Start()
    {
        subscription = NetworkEvents.Subscribe<MyEvent>(OnMyEvent, this);
    }

    void OnMyEvent(MyEvent evt)
    {
        // Handle event
    }

    void OnDestroy()
    {
        subscription?.Dispose();
    }
}
```

# Debugging

```csharp
int count = NetworkEvents.GetSubscriberCount<PlayerShotEvent>();
Debug.Log($"Listeners: {count}");
```


## Quick Reference

| Action | Code |
|--------|------|
| Define event | `public class MyEvent { public int Data; }` |
| Send event | `Events.Publish(new MyEvent { Data = 5 });` |
| Listen (auto cleanup) | Inherit `EventListener`, use `Listen<MyEvent>(handler)` |
| Listen (manual) | `Events.Subscribe<MyEvent>(handler, this)` |
| Stop listening | Happens automatically with `EventListener` |
