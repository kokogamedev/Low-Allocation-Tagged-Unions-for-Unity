using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PsigenVision.Utilities.Editor;
using PsigenVision.Utilities;

namespace PsigenVision.TaggedUnion.Editor
{
    [CustomPropertyDrawer(typeof(AnyPrimitive))]
    [CustomPropertyDrawer(typeof(AnyPrimitiveLiteral))]
    [CustomPropertyDrawer(typeof(AnyValue))]
    [CustomPropertyDrawer(typeof(AnyLiteral))]
    [CustomPropertyDrawer(typeof(AnyAnimatorParamValue))]
    [CustomPropertyDrawer(typeof(AnyAnimatorParamLiteral))]
    [CustomPropertyDrawer(typeof(AnyRange))]
    public class AnyUnionDrawer : PropertyDrawer
    {
        /// <summary>
        /// Represents a mapping structure used to handle data related to types and their associated property paths.
        /// The variable functions as a dictionary where keys are types and values are dictionaries.
        /// The inner dictionary maps integers (representing indices) to tuples that encapsulate information
        /// about whether the data is nested and the corresponding property path for value retrieval or manipulation.
        /// </summary>
        /// <remarks>
        /// This variable is designed to handle various types, such as AnyPrimitive, AnyValue, AnyPrimitiveLiteral, AnyLiteral,
        /// and AnyAnimatorParamValue. It ensures proper handling of nested and non-nested property values
        /// based on their type and index. Each defined type is expected to have its corresponding property path structure,
        /// which is carefully described within its mapping configuration. ALSO, any nested Any-Types must match the indices of their
        /// ValueType enum to those of the nested type's ValueType enum to properly use this property drawer.
        /// </remarks>
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
            {
                typeof(AnyPrimitive), new ()
                {
                    //{inspector enum index,
                    //(isNested: contained within member field,
                    //path: the dot separated path to the field to draw)},
                    { 1, (isNested: false, path: "value.intValue") },
                    { 2, (isNested: false, path: "value.floatValue") },
                    { 3, (isNested: false, path: "value.boolValue") }
                }
            },
            {
                typeof(AnyValue), new ()
                {
                    { 1, (isNested: false, path: "value.intValue") },
                    { 2, (isNested: false, path: "value.floatValue") },
                    { 3, (isNested: false, path: "value.boolValue") },
                    { 4, (isNested: false, path: "value.vector2Value") },
                    { 5, (isNested: false, path: "value.vector3Value") },
                    { 6, (isNested: false, path: "value.quaternionValue") },
                    { 7, (isNested: false, path: "value.colorValue") }
                }
            },
            {
                typeof(AnyAnimatorParamValue), new ()
                {
                    { 1, (isNested: false, path: "value.floatValue") },
                    { 2, (isNested: false, path: "value.boolValue") },
                    { 3, (isNested: false, path: "value.vector2Value") },
                }
            },
            {
                typeof(AnyRange), new ()
                {
                    { 1, (isNested: false, path: "value.floatRange") },
                    { 2, (isNested: false, path: "value.intRange") }
                }
            },
            {
                typeof(AnyPrimitiveLiteral), new ()
                {
                    { 1, (isNested: true, path: "value") },
                    { 2, (isNested: true, path: "value") },
                    { 3, (isNested: true, path: "value") },
                    { 4, (isNested: false, path: "stringValue") }
                }
            },
            {
                typeof(AnyLiteral), new ()
                {
                    { 1, (isNested: true, path: "value") },
                    { 2, (isNested: true, path: "value") },
                    { 3, (isNested: true, path: "value") },
                    { 4, (isNested: true, path: "value") },
                    { 5, (isNested: true, path: "value") },
                    { 6, (isNested: true, path: "value") },
                    { 7, (isNested: true, path: "value") },
                    { 8, (isNested: false, path: "stringValue") }
                }
            },
            {
                typeof(AnyAnimatorParamLiteral), new ()
                {
                    { 1, (isNested: true, path: "value") },
                    { 2, (isNested: true, path: "value") },
                    { 3, (isNested: true, path: "value") },
                    { 4, (isNested: false, path: "stringValue")}
                }
            }
        };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // --------Enforce Initial Property Update--------
            property.serializedObject.Update();

