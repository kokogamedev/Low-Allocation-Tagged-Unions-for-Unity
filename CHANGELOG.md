# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2023-11-01

### This is the initial release of **TaggedUnion**.

### Added
- **Core TaggedUnion Types**:
    - `AnyPrimitive`: A tagged union for primitive types (`int`, `float`, `bool`) with zero allocations.
    - `AnyValue`: A tagged union supporting Unity types like `Vector3`, `Quaternion`, and custom structs.
    - `AnyRange`: A tagged union for value ranges (`Vector2`, `Vector2Int`) to handle constraints and limits efficiently.
    - `AnyPrimitiveLiteral`: Extends `AnyPrimitive` with rare string support for lightweight key-value pairs.
    - `AnyLiteral`: A comprehensive tagged union supporting both strings and Unity types.
    - `AnyAnimatorParamValue`: Specialized tagged union for working with Unity Animator parameters (`float`, `Vector2`, `bool`).
    - `AnyAnimatorParamLiteral`: Supports string-based animator parameters alongside stored values.

- **Inspector Support**:
    - Built-in `PropertyDrawer`: Enables intuitive type selection and value editing directly in the Unity Inspector for all tagged union types.
    - Serialized support for better integration with Unity workflows.

- **Example Use Cases**:
    - Animator blackboards with `AnyAnimatorParamValue` for handling state-driven behaviors.
    - Movement and zoom ranges with `AnyRange`.
    - Streamlined AI and state blackboards with `AnyLiteral`.

- **Utilities**:
    - Implicit conversions for all tagged union types for ease of use, e.g., `Vector2` from `AnyRange` or `int` from `AnyPrimitive`.
    - Factory methods (`From()`) for strong typed creation of tagged union values.

### Notes
- This package is designed with Unity's performance constraints in mind, ensuring zero allocations and minimal garbage collection overhead.
- The **first release** lays a solid foundation for scalable blackboard systems, parameter management, and efficient value range handling within Unity projects.
- Comprehensive examples, documentation, and Inspector integration make it beginner-friendly while also catering to advanced users.
- Great for Unity applications like AI, animation (Mecanim parameters), system states, and large-scale memory-efficient data management.

---