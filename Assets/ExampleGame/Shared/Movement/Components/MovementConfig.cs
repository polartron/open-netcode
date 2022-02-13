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

        public static readonly MovementConfig Player = new MovementConfig()
        {
            Acceleration = 64f, MaxSpeed = 5f, StoppingSpeed = 2f, Friction = 4f
        };
                
        public static readonly MovementConfig Ai = new MovementConfig()
        {
            Acceleration = 64f, MaxSpeed = 4f, StoppingSpeed = 2f, Friction = 4f
        };
    }
}