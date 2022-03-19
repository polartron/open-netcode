using Unity.Entities;

namespace ExampleGame.Shared.Movement.Components
{
    [GenerateAuthoringComponent]
    public struct MovementConfig : IComponentData
    {
        public float Acceleration;
        public float MaxSpeed;
        public float StoppingSpeed;
        public float Friction;
    }
}