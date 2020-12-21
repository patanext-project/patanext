using System;
using System.Linq;
using DefaultEcs;
using GameHost.Core.Ecs;
using GameHost.Injection;
using GameHost.Utility;

namespace StormiumTeam.GameBase
{
    public struct ResPath
    {
        public string Author;
        public string ModPack;
        public string File;
    
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
            if (path.StartsWith("cr://"))
            {
                var asSpan = path.AsSpan("cr://".Length);

                inspection.Type = EType.MasterServer;

                firstDotIdx = asSpan.IndexOf('.');

                inspection.Author       = asSpan.Slice(0, firstDotIdx).ToString();
                inspection.ModPack      = asSpan.Slice(firstDotIdx + 1, (asSpan.IndexOf('/') - firstDotIdx) - 1).ToString();
                inspection.ResourcePath = asSpan.Slice(asSpan.IndexOf('/') + 1).ToString();
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
    }

    public struct ResPathDefaults
    {
        public string Author;
        public string ModPack;
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