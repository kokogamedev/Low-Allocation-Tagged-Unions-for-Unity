using PsigenVision.TaggedUnion;
using UnityEngine;

namespace PsigenVision.TaggedUnion
{
    public static class TaggedUnionExtensions
    {
        //Converting primitives to AnyPrimitive type (wrapper for C# union)
        public static AnyPrimitive AsPrimitive(this int val) => AnyPrimitive.From(val);  
        public static AnyPrimitive AsPrimitive(this bool val) => AnyPrimitive.From(val); 
        public static AnyPrimitive AsPrimitive(this float val) => AnyPrimitive.From(val);
        
        //Converting value-types to AnyValue types (wrapper for C# union)
        public static AnyValue AsValue(this int val) => AnyValue.From(val);
        public static AnyValue AsValue(this bool val) => AnyValue.From(val);
        public static AnyValue AsValue(this float val) => AnyValue.From(val);
        public static AnyValue AsValue(this Vector2 val) => AnyValue.From(val); 
        public static AnyValue AsValue(this Vector3 val) => AnyValue.From(val);
        public static AnyValue AsValue(this Quaternion val) => AnyValue.From(val);        

        //Converting value-types to AnyAnimatorParamValue types (wrapper for C# union)
        public static AnyAnimatorParamValue AsAnimValue(this bool val) => AnyAnimatorParamValue.From(val);
        public static AnyAnimatorParamValue AsAnimValue(this float val) => AnyAnimatorParamValue.From(val);
        public static AnyAnimatorParamValue AsAnimValue(this Vector2 val) => AnyAnimatorParamValue.From(val); 
        
        //Converting value-types to AnyRange types (wrapper for C# union)
        public static AnyRange AsRange(this Vector2 val) => AnyRange.From(val);
        public static AnyRange AsRange(this Vector2Int val) => AnyRange.From(val);
        
        //Converting value-types and string types to AnyLiteral types (wrapper for C# union)
        public static AnyLiteral AsLiteral(this int val) => AnyLiteral.From(val);
        public static AnyLiteral AsLiteral(this bool val) => AnyLiteral.From(val);
        public static AnyLiteral AsLiteral(this float val) => AnyLiteral.From(val);
        public static AnyLiteral AsLiteral(this Vector2 val) => AnyLiteral.From(val);
        public static AnyLiteral AsLiteral(this Vector3 val) => AnyLiteral.From(val);
        public static AnyLiteral AsLiteral(this Quaternion val) => AnyLiteral.From(val);
        public static AnyLiteral AsLiteral(this string val) => AnyLiteral.From(val);
        
        //Converting value-types and string types to AnyPrimitiveLiteral types (wrapper for C# union)
        public static AnyPrimitiveLiteral AsPrimitiveLiteral(this int val) => AnyPrimitiveLiteral.From(val);
        public static AnyPrimitiveLiteral AsPrimitiveLiteral(this bool val) => AnyPrimitiveLiteral.From(val);
        public static AnyPrimitiveLiteral AsPrimitiveLiteral(this float val) => AnyPrimitiveLiteral.From(val);
        public static AnyPrimitiveLiteral AsPrimitiveLiteral(this string val) => AnyPrimitiveLiteral.From(val);
        
        //Converting value-types and string types to AnyAnimatorParamLiteral types (wrapper for C# union)
        public static AnyAnimatorParamLiteral AsAnimLiteral(this bool val) => AnyAnimatorParamLiteral.From(val);
        public static AnyAnimatorParamLiteral AsAnimLiteral(this float val) => AnyAnimatorParamLiteral.From(val);
        public static AnyAnimatorParamLiteral AsAnimLiteral(this Vector2 val) => AnyAnimatorParamLiteral.From(val); 
        public static AnyAnimatorParamLiteral AsAnimLiteral(this string val) => AnyAnimatorParamLiteral.From(val); 
        
        
        //Converting between Any-types
        // AnyPrimitive → others
        public static AnyValue AsValue(this AnyPrimitive p) => p.Type switch
        {
            AnyPrimitive.ValueType.Int   => AnyValue.From((int)p),
            AnyPrimitive.ValueType.Float => AnyValue.From((float)p),
            AnyPrimitive.ValueType.Bool  => AnyValue.From((bool)p),
            _ => default
        };

