using System.Diagnostics.Contracts;
using OpenNetcode.Shared.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace OpenNetcode.Shared.Systems
{
    public struct FloatingOrigin : IComponentData
    {
        public static readonly int UnitsPerMeter = 256;

        public GameUnits Origin;
        public float3 Offset;

        [Pure]
        public Vector3 Snap(float3 position)
        {
            return GetUnityVector(GameUnits.FromUnityVector3(position - Offset, UnitsPerMeter));
        }

        [Pure]
        public GameUnits GetGameUnits(in float3 position)
        {
            return GameUnits.FromUnityVector3(position - Offset, UnitsPerMeter) + Origin;
        }

        [Pure]
        public float3 GetUnityVector(in GameUnits coordinate)
        {
            return (coordinate - Origin).ToUnityVector3(UnitsPerMeter) + Offset;
        }
    }

    [DisableAutoCreation]
    public partial class FloatingOriginSystem : SystemBase
    {
        private readonly float3 _offset;

        public FloatingOriginSystem(float3 offset)
        {
            _offset = float3.zero;
        }

        protected override void OnCreate()
        {
            EntityManager.CreateEntity(ComponentType.ReadOnly<FloatingOrigin>());

            SetSingleton(new FloatingOrigin()
            {
                Origin = GameUnits.zero,
                Offset = _offset
            });
        }

        protected override void OnUpdate()
        {

        }
    }
}