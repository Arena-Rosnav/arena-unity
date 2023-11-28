# Arena Unity
Unity Simulator for [Arena Rosnav](https://github.com/Arena-Rosnav/arena-rosnav). The executable will be implemented there as a simulator.  
This is run in Unity 2022.3.11f1.

## Getting Started
Make sure that you also initialize the submodule [arena-simulation-setup](https://github.com/Arena-Rosnav/arena-simulation-setup) in `Assets/arena-simulation-setup`.  
To do that use:

```bash
git clone --recurse-submodules https://github.com/Arena-Rosnav/arena-unity.git
```
You will need the [Arena Rosnav Main Repo](https://github.com/Arena-Rosnav/arena-rosnav) too if you want to use the Simulator.

## Usage
See [the official Docs](https://arena-rosnav.readthedocs.io/en/latest/packages/unity_simulator/).

## Note
It could be necessary to run 
```bash
git config http.postBuffer 524288000
```
to use git in this project

## Controls

**WASD**: Just like in any other application  
**Q**: Move Down  
**E**: Move Up
