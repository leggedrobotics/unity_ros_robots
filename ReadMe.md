# Robot Package for Unity ROS Teleoperation

This package provides various robot models for the [Unity ROS Teleoperation project](https://github.com/leggedrobotics/unity_ros_teleoperation). Each robot model can be imported individually into your Unity project, or certain metapackages can be used to import multiple related robots at once.

## Installation
To install, add the Robot Core package to your Unity project via the Unity Package Manager using the following Git URL:

```
git@github.com:leggedrobotics/unity_ros_robots.git?path=RobotCore
```
This will allow you to create Robot Database objects from the create menu: `Create > Robots > RobotDatabase`. Clicking on one of these objects will then use the data in [`robots.json`](RobotCore/Runtime/robots.json) to display the available robot packages and allow you to install them into your project via the Inspector UI.

## Robots
The following robot models are available in this package, either as individual packages or as part of metapackages:

| Robot | Image | Meta Packages |
|-------|-------|-------------|----------------|
| ALMA | | all, anymal |
| AnymalD | | all, anymal |
| B2W | | all |
| Dynaarm | | all, arms |
| Franka Panda Arm | | all, arms |
| GR2 | | all |
| Spot | | all |
| Tytan | | all, anymal |