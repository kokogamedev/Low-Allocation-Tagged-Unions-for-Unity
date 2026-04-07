# TaggedUnion Utilties

## TypeExtensions

This static utilties class contains extension methods for `System.Type`.

### **Overview of the `GetFieldViaPath` Method**

The `GetFieldViaPath` method is designed to dynamically traverse a dot-separated field path in a type hierarchy and resolve the corresponding `System.Reflection.FieldInfo` for the final field in the path. Special handling is incorporated for array or generic collection indexing (e.g., `fieldName[3]`) to enable static reflection on their element types. Here's a step-by-step breakdown of how it operates:

---

#### 1. **Define Search Parameters**
The method starts by setting up the **binding flags** (`BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance`) to ensure reflection can access both public and private instance fields.

```csharp
System.Reflection.BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
```

---

#### 2. **Initialize Variables for Traversal**
The method:
- **Initializes the starting type** (`containingObjectType`) as the input `type`, which is iteratively updated as the method traverses the path.
- Prepares an array of field names (`pathsPerDot`) **split by the dot separator** (`.`) from the `path` string, which indicates the hierarchy to traverse.

---

#### 3. **Loop Through Dot-Separated Field Names**
The method iterates over each segment of the path (`pathsPerDot`) to progressively resolve fields along a type's hierarchy.

```csharp
for (int i = 0; i < pathsPerDot.Length; i++)
{
    ...
}
```

Within this loop, each part is processed step-by-step as follows:

---

#### 4. **Check for Collection Indexing (`[index]`)**
The method identifies whether the current path segment references an array or a generic collection by **checking for square bracket notation** (`[index]`).

```csharp
bool isCollection = part.Contains("[") && part.EndsWith("]");
```

If a collection indexer is detected:
- The `TryParseCollectionPart` method is used to **extract the field name (before `[`) and the index** (inside `[ ]`).
- Field names using an **invalid collection format** result in `null` being returned.

```csharp
if (!part.TryParseCollectionPart(out fieldName, out collectionIndex))
    return null; // Invalid or improperly formatted path segment.
```

---

#### 5. **Resolve Field Information Dynamically with Reflection**
For the current segment, `containingObjectType.GetField` is used to retrieve reflection metadata (`FieldInfo`) about the field.

```csharp
fieldInfo = containingObjectType.GetField(fieldName, flags);
```

- If `fieldInfo == null` (i.e., the field is not found in the current type):
   - The method attempts to resolve the field **in the base type** (if available) via recursion.

```csharp
if (fieldInfo == null)
    return containingObjectType.BaseType != null ? GetFieldViaPath(containingObjectType.BaseType, path) : null;
```

---

#### 6. **Handle Non-Collection Fields**
For a field segment that is **not a collection index** (`isCollection == false`):
- **Update the type** (`containingObjectType`) that will be used for resolving the next field in the hierarchy.

```csharp
containingObjectType = fieldInfo.FieldType;
```

---

#### 7. **Handle Collection Types (Array, List<T>, etc.)**
When a collection is encountered (`isCollection == true`), the method inspects the type of the field to determine whether it is an **array** or a **generic collection**.

1. **Array Type Handling**:
   - If the resolved field's type (`FieldInfo.FieldType`) is an **array**, its **element type** (`GetElementType()`) is used for subsequent traversal.

```csharp
if (fieldInfo.FieldType.IsArray)
    containingObjectType = fieldInfo.FieldType.GetElementType();
```

2. **Generic Collection Handling**:
   - If the field is a **generic type** (e.g., `List<T>`), the method confirms that it is indexable by checking if it implements `System.Collections.IEnumerable`.
   - The **generic argument type (`T`)** is extracted using `GetGenericArguments()[0]`, and traversal continues with this type.

```csharp
else if (fieldInfo.FieldType.IsGenericType)
{
    if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(fieldInfo.FieldType))
        return null; // Not a valid collection type; return null.

    containingObjectType = fieldInfo.FieldType.GetGenericArguments()[0];
}
```

If neither case applies (i.e., the field is not an array or collection, but indexing was attempted), the method returns `null` for improper use.

---

