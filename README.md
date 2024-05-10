# Arena Unity
Unity Simulator for [Arena Rosnav](https://github.com/Arena-Rosnav/arena-rosnav). The executable will be implemented there as a simulator.  
This is run in Unity 2022.3.11f1.

## Getting Started
You will need the [Arena Rosnav Main Repo](https://github.com/Arena-Rosnav/arena-rosnav) too if you want to use the Simulator.
Place this folder inside of the ′/src/arena′ folder of that Repository.

## Installation and Usage
See [the official Docs](https://arena-rosnav.readthedocs.io/en/latest/packages/unity/unity_simulator/).

## Controls
When running the simulator in development mode (in the Game Tab) or executing the build, inside of the simulation use following controls:  
**WASD**: Just like in any other application  
**Q**: Move Down  
**E**: Move Up  
**Shift + WASD**: Move even faster  

## Note
It could be necessary to run 
```bash
git config http.postBuffer 524288000
```
to use git in this project
