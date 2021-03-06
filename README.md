## Hello from 23/5/22 (an update)
This project is "on hold" until I've stress tested it in the current company I'm working at.
Unity Transport is also not production ready as it is lacking NAT Punchthrough and other features I want. Unity Relay also costs money per CCU so there's that.
I've ditched Unity Transport at the moment as Steam Relay works really well. I'm hoping to get all this into this public repository when Unity slowly crawls towards an 1.0 release of DOTS and when more people are ready to adopt this new workflow. 



![signal-2022-01-09-191118](https://user-images.githubusercontent.com/38894848/148695519-838ba0d3-cdc2-462d-b38d-ddad0ee1499d.png)

Open Netcode is a feature rich networking package for Unity DOTS.


**State:** Early Preview. I'm waiting for Unity DOTS 0.50 to release so I can get the package to an alpha stage. Don't use this for anything yet. MIT license will be added when it's ready.

## Main Features
- Clientside Prediction & Server Reconciliation
- Public and Private Snapshots
- Events
- Delta Compression
- Spatial Hashing
- Bit Compression
- Code Generation
- Tick Synchronization and Tick Dilation

## Requirements
- Unity 2020.3.24f1 (Probably works with later versions in the same tech branch)
- Use of the custom DOTS packages included in the project. Once Entities 0.50 is released they will no longer be necessary.
- Understanding what Unity DOTS is and how to write DOTS code.

## TODO
- Lag Compensation
- Clientside replay system.
- A generic way to predict multiple components. It's only predicting EntityPosition at the moment.
- Custom logging.
- Implement RCON protocol.
- Unit tests. Look into making an integration test for the whole pipeline.
- Use the ProfilerRecorder API to display stats in the profiler.
- Snapshot Prediction as described in the Unity FPSSample Deep Dive video.
- LZM compression of snapshots.
- Wrapper around the Unity Physics Package to use it for physics simulation.
- Hybrid read-only way for "attaching" gameobjects to networked entities.
- Unity Relay Service
