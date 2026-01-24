import cv2
import time
import os
from cvzone.HandTrackingModule import HandDetector
from cvzone.FaceDetectionModule import FaceDetector
import socket
import urllib.request as urlreq
import numpy as np
import random

def normalize(point):
    new_x = round(((point[0] - 0) / (1280)) * (7 - (-7)) + (-7), 5)
    new_y = round(((point[1] - 0) / (720)) * (4 - (-4)) + (-4), 5)
    return [new_x, new_y]

cap = cv2.VideoCapture(0)

cap.set(3, 1280)
cap.set(4, 720)

# hands
detector = HandDetector(detectionCon=.8, maxHands=1)
face_detector = FaceDetector()

# face (stolen from "https://github.com/Danotsonof/facial-landmark-detection/tree/master")
haarcascade_url = "https://raw.githubusercontent.com/opencv/opencv/master/data/haarcascades/haarcascade_frontalface_alt2.xml"
haarcascade = "haarcascade_frontalface_alt2.xml"
haarcascade_clf = "data/" + haarcascade
if (os.path.isdir('data')):
    if (haarcascade in os.listdir('data')):
        print("File exists")
    else:
        urlreq.urlretrieve(haarcascade_url, haarcascade_clf)
        print("File downloaded")
else:
    os.mkdir('data')
    urlreq.urlretrieve(haarcascade_url, haarcascade_clf)
    print("File downloaded")
detector2 = cv2.CascadeClassifier(haarcascade_clf)
LBFmodel_url = "https://github.com/kurnianggoro/GSOC2017/raw/master/data/lbfmodel.yaml"
LBFmodel = "LFBmodel.yaml"
LBFmodel_file = "data/" + LBFmodel
if (os.path.isdir('data')):
    if (LBFmodel in os.listdir('data')):
        print("File exists")
    else:
        urlreq.urlretrieve(LBFmodel_url, LBFmodel_file)
        print("File downloaded")
else:
    os.mkdir('data')
    urlreq.urlretrieve(LBFmodel_url, LBFmodel_file)

landmark_detector  = cv2.face.createFacemarkLBF()
landmark_detector.loadModel(LBFmodel_file)

socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr = ("127.0.0.1", 42069)

wait_time = random.randint(60, 120)
previous_time = time.time()

while True:
    _, img = cap.read()
    hands, img = detector.findHands(img)

    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

    faces = detector2.detectMultiScale(gray)
    face = None

    for (x, y, w, d) in faces:
        _, landmarks = landmark_detector.fit(gray, np.array(faces))

        for x,y in landmarks[0][0]:
            cv2.circle(img, (int(x), int(y)), 1, (255, 0, 0), 2)

        face = landmarks[0][0]

    current_time = time.time()
    if current_time - previous_time > wait_time:
        hands_or_face = random.randint(0, 1)
        found_value = False
        to_send = None

        print(f"Chosen: {hands_or_face == 0 and 'hands' or 'face'}")

        if hands_or_face == 0:
            if hands:
                to_send = hands[0]["lmList"]
                found_value = True 
            elif face is not None and face.any():
                print("No hands data, fallback to face")
                to_send = face
                found_value = True 
                hands_or_face = 1

        if hands_or_face == 1 and not found_value:
            if face is not None and face.any():
                to_send = face
                found_value = True 
            elif hands:
                print("No face data, fallback to hands")
                to_send = hands[0]["lmList"]
                found_value = True 
                hands_or_face = 0

        if found_value:
            print("There is some data")
            print(len(to_send))
            data = []

            for (_, lm) in enumerate(to_send):
                new_point = normalize(lm)
                if type(new_point[0]) is np.float32:
                    new_point[0] = new_point[0].item()
                    new_point[1] = new_point[1].item()
                data.append(new_point)

            print(f"Sending {hands_or_face == 0 and 'hands' or 'face'} data...")
            foo = str.encode(f"{hands_or_face}" + str(data))
            socket.sendto(str.encode(f"{hands_or_face}," + str(data)), addr)
        else:
            print("No data")

        wait_time = random.randint(60, 120)
        previous_time = current_time

    cv2.imshow("Hand", img)
    cv2.waitKey(1)
