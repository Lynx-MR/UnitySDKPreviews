# Lynx Unity SDK Previews

# Context

The Previews package is an additional package for features considered as beta at the moment.

# Requirements

This package requires the Lynx Core SDK to work properly.
To use the previews samples scenes, we also recommend to add the Lynx Modules SDK for handtracking features.

# Description

## Capture

Contains scripts to access Lynx-R1 cameras, pointcloud and 6 dof data.


## Samples

To illustrate these different features, the samples are currently divided into 5 categories:
* Common Assets
* HeadPose
* PointCloud
* Recorder
* VideoCapture

### Common Assets

Contains the shared assets used by the other samples.

### HeadPose

Contains a scene with a 3D headset that moves as you move the headset.

### PointCloud

A scene displaying cubes that match with retrieved position of each points from the point cloud.

### Recorder

A scene with 3D elements displaying YUV image of AR + VR.
By using hand menu, the user can start/stop recording. Each videos are saved on the headset.


### VideoCapture

Different scenes to provide examples of the features available for video capture (image from all sensors, GPU process, resizing, fish eye rectification, AR + VR buffers).