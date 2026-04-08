using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using PsigenVision.TaggedUnion.UnsafeBackend;

namespace PsigenVision.TaggedUnion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyPrimitive
    {
        public enum ValueType : byte
        {
            None = 0,
            Int,
            Float,
            Bool
        } //Inheriting a C# enum from byte changes its underlying type from the default int (32-bit) to byte (8-bit). This modification affects storage, range, and interoperability

        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code

        // Private union (not directly accessible) 
        [FieldOffset(1), SerializeField] private PrimitiveUnion value;

        // Implicit conversions (zero-boxing)    
        public static implicit operator int(AnyPrimitive p) => p.Convert<int>();
        public static implicit operator float(AnyPrimitive p) => p.Convert<float>();
        public static implicit operator bool(AnyPrimitive p) => p.Convert<bool>();

        // Safe conversion (zero-boxing, returns default on type mismatch)
        //The typeof(T) check is done at compile time for each concrete call site (the JIT can often eliminate the entire branch).
        //Unsafe.As is the lowest-level, most optimizable way to reinterpret memory.
        private T Convert<T>() => type switch
        {
            ValueType.Int when typeof(T) == typeof(int)
                => Unsafe.As<int, T>(ref value.intValue),

            ValueType.Float when typeof(T) == typeof(float)
                => Unsafe.As<float, T>(ref value.floatValue),

            ValueType.Bool when typeof(T) == typeof(bool)
                => Unsafe.As<bool, T>(ref value.boolValue),
            
            _ => default! //! is the null-forgiving operator, and suppresses nullability warnings
        };

        // Factory methods    
        public static AnyPrimitive From(int v) => new() { type = ValueType.Int, value = { intValue = v } };
        public static AnyPrimitive From(float v) => new() { type = ValueType.Float, value = { floatValue = v } };
        public static AnyPrimitive From(bool v) => new() { type = ValueType.Bool, value = { boolValue = v } };

        public void Clear()
        {
            value = default;
            type = ValueType.None;
        }
        public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
        {
            if (!bypassTypeCheck && type == valueType) return;
            //Debug.Log($"SetType called on {this.GetType().Name} with {valueType}");
            value = default;
            type = valueType;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyPrimitiveLiteral
    {
        public enum ValueType : byte
        {
            None = 0,
            Int,
            Float,
            Bool,
            String
        } //Inheriting a C# enum from byte changes its underlying type from the default int (32-bit) to byte (8-bit). This modification affects storage, range, and interoperability

        // 1. Put the Reference Type at Offset 0. 
        // This is the most reliable way to avoid alignment errors.
        [FieldOffset(0), SerializeField] private string stringValue; 

        // 2. Put the Value Types AFTER the reference type.
        // On 64-bit systems, a string pointer is 8 bytes. 
        // So we start the next field at offset 8.
        [FieldOffset(8), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code
        
        // Private union (not directly accessible) 
	    [FieldOffset(9), SerializeField] private AnyPrimitive value; // value types overlap here
        
        // Implicit operators
        public static implicit operator int(AnyPrimitiveLiteral v) => v.value;
        public static implicit operator float(AnyPrimitiveLiteral v) => v.value;
        public static implicit operator bool(AnyPrimitiveLiteral v) => v.value;
        public static implicit operator string(AnyPrimitiveLiteral v) => v.stringValue;


        // Factory methods
        public static AnyPrimitiveLiteral From(int v) => new() { type = ValueType.Int, value = AnyPrimitive.From(v) };

        public static AnyPrimitiveLiteral From(float v) =>
            new() { type = ValueType.Float, value = AnyPrimitive.From(v) };

        public static AnyPrimitiveLiteral From(bool v) => new() { type = ValueType.Bool, value = AnyPrimitive.From(v) };
        public static AnyPrimitiveLiteral From(string v) => new() { type = ValueType.String, stringValue = v };

        public void Clear()
        {
            if (type == ValueType.String) stringValue = default;
            else value = default;
            type = ValueType.None;
        }
        
        public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
        {
            if (!bypassTypeCheck && type == valueType) return;
            //Debug.Log($"SetType called on {this.GetType().Name} with {valueType}");

            if (forceClear)
            {
                stringValue = default;
                value = default;
            }
            else if (type == ValueType.String) stringValue = default;
            else value = default;
            
            type = valueType;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyValue
    {
        public enum ValueType : byte
        {
            None = 0,
            Int,
            Float,
            Bool,
            Vector2,
            Vector3,
            Quaternion,
            Color
        } //Inheriting a C# enum from byte changes its underlying type from the default int (32-bit) to byte (8-bit). This modification affects storage, range, and interoperability

        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code

        // Private union (not directly accessible) 
	    [FieldOffset(1), SerializeField] private ValueUnion value; // no string here → smaller!

        // Implicit conversions (zero-boxing)    
        public static implicit operator int(AnyValue v) => v.Convert<int>();
        public static implicit operator float(AnyValue v) => v.Convert<float>();
        public static implicit operator bool(AnyValue v) => v.Convert<bool>();
        public static implicit operator Vector2(AnyValue v) => v.Convert<Vector2>();
        public static implicit operator Vector3(AnyValue v) => v.Convert<Vector3>();
        public static implicit operator Quaternion(AnyValue v) => v.Convert<Quaternion>();
        public static implicit operator Color(AnyValue v) => v.Convert<Color>();

        // Safe conversion (zero-boxing, returns default on type mismatch)
        //The typeof(T) check is done at compile time for each concrete call site (the JIT can often eliminate the entire branch).
        //Unsafe.As is the lowest-level, most optimizable way to reinterpret memory.
        private T Convert<T>() => type switch
        {
            ValueType.Int when typeof(T) == typeof(int)
                => Unsafe.As<int, T>(ref value.intValue),

            ValueType.Float when typeof(T) == typeof(float)
                => Unsafe.As<float, T>(ref value.floatValue),

            ValueType.Bool when typeof(T) == typeof(bool)
                => Unsafe.As<bool, T>(ref value.boolValue),

            ValueType.Vector2 when typeof(T) == typeof(Vector2)
                => Unsafe.As<Vector2, T>(ref value.vector2Value),

            ValueType.Vector3 when typeof(T) == typeof(Vector3)
                => Unsafe.As<Vector3, T>(ref value.vector3Value),

            ValueType.Quaternion when typeof(T) == typeof(Quaternion)
                => Unsafe.As<Quaternion, T>(ref value.quaternionValue),

            ValueType.Quaternion when typeof(T) == typeof(Color)
                => Unsafe.As<Color, T>(ref value.colorValue),

            _ => default!
        };

        // Factory methods     
        public static AnyValue From(int v) => new() { type = ValueType.Int, value = { intValue = v } };
        public static AnyValue From(float v) => new() { type = ValueType.Float, value = { floatValue = v } };
        public static AnyValue From(bool v) => new() { type = ValueType.Bool, value = { boolValue = v } };
        public static AnyValue From(Vector2 v) => new() { type = ValueType.Vector2, value = { vector2Value = v } };
        public static AnyValue From(Vector3 v) => new() { type = ValueType.Vector3, value = { vector3Value = v } };

        public static AnyValue From(Quaternion q) =>
            new() { type = ValueType.Quaternion, value = { quaternionValue = q } };

        public static AnyValue From(Color c) => new() { type = ValueType.Color, value = { colorValue = c } };

        public void Clear()
        {
            value = default;
            type = ValueType.None;
        }
        
        public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
        {
            //Debug.Log($"SetType called on {this.GetType().Name} with {valueType}");
            if (!bypassTypeCheck && type == valueType) return;
            value = default;
            type = valueType;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyLiteral
    {
        public enum ValueType : byte
        {
            None = 0,
            Int,
            Float,
            Bool,
            Vector2,
            Vector3,
            Quaternion,
            Color,
            String
        } //Inheriting a C# enum from byte changes its underlying type from the default int (32-bit) to byte (8-bit). This modification affects storage, range, and interoperability

        // 1. Put the Reference Type at Offset 0. 
        // This is the most reliable way to avoid alignment errors.
        [FieldOffset(0), SerializeField] private string stringValue; 

        // 2. Put the Value Types AFTER the reference type.
        // On 64-bit systems, a string pointer is 8 bytes. 
        // So we start the next field at offset 8.
        [FieldOffset(8), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code

        // Private union (not directly accessible) 
        [FieldOffset(9), SerializeField] private AnyValue value; // value types overlap here

        // Implicit operators
        public static implicit operator int(AnyLiteral l) => l.value;
        public static implicit operator float(AnyLiteral l) => l.value;
        public static implicit operator bool(AnyLiteral l) => l.value;
        public static implicit operator string(AnyLiteral l) => l.stringValue;
        public static implicit operator Vector2(AnyLiteral l) => l.value;
        public static implicit operator Vector3(AnyLiteral l) => l.value;
        public static implicit operator Quaternion(AnyLiteral l) => l.value;
        public static implicit operator Color(AnyLiteral l) => l.value;

        // Factory methods
        public static AnyLiteral From(int v) => new() { type = ValueType.Int, value = AnyValue.From(v) };
        public static AnyLiteral From(float v) => new() { type = ValueType.Float, value = AnyValue.From(v) };
        public static AnyLiteral From(bool v) => new() { type = ValueType.Bool, value = AnyValue.From(v) };
        public static AnyLiteral From(string str) => new() { type = ValueType.String, stringValue = str };
        public static AnyLiteral From(Vector2 v) => new() { type = ValueType.Vector2, value = AnyValue.From(v) };
        public static AnyLiteral From(Vector3 v) => new() { type = ValueType.Vector3, value = AnyValue.From(v) };
        public static AnyLiteral From(Quaternion q) => new() { type = ValueType.Quaternion, value = AnyValue.From(q) };

        public static AnyLiteral From(Color c) => new() { type = ValueType.Color, value = AnyValue.From(c) };

        public void Clear()
        {
            if (type == ValueType.String) stringValue = default;
            else value = default;
            type = ValueType.None;
        }
        
        public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
        {
            if (!bypassTypeCheck && type == valueType) return;
            //Debug.Log($"SetType called on {this.GetType().Name} with {valueType}");

            if (forceClear)
            {
                stringValue = default;
                value = default;
            }
            else if (type == ValueType.String) stringValue = default;
            else value = default;
            
            type = valueType;
        }
    }
    
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParamValue
    {
        public enum ValueType : byte
        {
            None = 0,
            Float,
            Bool,
            Vector2
        } //Inheriting a C# enum from byte changes its underlying type from the default int (32-bit) to byte (8-bit). This modification affects storage, range, and interoperability

        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code

        // Private union (not directly accessible) 
	    [FieldOffset(1), SerializeField] private AnimatorParameterUnion value; // no string here → smaller!

        // Implicit conversions (zero-boxing)    
        public static implicit operator float(AnyAnimatorParamValue v) => v.Convert<float>();
        public static implicit operator bool(AnyAnimatorParamValue v) => v.Convert<bool>();
        public static implicit operator Vector2(AnyAnimatorParamValue v) => v.Convert<Vector2>();

        // Safe conversion (zero-boxing, returns default on type mismatch)
        //The typeof(T) check is done at compile time for each concrete call site (the JIT can often eliminate the entire branch).
        //Unsafe.As is the lowest-level, most optimizable way to reinterpret memory.
        private T Convert<T>() => type switch
        {
            ValueType.Float when typeof(T) == typeof(float)
                => Unsafe.As<float, T>(ref value.floatValue),

            ValueType.Bool when typeof(T) == typeof(bool)
                => Unsafe.As<bool, T>(ref value.boolValue),

            ValueType.Vector2 when typeof(T) == typeof(Vector2)
                => Unsafe.As<Vector2, T>(ref value.vector2Value),

            _ => default!
        };

        // Factory methods     
        public static AnyAnimatorParamValue From(float v) => new() { type = ValueType.Float, value = { floatValue = v } };
        public static AnyAnimatorParamValue From(bool v) => new() { type = ValueType.Bool, value = { boolValue = v } };
        public static AnyAnimatorParamValue From(Vector2 v) => new() { type = ValueType.Vector2, value = { vector2Value = v } };

        public void Clear()
        {
            value = default;
            type = ValueType.None;
        }
        
        public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
        {
            //Debug.Log($"SetType called on {this.GetType().Name} with {valueType}");
            if (!bypassTypeCheck && type == valueType) return;
            value = default;
            type = valueType;
        }
    }
    
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyAnimatorParamLiteral
    {
        public enum ValueType : byte
        {
            None = 0,
            Float,
            Bool,
            Vector2,
            String
        } //Inheriting a C# enum from byte changes its underlying type from the default int (32-bit) to byte (8-bit). This modification affects storage, range, and interoperability

        // 1. Put the Reference Type at Offset 0. 
        // This is the most reliable way to avoid alignment errors.
        [FieldOffset(0), SerializeField] private string stringValue; 

        // 2. Put the Value Types AFTER the reference type.
        // On 64-bit systems, a string pointer is 8 bytes. 
        // So we start the next field at offset 8.
        [FieldOffset(8), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code

        // Private union (not directly accessible) 
	    [FieldOffset(9), SerializeField] private AnyAnimatorParamValue value; // no string here → smaller!

        // Implicit conversions (zero-boxing)    
        public static implicit operator float(AnyAnimatorParamLiteral v) => v.value;
        public static implicit operator bool(AnyAnimatorParamLiteral v) => v.value;
        public static implicit operator Vector2(AnyAnimatorParamLiteral v) => v.value;
        public static implicit operator string(AnyAnimatorParamLiteral v) => v.stringValue;
            

        // Factory methods     
        public static AnyAnimatorParamLiteral From(float v) => new() { type = ValueType.Float, value = AnyAnimatorParamValue.From(v) };
        public static AnyAnimatorParamLiteral From(bool v) => new() { type = ValueType.Bool, value = AnyAnimatorParamValue.From(v) };
        public static AnyAnimatorParamLiteral From(Vector2 v) => new() { type = ValueType.Vector2, value = AnyAnimatorParamValue.From(v) };
        public static AnyAnimatorParamLiteral From(string str) => new() { type = ValueType.String, stringValue = str };

        public void Clear()
        {
            if (type == ValueType.String) stringValue = default; 
            else value = default;
            type = ValueType.None;
        }
        
        public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
        {
            if (!bypassTypeCheck && type == valueType) return;
            //Debug.Log($"SetType called on {this.GetType().Name} with {valueType}");

            if (forceClear)
            {
                stringValue = default;
                value = default;
            }
            else if (type == ValueType.String) stringValue = default;
            else value = default;
            
            type = valueType;
        }
    }
    
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct AnyRange
    {
        public enum ValueType : byte
        {
            None = 0,
            Float,
            Int
        } //Inheriting a C# enum from byte changes its underlying type from the default int (32-bit) to byte (8-bit). This modification affects storage, range, and interoperability

        [FieldOffset(0), SerializeField] private ValueType type;
        public ValueType Type => type; //This forces the user to go through the SetType method to change/set the ValueType in code

        // Private union (not directly accessible) 
	    [FieldOffset(1), SerializeField] private DefaultRangeUnion value; // no string here → smaller!

        // Implicit conversions (zero-boxing)    
        public static implicit operator Vector2(AnyRange v) => v.Convert<Vector2>();
        public static implicit operator Vector2Int(AnyRange v) => v.Convert<Vector2Int>();

        // Safe conversion (zero-boxing, returns default on type mismatch)
        //The typeof(T) check is done at compile time for each concrete call site (the JIT can often eliminate the entire branch).
        //Unsafe.As is the lowest-level, most optimizable way to reinterpret memory.
        private T Convert<T>() => type switch
        {
            ValueType.Int when typeof(T) == typeof(Vector2Int)
                => Unsafe.As<Vector2Int, T>(ref value.intRange),

            ValueType.Float when typeof(T) == typeof(Vector2)
                => Unsafe.As<Vector2, T>(ref value.floatRange),

            _ => default!
        };

        // Factory methods     
        public static AnyRange From(Vector2Int v) => new() { type = ValueType.Int, value = { intRange =  v } };
        public static AnyRange From(Vector2 v) => new() { type = ValueType.Float, value = { floatRange = v } };

        public void Clear()
        {
            value = default;
            type = ValueType.None;
        }
        
        public void SetType(ValueType valueType, bool bypassTypeCheck = false, bool forceClear = false)
        {
            //Debug.Log($"SetType called on {this.GetType().Name} with {valueType}");
            if (!bypassTypeCheck && type == valueType) return;
            value = default;
            type = valueType;
        }
    }
}