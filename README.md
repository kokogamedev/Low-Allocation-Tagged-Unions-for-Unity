# **TaggedUnion** — Low-Allocation Tagged Unions for Unity

**Package:** `com.psigenvision.taggedunion`  
**Version:** 1.0.0  
**Namespace:** `PsigenVision.TaggedUnion`

This package provides memory-efficient, zero-boxing, type-safe **tagged unions** (also called variants or discriminated unions) for Unity. It was inspired by Youtuber git-amend's **C# Blackboard for Unity** tutorial. Check out the original work at [Unity-Behaviour-Trees on GitHub](https://github.com/adammyhre/Unity-Behaviour-Trees/tree/master).

**NOTE**: This package has a depenency on the ["com.psigenvision.editorscriptingutils"](https://github.com/kokogamedev/Unity-Editor-Scripting-Utils) package.

---

## **Features**

1. **Zero Boxing & Allocation-Free Hot Paths**:
    - Minimizes garbage collector pressure.

2. **Specialized Types** for Focused Performance:
    - `AnyPrimitive` (primitives).
    - `AnyValue` (primitives + Unity value types like `Vector3`, `Quaternion`).
    - `AnyAnimatorParamValue` (specialized for Unity’s Animator).
    - `AnyRange` (value ranges like `Vector2` and `Vector2Int`).
    - `AnyPrimitiveLiteral`, `AnyLiteral`, and `AnyAnimatorParamLiteral` (string-inclusive types for tags or states).


3. **Inspector-First Workflow**:
    - Built-in `PropertyDrawer` for Unity Inspector compatibility.


4. **Compact & Scalable** Memory:
    - Suitable for systems managing large-scale entity states.

---

## **Core Types**

### **1. AnyPrimitive**
- Minimal struct for `int`, `float`, `bool`. (~8–12 bytes).

#### Example:
```csharp
AnyPrimitive health = AnyPrimitive.From(100);
int hp = health; // Implicit conversion
```

---

### **2. AnyValue**
- Workhorse for systems supporting Unity types like `Vector3`. (~16–20 bytes).

#### Example:
```csharp
AnyValue blendParam = AnyValue.From(new Vector2(0.8f, 0.3f));
Vector2 direction = blendParam; // Implicit conversion
```

---

### **3. AnyAnimatorParamValue**
- Specialized tagged union for Unity Animator parameters (`int`, `float`, `bool`). (~16–20 bytes).
- Optimized for systems like Mecanim blackboards.

#### Example:
```csharp
AnyAnimatorParamValue param = AnyAnimatorParamValue.From(1.0f);
float blend = param; // Implicit conversion
```

---

### **4. AnyPrimitiveLiteral**
- Adds rare string support. (~16–20 bytes).

#### Example:
```csharp
AnyPrimitiveLiteral playerName = AnyPrimitiveLiteral.From("Player");
string name = playerName; // Implicit conversion
```

---

### **5. AnyLiteral**
- Comprehensive union supporting strings + Unity types. (~24–28 bytes).

#### Example:
```csharp
AnyLiteral param = AnyLiteral.From("AttackState");
string state = param; // Implicit conversion
```

---

### **6. AnyAnimatorParamLiteral**
- Combines string support with Animator parameter functionality.
- Use this for animators requiring both parameter names and tagged data. (~24–28 bytes).

#### Example:
```csharp
AnyAnimatorParamLiteral animState = AnyAnimatorParamLiteral.From("Running");
string state = animState; // Implicit conversion
```

---

### **7. AnyRange**
- Stores value ranges (`Vector2`, `Vector2Int`). Perfect for systems requiring movement ranges, zoom ranges, or animation constraints. (~16–20 bytes).

#### Example:
```csharp
AnyRange zoomRange = AnyRange.From(new Vector2(1.0f, 5.0f));
Vector2 range = zoomRange; // Implicit conversion
```

---

## **Inspector Support**

Includes **PropertyDrawer** for Unity Inspector:
- Type dropdown (e.g., `Float`, `Vector3`).
- Displays only active value fields.

---

## **Example Use Case**

A simple **Blackboard** for Unity Animators:
```csharp
Blackboard<AnyAnimatorParamValue> animatorBlackboard = new();
animatorBlackboard.Set(Animator.StringToHash("MoveSpeed"), AnyAnimatorParamValue.From(5.2f));
float speed = animatorBlackboard.Get<float>(Animator.StringToHash("MoveSpeed"));
```

Another example with `AnyRange`:
```csharp
Blackboard<AnyRange> movementRange = new();
movementRange.Set("ZoomLevel", AnyRange.From(new Vector2(2.0f, 10.0f)));
Vector2 zoom = movementRange.Get<Vector2>("ZoomLevel");
```

---

## **Installation**

1. Install ["com.psigenvision.editorscriptingutils"](https://github.com/kokogamedev/Unity-Editor-Scripting-Utils) dependency via Git URL or local Unity folder.
2. Add this package via Git URL or local Unity folder.
2. Include the namespace:
    ```csharp
    using PsigenVision.TaggedUnion;
    ```
3. Start building scalable systems. Example:
    ```csharp
    AnyPrimitive health = AnyPrimitive.From(10);
    int healthRemaining = health; // Implicit conversion back to int
    ```

---

## **When to Use?**

Use **TaggedUnion** if:
- Performance and GC minimization are priorities.
- You need scalable blackboards for AI, animation, or crowd systems.
- You want to avoid `object` or inefficient generic collections.

---

## **Extending the API**

Want to support custom types (e.g., `Color` for Shader Graphs)?  
Extend the library easily!

1. Add the type to the enum (`ValueType`, `AnimatorValueType`, etc.).
2. Define it in the tagged union structs.
3. Create implicit operators and factory methods (`From()`).

Example adding `Color`:
```csharp
public enum AnimatorValueType
{
    None = 0, Int, Float, Color
}

[StructLayout(LayoutKind.Explicit)]
struct AnimatorValueUnion
{
    [FieldOffset(0)] public float floatValue;
    [FieldOffset(0)] public bool boolValue;
    [FieldOffset(0)] public Vector2 vector2Value;
    [FieldOffset(0)] public Color colorValue;
}
```

### **Optimized for Unity, with ❤️**

Feel free to extend this package for custom requirements!!. 🚀