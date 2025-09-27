"# ROS-Unity Communication"



## Unity side:



You need to install the ROS TCP Connector package from Unity-Technologies!(If not in package manager)



I created two game objects one named Publish and the other called RosConnector



-> Ros Connector contains the ROSConnection script from the ROS TCP Connector package responsible for binding to the ROS noetic server.The ip address is set to 127.0.0.1 and the port to 10000



-> Publish object contains a script made from scratch called CoordinatesPublisher.cs which is responsible for sending points msg coordinates from the clicked points of the DICOM images



-> On the BiopsyClickManager.cs:

inside the BiopsyClickManager class i added:

public CoordinatesPublisher coordinatesPublisher; 

public Vector3? xClick; //  changed from private to public

public Vector3? yClick; //  changed from private to public

public Vector3? zClick; //  changed from private to public





I also included after the DrawLine function this function to send the messages over to ROS:

  void PublishPointsToROS()

   {

       if (coordinatesPublisher != null)

       {

           coordinatesPublisher.PublishPoints();

           Debug.Log("Points published to ROS!");

       }

       else

       {

           Debug.LogWarning("Coordinates publisher not set!");

       }

   }

then i called this function to the update function.




## ROS side:



Step 1: Open VS code and open the folder ros/ros-devcontainer-vscode  of this repository.





Step 2: Open Docker desktop





Step 3: Then on the terminal type: docker compose up







Step 4: Now the server is running on your docker desktop. Click on the left side of VS code and click Attach to an existing container and choose the workspace-1 devcontainer







Step 5: (If the ROS-TCP-ENDPOINT is visible on src skip this step) Install the ros-tcp-endpoint package from Unity -Technologies by following this series of commands:


cd /workspace/src



git clone https://github.com/Unity-Technologies/ROS-TCP-Endpoint.git



cd /workspace



rosdep update



rosdep install --from-paths src --ignore-src -r -y



catkin\_make



source devel/setup.bash



rospack find ros\_tcp\_endpoint




Step 6:I made a receiver.py python code in order to set the Ros as a server and acquire the x,y,z coordinates clicked from Unity

This code is stored inside the /src/scripts folder in the workspace!



#!/usr/bin/env python3

import rospy

from geometry_msgs.msg import Point

import json 

import os



json\_entry\_coordinates = \[]

json\_target\_coordinates = \[]

entry\_file\_path = "/workspace/src/entry\_coordinates.json"

target\_file\_path = "/workspace/src/target\_coordinates.json"

combined\_file\_path = "/workspace/src/all\_coordinates.json"



def entry\_point\_callback(msg):

&nbsp;   global json\_entry\_coordinates

&nbsp;   rospy.loginfo("received entry point from Unity:")

&nbsp;   rospy.loginfo("X: %.2f, Y: %.2f, Z: %.2f", msg.x, msg.y, msg.z)

&nbsp;   print(f"ENTRY point -> X: {msg.x:.2f}, Y: {msg.y:.2f}, Z: {msg.z:.2f}")



&nbsp;   entry\_coordinate\_array = \[

&nbsp;       float(msg.x),

&nbsp;       float(msg.y),

&nbsp;       float(msg.z)

&nbsp;   ]

&nbsp;   json\_entry\_coordinates.append(entry\_coordinate\_array)



&nbsp;   save\_entry\_to\_json()

&nbsp;   save\_combined\_json()



def target\_point\_callback(msg):

&nbsp;   global json\_target\_coordinates

&nbsp;   rospy.loginfo("received target point from Unity:")

&nbsp;   rospy.loginfo("X: %.2f, Y: %.2f, Z: %.2f", msg.x, msg.y, msg.z)

&nbsp;   print(f"TARGET point -> X: {msg.x:.2f}, Y: {msg.y:.2f}, Z: {msg.z:.2f}")



&nbsp;   target\_coordinate\_array = \[

&nbsp;       float(msg.x),

&nbsp;       float(msg.y),

&nbsp;       float(msg.z)

&nbsp;   ]

