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
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<EntityVelocity>))]
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<PathComponent>))]
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<EntityPosition>))]
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
//</generated>
//<template:predicted>
//[assembly: RegisterGenericComponentType(typeof(Prediction<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityVelocity>))]
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityPosition>))]
//</generated>
//</generated>
//<template:publicsnapshot>
//[assembly: RegisterGenericComponentType(typeof(Prediction<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityVelocity>))]
[assembly: RegisterGenericComponentType(typeof(Prediction<PathComponent>))]
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityPosition>))]
//</generated>
//</generated>
//<template:privatesnapshot>
//[assembly: RegisterGenericComponentType(typeof(Prediction<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(Prediction<EntityHealth>))]
//</generated>
