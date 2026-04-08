# TaggedUnion — Low-Allocation Tagged Unions for Unity

**Package:** `com.psigenvision.taggedunion`  
**Version:** 1.0.0  
**Namespace:** `PsigenVision.TaggedUnion`

This package provides memory-efficient, zero-boxing, type-safe **tagged unions** (also called variants or discriminated unions) for Unity. It was inspired by Youtuber git-amend's own C# Blackboard for Unity tutorial (see <https://github.com/adammyhre/Unity-Behaviour-Trees/tree/master> for more details)

It was built specifically to support high-performance systems such as:
- Scriptable animators / blend trees
- Blackboards
- Behavior trees
- Crowd simulation state
- Any other place where you need a flexible key-value store that must stay fast and lightweight even at scale.

---

## Why Tagged Unions Exist

In C#, the naive way to store "any type of data" is to use `object`:

```csharp
object value = 42;          // boxing happens here
object value = new Vector3(); // boxing again
```

Every time you store a value type (`int`, `float`, `Vector3`, `bool`, etc.) into an `object`, the CLR **boxes** it — it wraps the value in a reference-type object on the heap. This creates garbage and GC pressure, especially if you read/write thousands of times per frame (e.g. animation parameters for 1000+ NPCs).

A **tagged union** solves this by combining:
- A small **tag** (an enum that says "what am I right now?")
- A **storage area** that can hold any of the supported types **in the same memory location**

C# does not have native unions like C/C++, so we simulate them safely with `[StructLayout(LayoutKind.Explicit)]` + `FieldOffset(0)`.

---

## Design Principles (Why We Built It This Way)

1. **Zero Boxing & Allocation-Free Hot Paths**  
   We never use `object`. All value-type storage is done with overlapping memory so only one value ever exists.

2. **Memory Efficiency via Real Union Behavior**  
   By overlapping fields inside a private union, the struct size is determined by the **largest** member + the enum tag instead of summing every field. This is the key memory win.