#### 8. **Return the Final FieldInfo**
Once all path segments have been processed, the method returns the `FieldInfo` for the **last field** resolved in the path.

```csharp
return fieldInfo;
```

---

### **Helper Method: `TryParseCollectionPart`**

This helper method simplifies the parsing of collection field segments (e.g., `"arrayVariable[3]"`). It extracts:
1. The **field name** before the opening bracket (`[`).
2. The **index** within the square brackets.

If the format of the input string is valid and the index is numerically valid, the method returns `true`. Otherwise, it returns `false`.

```csharp
private static bool TryParseCollectionPart(this string part, out string fieldName, out int? index)
{
    fieldName = null;
    index = null;

    int indexStart = part.IndexOf('[');
    if (indexStart < 0 || !part.EndsWith("]"))
    {
        return false; // Invalid format.
    }

    // Extract the field name and index.
    fieldName = part.Substring(0, indexStart);
    string indexPart = part.Substring(indexStart + 1, part.Length - indexStart - 2);

    // Validate and parse the index.
    if (int.TryParse(indexPart, out int parsedIndex))
    {
        index = parsedIndex;
        return true;
    }

    return false; // Invalid index format.
}
```

---

### **Key Features**

1. **Dynamic Traversal**:
   - Resolves fields in a **dot-separated path** for flat or nested class hierarchies.

2. **Support for Arrays and Generic Collections**:
   - Handles array types via `GetElementType()`.
   - Resolves generic collection types by reading `T` from `List<T>` or similar types.

3. **Graceful Error Handling**:
   - Returns `null` for invalid paths, collection misuse, or inaccessible fields.
   - Avoids runtime exceptions from malformed inputs.

4. **Type-Only Resolution**:
   - Does not operate on actual object instances (runtime data). The method only resolves fields/types at the static reflection level.

---

### **Examples**

#### Example 1: Array Field
```csharp
class Example { public int[] Numbers; }
FieldInfo field = typeof(Example).GetFieldViaPath("Numbers[3]");
// Resolves `Numbers` as `int[]` and type traversal focuses on `int`.
```

#### Example 2: Generic Field
```csharp
class Example { public List<string> Names; }
FieldInfo field = typeof(Example).GetFieldViaPath("Names[1].Length");
// Resolves `Names` to `List<string>`, element type to `string`, and `Length` as a property of `string`.
```

#### Example 3: Improper Path
- Input: `"InvalidFieldName[3]"` 👉 **Returns `null`** (field does not exist).
- Input: `"ArrayField[index]"` 👉 **Returns `null`** (invalid index format).

---


### **`GetSystemType`**

A utility method to retrieve the `System.Type` of a field represented by a `SerializedProperty` in Unity. Let me walk you through its purpose and functionality step by step:

```csharp
public static System.Type GetSystemType(this SerializedProperty property)
{
    System.Type parentType = property.serializedObject.targetObject.GetType();
    System.Reflection.FieldInfo fi = parentType.GetFieldViaPath(property.propertyPath);
    return fi.FieldType;
}
```

1. **Method Purpose**:
    - This method is an extension method for a `SerializedProperty` (a Unity-specific data type representing serialized fields in the Inspector).
    - It retrieves the `System.Type` of the field associated with the serialized property.

2. **Parameters**:
    - `SerializedProperty property`: The serialized property whose field's type we want to retrieve.

3. **Logic**:
    - **Step 1**: The method gets the type of the object that contains the serialized property (`serializedObject.targetObject.GetType()`).
        - `serializedObject` refers to the Unity object that owns the property (e.g., a `MonoBehaviour` or `ScriptableObject`).
        - `property.propertyPath` represents the nested path to the field or subfield (e.g., `"myField"`, `"myClass.myField"`, etc.).

    - **Step 2**: The method calls `GetFieldViaPath` on the type of the parent object, passing in the `propertyPath`. The `GetFieldViaPath` method resolves the field based on the dot-separated path of the property and returns its `FieldInfo`.

    - **Step 3**: The `FieldType` property of the retrieved `FieldInfo` is returned. This represents the type (e.g., `int`, `float`, `string`, or custom class) of the field.

---

### **Related Code: `GetFieldViaPath`**

