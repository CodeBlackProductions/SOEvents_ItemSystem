using ItemSystem.MainModule;
using ItemSystem.SubModules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Manages the data for the inspector. This includes creating the different UI elements based on object type.
    /// </summary>
    public static class InspectorDataManager
    {
        private enum ETypes
        {
            String, Int, Float
        }

        private static Dictionary<Type, ETypes> m_Typedictionary = new Dictionary<Type, ETypes>()
        {
            { typeof(string), ETypes.String },
            { typeof(int), ETypes.Int },
            { typeof(float), ETypes.Float }
        };

        public static VisualElement CreateEntry(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback)
        {
            if (_Property.CanRead && _Property.CanWrite && _Property.IsDefined(typeof(ItemToolkitAccess), false))
            {
                if (ConditionalHideAttribute.ShouldShowProperty(_ParentSO, _Property))
                {
                    VisualElement uiParent = new VisualElement();
                    uiParent.style.flexDirection = FlexDirection.Row;
                    uiParent.style.alignSelf = Align.FlexStart;
                    uiParent.style.paddingBottom = 10;
                    Label label = new Label($"{_Property.Name}: ");
                    uiParent.Add(label);

                    if (_Property.PropertyType.IsEnum)
                    {
                        return CreateUIforEnum(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (_Property.PropertyType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        return CreateUIforSO(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (typeof(IDictionary).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforDictionary(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (typeof(Array).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforArray(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (typeof(IProjectile).IsAssignableFrom(_Property.PropertyType))
                    {
                        return CreateUIforProjectiles(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (m_Typedictionary.ContainsKey(_Property.PropertyType) && _Property.Name != "TypeIndex")
                    {
                        return CreateUIforBasicDataTypes(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else if (_Property.Name == "TypeIndex")
                    {
                        return CreateUIforTypeSelection(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, uiParent);
                    }
                    else
                    {
                        return null;
                    }
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        #region InternalMethods

        private static VisualElement CreateUIforEnum(
           ScriptableObject _ParentSO,
           PropertyInfo _Property,
           InspectorPanel _ParentPanel,
           Action<bool> _InspectorValueChangeCallback,
           VisualElement _UIParent)
        {
            List<string> enumNames = Enum.GetNames(_Property.PropertyType).ToList();

            if (enumNames.Count > 0)
            {
                string currentEntry = Enum.GetName(_Property.PropertyType, _Property.GetValue(_ParentSO));

                if (currentEntry == null)
                {
                    currentEntry = $"Select Option";
                    enumNames.Insert(0, currentEntry);
                }

                DropdownField dropdownField = new DropdownField(enumNames, currentEntry);

                dropdownField.RegisterValueChangedCallback(c =>
                {
                    _Property.SetValue(_ParentSO, Enum.Parse(_Property.PropertyType, c.newValue));
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                });

                _UIParent.Add(dropdownField);

                return _UIParent;
            }

            Debug.LogWarning($"Enum entries could not be found for {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforSO(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback,
            VisualElement _UIParent)
        {
            List<ScriptableObject> soList = ItemEditor_AssetLoader.LoadAssetsByType<ScriptableObject>();
            if (soList == null)
            {
                soList = new List<ScriptableObject>();
            }
            List<string> soNames = new List<string>();
            for (int i = 0; i < soList.Count; i++)
            {
                if (soList[i].GetType().IsSubclassOf(_Property.PropertyType))
                {
                    soNames.Add($"{(soList[i] as IItemModule).ModuleName} ({soList[i].GetType().Name})");
                }
            }

            if (soNames.Count > 0)
            {
                var propertyVal = _Property.GetValue(_ParentSO);
                string currentEntry = null;

                if (propertyVal != null)
                {
                    currentEntry = $"{(_Property.GetValue(_ParentSO) as IItemModule).ModuleName} ({_Property.GetValue(_ParentSO).GetType().Name})";
                }

                if (currentEntry == null)
                {
                    currentEntry = $"Select {_Property.PropertyType}";
                    soNames.Insert(0, currentEntry);
                }

                DropdownField dropdownField = new DropdownField(soNames, currentEntry);

                dropdownField.RegisterValueChangedCallback(c =>
                {
                    _Property.SetValue(
                        _ParentSO, soList.Find(so =>
                        $"{(so as IItemModule).ModuleName} ({so.GetType().Name})" == c.newValue && _Property.PropertyType.IsAssignableFrom(so.GetType())
                        )
                    );
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                });

                _UIParent.Add(dropdownField);

                return _UIParent;
            }

            Debug.LogWarning($"ScriptableObject entries could not be found for {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforDictionary(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback,
            VisualElement _UIParent)
        {
            object dict = _Property.GetValue(_ParentSO);

            if (_Property.PropertyType == typeof(Dictionary<string, SO_Stat>))
            {
                Dictionary<string, SO_Stat> dictionary = dict as Dictionary<string, SO_Stat>;
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, SO_Stat>();
                }
                InspectorList<SO_Stat> statList = new InspectorList<SO_Stat>(dictionary, "Stats", true);

                statList.ItemAddCallback += (newItem) =>
                {
                    dictionary.Add(newItem.StatName, newItem);
                    _Property.SetValue(_ParentSO, dictionary);
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                };

                statList.ItemRemoveCallback += (removeItem) =>
                {
                    dictionary.Remove(removeItem.StatName);
                    _Property.SetValue(_ParentSO, dictionary);
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                };

                _UIParent.Add(statList);
                return _UIParent;
            }

            Debug.LogWarning($"Could not generate InspectorList for Dictionary {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforArray(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback,
            VisualElement _UIParent)
        {
            if (_Property.PropertyType == typeof(SO_Item_Effect[]))
            {
                InspectorList<SO_Item_Effect> effectList = ConvertArrayToInspectorList<SO_Item_Effect>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Effects", true);

                _UIParent.Add(effectList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Class_Type[]))
            {
                InspectorList<SO_Class_Type> typeList = ConvertArrayToInspectorList<SO_Class_Type>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Types", true);

                _UIParent.Add(typeList);
                return _UIParent;
            }

            if (_Property.PropertyType == typeof(SO_Item_Class[]))
            {
                InspectorList<SO_Item_Class> classList = ConvertArrayToInspectorList<SO_Item_Class>(_ParentSO, _Property, _ParentPanel, _InspectorValueChangeCallback, "Classes", true);

                _UIParent.Add(classList);
                return _UIParent;
            }

            Debug.LogWarning($"Could not generate InspectorList for Array {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforProjectiles(
           ScriptableObject _ParentSO,
           PropertyInfo _Property,
           InspectorPanel _ParentPanel,
           Action<bool> _InspectorValueChangeCallback,
           VisualElement _UIParent)
        {
            List<GameObject> projectileList = ItemEditor_AssetLoader.LoadAssetsByType<GameObject>().Where(asset => asset.GetComponent<IProjectile>() != null).ToList();
            if (projectileList == null)
            {
                projectileList = new List<GameObject>();
            }
            List<string> projectileNames = new List<string>();
            for (int i = 0; i < projectileList.Count; i++)
            {
                projectileNames.Add(projectileList[i].name);
            }

            if (projectileNames.Count > 0)
            {
                string currentEntry = (_Property.GetValue(_ParentSO) as IProjectile)?.Name;

                if (currentEntry == null || !projectileNames.Contains(currentEntry))
                {
                    currentEntry = "Select Projectile";
                    projectileNames.Insert(0, currentEntry);
                }

                DropdownField dropdownField = new DropdownField(projectileNames, currentEntry);

                dropdownField.RegisterValueChangedCallback(c =>
                {
                    _Property.SetValue(_ParentSO, projectileList.Find(obj =>
                        obj.name == c.newValue && obj.GetComponent<IProjectile>() != null)
                        .GetComponent<IProjectile>());

                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                });

                _UIParent.Add(dropdownField);

                return _UIParent;
            }

            Debug.LogWarning($"Projectile Class entries could not be found for {_ParentSO} : {_Property.Name}");
            return null;
        }

        private static VisualElement CreateUIforBasicDataTypes(
           ScriptableObject _ParentSO,
           PropertyInfo _Property,
           InspectorPanel _ParentPanel,
           Action<bool> _InspectorValueChangeCallback,
           VisualElement _UIParent)
        {
            TextField field = new TextField();
            switch (m_Typedictionary[_Property.PropertyType])
            {
                case ETypes.String:

                    if (!(_ParentSO.GetType() == typeof(SO_EditorSettings)))
                    {
                        field.maxLength = 20;
                    }

                    field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                    _UIParent.Add(field);

                    field.RegisterValueChangedCallback(t =>
                    {
                        _Property.SetValue(_ParentSO, t.newValue);
                        _InspectorValueChangeCallback?.Invoke(true);
                    });

                    field.RegisterCallback<FocusOutEvent>(t =>
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                    );

                    return _UIParent;

                case ETypes.Int:
                    field.maxLength = 5;
                    field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                    _UIParent.Add(field);

                    field.RegisterValueChangedCallback(t =>
                    {
                        if (int.TryParse(t.newValue, out int result))
                        {
                            _Property.SetValue(_ParentSO, result);
                        }
                        else
                        {
                            Debug.LogWarning("Invalid input");
                        }
                    });

                    field.RegisterCallback<FocusOutEvent>(t =>
                   _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                   );

                    return _UIParent;

                case ETypes.Float:
                    field.maxLength = 10;
                    field.value = _Property.GetValue(_ParentSO)?.ToString() ?? string.Empty;
                    _UIParent.Add(field);

                    field.RegisterValueChangedCallback(t =>
                    {
                        if (float.TryParse(t.newValue, out float result))
                        {
                            _Property.SetValue(_ParentSO, result);
                        }
                        else
                        {
                            Debug.LogWarning("Invalid input");
                        }
                    });

                    field.RegisterCallback<FocusOutEvent>(t =>
                   _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback)
                   );

                    return _UIParent;

                default:
                    return null;
            }
        }

        private static VisualElement CreateUIforTypeSelection(
           ScriptableObject _ParentSO,
           PropertyInfo _Property,
           InspectorPanel _ParentPanel,
           Action<bool> _InspectorValueChangeCallback,
           VisualElement _UIParent)
        {
            _UIParent.Clear();
            Label label = new Label("Type:");
            _UIParent.Add(label);

            PropertyInfo classProp = _ParentSO.GetType().GetProperty("Class");

            if (classProp == null)
            {
                Label noTypes = new Label("No types declared in class");
                _UIParent.Add(noTypes);
                return _UIParent;
            }
            SO_Item_Class itemClass = classProp.GetValue(_ParentSO) as SO_Item_Class;

            if (itemClass == null)
            {
                Label noTypes = new Label("No types declared in class");
                _UIParent.Add(noTypes);
                return _UIParent;
            }

            PropertyInfo typeProp = itemClass.GetType().GetProperty("Types");

            if (typeProp == null)
            {
                Label noTypes = new Label("No types declared in class");
                _UIParent.Add(noTypes);
                return _UIParent;
            }

            List<SO_Class_Type> typeList = new List<SO_Class_Type>(typeProp.GetValue(itemClass) as SO_Class_Type[]);

            if (typeList.Count > 0)
            {
                List<string> soNames = new List<string>();
                for (int i = 0; i < typeList.Count; i++)
                {
                    soNames.Add($"{(typeList[i] as IItemModule).ModuleName} ({typeList[i].GetType().Name})");
                }

                int currentEntry = (int)_Property.GetValue(_ParentSO);
                string currentEntryName = soNames[currentEntry];

                DropdownField dropdownField = new DropdownField(soNames, currentEntryName);

                dropdownField.RegisterValueChangedCallback(c =>
                {
                    _Property.SetValue(
                        _ParentSO, typeList.FindIndex(so =>
                        $"{(so as IItemModule).ModuleName} ({so.GetType().Name})" == c.newValue)
                    );
                    _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                    _InspectorValueChangeCallback?.Invoke(true);
                });

                _UIParent.Add(dropdownField);

                return _UIParent;
            }
            else
            {
                Label noTypes = new Label("No types declared in class");
                _UIParent.Add(noTypes);
                return _UIParent;
            }
        }

        private static InspectorList<T> ConvertArrayToInspectorList<T>(
            ScriptableObject _ParentSO,
            PropertyInfo _Property,
            InspectorPanel _ParentPanel,
            Action<bool> _InspectorValueChangeCallback,
            string _Title,
            bool _ShowAddAndRemove) where T : ScriptableObject
        {
            T[] array = _Property.GetValue(_ParentSO) as T[];
            if (array == null)
            {
                array = new T[0];
            }
            InspectorList<T> effectList = new InspectorList<T>(array, _Title, _ShowAddAndRemove);

            effectList.ItemAddCallback += (newItem) =>
            {
                T[] newArray = new T[array.Length + 1];
                Array.Copy(array, newArray, array.Length);
                newArray[newArray.Length - 1] = newItem;

                array = newArray;

                _Property.SetValue(_ParentSO, newArray);

                _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                _InspectorValueChangeCallback?.Invoke(true);
            };

            effectList.ItemRemoveCallback += (newItem) =>
            {
                T[] newArray = new T[array.Length - 1];
                int index = Array.IndexOf(array, newItem);

                if (index >= 0)
                {
                    Array.Copy(array, 0, newArray, 0, index);
                    Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
                }

                array = newArray;

                _Property.SetValue(_ParentSO, newArray);

                _ParentPanel.Show(_ParentSO, _InspectorValueChangeCallback);
                _InspectorValueChangeCallback?.Invoke(true);
            };

            return effectList;
        }

        #endregion InternalMethods
    }
}