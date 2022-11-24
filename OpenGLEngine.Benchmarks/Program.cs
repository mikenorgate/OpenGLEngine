using System.Reflection;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Running;
using OpenGLEngine.ECS;

namespace OpenGLEngine.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfig? config = null;
#if DEBUG
            config = new DebugInProcessConfig();
#endif

            var summary = BenchmarkRunner.Run<EntitySystemBenchmarks>(config);
        }
    }

    [MemoryDiagnoser()]
#if !DEBUG
    [HardwareCounters(HardwareCounter.BranchInstructions, HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses, HardwareCounter.TotalCycles)]
#endif
    //[InliningDiagnoser(true, true)]
    [KeepBenchmarkFiles]
    public class EntitySystemBenchmarks
    {
        private ECS.EntitySystem _ecs;
        private BenchmarkSystem _system;
        private Entity _entity1;

        [Params(1/*, 10, 1000, 1000, 10000, 100000*/)]
        public int EntityCount { get; set; }

        [Params(/*1, 2, 4, 8, 16, */32)]
        public int ComponentCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _ecs = new ECS.EntitySystem(EntityCount);

            for (var i = 1; i <= ComponentCount; i++)
            {
                var type = Assembly.GetAssembly(typeof(EntitySystemBenchmarks)).GetType($"{typeof(EntitySystemBenchmarks).FullName}+BenchmarkComponent{i}", true);

                var method = typeof(ECS.EntitySystem).GetMethod(nameof(ECS.EntitySystem.RegisterComponent))
                    .MakeGenericMethod(type);

                method.Invoke(_ecs, Array.Empty<object?>());
            }

            _system = _ecs.RegisterSystem<BenchmarkSystem>();

            var signature = new Signature();
            for (var i = 1; i <= ComponentCount; i++)
            {
                var type = Assembly.GetAssembly(typeof(EntitySystemBenchmarks)).GetType($"{typeof(EntitySystemBenchmarks).FullName}+BenchmarkComponent{i}", true);

                var method = typeof(ECS.EntitySystem).GetMethod(nameof(ECS.EntitySystem.GetComponentType))
                    .MakeGenericMethod(type);

                var componentType = (ComponentType)method.Invoke(_ecs, Array.Empty<object?>());

                signature.Set(componentType);
            }
            _ecs.SetSystemSignature<BenchmarkSystem>(signature);

            _entity1 = _ecs.CreateEntity();

            for (var i = 0; i < EntityCount; i++)
            {
                var entity = i == 0 ? _entity1 : _ecs.CreateEntity();

                for (var j = 1; j <= ComponentCount; j++)
                {
                    var type = Assembly.GetAssembly(typeof(EntitySystemBenchmarks)).GetType($"{typeof(EntitySystemBenchmarks).FullName}+BenchmarkComponent{j}", true);

                    var method = typeof(ECS.EntitySystem).GetMethod(nameof(ECS.EntitySystem.AddComponent))
                        .MakeGenericMethod(type);

                    var component = Activator.CreateInstance(type);

                    method.Invoke(_ecs, new object?[] { entity, component });
                }
            }
        }

        //[Benchmark]
        //public long EnumerateAllEntitiesInSystem()
        //{
        //    var i = 0L;
        //    foreach (var entity in _system.AllEntities)
        //    {
        //        i += entity.Id;
        //    }

        //    return i;
        //}

        [Benchmark]
        public BenchmarkComponent1 GetComponentFromEntity()
        {
            return _ecs.GetComponent<BenchmarkComponent1>(_entity1);
        }

        public class BenchmarkSystem : ECS.System
        {
            public IEnumerable<Entity> AllEntities => Entities;
        }
        

        public struct BenchmarkComponent1
        { }
        public struct BenchmarkComponent2
        { }
        public struct BenchmarkComponent3
        { }
        public struct BenchmarkComponent4
        { }
        public struct BenchmarkComponent5
        { }
        public struct BenchmarkComponent6
        { }
        public struct BenchmarkComponent7
        { }
        public struct BenchmarkComponent8
        { }
        public struct BenchmarkComponent9
        { }
        public struct BenchmarkComponent10
        { }
        public struct BenchmarkComponent11
        { }
        public struct BenchmarkComponent12
        { }
        public struct BenchmarkComponent13
        { }
        public struct BenchmarkComponent14
        { }
        public struct BenchmarkComponent15
        { }
        public struct BenchmarkComponent16
        { }
        public struct BenchmarkComponent17
        { }
        public struct BenchmarkComponent18
        { }
        public struct BenchmarkComponent19
        { }
        public struct BenchmarkComponent20
        { }
        public struct BenchmarkComponent21
        { }
        public struct BenchmarkComponent22
        { }
        public struct BenchmarkComponent23
        { }
        public struct BenchmarkComponent24
        { }
        public struct BenchmarkComponent25
        { }
        public struct BenchmarkComponent26
        { }
        public struct BenchmarkComponent27
        { }
        public struct BenchmarkComponent28
        { }
        public struct BenchmarkComponent29
        { }
        public struct BenchmarkComponent30
        { }
        public struct BenchmarkComponent31
        { }
        public struct BenchmarkComponent32
        { }
    }
}