            #region Property Implementation

            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            #region Type dropdown

            var typeProp = property.FindPropertyRelative("type");

            var previousIndex = typeProp.enumValueIndex;
            EditorGUI.BeginChangeCheck();

            EditorGUI.PropertyField(
                new Rect(position.x, position.y, position.width * 0.4f, EditorGUIUtility.singleLineHeight),
                typeProp, GUIContent.none);

            if (EditorGUI.EndChangeCheck() //Let the editor tell us if a change was made
                && previousIndex != typeProp.enumValueIndex //Check that the struct's ValueType property was modified
                && !TryUpdateValueType(property, typeProp.enumValueIndex)) //Try and update the value type of the current AnyType  
            {
                Debug.LogError($"Could not update ValueType of Any Struct {property.type}");
                return;
            }

            // --------Enforce Previous Property Updates--------
            property.serializedObject.Update(); // Force the SO to pull the data back from the object immediately

            #endregion

            //--------Skip drawing value field if None is selected--------
            if (typeProp.enumValueIndex == 0)
            {
                EditorGUI.EndProperty();
                return;
            }

            /*// --------Enforce Previous Property Updates--------
            // Force the SO to pull the data back from the object immediately
            typeProp.serializedObject.Update();*/

            #region Value Property Field

            if (!TryGetValueProperty(typeProp.enumValueIndex, property, out var valueProp))
            {
                Debug.LogError($"Could not get value property for dropdown selection with index {typeProp.enumValueIndex}");
                EditorGUI.EndProperty();
                return;
            }

            /*// --------Enforce Previous Property Updates--------
            valueProp.serializedObject.Update();*/
            
            EditorGUI.BeginChangeCheck();

            object currentBoxedValue = valueProp.boxedValue;
            
            EditorGUI.PropertyField(
                new Rect(position.x + position.width * 0.42f, position.y, position.width * 0.58f - 4, EditorGUIUtility.singleLineHeight), 
                valueProp, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
            {
                if (valueProp.serializedObject.ApplyModifiedProperties())
                {
                    // --------Sync Object with Serialized State--------
                    // Re-assign the entire struct back to the property so Unity knows it changed
                    if (!TryUpdateBoxedValue(typeProp.enumValueIndex, property, valueProp, property, property.GetSystemType()))
                        Debug.LogError("Failed not modify tagged union value");
                }
                
                //property.boxedValue = currentBoxedValue;
                // <-- This forces Unity to save the struct
                property.serializedObject.ApplyModifiedProperties();
            }

            #endregion

            /*// --------Final Full Sync Object with Serialized State--------
            property.serializedObject.ApplyModifiedProperties();*/

            EditorGUI.EndProperty();

            #endregion
        }

        /// <summary>
        /// Updates the value type of the given serialized property based on the current index and synchronizes it with the serialized object.
        /// </summary>
        /// <param name="property">The serialized property whose value type is being updated.</param>
        /// <param name="currentIndex">The current index specifying the new value type to be set.</param>
        /// <returns>True if the value type was successfully updated. Otherwise, false.</returns>
        private bool TryUpdateValueType(SerializedProperty property, int currentIndex)
        {
            /*// --------Sync Object with Serialized State--------
            property.serializedObject.ApplyModifiedProperties();*/
            
            
            // 1. Get the specific struct instance for THIS row
            var anyTypeStruct = property.boxedValue;

            //2. Try and get the enum equivalent value for the current type (if defined in the correct format)
            if (!anyTypeStruct.TryGetEnumByIndex("type", currentIndex, out var castedValueType))
            {
                Debug.LogErrorFormat("{0} does not possess a properly formatted value type enum", property.displayName);
                return false;
            }

            // 3. Use reflection on the struct instance itself (not the targetObject) to find the struct's SetType method
            // This works even if you don't know the exact type at compile time
            var setType = anyTypeStruct.GetType().GetMethod("SetType");

            if (setType == null)
            {
                Debug.LogErrorFormat("{0} does not possess a SetType method", property.displayName);
                return false;
            }

            //4. Invoke the found method, passing in the object-version of the enum value matching the struct's type enum
            setType.Invoke(anyTypeStruct, new[] { castedValueType, true, true }); //the two bool parameters bypass the type check and force clearing of data

            // 5. Push the modified struct back into the property
            // This is the "Save" step for structs
            property.boxedValue = anyTypeStruct;
            
            // --------Enforce Previous Property Updates--------
            property.serializedObject.ApplyModifiedProperties(); // Force the SO to pull the data back from the object immediately
            
            return true;
        }