        public static AnyPrimitiveLiteral AsPrimitiveLiteral(this AnyPrimitive p) => p.Type switch
        {
            AnyPrimitive.ValueType.Int   => AnyPrimitiveLiteral.From((int)p),
            AnyPrimitive.ValueType.Float => AnyPrimitiveLiteral.From((float)p),
            AnyPrimitive.ValueType.Bool  => AnyPrimitiveLiteral.From((bool)p),
            _ => default
        };
        
        public static AnyLiteral AsLiteral(this AnyPrimitive p) => p.Type switch
        {
            AnyPrimitive.ValueType.Int   => AnyLiteral.From((int)p),
            AnyPrimitive.ValueType.Float => AnyLiteral.From((float)p),
            AnyPrimitive.ValueType.Bool  => AnyLiteral.From((bool)p),
            _ => default
        };
        
        public static AnyAnimatorParamValue AsAnimValue(this AnyPrimitive p) => p.Type switch
        { 
            AnyPrimitive.ValueType.Float => AnyAnimatorParamValue.From((float)p),
            AnyPrimitive.ValueType.Bool  => AnyAnimatorParamValue.From((bool)p),
            _ => default
        };
        
        public static AnyAnimatorParamLiteral AsAnimLiteral(this AnyPrimitive p) => p.Type switch
        { 
            AnyPrimitive.ValueType.Float => AnyAnimatorParamLiteral.From((float)p),
            AnyPrimitive.ValueType.Bool  => AnyAnimatorParamLiteral.From((bool)p),
            _ => default
        };
        

        // AnyValue → others
        public static AnyPrimitive AsPrimitive(this AnyValue v) => v.Type switch
        {
            AnyValue.ValueType.Int   => AnyPrimitive.From((int)v),
            AnyValue.ValueType.Float => AnyPrimitive.From((float)v),
            AnyValue.ValueType.Bool  => AnyPrimitive.From((bool)v),
            _ => default
        };
        
        public static AnyPrimitiveLiteral AsPrimitiveLiteral(this AnyValue v) => v.Type switch
        {
            AnyValue.ValueType.Int   => AnyPrimitiveLiteral.From((int)v),
            AnyValue.ValueType.Float => AnyPrimitiveLiteral.From((float)v),
            AnyValue.ValueType.Bool  => AnyPrimitiveLiteral.From((bool)v),
            _ => default
        };

        public static AnyAnimatorParamValue AsAnimValue(this AnyValue v) => v.Type switch
        { 
            AnyValue.ValueType.Float      => AnyAnimatorParamValue.From((float)v),
            AnyValue.ValueType.Bool       => AnyAnimatorParamValue.From((bool)v),
            AnyValue.ValueType.Vector2    => AnyAnimatorParamValue.From((Vector2)v),
            _ => default
        };
        
        public static AnyRange AsRange(this AnyValue v) => v.Type switch
        { 
            AnyValue.ValueType.Vector2    => AnyRange.From((Vector2)v),
            _ => default
        };

        public static AnyLiteral AsLiteral(this AnyValue v) => v.Type switch
        {
            AnyValue.ValueType.Int        => AnyLiteral.From((int)v),
            AnyValue.ValueType.Float      => AnyLiteral.From((float)v),
            AnyValue.ValueType.Bool       => AnyLiteral.From((bool)v),
            AnyValue.ValueType.Vector2    => AnyLiteral.From((Vector2)v),
            AnyValue.ValueType.Vector3    => AnyLiteral.From((Vector3)v),
            AnyValue.ValueType.Quaternion => AnyLiteral.From((Quaternion)v),
            _ => default
        };
        
        public static AnyAnimatorParamLiteral AsAnimLiteral(this AnyValue v) => v.Type switch
        { 
            AnyValue.ValueType.Float      => AnyAnimatorParamLiteral.From((float)v),
            AnyValue.ValueType.Bool       => AnyAnimatorParamLiteral.From((bool)v),
            AnyValue.ValueType.Vector2    => AnyAnimatorParamLiteral.From((Vector2)v),
            _ => default
        };