3. **Specialization Instead of One Mega-Type**  
   Instead of a single growing `AnyValue`, we provide four focused types:
   - `AnyPrimitive` — int/float/bool only (smallest)
   - `AnyValue` — primitives + common Unity value types
   - `AnyPrimitiveLiteral` — primitives + string (for when vectors aren't needed)
   - `AnyLiteral` — full value types + string (the "classic" choice)
   - `AnyAnimatorParamValue` (specialized for Unity’s Animator).
   - `AnyPrimitiveLiteral`, `AnyLiteral`, and `AnyAnimatorParamLiteral` (string-inclusive types for tags or states).
   - `AnyRange` (value ranges like `Vector2` and `Vector2Int`).


   This prevents hot-path data from paying for unused fields.

4. **Safety Without Sacrificing Performance**  
   We respect git-amend's original safety (return `default` on type mismatch) but use modern `Unsafe.As` + `when` guards for maximum speed.

6. **Extensibility Without Fragility**  
   Adding a new type only requires updating one enum, one union, and one `switch` case.

7. **Inspector-First Experience**  
   The included `AnyUnionDrawer` makes editing these types in the Inspector feel native.

---

## How Tagged Unions Work (Technical Deep Dive)

A tagged union consists of:
- **Tag** (`ValueType` enum) — tells you which kind of data is currently stored.
- **Storage area** — the actual bits for the value.

We use `[StructLayout(LayoutKind.Explicit)]` + `FieldOffset(0)` on every field inside a private nested union struct. This makes the fields **overlap** in memory.

Example memory layout for `AnyValue`:
- `type` (1 byte + padding) at offset 0
- `ValueUnion` (16 bytes — size of `Quaternion`) at offset 4
- Total size ≈ 20 bytes (instead of ~36+ bytes in the naïve "all fields" version)

### Safe Conversion Logic

The implicit operators call a private `Convert<T>()` method:

```csharp
private T Convert<T>()
{
    return type switch
    {
        ValueType.Int when typeof(T) == typeof(int)
            => Unsafe.As<int, T>(ref value.intValue),

        // ... other matching cases

        _ => default!   // safe fallback on mismatch
    };
}
```

This guarantees:
- `Unsafe.As` is only ever called when the types match.
- On mismatch you get the proper `default` value (exactly like git-amend's original).
- Zero boxing when the type matches.

---

## Core Types & Use Cases

### `AnyPrimitive`

**Size:** ~8–12 bytes  
**Best for:** High-volume crowd simulation, flags, health, timers, simple blend weights.

### `AnyValue`

**Size:** ~16–20 bytes  
**Best for:** Animation blend parameters, positions, rotations, directions. The main workhorse for your scriptable animator.

### `AnyAnimatorParamValue`

**Size:** ~16–20 bytes
**Best for:** Optimized for systems like Mecanim blackboards. Specialized tagged union for working with Unity Animator parameters (`float`, `Vector2`, `bool`).

### `AnyRange`

**Size:** ~16–20 bytes
**Best for:**  Designed for systems requiring movement ranges, zoom ranges, or animation constraints. A tagged union for value ranges (`Vector2`, `Vector2Int`) to handle constraints and limits efficiently.


### `AnyPrimitiveLiteral`

**Size:** ~16–20 bytes  
**Best for:** Blackboards that only ever need primitives + strings (common in many animation systems). Smallest memory footprint when vectors aren't required.

### `AnyLiteral`

**Size:** ~24–28 bytes  
**Best for:** General-purpose blackboards that need the full set of value types plus occasional string names (state names, tags, etc.).

### `AnyAnimatorParamLiteral`

**Size:** ~24–28 bytes
**Best for:** Use this for animators requiring both parameter names and tagged data. Supports string-based animator parameters alongside stored values.



---

## Extension Methods (`AnyExtensions`)

These let you safely convert between types (e.g. downsize a blackboard entry):

```csharp
//Converting primitives to AnyPrimitive type 
public static AnyPrimitive AsPrimitive(this int val);
public static AnyPrimitive AsPrimitive(this bool val);
public static AnyPrimitive AsPrimitive(this float val);
  
//Converting value-types to AnyValue types
public static AnyValue AsValue(this int val);
public static AnyValue AsValue(this bool val);
public static AnyValue AsValue(this float val);
public static AnyValue AsValue(this Vector2 value);
public static AnyValue AsValue(this Vector3 value);
public static AnyValue AsValue(this Quaternion value);          
  
//Converting value-types and string types to AnyLiteral types
public static AnyLiteral AsLiteral(this int val); 
public static AnyLiteral AsLiteral(this bool val);
public static AnyLiteral AsLiteral(this float val);
public static AnyLiteral AsLiteral(this Vector2 val); 
public static AnyLiteral AsLiteral(this Vector3 val);  
public static AnyLiteral AsLiteral(this Quaternion val);
public static AnyLiteral AsLiteral(this string val);  
  
//Converting value-types and string types to AnyPrimitiveLiteral types (wrapper for C# union)  
public static AnyPrimitiveLiteral AsPrimitiveLiteral(this int val);
public static AnyPrimitiveLiteral AsPrimitiveLiteral(this bool val);
public static AnyPrimitiveLiteral AsPrimitiveLiteral(this float val);
public static AnyPrimitiveLiteral AsPrimitiveLiteral(this string val);
  
//Converting between Any-types
public static AnyPrimitive AsPrimitive(this AnyValue v);
public static AnyPrimitive AsPrimitive(this AnyLiteral l);
public static AnyPrimitive AsPrimitive(this AnyPrimitiveLiteral l);
public static AnyValue AsValue(this AnyPrimitive p);
public static AnyValue AsValue(this AnyLiteral l);
public static AnyValue AsValue(this AnyPrimitiveLiteral l);
public static AnyLiteral AsLiteral(this AnyPrimitive p);
public static AnyLiteral AsLiteral(this AnyValue v);
public static AnyLiteral AsLiteral(this AnyPrimitiveLiteral l);
```

They are zero-cost thin wrappers and do not increase struct size.

---

## Custom Inspector (`AnyUnionDrawer`)

The package includes a `[CustomPropertyDrawer]` that:
- Shows a compact **Type** dropdown next to the label.
- Displays **only the active value field** with a friendly label.
- Works automatically for all four types.

---

## When to Use This Package

**Use it when:**
- You need a flexible but high-performance Blackboard.
- You care about performance in animation, AI, or crowd systems.
- You want to avoid `object` boxing entirely.
- You plan to scale to hundreds or thousands of instances.

**Do not use it when:**
- You only have a handful of parameters (a simple `Dictionary<string, object>` is fine).
- You need completely arbitrary types without planning ahead.

---

## Adding New Types (Extensibility Guide)

To add a new type (e.g. `Color`):

1. Add it to the appropriate enum (`ValueType`).
2. Add the field to the private union struct with `[FieldOffset(0)]` (or create a new one that follows the appropriate rules)
3. Add the implicit operator.
4. Add the matching case in `Convert<T>()`.
5. Add the static `From(…)` factory.
6. Update the `AnyUnionDrawer` if you want a custom label.

---

## Installation & Usage

1. Add the package via Git URL or local folder.
2. `using TaggedUnion;`
3. Start using the types:

```csharp
AnyValue blend = AnyValue.From(new Vector2(0.8f, 0.3f));
Vector2 dir = blend;   // implicit conversion
```

Full source is in the `Runtime/` folder. Editor code is in `Editor/`.

---

**Happy coding!**  
This system was designed with long-term scalability, performance, and clean architecture in mind. Every choice was made to balance safety, memory efficiency, and usability while staying faithful to the spirit of git-amend’s original tutorial.

— Built with ❤️ for the scriptable animator project and beyond

---

### API Reference

(See the individual struct files in `Runtime/` for the complete code. The structs are intentionally small and focused.)

**AnyPrimitive**, **AnyValue**, **AnyPrimitiveLiteral**, **AnyLiteral** — all follow the same safe tagged-union pattern described above.

**Extension methods** are in `AnyExtensions.cs`.

**Custom PropertyDrawer** is in `Editor/AnyUnionDrawer.cs`.

---


All types live in the `PsigenVision.TaggedUnion` namespace.


### 1. `AnyPrimitive`


**Purpose** The smallest tagged union — only the three most common primitive types. Ideal for high-volume systems (crowd simulation, thousands of NPCs, flags, timers, etc.).

**Size** ≈ 8–12 bytes (depending on padding).

```csharp  
[Serializable]  
[StructLayout(LayoutKind.Explicit)]  
public struct AnyPrimitive  
{  
    public enum ValueType : byte    {        None = 0,        Int,        Float,        Bool    }  
    [FieldOffset(0)] public ValueType type;  
    // Private union (not directly accessible)    
    [FieldOffset(4)] private PrimitiveUnion value;  
    // Implicit conversions (zero-boxing)    
    public static implicit operator int(AnyPrimitive v);    
    public static implicit operator float(AnyPrimitive v);    
    public static implicit operator bool(AnyPrimitive v);  
    // Factory methods (recommended way to create)    
    public static AnyPrimitive From(int v);    
    public static AnyPrimitive From(float v);    
    public static AnyPrimitive From(bool v);  
    // Optional helper    
    public void Clear(); // sets type = None}  
}
```

**Usage Example**
```csharp  
AnyPrimitive health = AnyPrimitive.From(100);  
AnyPrimitive isCrouching = AnyPrimitive.From(true);  
  
int hp = health;           // implicit conversion  
bool crouch = isCrouching;  
```  
  
---  


### 2. `AnyValue`


**Purpose** The main workhorse for most game systems, especially your scriptable animator. Contains primitives + common Unity value types. **No string or reference fields** — keeps it lean for animation blend trees and large crowds.

**Size** ≈ 16–20 bytes.

```csharp  
[Serializable]  
[StructLayout(LayoutKind.Explicit)]  
public struct AnyValue  
{  
    public enum ValueType : byte    {        None = 0,        Int,        Float,        Bool,        Vector2,        Vector3,        Quaternion    }  
    [FieldOffset(0)] public ValueType type;  
    // Private union (not directly accessible)    
    [FieldOffset(4)] private ValueUnion value;  
    // Implicit conversions (zero-boxing)    
    public static implicit operator int(AnyValue v);    
    public static implicit operator float(AnyValue v);    
    public static implicit operator bool(AnyValue v);    
    public static implicit operator Vector2(AnyValue v);    
    public static implicit operator Vector3(AnyValue v);    
    public static implicit operator Quaternion(AnyValue v);  
    // Factory methods    
    public static AnyValue From(int v);    
    public static AnyValue From(float v);    
    public static AnyValue From(bool v);    
    public static AnyValue From(Vector2 v);    
    public static AnyValue From(Vector3 v);    
    public static AnyValue From(Quaternion v);  
    public void Clear();
    }  
```  

**Usage Example** (perfect for your scriptable animator)
```csharp  
AnyValue blendParam = AnyValue.From(new Vector2(0.8f, 0.3f));  
AnyValue aimRotation = AnyValue.From(Quaternion.Euler(0, 45, 0));  
  
Vector2 direction = blendParam;   // implicit  
Quaternion rot = aimRotation;  
```  
  
---  

### 3. `AnyAnimatorParamValue`

**Purpose**: Represents animator parameters as lean, union-based structs for runtime usage. Optimized for performance and simplicity when storing commonly used animator parameter values like floats, booleans, or Vector2.

**Size**: ≈ 12–16 bytes.

```csharp
[Serializable]
[StructLayout(LayoutKind.Explicit)]
public struct AnyAnimatorParamValue
{
    public enum ValueType : byte { None = 0, Float, Bool, Vector2 }
    [FieldOffset(0)] public ValueType type;
    // Private union
    [FieldOffset(4)] private AnimatorParameterUnion value;
    // Implicit conversions
    public static implicit operator float(AnyAnimatorParamValue v);
    public static implicit operator bool(AnyAnimatorParamValue v);
    public static implicit operator Vector2(AnyAnimatorParamValue v);
    // Factory methods
    public static AnyAnimatorParamValue From(float v);
    public static AnyAnimatorParamValue From(bool v);
    public static AnyAnimatorParamValue From(Vector2 v);
    public void Clear();
}
```

**Usage Example**:
```csharp
AnyAnimatorParamValue paramValue = AnyAnimatorParamValue.From(0.5f);
float playbackSpeed = paramValue;   // implicit conversion

AnyAnimatorParamValue direction = AnyAnimatorParamValue.From(new Vector2(1.0f, 0.8f));
Vector2 animDirection = direction; // implicit conversion
```

---

### 4. `AnyRange`

**Purpose**: A versatile range type for representing both integer and float ranges. Particularly useful for any game system relying on random values, thresholds, or clamps.

**Size**: ≈ 16 bytes.

```csharp
[Serializable]
[StructLayout(LayoutKind.Explicit)]
public struct AnyRange
{
    public enum RangeType : byte { None = 0, Int, Float }
    [FieldOffset(0)] public RangeType type;
    // Private union
    [FieldOffset(4)] private DefaultRangeUnion range;
    // Implicit conversions
    public static implicit operator Vector2Int(AnyRange r);
    public static implicit operator Vector2(AnyRange r);
    // Factory methods
    public static AnyRange From(Vector2Int intRange);
    public static AnyRange From(Vector2 floatRange);
    public void Clear();
}
```

**Usage Example**:
```csharp
AnyRange healthRange = AnyRange.From(new Vector2Int(50, 100));
Vector2Int healthMinMax = healthRange; // implicit conversion

AnyRange speedRange = AnyRange.From(new Vector2(1.0f, 3.5f));
Vector2 speedMinMax = speedRange;     // implicit conversion
```

---

### 5. `AnyPrimitiveLiteral`

**Purpose**  
The smallest tagged union that supports strings — only the three most common primitive types as well as a string reference (not included in the union). Ideal for high-volume systems (crowd simulation, thousands of NPCs, flags, timers, etc.) with the potential for string references (tagging with strings).

**Size**  
≈ 16–20 bytes (depending on padding - the extra string reference is the only added cost compared to `AnyPrimitive`).

```csharp  
[Serializable]  
[StructLayout(LayoutKind.Explicit)]  
public struct AnyPrimitive  
{  
    public enum ValueType : byte    {        None = 0,        Int,        Float,        Bool    }  
    [FieldOffset(0)] public ValueType type;  
    // Private union (not directly accessible)    
    [FieldOffset(4)] private PrimitiveUnion value;  
    // Implicit conversions (zero-boxing)    
    public static implicit operator int(AnyPrimitive v);    
    public static implicit operator float(AnyPrimitive v);    
    public static implicit operator bool(AnyPrimitive v);  
    // Factory methods (recommended way to create)    
    public static AnyPrimitive From(int v);    
    public static AnyPrimitive From(float v);    
    public static AnyPrimitive From(bool v);  
    // Optional helper    
    public void Clear(); // sets type = None}  
}
```

**Usage Example**
```csharp  
//Using AnyPrimitive without support of strings
AnyPrimitive healthPrimitive = AnyPrimitive.From(100);  
AnyPrimitive isCrouchingPrimitive = AnyPrimitive.From(true);  

//Wanted to include the player name under the same Any-type - supported by AnyPrimitiveLiteral
AnyPrimitiveLiteral playerName = "Koko";

//Convert old AnyPrimitive type to AnyPrimitiveLiteral, as both support primitives
AnyPrimitiveLiteral health = healthPrimitive.AsPrimitiveLiteral();
AnyPrimitiveLiteral isCrouching = isCrouchingPrimitive.AsPrimitiveLiteral();

//implicit conversion
int hp = health;           
bool crouch = isCrouching; 
```

 
---

### 6. `AnyLiteral`

**Purpose**  
Provides the exact same behavior as git-amend’s original `AnyValue` (value types + string) while using the modern tagged-union design. Use this type when you want to stay faithful to the tutorial but still get the memory and zero-boxing benefits.

**Size**  
≈ 24–28 bytes (the extra string reference is the only added cost compared to `AnyValue`).

```csharp
[Serializable]
[StructLayout(LayoutKind.Explicit)]
public struct AnyLiteral
{
    public enum ValueType : byte
    {
        None = 0,
        Int,
        Float,
        Bool,
        String,
        Vector2,
        Vector3,
        Quaternion
    }

    [FieldOffset(0)] public ValueType type;

    [FieldOffset(4)] private ValueUnion value;           // value types overlap here
    [FieldOffset(20)] public string stringValue;         // reference kept separate

    [StructLayout(LayoutKind.Explicit)]
    private struct ValueUnion
    {
        [FieldOffset(0)] public int intValue;
        [FieldOffset(0)] public float floatValue;
        [FieldOffset(0)] public bool boolValue;
        [FieldOffset(0)] public Vector2 vector2Value;
        [FieldOffset(0)] public Vector3 vector3Value;
        [FieldOffset(0)] public Quaternion quaternionValue;
    }

    // Implicit conversions (zero-boxing)
    public static implicit operator int(AnyLiteral v);
    public static implicit operator float(AnyLiteral v);
    public static implicit operator bool(AnyLiteral v);
    public static implicit operator string(AnyLiteral v);
    public static implicit operator Vector2(AnyLiteral v);
    public static implicit operator Vector3(AnyLiteral v);
    public static implicit operator Quaternion(AnyLiteral v);

    // Factory methods
    public static AnyLiteral From(int v);
    public static AnyLiteral From(float v);
    public static AnyLiteral From(bool v);
    public static AnyLiteral From(string v);
    public static AnyLiteral From(Vector2 v);
    public static AnyLiteral From(Vector3 v);
    public static AnyLiteral From(Quaternion v);

    public void Clear();
}
```

**Usage Example**
```csharp
AnyLiteral param = AnyLiteral.From("Attack");
AnyLiteral blend = AnyLiteral.From(new Vector2(0.7f, 0.4f));

string stateName = param;      // implicit
Vector2 direction = blend;
```

---

### 7. `AnyAnimatorParamLiteral`

**Purpose**: Adds string support to animator parameters, making it a hybrid solution for cases where string-based parameter access is also required. Useful for design-time flexibility without sacrificing runtime performance.

**Size**: ≈ 16–20 bytes.

```csharp
[Serializable]
[StructLayout(LayoutKind.Explicit)]
public struct AnyAnimatorParamLiteral
{
    public enum ValueType : byte { None = 0, Float, Bool, Vector2, String }
    [FieldOffset(0)] public ValueType type;
    // Private union
    [FieldOffset(4)] private AnimatorParameterUnion value;
    [FieldOffset(8)] private string stringValue;
    // Implicit conversions
    public static implicit operator float(AnyAnimatorParamLiteral v);
    public static implicit operator bool(AnyAnimatorParamLiteral v);
    public static implicit operator Vector2(AnyAnimatorParamLiteral v);
    public static implicit operator string(AnyAnimatorParamLiteral v);
    // Factory methods
    public static AnyAnimatorParamLiteral From(float v);
    public static AnyAnimatorParamLiteral From(bool v);
    public static AnyAnimatorParamLiteral From(Vector2 v);
    public static AnyAnimatorParamLiteral From(string v);
    public void Clear();
}
```

**Usage Example**:
```csharp
AnyAnimatorParamLiteral paramName = AnyAnimatorParamLiteral.From("RunSpeed");
string parameter = paramName;    // implicit conversion

AnyAnimatorParamLiteral blendParam = AnyAnimatorParamLiteral.From(new Vector2(0.5f, 1.0f));
Vector2 blendValues = blendParam;    // implicit conversion
```

---

### 8. `AnyUnionDrawer` (Editor only)


**Purpose** Custom PropertyDrawer that makes all four types look clean and native in the Inspector.

- Shows a compact **Type** dropdown next to the label.
- Displays **only the active value field** with a friendly label.
- Automatically applied to `AnyPrimitive`, `AnyValue`, `AnyPrimitiveLiteral`, and `AnyLiteral`.

No public API — it is automatically used by Unity’s Inspector.
  
---  
### 9. Unsafe Union Structures


**Purpose** Encapsulate all values to be stored in a union in the same struct, with explicit layout and packing. Due to the manner in which they are defined, their memory footprint is minimized to the size of their largest members.

**Defined Structures**:
1. `PrimitiveUnion` - encapsulates all primitive value types (`int`, `float`, `bool`)
2. `ValueUnion`- encapsulates common value types in Unity
3. `AnimatorParameterUnion`- encapsulates common mecanim animator parameter types in Unity
4. `DefaultRangeUnion` - encapsulates Vector2 ranges for both float and int

**Warning**: These structures are defined plainly and without protection. They are `internal` and are not meant to be used or accessed directly. All "Any-Types" are essentially wrappers around these structures, and provide a safer interface into these unions. 


### Related: Generic `Blackboard<T>` (Separate Package)


While `Blackboard<T>` lives in its own package, here is the typical usage pattern with the types from this package:

```csharp  
Blackboard<AnyValue> animatorBlackboard = new();  
Blackboard<AnyPrimitive> crowdBlackboard = new();  
  
// Example usage  
animatorBlackboard.Set(Animator.StringToHash("MoveSpeed"), AnyValue.From(5.2f));  
Vector2 blendDir = animatorBlackboard.Get<Vector2>(Animator.StringToHash("BlendDirection"));  
```  

This design keeps your animation hot paths lightweight while still allowing other systems to use the same ecosystem.
  
---  

## **Extending the API**

Want to support custom types (e.g., lets say... `Vector3` for use with Animators)?

Extend the library takes a few steps, but generally it's easy in the current architecture.

You actually have two options, technically (the second of which is better, but we will get to that later):
1. Modify an existing tagged union
2. Extend an existing tagged union by creating another one that has it as a member.

For a full guide on the extension process, see the _Extending Tagged Unions.md_ documentation contained in this package (under the Documentation folder). Happy Extending!! 