&nbsp;   json\_target\_coordinates.append(target\_coordinate\_array)



&nbsp;   save\_target\_to\_json()

&nbsp;   save\_combined\_json()



def save\_entry\_to\_json():

&nbsp;   try:

&nbsp;       with open(entry\_file\_path, 'w') as f:

&nbsp;           json.dump(json\_entry\_coordinates, f, indent=2)

&nbsp;       print(f"Entry coordinates saved to {entry\_file\_path}")

&nbsp;       print(f"Total entry points: {len(json\_entry\_coordinates)}")

&nbsp;   except Exception as e:

&nbsp;       rospy.logerr(f"Error saving entry coordinates to JSON: {e}")



def save\_target\_to\_json():

&nbsp;   try:

&nbsp;       with open(target\_file\_path, 'w') as f:

&nbsp;           json.dump(json\_target\_coordinates, f, indent=2)

&nbsp;       print(f"Target coordinates saved to {target\_file\_path}")

&nbsp;       print(f"Total target points: {len(json\_target\_coordinates)}")

&nbsp;   except Exception as e:

&nbsp;       rospy.logerr(f"Error saving target coordinates to JSON: {e}")



def save\_combined\_json():

&nbsp;   try:

&nbsp;       combined\_data = {

&nbsp;           "entry\_coordinates": json\_entry\_coordinates,

&nbsp;           "target\_coordinates": json\_target\_coordinates

&nbsp;       }

&nbsp;       with open(combined\_file\_path, 'w') as f:

&nbsp;           json.dump(combined\_data, f, indent=2)

&nbsp;       print(f"Combined coordinates saved to {combined\_file\_path}")

&nbsp;       

&nbsp;       # Check if we have both entry and target points for biopsy planning

&nbsp;       if len(json\_entry\_coordinates) >= 1 and len(json\_target\_coordinates) >= 1:

&nbsp;           print("✓ Ready for biopsy planning! Have entry and target points")

&nbsp;       

&nbsp;   except Exception as e:

&nbsp;       rospy.logerr(f"Error saving combined coordinates: {e}")



def listener():

&nbsp;   rospy.init\_node('coordinate\_listener', anonymous=True)

&nbsp;   

&nbsp;   # Subscribe to both entry and target point topics

&nbsp;   rospy.Subscriber('/entry\_point', Point, entry\_point\_callback)

&nbsp;   rospy.Subscriber('/target\_point', Point, target\_point\_callback)

&nbsp;   

&nbsp;   rospy.loginfo("Coordinate listener started. Waiting for Unity coordinates...")

&nbsp;   print("=== ROS Coordinate Listener Active ===")

&nbsp;   print("Listening for:")

&nbsp;   print("  - Entry points on: /entry\_point")

&nbsp;   print("  - Target points on: /target\_point")

&nbsp;   print("Waiting for Unity to send data...")

&nbsp;   

&nbsp;   rospy.spin()



if \_\_name\_\_ == '\_\_main\_\_':

&nbsp;   try:

&nbsp;       listener()

&nbsp;   except rospy.ROSInterruptException:

&nbsp;       rospy.loginfo("Coordinate listener node terminated.")

&nbsp;       print("Coordinate listener stopped.")





Step 7: Open three terminals







Terminal 1:







source /opt/ros/noetic/setup.bash







roscore







Terminal 2:





source /workspace/devel/setup.bash


roslaunch ros_tcp_endpoint endpoint.launch tcp_ip:=0.0.0.0 tcp_port:=10000







Terminal 3:




source /workspace/devel/setup.bash



cd /workspace/src/scripts



python3 receiver.py







IMPORTANT: In order for the whole process to work properly , first you need to run the ROS side and then the Unity side.

Also inside the docker-compose.yml file of the ros-devcontainer-vscode folder you need to navigate to workspace section. Then on the port section you need to add 10000:10000 on the ports.



