The `GetFieldViaPath` method is a utility to resolve a `FieldInfo` object corresponding to a field represented by a property path.

#### Key Concepts in `GetFieldViaPath`:
- **Path Parsing**:
    - The method splits the `propertyPath` into segments (e.g., `"myClass.myField"` becomes `["myClass", "myField"]`).
    - It resolves each segment as a field of the parent class.

- **Nested and Indexed Fields**:
    - Handles nested fields for classes, structs, arrays, and generic collections (e.g., `List<T>`).
    - Uses type information (`FieldType`, `GetElementType()`, `GetGenericArguments()`) to traverse complex structures.

- **Base Types**:
    - If the field is not found in the current type, it recursively searches the base type (to support inheritance).

---

### **Application**

This utility is commonly used in Unity development when working with custom editor scripts or property drawers. It allows developers to manipulate serialized fields more effectively by:
1. Resolving their types dynamically.
2. Using the resolved type to customize inspection, validation, or runtime behavior.

---

### **Example Usage in Unity**

Suppose you have a target object like this:

```csharp
public class ExampleScript : MonoBehaviour
{
    public int myInt;
    public MyClass myClass;

    [System.Serializable]
    public class MyClass
    {
        public float myFloat;
    }
}
```

You can use this method inside a custom editor to determine the type of a serialized property:

```csharp
[CustomEditor(typeof(ExampleScript))]
public class ExampleScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SerializedProperty property = serializedObject.FindProperty("myClass.myFloat");
        
        // Get the type of the property
        Type propertyType = property.GetSystemType();
        
        EditorGUILayout.LabelField("Field Type:", propertyType.Name); // Output: "Single" (float)
    }
}
```

---

### **Advantages of This Approach**
- **Handles Nested Fields**: Includes support for resolving nested or complex fields (e.g., arrays, lists, nested classes).
- **Dynamic Type Retrieval**: Can dynamically resolve the type without hardcoding or manual inspection.
- **Recursively Handles Inherited Types**: Ensures fields in base classes are also resolved when needed.

This method is particularly helpful when working within Unity's Editor scripting, where fields need to be resolved based on `SerializedProperty` paths.

---

### GetValue

The `GetValue` method is an extension method for a `SerializedProperty` (a Unity-specific data type), and it retrieves the runtime value of the field represented by the serialized property.

#### Purpose:
To dynamically fetch the value of a serialized field from a Unity Object, even for fields accessed through nested or complex paths.

#### Method Signature:
```csharp
public static object GetValue(this SerializedProperty property)
```

#### Parameters:
- **`SerializedProperty property`**: The serialized property whose value you want to retrieve.

#### How It Works:
1. **Obtain the Parent Type**:
   The type of the object containing the serialized property is determined:
   ```csharp
   System.Type parentType = property.serializedObject.targetObject.GetType();
   ```
    - `serializedObject.targetObject`: Refers to the Unity Object (e.g., `MonoBehaviour`, `ScriptableObject`) that owns the serialized property.
    - `parentType`: Represents the type of this object.

2. **Retrieve Field Information**:
   Using the `GetFieldViaPath` method, the code resolves the field's `FieldInfo` based on the dot-separated path of the serialized property:
   ```csharp
   System.Reflection.FieldInfo fi = parentType.GetFieldViaPath(property.propertyPath);
   ```
   This supports resolving fields in nested objects, arrays, and lists.

3. **Fetch the Field Value**:
   The `GetValue` method of the `FieldInfo` class is used to get the runtime value of the field:
   ```csharp
   return fi.GetValue(property.serializedObject.targetObject);
   ```
    - `fi`: The `FieldInfo` of the resolved field.
    - `property.serializedObject.targetObject`: The actual runtime instance of the object containing the field.

   The result is the value of the field, returned as an `object`.

---

### **Application**

This method is particularly useful in Unity Editor scripting or reflection-based tasks where you need to fetch the value of a serialized field during runtime.

---

### **Example Usage in Unity**

Suppose you have a Unity script like this:

```csharp
public class ExampleScript : MonoBehaviour
{
    public int myInt;
    public NestedClass myClass;

    [System.Serializable]
    public class NestedClass
    {
        public string myString;
    }
}
```

