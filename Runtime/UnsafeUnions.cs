using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PsigenVision.TaggedUnion.UnsafeBackend
{
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    internal struct PrimitiveUnion
    {
        [FieldOffset(0)] public int intValue;
        [FieldOffset(0)] public float floatValue;
        [FieldOffset(0)] public bool boolValue;
    }

    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    internal struct ValueUnion
    {
        [FieldOffset(0)] public int intValue;
        [FieldOffset(0)] public float floatValue;
        [FieldOffset(0)] public bool boolValue;
        [FieldOffset(0)] public Vector2 vector2Value;
        [FieldOffset(0)] public Vector3 vector3Value;
        [FieldOffset(0)] public Quaternion quaternionValue;
        [FieldOffset(0)] public Color colorValue;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    internal struct AnimatorParameterUnion
    {
        [FieldOffset(0)] public float floatValue;
        [FieldOffset(0)] public bool boolValue;
        [FieldOffset(0)] public Vector2 vector2Value;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    internal struct DefaultRangeUnion
    {
        [FieldOffset(0)] public Vector2 floatRange;
        [FieldOffset(0)] public Vector2Int intRange;
    }
}