        /// <summary>
        ///     Determines the field path to the active value within a serialized property
        ///     based on its type and an enumerated index that specifies the current value selection.
        /// </summary>
        /// <param name="enumIndex">The index of the selected value type as represented in the enum.</param>
        /// <param name="property">The serialized property whose active field path is being resolved.</param>
        /// <returns>The field path string corresponding to the active value field, or null if no matching field is found.</returns>
        private bool TryGetValueProperty(int enumIndex, SerializedProperty property, out SerializedProperty valueProp)
        {
            if (property == null) return NoValue(out valueProp);
            
            //Get the type of the passed in property
            var propType = property.GetSystemType();

            if (!AnyTypeData.ContainsKey(propType)) return NoValue(out valueProp); //This is not the correct type being drawn at all somehow

            if (!AnyTypeData[propType].TryGetValue(enumIndex, out var propertyData))//The selected enum index does not have a property to draw
                return NoValue(out valueProp);

            if (propertyData.isNested) TryGetValueProperty(enumIndex, property.FindPropertyRelative(propertyData.path), out valueProp);
            else valueProp = property.FindPropertyRelative(propertyData.path);
            
            return valueProp != null;

            bool NoValue(out SerializedProperty valueProp)
            {
                valueProp = null;
                return false;
            }
        }

        /// <summary>
        /// Retrieves the boxed value of a serialized property based on the given type, enumeration index, and path within the property structure.
        /// </summary>
        /// <param name="enumIndex">The index of the enumeration determining the type or data to extract.</param>
        /// <param name="startingProperty">The serialized property from which the boxed value retrieval begins.</param>
        /// <param name="valueProp">The serialized value property used for fetching the targeted value.</param>
        /// <param name="boxCarrier">The serialized property carrier used to extract the boxed value from the final field path.</param>
        /// <param name="outerType">The outermost type of the serialized object utilized in the hierarchy traversal.</param>
        /// <param name="fieldPath">The hierarchical field path within the serialized object that identifies the target property. Optional; defaults to an empty string.</param>
        /// <returns>The boxed value retrieved from the serialized property at the specified path.</returns>
        private bool TryUpdateBoxedValue(int enumIndex, SerializedProperty startingProperty, SerializedProperty valueProp,
            SerializedProperty boxCarrier, Type outerType, string fieldPath = "")
        {
            var currentType = startingProperty.GetSystemType();
            var currentData = AnyTypeData[currentType][enumIndex];
            fieldPath += (fieldPath != "") ? "." + currentData.path : currentData.path;
            
            if (currentData.isNested)
            {
                return TryUpdateBoxedValue(enumIndex, startingProperty.FindPropertyRelative(currentData.path), valueProp,
                    boxCarrier, outerType, fieldPath);
            }
            
            //Debug.Log($"Final Field Path is {fieldPath}");
            if (boxCarrier.TrySetBoxedValueViaPath(outerType, fieldPath, valueProp, out var modifiedObject))
            {
                boxCarrier.boxedValue = modifiedObject;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Gets the height of the property when drawn in the inspector.
        /// </summary>
        /// <param name="property">The serialized property to calculate the height for.</param>
        /// <param name="label">The label of the property.</param>
        /// <returns>The height, in pixels, required to draw the property.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
    }
}