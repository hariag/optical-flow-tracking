# Python and C++ optical flow tracking
Using Optical Flow for Detection Motion object.



Original code from the forked repo and  https://github.com/opencv/opencv/blob/2.4.12/samples/python2/opt_flow.py   
1. using video path : python optical_flow.py [video source]  
2. using camera : python optical_flow.py   

Little changing to the original python script to make it run with opencv 3.2.0 and ported also to C++  

C++ flow -> hsv -> bgr function:  
https://stackoverflow.com/questions/7693561/opencv-displaying-a-2-channel-image-optical-flow