        // AnyLiteral → others
        public static AnyPrimitive AsPrimitive(this AnyLiteral l) => l.Type switch
        {
            AnyLiteral.ValueType.Int   => AnyPrimitive.From((int)l),
            AnyLiteral.ValueType.Float => AnyPrimitive.From((float)l),
            AnyLiteral.ValueType.Bool  => AnyPrimitive.From((bool)l),
            _ => default
        };
        
        public static AnyValue AsValue(this AnyLiteral l) => l.Type switch
        {
            AnyLiteral.ValueType.Int        => AnyValue.From((int)l),
            AnyLiteral.ValueType.Float      => AnyValue.From((float)l),
            AnyLiteral.ValueType.Bool       => AnyValue.From((bool)l),
            AnyLiteral.ValueType.Vector2    => AnyValue.From((Vector2)l),
            AnyLiteral.ValueType.Vector3    => AnyValue.From((Vector3)l),
            AnyLiteral.ValueType.Quaternion => AnyValue.From((Quaternion)l),
            _ => default
        };
        
        public static AnyAnimatorParamValue AsAnimValue(this AnyLiteral l) => l.Type switch
        { 
            AnyLiteral.ValueType.Float      => AnyAnimatorParamValue.From((float)l),
            AnyLiteral.ValueType.Bool       => AnyAnimatorParamValue.From((bool)l),
            AnyLiteral.ValueType.Vector2    => AnyAnimatorParamValue.From((Vector2)l),
            _ => default
        };
        
        public static AnyRange AsRange(this AnyLiteral l) => l.Type switch
        { 
            AnyLiteral.ValueType.Vector2    => AnyRange.From((Vector2)l),
            _ => default
        };
        
        public static AnyPrimitiveLiteral AsPrimitiveLiteral(this AnyLiteral l) => l.Type switch
        {
            AnyLiteral.ValueType.Int   => AnyPrimitiveLiteral.From((int)l),
            AnyLiteral.ValueType.Float => AnyPrimitiveLiteral.From((float)l),
            AnyLiteral.ValueType.Bool  => AnyPrimitiveLiteral.From((bool)l),
            AnyLiteral.ValueType.String => AnyPrimitiveLiteral.From((string)l),
            _ => default
        };
        
        public static AnyAnimatorParamLiteral AsAnimLiteral(this AnyLiteral l) => l.Type switch
        { 
            AnyLiteral.ValueType.Float      => AnyAnimatorParamLiteral.From((float)l),
            AnyLiteral.ValueType.Bool       => AnyAnimatorParamLiteral.From((bool)l),
            AnyLiteral.ValueType.Vector2    => AnyAnimatorParamLiteral.From((Vector2)l),
            AnyLiteral.ValueType.String => AnyAnimatorParamLiteral.From((string)l),
            _ => default
        };

        
        // AnyPrimitiveLiteral → others
        public static AnyPrimitive AsPrimitive(this AnyPrimitiveLiteral l) => l.Type switch
        {
            AnyPrimitiveLiteral.ValueType.Int   => AnyPrimitive.From((int)l),
            AnyPrimitiveLiteral.ValueType.Float => AnyPrimitive.From((float)l),
            AnyPrimitiveLiteral.ValueType.Bool  => AnyPrimitive.From((bool)l),
            _ => default
        };
        
        public static AnyValue AsValue(this AnyPrimitiveLiteral l) => l.Type switch
        {
            AnyPrimitiveLiteral.ValueType.Int   => AnyValue.From((int)l),
            AnyPrimitiveLiteral.ValueType.Float => AnyValue.From((float)l),
            AnyPrimitiveLiteral.ValueType.Bool  => AnyValue.From((bool)l),
            _ => default
        };
        
