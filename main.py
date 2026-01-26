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

landmark_detector = cv2.face.createFacemarkLBF()
landmark_detector.loadModel(LBFmodel_file)

socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr = ("127.0.0.1", 42069)

wait_time = random.randint(60, 120)
last_face_draw = time.time()

steady_time = 0
previous_hand_pos = None
threshold = 10
last_hand_draw = time.time() - 15
previous_time = time.time()

while True:
    _, img = cap.read()
    hands, img = detector.findHands(img)

    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

    faces = detector2.detectMultiScale(gray)
    face = None

    for (x, y, w, d) in faces:
        _, landmarks = landmark_detector.fit(gray, np.array(faces))

        for x, y in landmarks[0][0]:
            cv2.circle(img, (int(x), int(y)), 1, (255, 0, 0), 2)

        face = landmarks[0][0]

    cv2.imshow("Hand", img)
    cv2.waitKey(1)

    current_time = time.time()

    if hands:
        if current_time - last_hand_draw <= 20:
            previous_time = current_time
            last_face_draw = current_time
            continue

        dt = current_time - previous_time
        landmarks = hands[0]["lmList"]

        if previous_hand_pos is not None and abs(landmarks[0][0] - previous_hand_pos[0]) <= threshold and abs(landmarks[0][1] - previous_hand_pos[1]) <= threshold:
            steady_time += dt

        if steady_time > 1:
            data = []
            for (_, lm) in enumerate(landmarks):
                new_point = normalize(lm)
                data.append(new_point)

            print("Found hands, sending...")
            foo = str.encode("0," + str(data))
            socket.sendto(foo, addr)
            last_hand_draw = current_time
            steady_time = 0
            previous_time_hands = current_time
            last_face_draw = current_time

            continue

        previous_hand_pos = landmarks[0]
    else:
        last_hand_draw = current_time - 20

    if current_time - last_face_draw > wait_time:
        if face is not None and face.any():
            data = []

            for (_, lm) in enumerate(face):
                new_point = normalize(lm)
                new_point[0] = new_point[0].item()
                new_point[1] = new_point[1].item()
                data.append(new_point)

            print("No hands, sending face data...")
            foo = str.encode("1," + str(data))
            socket.sendto(foo, addr)

        wait_time = random.randint(60, 120)
        last_face_draw = current_time

    previous_time = current_time
