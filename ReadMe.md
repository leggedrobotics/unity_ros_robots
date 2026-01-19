# Robot Package for Unity ROS Teleoperation

This package provides various robot models for the [Unity ROS Teleoperation project](https://github.com/leggedrobotics/unity_ros_teleoperation). Each robot model can be imported individually into your Unity project through the Robot Database asset. This acts as a central repository for all available robots either locally, pulled from external Git repositories such as this one, or imported from .unitypackages.

## Installation
To install, add the Robot Core package to your Unity project via the Unity Package Manager using the following Git URL:

```
git@github.com:leggedrobotics/unity_ros_robots.git?path=RobotCore
```
This will allow you to create Robot Database objects from the create menu: `Create > Robots > RobotDatabase`. Clicking on one of these objects will then use the data in [`robots.json`](RobotCore/Runtime/robots.json) to display the available robot packages and allow you to install them into your project via the Inspector UI.

## Robots
The following robot models are available in this package and can be installed via the Robot Database:

| Robot | Image | 
|-------|-------|
| [ALMA](Alma) | ![ALMA](/docs/images/alma.jpeg) | 
| [AnymalD](AnymalD) | ![AnymalD](/docs/images/anymald.jpeg) |
| [B2W](B2W) | ![B2W](/docs/images/b2w.jpeg) | 
| [Dynaarm](Dynaarm) | ![Dynaarm](/docs/images/dynaarm.jpeg) |
| [Franka Panda Arm](FrankaPanda) | ![Franka Panda Arm](/docs/images/franka_panda.jpeg) |
| [GR2](Gr2) | ![GR2](/docs/images/gr2.jpeg) | 
| [Spot](Spot) | ![Spot](/docs/images/spot.jpeg) |