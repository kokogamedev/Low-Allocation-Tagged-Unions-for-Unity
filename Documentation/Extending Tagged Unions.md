# **Extending  Tagged Union API**

Want to support custom types (e.g., let's say... `Vector3` for use with Animators)?

Extending the library takes a some steps, but generally it's easy in the current architecture. 

---
**_CAUTION_**: 

_This guide generally focuses on value types as members of tagged unions. Reference types do not play nice being at the same memory location as other types. With reference types, some additional byte-shifting is necessary within the struct layout._ 

_In fact, it is usually smarter to place reference type members before value type members, and byte-shift all value type members away from the reference type. However, for simplicity, we won't cover that in this example. Generally, you don't want to mix reference types with unions anyway if you can help it._

_However, if you want an example of placing reference types inside unions, check out any of the `Literal` tagged unions contained in the API. Inclusion of the `string` member demonstrates what is necessary for including reference types in a tagged union._ 

---
You actually have two options, technically (the second of which is better, but we will get to that later):
1. Modify an existing tagged union
2. Extend an existing tagged union by creating another one that has it as a member.

As a preface, going with option 1 on a tagged union native to this API can lead to unexpected side effects that proceed simply from the fact that you are modifying another API! It is always better to extend than to modify. HOWEVER, you can absolutely follow the steps for modifying an existing tagged union if the tagged union you are modifying is your own extension. Due to the nature of this architecture, if you follow the steps appropriately, the modification process follows precisely the same rules every time!

## Steps for Modifying an Existing Union
1. Define it in the tagged union structs.
2. Add the type to the enum (`ValueType`).
3. Create implicit operators and factory methods (`From()`).
4. If you want to take advantage of the property drawer, modify the AnyType dictionary in the AnyUnionDrawer as indicated by the examples.

### Example adding `Vector3`:
#### _In AnimatorValueUnion_
##### 1. Add the new type as a member, `Vector 3 vector3Value`, within the associated internal union struct, `AnimatorValueUnion`.
```csharp
[StructLayout(LayoutKind.Explicit)]
struct AnimatorValueUnion
{
    [FieldOffset(0)] public float floatValue;
    [FieldOffset(0)] public bool boolValue;
    [FieldOffset(0)] public Vector2 vector2Value;
    [FieldOffset(0)] public Vector3 vector3Value;
}
```

#### _In AnyAnimatorParamValue:_

##### 2. Change `ValueType` enum
```csharp
public enum ValueType
{
    None = 0, Int, Float, Vector2, Vector3
}
```

##### 3. Add factory methods and implicit operators to `AnyAnimatorParamValue`
```csharp
//Implicit operator
public static implicit operator bool(AnyAnimatorParamValue p) => p.Convert<Vector3>();

//Add case to Convert<T> method's switch statement
  private T Convert<T>() => type switch
  {
      ValueType.Int when typeof(T) == typeof(int)
          => Unsafe.As<int, T>(ref value.intValue),
      
      //other cases here
      
      ValueType.Vector3 when typeof(T) == typeof(Vector3)
          => Unsafe.As<Vector3, T>(ref value.vector3Value),
      
      _ => default! //! is the null-forgiving operator, and suppresses nullability warnings
  };
  
  //Add factory method
  public static AnyAnimatorParamValue From(Vector3 v) => new() { type = ValueType.Vector3, value = { vector3Value = v } };
```

#### _In AnyUnionDrawer_:

##### 4. Add case to property drawer's `AnyTypeData` dictionary
```csharp
private readonly Dictionary<Type, Dictionary<int, (bool isNested, string path)>> AnyTypeData = new() 
{
    {
        typeof(AnyAnimatorParamValue), new ()
        {
            //entries for other options here (currently only entries for enum index 1, 2, 3, so add fourth)
            {4, (isNested: false, path: value.vector3Value)}
        }
    }
    
}
```

That should introduce Vector3 as an option in the `AnyAnimatorParamValue` tagged union.

### **WARNING:** Modification is not Preferred over Creating New Structure
The previous example, while it seems simple enough, introduced a problem... you don't know what other Any-Types/Tagged Unions use that structure! Inadvertently, you accidentally messed with `AnyAnimatorParamLiteral`, and that would no longer function properly if you attempted to use it.

**Solution:** Take a lesson from the `AnyAnimatorParamLiteral` tagged union - create a new "Any-Type" tagged  union that has `AnyAnimatorParamValue` as a member, and introduce a new `Vector3 vector3Value` at the same field offset as that member.

Admittedly, this process is more intimidating. You would have to code that new structure, using the other tagged unions as a template, and then add it as  a new entry to the drawer. However, extending an already existing tagged union is actually easier than creating a new one from scratch. The property drawer actually supports nesting for serialization out of the box (recall that strange `isNested` tuple parameter in the `AnyTypeData` struct? That is what it is for!).

### Steps for Extending an Existing Tagged Union
1. Create a new structure that is `Serializable` and explicitly managed and packed (
   `[StructLayout(LayoutKind.Explicit, Pack = 1)]`).
2. Implement the required structure for a tagged union in this architecture:
   1. ValueType enum (inherit this from `byte` for maximum memory-saving, which is the goal of unions, but be aware that CPU performance may drop slightly as modern processors are optimized for 32-bit operations) that has the same members in the same order as the tagged union's enum that you are extending. Add your own new types to the end.
   2. _Private Serialized_ field member of ValueType called `type` at `FieldOffset[0]` alongside a public getter property named `Type`.
   3. _Private Serialized_ field member of the tagged union we are extending (usually I call it `value` for consistency) at `[FieldOffset(1)]` if your `ValueType` inherited from `byte` (and `FieldOffset[4]` otherwise).
   4. _Private Serialized_ field members for your new types each at the same field offset, which is actually the precise byte location of the instance of the tagged union you are extending. 
      * **_CAUTION_**: This only applies to value types. 
   5. Implicit operators for each of your intended types
   6. Factory methods for each of your intended types
   7. `SetType` method that is basically identical to those of the other tagged unions, except you must also reset the values of the new type-members you have added.
   8. (Optional) Introduce a `Clear` method that sets all internal members to default. This is not essential, but its handy.
3. Add a new entry into the `AnyUnionDrawer`  property drawer's `AnyTypeData` dictionary for your new tagged union, and also make sure to add `[CustomPropertyDrawer(typeof(YourTaggedUnion))]` to the top!


_Now, I realize that was insane compared to the first option... but trust me it is safer, and isn't as bad as it looks! See the example below!_

#### **Example with Adding Vector3 to Animator Parameter Tagged Union**

Let's follow that procedure for the case of adding a `Vector3` type to the animator parameter tagged union. This time, lets make our own, `AnyAnimatorParam3DValue`!

##### _1. Create `AnyAnimatorParam3DValue` that is `Serializable` and explicitly managed and packed - `[StructLayout(LayoutKind.Explicit, Pack = 1)]`)._

```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
    }
}
```

##### 2. Implement the required structure for a tagged union in this architecture:
   **_2.1. ValueType enum (inherit this from `byte` for maximum memory-saving if your enum won't contain more than 8) that has the same members in the same order as the tagged union's enum that you are extending. Add your own new types to the end._**
```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
        //Match the order of AnyAnimatorParamValue.ValueType and add Vector3 to the end
        public enum ValueType : byte
        {
            None = 0, Float, Bool, Vector2,
            Vector3
        }
    }
}
```


**_2.2. Private Serialized field member of ValueType called `type` at `FieldOffset[0]` alongside a public getter property named `Type`._**
```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
        public enum ValueType : byte
        {
            None = 0, Float, Bool, Vector2,
            Vector3
        }
        
        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code
    }
}
```


**_2.3. Private Serialized field member of the tagged union we are extending (usually I call it `value` for consistency) at `[FieldOffset(1)]` if your `ValueType` inherited from `byte` (and `FieldOffset[4]` otherwise)._**
```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
        public enum ValueType : byte
        {
            None = 0, Float, Bool, Vector2,
            Vector3
        }
        
        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to 
        [FieldOffset(1), SerializeField] private AnyAnimatorParamValue value; //extend this tag union!
    }
}
```

**_2.4. _Private Serialized_ field members for your new types each at the same field offset possessed by the instance of the tagged union we are extending_**

This is cake, as the professionals say. Simply add each of your new types as member fields with the same FieldOffset as the tagged union member, so `vector3Value` will have an offset of 1!


```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
        public enum ValueType : byte
        {
            None = 0, Float, Bool, Vector2,
            Vector3
        }
        
        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code
        [FieldOffset(1), SerializeField] private AnyAnimatorParamValue value; //extend this tag union!
        [FieldOffset(1), SerializeField] private vector3 vector3Value; //our added type!!
    }
}
```

**_2.5. Implicit operators for each of your intended types_**

This part seems scary at first, but because of how this architecture is built (these implicit operators exactly, actually), the implicit operator definition for every type contained in the tagged union we are extending is precisely the same formula:

`public static implicit operator TYPE(OurNewTaggedUnionType v) => v.value;`

All others follow this pattern:

`public static implicit operator TYPE(OurNewTaggedUnionType v) => v.TYPENAME;`

```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
        public enum ValueType : byte
        {
            None = 0, Float, Bool, Vector2,
            Vector3
        }
        
        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code
        [FieldOffset(1), SerializeField] private AnyAnimatorParamValue value; //extend this tag union!
        [FieldOffset(1), SerializeField] private vector3 vector3Value; //our added type!!
        
        //Introduce implicit operators for the types already contained in the AnyAnimatorParamValue tagged union!
        public static implicit operator float(AnyAnimatorParam3DValue v) => v.value;
        public static implicit operator bool(AnyAnimatorParam3DValue v) => v.value;
        public static implicit operator Vector2(AnyAnimatorParam3DValue v) => v.value;
        
        //Introduce our new type's implicit operator
        public static implicit operator Vector3(AnyAnimatorParam3DValue v) => v.vector3Value;
    }
}
```

**_2.5. Factory methods for each of your intended types_**

Again, these follow a very consistent format, much like our implicit operators did!

The factory method definition for every type contained in the tagged union we are extending is precisely the same formula:

`public static OurNewTaggedUnionType From(TYPE v) => new() { type = ValueType.TYPETAG, value = TaggedUnionWeAreExtending.From(v) };`

All others follow this pattern:

`public static OurNewTaggedUnionType From(TYPE v) => new() { type = ValueType.TYPETAG, value = { typeVariable = v } };`

```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
       public enum ValueType : byte
        {
            None = 0, Float, Bool, Vector2,
            Vector3
        }
        
        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code
        [FieldOffset(1), SerializeField] private AnyAnimatorParamValue value; //extend this tag union!
        [FieldOffset(1), SerializeField] private vector3 vector3Value; //our added type!!
        
        //Introduce implicit operators for the types already contained in the AnyAnimatorParamValue tagged union!
        public static implicit operator float(AnyAnimatorParam3DValue v) => v.value;
        public static implicit operator bool(AnyAnimatorParam3DValue v) => v.value;
        public static implicit operator Vector2(AnyAnimatorParam3DValue v) => v.value;
        
        //Introduce our new type's implicit operator
        public static implicit operator Vector3(AnyAnimatorParam3DValue v) => v.vector3Value;
        
        //Introduce factory methods for the types already contained in the AnyAnimatorParamValue tagged union!
        public static AnyAnimatorParam3DValue From(float v) => new() { type = ValueType.Float, value = AnyAnimatorParamValue.From(v) };
        
        public static AnyAnimatorParam3DValue From(bool v) => new() { type = ValueType.Bool, value = AnyAnimatorParamValue.From(v) };
        
        public static AnyAnimatorParam3DValue From(Vector2 v) => new() { type = ValueType.Vector2, value = AnyAnimatorParamValue.From(v) };
        
        //Introduce our new type's factory method
        public static AnyAnimatorParam3DValue From(Vector3 v) => new() { type = ValueType.Vector3, value = { vector3Value = v } };
    }
}
```

**_2.6. `SetType` method that is basically identical to those of the other tagged unions, except you must also reset the values of the new type-members you have added._**

Honestly, this one is identical for each extended tagged union in terms of format. You can just use the template below and fill in as needed for your use case (your added types).

Template:
```csharp
  public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
  {
      if (!bypassTypeCheck && type == valueType) return;

      if (forceClear)
      {
          yourNewTypeValue = default;
          //^^do this for every new type you added ^^
          value = default;
      }
      else if (type == ValueType.TYPETAG) yourNewTypeValue = default;
      else if ... repeat the pattern...
      else value = default;
      
      type = valueType;
  }
```


```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
         public enum ValueType : byte
        {
            None = 0, Float, Bool, Vector2,
            Vector3
        }
        
        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code
        [FieldOffset(1), SerializeField] private AnyAnimatorParamValue value; //extend this tag union!
        [FieldOffset(1), SerializeField] private vector3 vector3Value; //our added type!!
        
        //... all the rest that was added before here
        
       public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
        {
            if (!bypassTypeCheck && type == valueType) return;
      
            if (forceClear)
            {
                vector3Value = default; //This is our added type
                value = default;
            }
            //Add your one new type below for Vector3
            else if (type == ValueType.Vector3) vector3Value = default;
            else value = default;
            
            type = valueType;
        }
        
    }
}
```

_2.7. (Optional) Introduce a `Clear` method that sets all internal members to default. This is not essential, but its handy._

```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
        //All the rest here
        public void Clear() 
        {
           type = ValueType.None;
           
           vector3Value = default; //This is our added type
           value = default;
           
           //OR you could just call SetType(ValueType.None, false, true), as it is the same thing
        }
    }
}
```

Your final structure, in all its glory, is:

```csharp
namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParam3DValue
    {
       public enum ValueType : byte
        {
            None = 0, Float, Bool, Vector2,
            Vector3
        }
        
        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code
        [FieldOffset(1), SerializeField] private AnyAnimatorParamValue value; //extend this tag union!
        [FieldOffset(1), SerializeField] private vector3 vector3Value; //our added type!!
        
        //Introduce implicit operators for the types already contained in the AnyAnimatorParamValue tagged union!
        public static implicit operator float(AnyAnimatorParam3DValue v) => v.value;
        public static implicit operator bool(AnyAnimatorParam3DValue v) => v.value;
        public static implicit operator Vector2(AnyAnimatorParam3DValue v) => v.value;
        
        //Introduce our new type's implicit operator
        public static implicit operator Vector3(AnyAnimatorParam3DValue v) => v.vector3Value;
        
        //Introduce factory methods for the types already contained in the AnyAnimatorParamValue tagged union!
        public static AnyAnimatorParam3DValue From(float v) => new() { type = ValueType.Float, value = AnyAnimatorParamValue.From(v) };
        
        public static AnyAnimatorParam3DValue From(bool v) => new() { type = ValueType.Bool, value = AnyAnimatorParamValue.From(v) };
        
        public static AnyAnimatorParam3DValue From(Vector2 v) => new() { type = ValueType.Vector2, value = AnyAnimatorParamValue.From(v) };
        
        //Introduce our new type's factory method
        public static AnyAnimatorParam3DValue From(Vector3 v) => new() { type = ValueType.Vector3, value = { vector3Value = v } };
        
       //Introduce factory methods for the types already contained in the AnyAnimatorParamValue tagged union!
        public static AnyAnimatorParam3DValue From(float v) => new() { type = ValueType.Float, value = AnyAnimatorParamValue.From(v) };
        
        public static AnyAnimatorParam3DValue From(bool v) => new() { type = ValueType.Bool, value = AnyAnimatorParamValue.From(v) };
        
        public static AnyAnimatorParam3DValue From(Vector2 v) => new() { type = ValueType.Vector2, value = AnyAnimatorParamValue.From(v) };
        
        //Introduce our new type's factory method
        public static AnyAnimatorParam3DValue From(Vector3 v) => new() { type = ValueType.Vector3, value = { vector3Value = v } };
        
        public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
        {
            if (!bypassTypeCheck && type == valueType) return;
      
            if (forceClear)
            {
                vector3Value = default; //This is our added type
                value = default;
            }
            //Add your one new type below for Vector3
            else if (type == ValueType.Vector3) vector3Value = default;
            else value = default;
            
            type = valueType;
        }
        
        public void Clear() 
        {
           type = ValueType.None;
           
           vector3Value = default; //This is our added type
           value = default;
           
           //OR you could just call SetType(ValueType.None, false, true), as it is the same thing
        }
    }
}
```


##### _3. Add a new entry into the `AnyUnionDrawer`  property drawer's `AnyTypeData` dictionary for your new tagged union, and also make sure to add `[CustomPropertyDrawer(typeof(YourTaggedUnion))]` to the top!_

The property drawer is written to be able to operate generally on any object that follows the above structure! All you have to do is add the configurations to the dictionary! There is a helpful little template included inside it (see below).

```csharp
        private readonly Dictionary<Type, Dictionary<int, (bool isNested, string path)>> AnyTypeData = new()
        {
            //NOTE: If nesting AnyTypes, the index of the ValueType in the outer type MUST match the index of the ValueType in the contained type
            /* 
            {
                typeof(YourAnyType), new ()
                {
                    {inspector enum index,
                    (isNested: is the field contained within another AnyType that is a member of this AnyType struct?,
                    path: the dot separated path to the field to draw)}
                }
            },*/
            ...
        }
```

That is the essential pattern, HOWEVER, it is even easier in the case that you have chosen to extend another tagged union. Again, a pattern emerges.

For every entry that is contained inside the tagged union you are extending, this is its template:

`{ INDEX, (isNested: true, path: "value") },`

where `INDEX` is the integer index that matches the entry in this `AnyTypeData` dictionary for the tagged union we are extending. That just so happens to also be the counted order in the enum (first one has `1`, second one has `2`, etc).

For every entry that is NOT contained, you enter the appropriate index (the order in the enum), and list the dot-separated path to your extended type in the following template:

`{ INDEX, (isNested: false, path: "your.dot.separated.path.to.YOURTYPE") },`

So, for our example, here is the entry!

```csharp
   {
       typeof(AnyAnimatorParam3DValue), new ()
       {
           { 1, (isNested: true, path: "value") },
           { 2, (isNested: true, path: "value") },
           { 3, (isNested: true, path: "value") },
           { 4, (isNested: false, path: "vector3Value") }
       }
   },
```

AND... don't forget to add `[CustomPropertyDrawer(typeof(YourTaggedUnion))]` to the top of the drawer! You would be surprised how easy that is to forget (*sigh*...trust me... *insert flashbacks here*).

**And that is it**. You have now created your own tagged union that will serialize in the inspector properly, extends the already created `AnyAnimatorParamValue` tagged union, and adds a `Vector3` option. Now THIS struct... you can extend and modify to your heart's content, because you are in charge of its "hierarchy." There is no danger in breaking the system.

Happy extending!!