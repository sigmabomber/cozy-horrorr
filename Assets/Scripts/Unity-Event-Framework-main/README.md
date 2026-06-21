# Game Event System - Simple Guide

## What Are Events?

Events let different parts of your game talk to each other **without knowing about each other**.

Think of it like a radio:
-  **Publisher** = Radio station broadcasting
-  **Event** = The radio signal (what happened)
-  **Subscriber** = People listening to the radio

The radio station doesn't know who's listening. Listeners don't need to know where the station is. They just tune in to the signal they care about.

---

## The Problem Events Solve

###  Without Events (Messy):

```csharp
public class Enemy : MonoBehaviour
{
    public QuestSystem questSystem;
    public AudioManager audioManager;
    public ParticleSystem particles;
    public UIManager uiManager;
    // Need references to EVERYTHING!
    
    void Die()
    {
        questSystem.OnEnemyKilled();
        audioManager.PlaySound("death");
        particles.SpawnEffect(transform.position);
        uiManager.UpdateKillCount();
        // If you add a new system, you have to come back here!
    }
}
```

**Problems:**
- Enemy needs to know about every system
- Hard to add new systems later
- Everything is tightly connected (hard to test/debug)

###  With Events (Clean):

```csharp
public class Enemy : MonoBehaviour
{
    void Die()
    {
        // Just announce what happened
        Events.Publish(new EnemyDiedEvent 
        { 
            Position = transform.position 
        });
        
        Destroy(gameObject);
    }
}
```

**Benefits:**
- Enemy doesn't know who's listening
- Easy to add new systems (they just listen)
- Everything is decoupled (easy to test/debug)

---

## How To Use The Event System

### Step 1: Define Your Event

Just create a simple class with data about what happened:

```csharp
public class EnemyDiedEvent
{
    public Vector3 Position;
    public int Score;
}
```

That's it! No inheritance, no interfaces, just a plain class.

### Step 2: Publish The Event

When something happens, announce it:

```csharp
public class Enemy : MonoBehaviour
{
    void Die()
    {
        Events.Publish(new EnemyDiedEvent 
        { 
            Position = transform.position,
            Score = 100
        });
    }
}
```

### Step 3: Listen For The Event

Any script can listen. Just inherit from `EventListener` and use `Listen()`:

```csharp
public class QuestSystem : EventListener
{
    void Start()
    {
        Listen<EnemyDiedEvent>(OnEnemyDied);
    }
    
    void OnEnemyDied(EnemyDiedEvent evt)
    {
        Debug.Log($"Enemy died at {evt.Position}");
        currentQuest.enemiesKilled++;
    }
}
```

**That's it!** When Enemy publishes, QuestSystem automatically receives it.

---

## Complete Example

Let's make a coin collection system:

### 1. Define the event:

```csharp
public class CoinCollectedEvent
{
    public int Amount;
    public Vector3 Position;
}
```

### 2. Publish when coin is collected:

```csharp
public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Announce the coin was collected
            Events.Publish(new CoinCollectedEvent
            {
                Amount = coinValue,
                Position = transform.position
            });
            
            Destroy(gameObject);
        }
    }
}
```

### 3. Different systems listen independently:

```csharp
// Audio system plays sound
public class AudioManager : EventListener
{
    void Start()
    {
        Listen<CoinCollectedEvent>(evt => PlaySound("coin"));
    }
}

// UI updates the display
public class UIManager : EventListener
{
    private int totalCoins = 0;
    
    void Start()
    {
        Listen<CoinCollectedEvent>(OnCoinCollected);
    }
    
    void OnCoinCollected(CoinCollectedEvent evt)
    {
        totalCoins += evt.Amount;
        coinText.text = totalCoins.ToString();
    }
}

// Particle system spawns effects
public class VFXManager : EventListener
{
    void Start()
    {
        Listen<CoinCollectedEvent>(evt =>
        {
            Instantiate(sparkles, evt.Position, Quaternion.identity);
        });
    }
}

// Quest system tracks progress
public class QuestSystem : EventListener
{
    void Start()
    {
        Listen<CoinCollectedEvent>(evt =>
        {
            currentQuest.coinsCollected += evt.Amount;
        });
    }
}
```

Notice how:
-  Coin doesn't know about any of these systems
-  Each system only cares about coin events
-  Adding a new system? Just add it and listen - no need to touch Coin.cs
-  Removing a system? Just delete it - everything still works

---

## Common Event Examples

Here are events you might use in your game:

```csharp
// Player events
public class PlayerDiedEvent 
{ 
    public Vector3 Position; 
    public string Cause; 
}

public class PlayerHealthChangedEvent 
{ 
    public int OldHealth; 
    public int NewHealth; 
}

// Combat events
public class WeaponFiredEvent 
{ 
    public Vector3 Position; 
    public string WeaponName; 
}

public class DamageTakenEvent 
{ 
    public GameObject Target; 
    public int Damage; 
}

// Game events
public class LevelCompletedEvent 
{ 
    public int LevelNumber; 
    public float Time; 
}

public class GamePausedEvent 
{ 
    public bool IsPaused; 
}

// UI events
public class ButtonClickedEvent 
{ 
    public string ButtonName; 
}

public class MenuOpenedEvent 
{ 
    public string MenuName; 
}
```

---

## Important Rules

###  DO:

1. **Inherit from `EventListener`** - Gets automatic cleanup
   ```csharp
   public class MySystem : EventListener
   ```

2. **Use `Listen<T>()` in Start()** - Subscribe to events
   ```csharp
   void Start() 
   { 
       Listen<MyEvent>(OnMyEvent); 
   }
   ```

3. **Use `Events.Publish()`** - Send events
   ```csharp
   Events.Publish(new MyEvent { Data = value });
   ```

###  DON'T:

1. **Don't forget to inherit `EventListener`** - Or you'll get memory leaks!
   ```csharp
   // BAD - Will leak!
   public class MySystem : MonoBehaviour
   
   // GOOD - Auto cleanup!
   public class MySystem : EventListener
   ```

2. **Don't publish null events**
   ```csharp
   // BAD
   Events.Publish(null);
   
   // GOOD
   Events.Publish(new MyEvent());
   ```

3. **Don't store event data** - Events are notifications, not state
   ```csharp
   // BAD
   private CoinCollectedEvent lastEvent; // Don't store events!
   
   // GOOD
   private int totalCoins; // Store the data you need
   ```

---

## Advanced: Manual Subscription

If you can't inherit from `EventListener`, manage subscriptions manually:

```csharp
public class MySystem : MonoBehaviour
{
    private IDisposable subscription;
    
    void Start()
    {
        // Save the subscription
        subscription = Events.Subscribe<MyEvent>(OnMyEvent, this);
    }
    
    void OnMyEvent(MyEvent evt)
    {
        // Handle event
    }
    
    void OnDestroy()
    {
        // Clean up manually
        subscription?.Dispose();
    }
}
```

The `this` parameter helps prevent memory leaks by tracking ownership.

---

## Debugging

### Check how many systems are listening:

```csharp
int count = Events.GetSubscriberCount<EnemyDiedEvent>();
Debug.Log($"Listeners: {count}");
```

---

## Quick Reference

| Action | Code |
|--------|------|
| Define event | `public class MyEvent { public int Data; }` |
| Send event | `Events.Publish(new MyEvent { Data = 5 });` |
| Listen (auto cleanup) | Inherit `EventListener`, use `Listen<MyEvent>(handler)` |
| Listen (manual) | `Events.Subscribe<MyEvent>(handler, this)` |
| Stop listening | Happens automatically with `EventListener` |

---