        public static AnyLiteral AsLiteral(this AnyPrimitiveLiteral l) => l.Type switch
        {
            AnyPrimitiveLiteral.ValueType.Int   => AnyLiteral.From((int)l),
            AnyPrimitiveLiteral.ValueType.Float => AnyLiteral.From((float)l),
            AnyPrimitiveLiteral.ValueType.Bool  => AnyLiteral.From((bool)l),
            AnyPrimitiveLiteral.ValueType.String => AnyLiteral.From((string)l),
            _ => default
        };
        
        public static AnyAnimatorParamValue AsAnimValue(this AnyPrimitiveLiteral l) => l.Type switch
        { 
            AnyPrimitiveLiteral.ValueType.Float => AnyAnimatorParamValue.From((float)l),
            AnyPrimitiveLiteral.ValueType.Bool  => AnyAnimatorParamValue.From((bool)l),
            _ => default
        };
        
        public static AnyAnimatorParamLiteral AsAnimLiteral(this AnyPrimitiveLiteral l) => l.Type switch
        { 
            AnyPrimitiveLiteral.ValueType.Float => AnyAnimatorParamLiteral.From((float)l),
            AnyPrimitiveLiteral.ValueType.Bool  => AnyAnimatorParamLiteral.From((bool)l),
            AnyPrimitiveLiteral.ValueType.String => AnyAnimatorParamLiteral.From((string)l),
            _ => default
        };
        
        // AnyAnimatorParamValue → others
        public static AnyPrimitive AsPrimitive(this AnyAnimatorParamValue v) => v.Type switch
        {
            AnyAnimatorParamValue.ValueType.Float      => AnyPrimitive.From((float)v),
            AnyAnimatorParamValue.ValueType.Bool       => AnyPrimitive.From((bool)v),
            _ => default
        };
        
        public static AnyPrimitiveLiteral AsPrimitiveLiteral(this AnyAnimatorParamValue v) => v.Type switch
        {
            AnyAnimatorParamValue.ValueType.Float      => AnyPrimitiveLiteral.From((float)v),
            AnyAnimatorParamValue.ValueType.Bool       => AnyPrimitiveLiteral.From((bool)v),
            _ => default
        };

        public static AnyValue AsValue(this AnyAnimatorParamValue v) => v.Type switch
        { 
            AnyAnimatorParamValue.ValueType.Float      => AnyValue.From((float)v),
            AnyAnimatorParamValue.ValueType.Bool       => AnyValue.From((bool)v),
            AnyAnimatorParamValue.ValueType.Vector2    => AnyValue.From((Vector2)v),
            _ => default
        };
        
        public static AnyRange AsRange(this AnyAnimatorParamValue v) => v.Type switch
        { 
            AnyAnimatorParamValue.ValueType.Vector2    => AnyRange.From((Vector2)v),
            _ => default
        };

        public static AnyLiteral AsLiteral(this AnyAnimatorParamValue v) => v.Type switch
        {
            AnyAnimatorParamValue.ValueType.Float      => AnyLiteral.From((float)v),
            AnyAnimatorParamValue.ValueType.Bool       => AnyLiteral.From((bool)v),
            AnyAnimatorParamValue.ValueType.Vector2    => AnyLiteral.From((Vector2)v),
            _ => default
        };
        
        public static AnyAnimatorParamLiteral AsAnimLiteral(this AnyAnimatorParamValue v) => v.Type switch
        { 
            AnyAnimatorParamValue.ValueType.Float      => AnyAnimatorParamLiteral.From((float)v),
            AnyAnimatorParamValue.ValueType.Bool       => AnyAnimatorParamLiteral.From((bool)v),
            AnyAnimatorParamValue.ValueType.Vector2    => AnyAnimatorParamLiteral.From((Vector2)v),
            _ => default
        };
        
        // AnyRange → others
        public static AnyValue AsValue(this AnyRange v) => v.Type switch
        { 
            AnyRange.ValueType.Float    => AnyValue.From((Vector2)v),
            _ => default
        };
        
        public static AnyLiteral AsLiteral(this AnyRange v) => v.Type switch
        {
            AnyRange.ValueType.Float    => AnyLiteral.From((Vector2)v),
            _ => default
        };
        
