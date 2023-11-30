# MediaPipe Body
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

import cv2
import threading
import time
import global_vars 
import struct
import socket

# the capture thread captures images from the WebCam on a separate thread (for performance)
class CaptureThread(threading.Thread):
    cap = None
    ret = None
    frame = None
    isRunning = False
    counter = 0
    timer = 0.0
    def run(self):
        self.cap = cv2.VideoCapture(0) # sometimes it can take a while for certain video captures
        if global_vars.USE_CUSTOM_CAM_SETTINGS:
            self.cap.set(cv2.CAP_PROP_FPS, global_vars.FPS)
            self.cap.set(cv2.CAP_PROP_FRAME_WIDTH,global_vars.WIDTH)
            self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT,global_vars.HEIGHT)

        time.sleep(1)
        
        print("Opened Capture @ %s fps"%str(self.cap.get(cv2.CAP_PROP_FPS)))
        while not global_vars.KILL_THREADS:
            self.ret, self.frame = self.cap.read()
            self.isRunning = True
            if global_vars.DEBUG:
                self.counter = self.counter+1
                if time.time()-self.timer>=3:
                    print("Capture FPS: ",self.counter/(time.time()-self.timer))
                    self.counter = 0
                    self.timer = time.time()

# the body thread actually does the 
# processing of the captured images, and communication with unity
class BodyThread(threading.Thread):

    data = ""
    dirty = True
    pipe = None
    timeSinceCheckedConnection = 0
    timeSincePostStatistics = 0
    left_threshold = 0.001
    right_threshold = 0.0

    def run(self):
        mp_drawing = mp.solutions.drawing_utils
        mp_pose = mp.solutions.pose
        udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        udp_destination_ip = "10.125.19.72"  # Change to the IP address of the destination device
        udp_destination_port = 5095  # Change to the desired UDP port on the destination device

        capture = CaptureThread()
        capture.start()

        with mp_pose.Pose(min_detection_confidence=0.80, min_tracking_confidence=0.5, model_complexity = global_vars.MODEL_COMPLEXITY,static_image_mode = False,enable_segmentation = True) as pose: 
            
            while not global_vars.KILL_THREADS and capture.isRunning==False:
                print("Waiting for camera and capture thread.")
                time.sleep(0.5)
            print("Beginning capture")
                
            while not global_vars.KILL_THREADS and capture.cap.isOpened():
                ti = time.time()

                # Fetch stuff from the capture thread
                ret = capture.ret
                image = capture.frame
                                
                # Image transformations and stuff
                image = cv2.flip(image, 1)
                image.flags.writeable = global_vars.DEBUG
                
                # Detections
                results = pose.process(image)
                tf = time.time()
                
                # Rendering results
                if global_vars.DEBUG:
                    if time.time()-self.timeSincePostStatistics>=1:
                        print("Theoretical Maximum FPS: %f"%(1/(tf-ti)))
                        self.timeSincePostStatistics = time.time()
                        
                    if results.pose_landmarks:
                        
                        #---------------------------
                        landmarks = results.pose_world_landmarks.landmark

                        # Define the indices of the hip and head landmarks
                        hip_index = 23  # Adjust this index based on your provided landmark indices
                        head_index = 0  # The head landmark is typically at index 0

                        # Get the positions of the hip and head landmarks
                        hip_position = [landmarks[hip_index].x, landmarks[hip_index].y, landmarks[hip_index].z]
                        head_position = [landmarks[head_index].x, landmarks[head_index].y, landmarks[head_index].z]

                        # Define a threshold for considering the user as "Centered"
                        center_threshold = 0.1  # Adjust the threshold as needed
                        right_sensitivity = -0.1  # Adjust this value to make "Right" more sensitive
                        left_sensitivity = 0.23   # Adjust this value to make "Left" more sensitive
                        # Calculate the difference in x-coordinates between hip and head landmarks
                        x_difference = abs(hip_position[0] - head_position[0])
                        print(hip_position[0]) #0.14b
                        print(head_position[0]) # 0.22

                        # Determine the leaning direction based on hip and head positions
                        # if x_difference == 0:
                        #     leaning_direction = "Center"
                        # elif hip_position[0] < head_position[0]:
                        #     eaning_direction = "Right"
                        # else:
                        #     leaning_direction = "Left"

                        if x_difference < center_threshold:
                            leaning_direction = "Center"
                        elif hip_position[0] < head_position[0] - right_sensitivity:
                            leaning_direction = "Right"
                        elif hip_position[0] > head_position[0] + left_sensitivity:
                            leaning_direction = "Left"
                        else:
                            leaning_direction = "Center"
                        # if x_difference < self.left_threshold:
                        #     leaning_direction = "Left"
                        # elif x_difference > self.right_threshold:
                        #     leaning_direction = "Right"
                        # else:
                        #     leaning_direction = "Center"

                        print("Leaning Direction:", leaning_direction)
                        udp_message = leaning_direction.encode('utf-8')
                        udp_socket.sendto(udp_message, (udp_destination_ip, udp_destination_port))
                        #---------------------------
                        mp_drawing.draw_landmarks(image, results.pose_landmarks, mp_pose.POSE_CONNECTIONS, 
                                                mp_drawing.DrawingSpec(color=(255, 100, 0), thickness=2, circle_radius=4),
                                                mp_drawing.DrawingSpec(color=(255, 255, 255), thickness=2, circle_radius=2),
                                                )
                    cv2.imshow('Body Tracking', image)
                    cv2.waitKey(3)

                if self.pipe==None and time.time()-self.timeSinceCheckedConnection>=1:
                    try:
                        self.pipe = open(r'\\.\pipe\UnityMediaPipeBody', 'r+b', 0)
                    except FileNotFoundError:
                        print("Waiting for Unity project to run...")
                        self.pipe = None
                    self.timeSinceCheckedConnection = time.time()
                    
                if self.pipe != None:
                    # Set up data for piping
                    self.data = ""
                    i = 0
                    if results.pose_world_landmarks:
                        hand_world_landmarks = results.pose_world_landmarks
                        for i in range(0,33):
                            self.data += "{}|{}|{}|{}\n".format(i,hand_world_landmarks.landmark[i].x,hand_world_landmarks.landmark[i].y,hand_world_landmarks.landmark[i].z)
                    
                    s = self.data.encode('utf-8') 
                    try:     
                        self.pipe.write(struct.pack('I', len(s)) + s)   
                        self.pipe.seek(0)    
                    except Exception as ex:  
                        print("Failed to write to pipe. Is the unity project open?")
                        self.pipe= None
                        
                #time.sleep(1/20)
                        
        # self.pipe.close()
        capture.cap.release()
        cv2.destroyAllWindows()