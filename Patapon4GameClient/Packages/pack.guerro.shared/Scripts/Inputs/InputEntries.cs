using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Packages.pack.guerro.shared.Scripts.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Packet.Guerro.Shared.Inputs
{
    public partial class CInputManager
    {
        public abstract class InputSettingBase : IDisposable
        {
            public abstract Type ResultType { get; }

            protected string LastLayout;

            public string NameId      { get; internal set; }
            public string DisplayName { get; internal set; }
            public string Translation { get; internal set; }

            public abstract FastDictionary<string, object> GetDefaults();

            public abstract bool ContainsLayout(string layoutType);

            public abstract void Refresh();

            public abstract void Dispose();            
        }
        
        public abstract class InputSetting<TValue, TResult> : InputSettingBase
        {
            public override Type ResultType { get; } = typeof(TResult);
            
            protected TValue LastValue;

            public ReadOnlyDictionary<string, TValue> Defaults;

            public TValue GetLayout(string layout)
            {
                if (LastLayout == layout)
                {
                    return LastValue;
                }

                LastValue  = Defaults[layout];
                LastLayout = layout;

                return LastValue;
            }
        }

        public interface IInputResult
        {
            object GetValueBoxed();
        }

        public static class Result
        {
            public struct Push : IInputResult
            {
                public float Value;
                
                public object GetValueBoxed()
                {
                    return Value;
                }
            }

            public struct Axis1D : IInputResult
            {
                public float Value;
                
                public object GetValueBoxed()
                {
                    return Value;
                }
            }
            
            public struct Axis2D : IInputResult
            {
                public Vector2 Value;
                
                public object GetValueBoxed()
                {
                    return Value;
                }
            }
        }

        public static class Settings
        {
            public enum EType
            {
                Push,
                Axis1D,
                Axis2D
            }
            
            public class Push : InputSetting<ReadOnlyCollection<string>, Result.Push>
            {
                internal FastDictionary<string, string[]> RWDefaults;

                internal Push(string nameId)
                {
                    NameId = nameId;
                    DisplayName = string.Empty;
                    Translation = string.Empty;
                    
                    RWDefaults = new FastDictionary<string, string[]>();
                }
                
                public Push(string nameId, string displayName, string translation, FastDictionary<string, string[]> defaults)
                {
                    NameId      = nameId;
                    DisplayName = displayName;
                    Translation = translation;

                    RWDefaults = defaults;
                    Refresh();
                }
                
                public override FastDictionary<string, object> GetDefaults()
                {
                    throw new System.NotImplementedException();
                }

                public override bool ContainsLayout(string layoutType)
                {
                    return Defaults.ContainsKey(layoutType);
                }

                public override void Refresh()
                {
                    Defaults = new ReadOnlyDictionary<string, ReadOnlyCollection<string>>
                        (RWDefaults.ToDictionary(k => k.Key, v => new ReadOnlyCollection<string>(v.Value)));      
                }

                public override void Dispose()
                {
                    Defaults = null;
                }
            }

            public class Axis1D : InputSetting<ReadOnlyDictionary<string, ReadOnlyCollection<string>>, Result.Axis1D>
            {
                internal FastDictionary<string, FastDictionary<string, string[]>> RWDefaults;
                
                internal Axis1D(string nameId)
                {
                    NameId      = nameId;
                    DisplayName = string.Empty;
                    Translation = string.Empty;
                    
                    RWDefaults = new FastDictionary<string, FastDictionary<string, string[]>>();
                }
                
                public Axis1D(string                                                   nameId, string displayName,
                              string                                                   translation,
                              FastDictionary<string, FastDictionary<string, string[]>> defaults)
                {
                    NameId      = nameId;
                    DisplayName = displayName;
                    Translation = translation;
                    
                    foreach (var innerDico in defaults.Values)
                    {
                        var hasAllData = new NativeArray<bool1>(2, Allocator.Temp);
                        foreach (var key in innerDico.Keys)
                        {
                            if (key == "-x") hasAllData[0] = true;
                            if (key == "+x") hasAllData[1] = true;
                        }
                        
                        for (int i = 0; i != 2; i++)
                        {
                            if (hasAllData[i])
                                continue;
                        
                            var id = InputDimension.GetDimensionStringId(i);
                            innerDico[id] = new [] {"invalid"};
                        }
                        
                        hasAllData.Dispose();
                    }  

                    // I'm not proud of that, seriously, it's ugly
                    RWDefaults = defaults;
                    Refresh();
                }

                public override FastDictionary<string, object> GetDefaults()
                {
                    throw new System.NotImplementedException();
                }

                public override bool ContainsLayout(string layoutType)
                {
                    return Defaults.ContainsKey(layoutType);
                }

                public override void Refresh()
                {
                    Defaults = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, ReadOnlyCollection<string>>>
                    (
                        RWDefaults.ToDictionary
                        (
                            k => k.Key,
                            v => new ReadOnlyDictionary<string, ReadOnlyCollection<string>>
                            (
                                v.Value.ToDictionary
                                (
                                    innerK => innerK.Key,
                                    innerV => new ReadOnlyCollection<string>(innerV.Value)
                                )
                            )
                        )
                    );
                }

                public override void Dispose()
                {
                    Defaults = null;
                }
            }
            
            public class Axis2D : InputSetting<ReadOnlyDictionary<string, ReadOnlyCollection<string>>, Result.Axis2D>
            {
                internal FastDictionary<string, FastDictionary<string, string[]>> RWDefaults;
                
                internal Axis2D(string nameId)
                {
                    NameId      = nameId;
                    DisplayName = string.Empty;
                    Translation = string.Empty;
                    
                    RWDefaults = new FastDictionary<string, FastDictionary<string, string[]>>();
                }
                
                public Axis2D(string                                                   nameId, string displayName,
                              string                                                   translation,
                              FastDictionary<string, FastDictionary<string, string[]>> defaults)
                {
                    NameId      = nameId;
                    DisplayName = displayName;
                    Translation = translation;

                    // Validate
                    foreach (var innerDico in defaults.Values)
                    {
                        var hasAllData = new NativeArray<bool1>(4, Allocator.Temp);
                        foreach (var key in innerDico.Keys)
                        {
                            if (key == "-x") hasAllData[0] = true;
                            if (key == "+x") hasAllData[1] = true;
                            if (key == "-y") hasAllData[2] = true;
                            if (key == "+y") hasAllData[3] = true;
                        }
                        
                        for (int i = 0; i != 4; i++)
                        {
                            if (hasAllData[i])
                                continue;
                        
                            var id = InputDimension.GetDimensionStringId(i);
                            innerDico[id] = new [] {"invalid"};
                        }
                        
                        hasAllData.Dispose();
                    }  
                    
                    // I'm not proud of that, seriously, it's ugly
                    RWDefaults = defaults;
                    Refresh();
                }

                public override FastDictionary<string, object> GetDefaults()
                {
                    throw new System.NotImplementedException();
                }
                
                public override bool ContainsLayout(string layoutType)
                {
                    return Defaults.ContainsKey(layoutType);
                }

                public override void Refresh()
                {
                    Defaults = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, ReadOnlyCollection<string>>>
                    (
                        RWDefaults.ToDictionary
                        (
                            k => k.Key,
                            v => new ReadOnlyDictionary<string, ReadOnlyCollection<string>>
                            (
                                v.Value.ToDictionary
                                (
                                    innerK => innerK.Key,
                                    innerV => new ReadOnlyCollection<string>(innerV.Value)
                                )
                            )
                        )
                    );
                }

                public override void Dispose()
                {
                    Defaults = null;
                }
            }
        }
    }
}