You can use the `GetValue` method in a custom editor to fetch the value of the nested `myString` field:

```csharp
[CustomEditor(typeof(ExampleScript))]
public class ExampleScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SerializedProperty property = serializedObject.FindProperty("myClass.myString");
        
        // Retrieve the value of the property
        object value = property.GetValue();
        
        EditorGUILayout.LabelField("Field Value:", value != null ? value.ToString() : "null");
    }
}
```

---

### **Advantages of This Approach**
- **Dynamic Value Access**: Retrieves the runtime value dynamically without hardcoding field types or paths.
- **Supports Nested Fields**: Handles values in nested objects, arrays, and lists using the dot-separated `propertyPath`.
- **Simplifies Reflection in Unity**: Provides a simple interface to access serialized data through Unity's `SerializedProperty` system.

This method is ideal for creating custom inspectors or tools that need to reflect on serialized fields dynamically at runtime.

---

### SetValue

The `SetValue` method is an extension method for a `SerializedProperty` (a Unity-specific data type). It allows you to set the runtime value of a field represented by the serialized property.

#### Purpose:
To dynamically assign a value to a serialized field on a Unity Object, even for fields accessed through nested or complex paths.

#### Method Signature:
```csharp
public static void SetValue(this SerializedProperty property, object value)
```

#### Parameters:
- **`SerializedProperty property`**: The serialized property whose value you want to set.
- **`object value`**: The new value to assign to the field represented by the `SerializedProperty`.

#### How It Works:
1. **Obtain the Parent Type**:
   The type of the object containing the serialized property is determined:
   ```csharp
   System.Type parentType = property.serializedObject.targetObject.GetType();
   ```
    - `serializedObject.targetObject`: Refers to the Unity Object (e.g., `MonoBehaviour`, `ScriptableObject`) that owns the serialized property.
    - `parentType`: Represents the type of this object.

2. **Retrieve Field Information**:
   Using the `GetFieldViaPath` method, the code resolves the field's `FieldInfo` based on the dot-separated path of the serialized property:
   ```csharp
   System.Reflection.FieldInfo fi = parentType.GetFieldViaPath(property.propertyPath);
   ```
   This supports resolving fields in nested objects, arrays, and lists.

3. **Set the Field Value**:
   The `SetValue` method of the `FieldInfo` class is used to assign the value to the runtime field:
   ```csharp
   fi.SetValue(property.serializedObject.targetObject, value);
   ```
    - `fi`: The `FieldInfo` of the resolved field.
    - `value`: The new value to set for the field.
    - `property.serializedObject.targetObject`: The runtime instance of the object containing the field.

   This directly modifies the runtime field value while following proper reflection-based mechanisms.

---

### **Application**

This method is useful when creating editor extensions, tools, or runtime systems in Unity that need to modify serialized fields dynamically using `SerializedProperty`.

---

### **Example Usage in Unity**

Suppose you have a script like this on a GameObject:

```csharp
public class ExampleScript : MonoBehaviour
{
    public int myInt;
    public NestedClass myClass;

    [System.Serializable]
    public class NestedClass
    {
        public string myString;
    }
}
```

You can use `SetValue` in a custom editor to set the value of a serialized field (e.g., update `myString`):

```csharp
[CustomEditor(typeof(ExampleScript))]
public class ExampleScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SerializedProperty property = serializedObject.FindProperty("myClass.myString");
        
        // Set a new value for the field
        if (GUILayout.Button("Update String to 'Hello'"))
        {
            property.SetValue("Hello");
        }
        
        // Apply any modified properties to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
```

---

### **Advantages of This Approach**
- **Dynamic Value Assignment**: Assigns values dynamically without hardcoding specific field types or paths.
- **Supports Nested Fields**: Works with nested objects, arrays, and lists by resolving the correct field dynamically using the `propertyPath`.
- **Compatible with Unity's SerializedProperty System**: Integrates seamlessly, preserving Unity's handling of serialized data.
- **Useful for Editor Scripting**: Provides a simple and reliable reflection-based solution to modify values directly through custom inspectors or tools.

This method is ideal for scenarios where serialized data needs to be modified programmatically, such as during custom editor development or runtime property adjustments.