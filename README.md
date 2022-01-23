![signal-2022-01-09-191118](https://user-images.githubusercontent.com/38894848/148695519-838ba0d3-cdc2-462d-b38d-ddad0ee1499d.png)

Open Netcode is a feature rich networking package for Unity DOTS. It's open source and free to use forever. It's up to the developer how many players they can squeeze into their game, so it does not have a concurrent users limit like most other packages with these features.

**State:** Early Preview. Don't use this in production. 

## Main Features
- Clientside Prediction & Server Reconciliation
- Public and Private Snapshots
- Delta Compression
- Spatial Hashing
- Bit Compression
- Code Generation
- Tick Synchronization and Tick Dilation

## Requirements
- Unity 2020.3.24f1 (Probably works with later versions in the same tech branch)
- Use of the custom DOTS packages included in the project. Once Entities 0.50 is released they will no longer be necessary.
- Understanding what Unity DOTS is and how it works.

## TODO
- Events. Right now you're only getting component updates and that's not crescent fresh.
- Project wide cleanup of prototype code. Add .dotsettings & .editorconfig files to project root.
- Custom logging.
- Implement RCON protocol.
- Unit tests. Look into making an integration test for the whole pipeline.
- Use the ProfilerRecorder API to display stats in the profiler.
- Snapshot Prediction as described in the Unity FPSSample Deep Dive video.
- LZM compression of snapshots.
- Wrapper around the Unity Physics Package to use it for physics simulation.
- Hybrid read-only way for "attaching" gameobjects to networked entities.
- Unity Relay Service
