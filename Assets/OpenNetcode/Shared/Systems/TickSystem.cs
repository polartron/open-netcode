using OpenNetcode.Shared.Time;
using Unity.Core;
using Unity.Entities;

namespace OpenNetcode.Shared.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    public class TickPreSimulationSystemGroup : FixedStepSimulationSystemGroup
    {
        public TickPreSimulationSystemGroup()
        {
            FixedRateManager = null;
        }
    }
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    public class TickSimulationSystemGroup : FixedStepSimulationSystemGroup
    {
        public TickSimulationSystemGroup()
        {
            FixedRateManager = null;
        }
    }
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
    public class TickPostSimulationSystemGroup : FixedStepSimulationSystemGroup
    {
        public TickPostSimulationSystemGroup()
        {
            FixedRateManager = null;
        }
    }
    
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderLast = true)]
    [DisableAutoCreation]
    public class TickPreSimulationEntityCommandBufferSystem : EntityCommandBufferSystem
    {
    }

    [UpdateInGroup(typeof(TickSimulationSystemGroup), OrderLast = true)]
    [DisableAutoCreation]
    public class TickSimulationEntityCommandBufferSystem : EntityCommandBufferSystem
    {
    }

    [UpdateInGroup(typeof(TickPostSimulationSystemGroup), OrderLast = true)]
    [DisableAutoCreation]
    public class TickPostSimulationEntityCommandBufferSystem : EntityCommandBufferSystem
    {
    }
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class TickSystem : SystemBase, ITickable
    {
        public int Tick => (int) _ticker.TickFloat;
        public float TickFloat => _ticker.TickFloat;
        public double TickerTime => _ticker.Time;
        
        private Ticker _ticker;
        private TickPreSimulationSystemGroup _tickPreSimulationSystemGroup;
        private TickSimulationSystemGroup _tickSimulationSystemGroup;
        private TickPostSimulationSystemGroup _tickPostSimulationSystemGroup;

        public void Reset()
        {
            _ticker.Reset();
        }
        
        public TickSystem(int ticksPerSecond, long timeInMs)
        {
            _ticker = new Ticker(ticksPerSecond, timeInMs);
            _ticker.Add(this);
        }

        public void SetTime(double time)
        {
            _ticker.SetTime(time);
        }

        protected override void OnCreate()
        {
            _tickPreSimulationSystemGroup = World.AddSystem(new TickPreSimulationSystemGroup());
            _tickSimulationSystemGroup = World.AddSystem(new TickSimulationSystemGroup());
            _tickPostSimulationSystemGroup = World.AddSystem(new TickPostSimulationSystemGroup());

            AddPreSimulationSystem(new TickPreSimulationEntityCommandBufferSystem());
            AddSimulationSystem(new TickSimulationEntityCommandBufferSystem());
            AddPostSimulationSystem(new TickPostSimulationEntityCommandBufferSystem());

            base.OnCreate();
        }

        public void AddPreSimulationSystem(ComponentSystemBase system)
        {
            World.AddSystem(system);
            _tickPreSimulationSystemGroup.AddSystemToUpdateList(system);
            _tickPreSimulationSystemGroup.SortSystems();
        }
        
        public void AddSimulationSystem(ComponentSystemBase system)
        {
            World.AddSystem(system);
            _tickSimulationSystemGroup.AddSystemToUpdateList(system);
            _tickSimulationSystemGroup.SortSystems();
        }
        
        public void AddPostSimulationSystem(ComponentSystemBase system)
        {
            World.AddSystem(system);
            _tickPostSimulationSystemGroup.AddSystemToUpdateList(system);
            _tickPostSimulationSystemGroup.SortSystems();
        }

        protected override void OnUpdate()
        {
            _ticker.Update();
        }

        //Used to re-simulate after mispredictions
        public void StepSimulation()
        {
            World.PushTime(new TimeData(Time.ElapsedTime, TimeConfig.FixedDeltaTime));
            _tickSimulationSystemGroup.Update();
            World.PopTime();
        }

        public void OnTick(float deltaTime, int tick)
        {
            SetSingleton(new TickData()
            {
                Value = tick
            });
            
            World.PushTime(new TimeData(Time.ElapsedTime, TimeConfig.FixedDeltaTime));
            _tickPreSimulationSystemGroup.Update();
            _tickSimulationSystemGroup.Update();
            _tickPostSimulationSystemGroup.Update();
            World.PopTime();
        }
    }
}
