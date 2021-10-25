using System;
using System.Linq;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Utility;

namespace StormiumTeam.GameBase
{
    public struct ResPath : IEquatable<ResPath>
    {
        public readonly EType  Type;
        public readonly string Author;
        public readonly string ModPack;
        public readonly string Resource;

        private string? computedFullString;

        public string FullString => computedFullString ??= Create(Author, ModPack, Resource, Type);

        public ResPath(EType type, string author, string modPack, string resource)
        {
            Type     = type;
            Author   = author;
            ModPack  = modPack;
            Resource = resource;

            computedFullString = null;
        }

        public ResPath(EType type, string author, string modPack, string[] resourceDeepness)
            : this(type, author, modPack, string.Join("/", resourceDeepness))
        {
        }

        public ResPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                this = default;

                Author = ModPack = Resource = string.Empty;
                
                return;
            }

            computedFullString = fullPath;

            var inspection = Inspect(fullPath);
            Type     = inspection.Type;
            Author   = inspection.Author;
            ModPack  = inspection.ModPack;
            Resource = inspection.ResourcePath;
        }

        public override bool Equals(object? obj)
        {
            return obj is ResPath resPath && Equals(resPath);
        }

        public readonly bool Equals(ResPath other)
        {
            return Author == other.Author && ModPack == other.ModPack && Resource == other.Resource;
        }
        
        public readonly bool EqualsWithType(in ResPath other)
        {
            return Type == other.Type && Author == other.Author && ModPack == other.ModPack && Resource == other.Resource;
        }

        public override string ToString()
        {
            return FullString;
        }

        public enum EType
        {
            /// <summary>
            /// Prioritize MasterServer
            /// </summary>
            MasterServer,
            /// <summary>
            /// Prioritize Client
            /// </summary>
            ClientResource,
        }

        public struct Inspection
        {
            public EType  Type;
            public bool   IsGUID;
            public string Author;
            public string ModPack;
            public string ResourcePath;

            public bool IsCore;
        }

        public static Inspection Inspect(string path)
        {
            Inspection inspection = default;

            var firstDotIdx    = path.IndexOf('.');
            var protocolEndIdx = path.IndexOf("://");

            // MasterServer
            if (path.StartsWith("ms://"))
            {
                var asSpan = path.AsSpan("ms://".Length);

                inspection.Type = EType.MasterServer;

                firstDotIdx = asSpan.IndexOf('.');

                inspection.Author       = asSpan.Slice(0, firstDotIdx).ToString();
                inspection.ModPack      = asSpan.Slice(firstDotIdx + 1, (asSpan.IndexOf('/') - firstDotIdx) - 1).ToString();
                inspection.ResourcePath = asSpan.Slice(asSpan.IndexOf('/') + 1).ToString();
            }

            // Client Resource
            else if (path.StartsWith("cr://"))
            {
                var asSpan = path.AsSpan("cr://".Length);

                inspection.Type = EType.MasterServer;

                firstDotIdx = asSpan.IndexOf('.');

                inspection.Author       = asSpan.Slice(0, firstDotIdx).ToString();
                inspection.ModPack      = asSpan.Slice(firstDotIdx + 1, (asSpan.IndexOf('/') - firstDotIdx) - 1).ToString();
                inspection.ResourcePath = asSpan.Slice(asSpan.IndexOf('/') + 1).ToString();
            }

            // Treat path without a protocol as a masterserver path
            else if (path.Contains("/") && path.Contains("."))
            {
                return Inspect("ms://" + path);
            }
            
            inspection.IsCore = inspection.Author == inspection.ModPack && inspection.Author == "#";

            return inspection;
        }

        public static string Create(string author, string modPack, string resource, EType type)
        {
            return type switch
            {
                EType.MasterServer => $"ms://{author}.{modPack}/{resource}",
                EType.ClientResource => $"cr://{author}.{modPack}/{resource}"
            };
        }

        public static string Create(string author, string modPack, string[] resourceDeepness, EType type)
        {
            return Create(author, modPack, string.Join('/', resourceDeepness), type);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Author, ModPack, Resource);
        }
    }

    public struct ResPathDefaults
    {
        public string Author;
        public string ModPack;

        public readonly ResPath With(string[] resourceDeepness, ResPath.EType type)
        {
            return new (type, Author, ModPack, resourceDeepness);
        }
    }

    public class ResPathGen : AppObject
    {
        public ResPathGen(Context context) : base(context)
        {
            DependencyResolver.Add<DefaultEntity<ResPathDefaults>>();
            DependencyResolver.OnComplete(deps =>
            {
                StateEntity = deps.OfType<DefaultEntity<ResPathDefaults>>()
                                  .First()
                                  .Entity;
            });
        }

        private Entity stateEntity;

        public Entity StateEntity
        {
            get => stateEntity;
            set
            {
                stateEntity = value;
                if (!stateEntity.Has<ResPathDefaults>())
                    stateEntity.Set(new ResPathDefaults());
                
                DependencyResolver.Dependencies.Clear();
            }
        }

        public ref readonly ResPathDefaults GetDefaults() => ref StateEntity.Get<ResPathDefaults>();

        public string Create(string resource, ResPath.EType type)
        {
            ref readonly var defaults = ref GetDefaults();
            return ResPath.Create(defaults.Author, defaults.ModPack, resource, type);
        }

        public string Create(string[] resourceDeepness, ResPath.EType type)
        {
            ref readonly var defaults = ref GetDefaults();
            return ResPath.Create(defaults.Author, defaults.ModPack, resourceDeepness, type);
        }
    }
}