using BepInEx.Configuration;

using Configgy.Configuration.AutoGeneration;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Configgy
{
    /// <summary>
    /// A class that allows you to build a configuration menu from your assembly.
    /// </summary>
    public class ConfigBuilder
    {
        /// <summary>
        /// The GUID of the config. This is used to identify the config in the configuration menu and serialize it's data.
        /// </summary>
        public string GUID { get; }

        /// <summary>
        /// The display name of the config in the root of the configuration menu.
        /// </summary>
        public string DisplayName { get; }

        internal bool initialized { get; private set; }

        internal Assembly owner;
        internal List<IConfigElement> _configElements;

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid">Your mod GUID ex:"JohnModder.ULTRAKILL.MyMod"</param>
        /// <param name="menuDisplayName">The display name for your config in the configuration menu.</param>
        public ConfigBuilder(string guid = null, string menuDisplayName = null)
        {
            this.owner = Assembly.GetCallingAssembly();
            this.GUID = (string.IsNullOrEmpty(guid) ? owner.GetName().Name : guid);
            this.DisplayName = (string.IsNullOrEmpty(menuDisplayName) ? GUID : menuDisplayName);
        }

        internal static HashSet<Type> environmentBuiltTypes = new HashSet<Type>();

        /// <summary>
        /// Builds provided all config elements within the provided types.
        /// </summary>
        /// <param name="types"></param>
        public void BuildTypes(params Type[] types)
        {
            foreach (var type in types)
            {
                BuildType(type);
            }
        }

        /// <summary>
        /// Builds all config elements defined in a specific type.
        /// </summary>
        /// <param name="type"></param>
        public void BuildType(Type type)
        {
            owner ??= Assembly.GetCallingAssembly();

            if (owner != type.Assembly)
                throw new Exception($"Configgy.ConfigBuilder:{GUID}: You can only build types originating from the assembly that owns the ConfigBuilder.");

            if (environmentBuiltTypes.Contains(type))
            {
                if (builtGlobal)
                    throw new Exception($"Configgy.ConfigBuilder:{GUID}: Type has already been built into an existing config. If this is a mistake and you called BuildAll, you must call BuildType before BuildAll.");
                else
                    throw new Exception($"Configgy.ConfigBuilder:{GUID}: Type has already been built into an existing config.");
            }

            _configElements ??= new List<IConfigElement>();
            ProcessType(type);
            BuildInternal();
        }

        private bool builtGlobal = false;

        /// <summary>
        /// Builds config elements from any un-built types' configgable fields and methods in your assembly.
        /// </summary>
        public void BuildAll()
        {
            //only build global once.
            if (builtGlobal)
                return;

            owner ??= Assembly.GetCallingAssembly();
            _configElements ??= new List<IConfigElement>();

            foreach (Type type in owner.GetTypes())
                ProcessType(type);

            builtGlobal = true;
            BuildInternal();
        }


        /// <summary>
        /// Builds your configuration menu and registers it with Configgy.
        /// </summary>
        [Obsolete($"Build is now obsolete. Use {nameof(ConfigBuilder)}.{nameof(BuildAll)} to build using your whole assembly.")]
        public void Build()
        {
            if (initialized)
                return;

            BuildAll();
        }

        internal void ProcessType(Type type)
        {
            //Possibly built by manual call. Ignore it.
            if (environmentBuiltTypes.Contains(type))
                return;

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                ProcessMethod(method);

            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                ProcessField(field);

            environmentBuiltTypes.Add(type);
        }

        internal void BuildInternal()
        {
            if (initialized)
                return;

            _configElements ??= new List<IConfigElement>();
            CreateSaverObject();

            OnConfigElementsChanged += (v) => ConfigurationManager.SubMenuElementsChanged();
            OnConfigElementsChanged?.Invoke(_configElements.ToArray());

            initialized = true;
            ConfigurationManager.RegisterConfiguraitonMenu(this);
        }

        /// <summary>
        /// Rebuilds the config menu. Do not use in a hot path as this is resource intensive.
        /// </summary>
        public void Rebuild()
        {
            OnConfigElementsChanged?.Invoke(_configElements.ToArray());
        }

        /// <summary>
        /// Invoked when the menu is rebuilt.
        /// </summary>
        public event Action<IConfigElement[]> OnConfigElementsChanged;

        internal IConfigElement[] GetConfigElements()
        {
            return _configElements.ToArray();
        }

        private void ProcessMethod(MethodInfo method)
        {
            ConfiggableAttribute cfg = method.GetCustomAttribute<ConfiggableAttribute>();

            if (cfg == null)
                return;

            if (method.ReturnType != typeof(void))
            {
                Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register method {method.DeclaringType.Name}.{method.Name}. But it's return type is not void. Skipping!");
                return;
            }

            if (method.GetParameters().Length > 0)
            {
                Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register method {method.DeclaringType.Name}.{method.Name}. But it has more than 0 parameters. Skipping!");
                return;
            }

            if (!method.IsStatic) //no instance!!
            {
                Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register method {method.DeclaringType.Name}.{method.Name}. But it is not static. Skipping!");
                return;
            }

            cfg.SetOwner(this);
            cfg.SetSerializationAddress($"{GUID}.{method.DeclaringType.Namespace}.{method.DeclaringType.Name}.{method.Name}"); //This isnt needed, but who cares.

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(method.Name);

            RegisterMethodAsButton(cfg, method);
        }

        private void ProcessField(FieldInfo field)
        {
            ConfiggableAttribute cfg = field.GetCustomAttribute<ConfiggableAttribute>();

            if (cfg == null)
                return;

            if (!field.IsStatic) //no instance!!
            {
                Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register field {field.DeclaringType.Name}.{field.Name}. But it is not static. Skipping!");
                return;
            }

            cfg.SetOwner(this);
            cfg.SetSerializationAddress($"{GUID}.{field.DeclaringType.Namespace}.{field.DeclaringType.Name}.{field.Name}");

            if (string.IsNullOrEmpty(cfg.DisplayName))
                cfg.SetDisplayNameFromCamelCase(field.Name);

            if (typeof(IConfigElement).IsAssignableFrom(field.FieldType))
            {
                IConfigElement cfgElement = (IConfigElement)field.GetValue(null);
                RegisterElementCore(cfg, cfgElement);
            }
            else
            {
                RegisterPrimitive(cfg, field);
            }
        }

        private void RegisterMethodAsButton(ConfiggableAttribute descriptor, MethodInfo method)
        {
            ConfigButton button = new ConfigButton(() =>
            {
                method.Invoke(null, null);
            });

            //Not really needed, but who cares.
            button.BindConfig(this);
            button.BindDescriptor(descriptor);
            RegisterElementCore(descriptor, button);
        }

        private void RegisterElementCore(ConfiggableAttribute descriptor, IConfigElement configElement)
        {
            configElement.BindDescriptor(descriptor);
            configElement.BindConfig(this);
            _configElements.Add(configElement);
        }

        /// <summary>
        /// Manually registers a config element with a descriptor. Call <see cref="ConfigBuilder.BuildWithoutScan"/> to rebuild the menu after adding elements.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="configElement"></param>
        public void RegisterElement(ConfiggableAttribute descriptor, IConfigElement configElement)
        {
            BuildInternal();
            RegisterElementCore(descriptor, configElement);
        }

        #region Element construction for BepInEx.Configuration support

        public void RegisterBepInExConfigEntry(ConfigEntryBase entry)
        {
            BuildInternal();

            //Use the display name for the parent of this entry.
            ConfiggableAttribute attribute = new ConfiggableAttribute(
                entry.Definition.Section,
                entry.Definition.Key,
                description: entry.Description.Description
                );

            attribute.SetOwner(this);

            // type T = entry.SettingType;
            // ConfigValueElement<T> element = MakeBepInExElement<T>(entry);
            var element = typeof(ConfigBuilder)
                .GetMethod(nameof(MakeBepInExElement), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(entry.SettingType)
                .Invoke(null, [entry])
                as ConfigValueElement;

            if (element is null)
            {
                Debug.LogWarning($"Configgy.ConfigBuilder:{GUID}: failed to auto generate BepInEx ConfigEntry {entry.Definition.Section}.{entry.Definition.Key}. It's type ({entry.SettingType.Name}) is not supported.");
                return; //skip unsupported types
            }

            RegisterElementCore(attribute, BepinElement.WrapUntyped(entry, element));
        }

        private static ConfigValueElement MakeBepInExElement<T>(ConfigEntry<T> entry)
        {
            AcceptableValueBase domain = entry.Description?.AcceptableValues;
            if (domain.IsAcceptableValueList())
            {
                // This was supposed to be just another arm in a switch statement,
                // but I can't refer to AcceptableValueList<T> unless the constraint is satisfied.
                // Some types passed to this method won't satisfy that constraint, so we can't add it to this method.
                //
                // This could hardly be any more pointlessly convoluted. Thanks, dotnet.
                var unbound = typeof(ConfigBuilder).GetMethod(nameof(MakeBepInExDropdown), BindingFlags.Static | BindingFlags.NonPublic);
                var bound = unbound.MakeGenericMethod(typeof(T));
                return (ConfigDropdown<T>)bound.Invoke(null, [entry]);
            }
            else if (domain is AcceptableValueRange<int> intRange)
            {
                return new IntegerSlider((int)entry.DefaultValue, intRange.MinValue, intRange.MaxValue);
            }
            else if (domain is AcceptableValueRange<float> floatRange)
            {
                return new FloatSlider((float)entry.DefaultValue, floatRange.MinValue, floatRange.MaxValue);
            }
            else
            {
                T defaultValue = entry.GetDefault();
                return ConfigValueElement.Create(defaultValue);
            }
        }

        private static ConfigDropdown<T> MakeBepInExDropdown<T>(ConfigEntry<T> entry) where T : IEquatable<T>
        {
            AcceptableValueList<T> domain = (AcceptableValueList<T>)entry.Description.AcceptableValues;
            T[] values = domain.AcceptableValues;

            Func<T, string> getName = entry.GetTag<Func<T, string>>() ?? (v => v.ToString());
            string[] names = values.Select(getName).ToArray();

            T defaultValue = entry.GetDefault();
            return new ConfigDropdown<T>(values, defaultValue, names);
        }

        #endregion

        #region Element construction for ConfiggableAttribute support

        private void RegisterPrimitive(ConfiggableAttribute descriptor, FieldInfo field)
        {
            ConfigValueElement element = MakeElementForField(field);
            if (element != null)
            {
                element.BindField(field);
                RegisterElementCore(descriptor, element);
            }
        }

        private ConfigValueElement MakeElementForField(FieldInfo field)
        {
            object baseValue = field.GetValue(null);

            if (field.GetCustomAttribute<RangeAttribute>() is RangeAttribute range)
            {
                if (baseValue is float f)
                {
                    float min = range.min, max = range.max;
                    float defaultValue = Mathf.Clamp(f, min, max);
                    return new FloatSlider(defaultValue, min, max);
                }
                else if (baseValue is int i)
                {
                    int min = (int)range.min, max = (int)range.max;
                    int defaultValue = Mathf.Clamp(i, min, max);
                    return new IntegerSlider(defaultValue, min, max);
                }
            }

            // type T = baseValue.GetType();
            // ConfigValueElement<T> element = ConfigValueElement.Create<T>(baseValue);
            var element = typeof(ConfigValueElement)
                .GetMethod(nameof(ConfigValueElement.Create))
                .MakeGenericMethod(baseValue.GetType())
                .Invoke(null, [baseValue])
                as ConfigValueElement;

            if (element is null)
            {
                Debug.LogError($"Configgy.ConfigBuilder:{GUID}: attempted to register field {field.DeclaringType.Name}.{field.Name}. But it's type ({field.FieldType.Name}) is not supported. Skipping!");
            }

            return element;
        }

        #endregion

        List<SerializedConfiggable> _data;

        private List<SerializedConfiggable> data
        {
            get
            {
                if(_data == null)
                {
                    LoadData();
                }
                return _data;
            }
        }

        internal void LoadData()
        {
            string folderPath = Path.Combine(Paths.DataFolder, owner.GetName().Name);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, GUID+".json");

            List<SerializedConfiggable> loadedData = new List<SerializedConfiggable>();


            if (!File.Exists(filePath))
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(loadedData);
                File.WriteAllText(filePath, json);
            }
            else
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    loadedData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SerializedConfiggable>>(json);
                    Debug.Log($"Loaded Config {GUID} with {loadedData.Count} values");
                }catch (System.Exception ex)
                {
                    Debug.LogError("Error Loading Configgy Data!");
                    Debug.LogException(ex);

                    int count = 0;
                    while(File.Exists(filePath+$".backup{((count > 0) ? $" ({count})": "")}"))
                    {
                        count++;
                    }

                    Debug.Log("Created configgy file backup.");
                    File.Copy(filePath, filePath + $".backup{((count > 0) ? $" ({count})" : "")}");
                    File.Delete(filePath);

                    loadedData = new List<SerializedConfiggable>();
                }
            }

            //Remove null values.
            loadedData = loadedData.Where(x => x.IsValid()).ToList();

            _data = loadedData;
        }

        public void SaveData()
        {
            if (_data == null)
                return;

            string folderPath = Path.Combine(Paths.DataFolder, owner.GetName().Name);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, GUID + ".json");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(_data, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private bool saveNextFrame;

        internal void SaveDeferred()
        {
            saveNextFrame = true;
        }

        private IEnumerator SaveChecker()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (saveNextFrame)
                {
                    SaveData();
                    saveNextFrame = false;
                }
            }
        }


        internal void CreateSaverObject()
        {
            GameObject saveChecker = new GameObject($"Configgy_Saver ({GUID})");
            BehaviourRelay br = saveChecker.AddComponent<BehaviourRelay>();
            br.StartCoroutine(SaveChecker());
            GameObject.DontDestroyOnLoad(saveChecker);
        }

        //Load config data.

        internal T GetValueAtAddress<T>(string address)
        {
            foreach (var sc in data)
            {
                if (sc.key == address)
                    return sc.GetValue<T>();
            }

            return default(T);
        }

        internal bool TryGetValueAtAddress<T>(string address, out T value)
        {
            value = default(T);

            foreach (var sc in data)
            {
                if (sc.key == address)
                {
                    try
                    {
                        value = sc.GetValue<T>();
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError("Failed to deserialize value");
                        Debug.LogException(ex);
                        break;
                    }
                }
            }

            return false;
        }

        internal void SetValueAtAddress(string address, object value)
        {
            foreach (var sc in data)
            {
                if (sc.key == address)
                {
                    sc.SetValue(value);
                    return;
                }
            }

            data.Add(new SerializedConfiggable(address, value));
        }
    }
}
