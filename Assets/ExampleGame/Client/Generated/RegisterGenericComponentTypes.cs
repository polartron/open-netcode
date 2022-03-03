using Unity.Entities;
using OpenNetcode.Client.Components;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

//<template:publicsnapshot>
//[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<EntityPosition>))]
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<EntityVelocity>))]
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<PathComponent>))]
//</generated>
//<template:privatesnapshot>
//[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<EntityHealth>))]
//</generated>
//<template:publicevent>
//[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<##TYPE##>))]
//[assembly: RegisterGenericComponentType(typeof(DynamicBuffer<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<BumpEvent>))]
[assembly: RegisterGenericComponentType(typeof(DynamicBuffer<BumpEvent>))]
//</generated>
//<template:input>
//[assembly: RegisterGenericComponentType(typeof(SavedInput<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(SavedInput<MovementInput>))]
[assembly: RegisterGenericComponentType(typeof(SavedInput<WeaponInput>))]
//</generated>
//<template:predicted>
//[assembly: RegisterGenericComponentType(typeof(Prediction<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityPosition>))]
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityVelocity>))]
//</generated>
//</generated>
//<template:publicsnapshot>
//[assembly: RegisterGenericComponentType(typeof(Prediction<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityPosition>))]
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityVelocity>))]
[assembly: RegisterGenericComponentType(typeof(Prediction<PathComponent>))]
//</generated>
//</generated>
//<template:privatesnapshot>
//[assembly: RegisterGenericComponentType(typeof(Prediction<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityHealth>))]
//</generated>
