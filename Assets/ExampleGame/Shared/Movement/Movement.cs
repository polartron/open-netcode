using System;
using ExampleGame.Shared.Movement.Components;
using UnityEngine;

namespace ExampleGame.Shared.Movement
{
    public static class Movement
    {
        public static void CalculateVelocity(ref Vector3 velocity, in MovementConfig config, in MovementInput input, float deltaTime)
        {
            Vector2 moveVector = input.Move;

            if (moveVector.magnitude > 1f)
            {
                moveVector = moveVector.normalized;
            }

            Accelerate(ref velocity, moveVector, config.Acceleration, config.MaxSpeed, deltaTime);
            Friction(ref velocity, config.Friction, config.StoppingSpeed, deltaTime);
        }

        public static void Move(ref Vector3 position, in Vector3 velocity, float deltaTime)
        {
            position += (velocity * deltaTime);
        }
        
        private static void Accelerate(ref Vector3 velocity, in Vector2 moveVector, float acceleration, float maxSpeed, float deltaTime)
        {
            float speed = velocity.magnitude;

            Vector2 moveAdjusted = moveVector;

            if (speed > 0 && speed > maxSpeed)
            {
                moveAdjusted *= maxSpeed / speed;
            }

            velocity.x += moveAdjusted.x * (acceleration * deltaTime);
            velocity.z += moveAdjusted.y * (acceleration * deltaTime);
        }
        
        private static void Friction(ref Vector3 velocity, float friction, float stopSpeed, float deltaTime)
        {
            float speed = velocity.magnitude;

            if (speed < 0.0001905f)
            {
                return;
            }

            var control = (speed < stopSpeed) ? stopSpeed : speed;
            float drop = control * friction * deltaTime;

            float newSpeed = speed - drop;

            if (newSpeed < 0)
            {
                newSpeed = 0;
            }

            if (Math.Abs(newSpeed - speed) > float.Epsilon)
            {
                newSpeed /= speed;
                velocity.x *= newSpeed;
                velocity.z *= newSpeed;
            }
        }
    }
}
