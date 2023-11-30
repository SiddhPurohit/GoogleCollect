# GoogleCollect
An endless runner game made using unity.
The projects consists of 2 parts - 
  1. Body detection (Python)
  2. Endless Runner game (Unity)

The python script runs on a desktop, whereas the unity code runs on an iPad.
The python code detects body tilt using mediapipe and opencv and sends that data to the iPad.
I have established a UDP socket connection between the Unity game and the python script for them to communicate.

Here is a small video of the execution-


https://github.com/SiddhPurohit/GoogleCollect/assets/84131034/26d2ba1b-bc96-40e8-804e-89e8b275e723


The project was made for GDSC-MPSTME.