        public static AnyAnimatorParamLiteral AsAnimLiteral(this AnyRange v) => v.Type switch
        { 
            AnyRange.ValueType.Float    => AnyAnimatorParamLiteral.From((Vector2)v),
            _ => default
        };
        
        public static AnyAnimatorParamValue AsAnimValue(this AnyRange v) => v.Type switch
        { 
            AnyRange.ValueType.Float    => AnyAnimatorParamValue.From((Vector2)v),
            _ => default
        };        
        
        // AnyAnimatorParamLiteral → others
        public static AnyPrimitive AsPrimitive(this AnyAnimatorParamLiteral v) => v.Type switch
        {
            AnyAnimatorParamLiteral.ValueType.Float      => AnyPrimitive.From((float)v),
            AnyAnimatorParamLiteral.ValueType.Bool       => AnyPrimitive.From((bool)v),
            _ => default
        };
        
        public static AnyPrimitiveLiteral AsPrimitiveLiteral(this AnyAnimatorParamLiteral v) => v.Type switch
        {
            AnyAnimatorParamLiteral.ValueType.Float      => AnyPrimitiveLiteral.From((float)v),
            AnyAnimatorParamLiteral.ValueType.Bool       => AnyPrimitiveLiteral.From((bool)v),
            AnyAnimatorParamLiteral.ValueType.String       => AnyPrimitiveLiteral.From((string)v),
            _ => default
        };

        public static AnyValue AsValue(this AnyAnimatorParamLiteral v) => v.Type switch
        { 
            AnyAnimatorParamLiteral.ValueType.Float      => AnyValue.From((float)v),
            AnyAnimatorParamLiteral.ValueType.Bool       => AnyValue.From((bool)v),
            AnyAnimatorParamLiteral.ValueType.Vector2    => AnyValue.From((Vector2)v),
            _ => default
        };
        
        public static AnyRange AsRange(this AnyAnimatorParamLiteral v) => v.Type switch
        { 
            AnyAnimatorParamLiteral.ValueType.Vector2    => AnyRange.From((Vector2)v),
            _ => default
        };

        public static AnyLiteral AsLiteral(this AnyAnimatorParamLiteral v) => v.Type switch
        {
            AnyAnimatorParamLiteral.ValueType.Float      => AnyLiteral.From((float)v),
            AnyAnimatorParamLiteral.ValueType.Bool       => AnyLiteral.From((bool)v),
            AnyAnimatorParamLiteral.ValueType.Vector2    => AnyLiteral.From((Vector2)v),
            AnyAnimatorParamLiteral.ValueType.String       => AnyLiteral.From((string)v),
            _ => default
        };
        
        public static AnyAnimatorParamValue AsAnimValue(this AnyAnimatorParamLiteral v) => v.Type switch
        { 
            AnyAnimatorParamLiteral.ValueType.Float      => AnyAnimatorParamValue.From((float)v),
            AnyAnimatorParamLiteral.ValueType.Bool       => AnyAnimatorParamValue.From((bool)v),
            AnyAnimatorParamLiteral.ValueType.Vector2    => AnyAnimatorParamValue.From((Vector2)v),
            _ => default
        };        
        
        //
        /*
            **union tricks only help with value types**.

            - Value types (`int`, `float`, `Vector3`, `Quaternion`, etc.) have a fixed, known size in memory. We can safely make them **overlap** using explicit layout.
            - **Reference types** (`string`, `UnityEngine.Object`, `Component`, `Material`, etc.) are **pointers** (8 bytes on 64-bit). More importantly, the CLR (and Unity's serializer / GC) has strict rules about overlapping reference types with value types or with each other in explicit layouts. Doing so can lead to:
              - Garbage collection issues
              - Serialization bugs
              - Undefined behavior in IL2CPP / Burst
              - Crashes or corrupted data

            **Rule of thumb:**
            - Use explicit layout + `FieldOffset(0)` **only** for value types.
            - For reference types, just use normal sequential layout and store the fields normally.

            In `AnyReference`, we only ever store **one** reference at a time (enforced by the `type` enum), but we don't gain anything by forcing them to overlap in memory. The memory savings would be negligible anyway (you're still paying for the largest reference pointer + the enum).
         */
    }
}