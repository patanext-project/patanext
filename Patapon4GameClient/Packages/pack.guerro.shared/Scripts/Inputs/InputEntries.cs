using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace Packet.Guerro.Shared.Inputs
{
    public partial class CInputManager
    {
        public interface IInputSetting : IDisposable
        {
            Type ResultType { get; }

            string NameId { get; }
            string DisplayName { get; }
            string Translation { get; }

            FastDictionary<string, object> GetDefaults();
            
            bool ContainsLayout(string layoutType);
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
            
            public struct Push : IInputSetting
            {
                public Type ResultType { get; }

                public string NameId      { get; }
                public string DisplayName { get; }
                public string Translation { get; }

                public ReadOnlyDictionary<string, ReadOnlyCollection<string>> Defaults;

                public Push(string nameId, string displayName, string translation, FastDictionary<string, string[]> defaults)
                {
                    ResultType = typeof(Result.Push);
                    NameId      = nameId;
                    DisplayName = displayName;
                    Translation = translation;

                    Defaults = new ReadOnlyDictionary<string, ReadOnlyCollection<string>>
                        (defaults.ToDictionary(k => k.Key, v => new ReadOnlyCollection<string>(v.Value)));
                }
                
                public FastDictionary<string, object> GetDefaults()
                {
                    throw new System.NotImplementedException();
                }

                public bool ContainsLayout(string layoutType)
                {
                    return Defaults.ContainsKey(layoutType);
                }

                public void Dispose()
                {
                    Defaults = null;
                }
            }

            public struct Axis1D : IInputSetting
            {
                public Type ResultType { get; }

                public string NameId      { get; }
                public string DisplayName { get; }
                public string Translation { get; }

                public ReadOnlyDictionary<string, ReadOnlyDictionary<string, ReadOnlyCollection<string>>> Defaults;

                /*
                 * Todo: validate if defaults are in 1D
                 */
                public Axis1D(string                                                   nameId, string displayName,
                              string                                                   translation,
                              FastDictionary<string, FastDictionary<string, string[]>> defaults)
                {
                    ResultType  = typeof(Result.Axis1D);
                    NameId      = nameId;
                    DisplayName = displayName;
                    Translation = translation;

                    // I'm not proud of that, seriously, it's ugly
                    Defaults = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, ReadOnlyCollection<string>>>
                    (
                        defaults.ToDictionary
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

                public FastDictionary<string, object> GetDefaults()
                {
                    throw new System.NotImplementedException();
                }

                public bool ContainsLayout(string layoutType)
                {
                    return Defaults.ContainsKey(layoutType);
                }

                public void Dispose()
                {
                    Defaults = null;
                }
            }
            
            public struct Axis2D : IInputSetting
            {
                public Type ResultType { get; }
                
                public string NameId { get; }
                public string DisplayName { get; }
                public string Translation { get; }

                public ReadOnlyDictionary<string, ReadOnlyDictionary<string, ReadOnlyCollection<string>>> Defaults;

                /*
                 * Todo: validate if the defaults are in 2D
                 */
                public Axis2D(string                                                   nameId, string displayName,
                              string                                                   translation,
                              FastDictionary<string, FastDictionary<string, string[]>> defaults)
                {
                    ResultType = typeof(Result.Axis2D);
                    NameId      = nameId;
                    DisplayName = displayName;
                    Translation = translation;

                    // I'm not proud of that, seriously, it's ugly
                    Defaults = new ReadOnlyDictionary<string, ReadOnlyDictionary<string, ReadOnlyCollection<string>>>
                    (
                        defaults.ToDictionary
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

                public FastDictionary<string, object> GetDefaults()
                {
                    throw new System.NotImplementedException();
                }
                
                public bool ContainsLayout(string layoutType)
                {
                    return Defaults.ContainsKey(layoutType);
                }

                public void Dispose()
                {
                    Defaults = null;
                }
            }
        }
    }
}