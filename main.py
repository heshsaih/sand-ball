import cv2
import time
import os
from cvzone.HandTrackingModule import HandDetector
from cvzone.FaceDetectionModule import FaceDetector
import socket
import urllib.request as urlreq
import numpy as np


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
tolerance = 25
previous_hands = []

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

previous_time = time.time()
how_long_stable = 0
sent = False

socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr = ("127.0.0.1", 42069)

while True:
    current_time = time.time()
    dt = current_time - previous_time

    _, img = cap.read()
    hands, img = detector.findHands(img)

    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

    faces = detector2.detectMultiScale(gray)

    for (x, y, w, d) in faces:
        _, landmarks = landmark_detector.fit(gray, np.array(faces))

        for landmark in landmarks:
            for x, y in landmark[0]:
                cv2.circle(img, (int(x), int(y)), 1, (255, 0, 0), 2)

    # if hands and previous_hands:
    #     current_landmarks = hands[0]["lmList"]
    #     previous_landmarks = previous_hands[0]["lmList"]
    #
    #     if len(current_landmarks) == len(previous_landmarks):
    #         is_stable = True
    #
    #         for i in range(len(current_landmarks)):
    #             difference_x = abs(current_landmarks[i][0] - previous_landmarks[i][0])
    #             difference_y = abs(current_landmarks[i][1] - previous_landmarks[i][1])
    #
    #             if difference_x > tolerance or difference_y > tolerance:
    #                 is_stable = False
    #                 sent = False
    #                 break
    #
    #         if is_stable:
    #             how_long_stable += dt
    #         else:
    #             how_long_stable = 0
    #     else:
    #         how_long_stable = 0
    # else:
    #     how_long_stable = 0
    #
    #
    # if how_long_stable > 3 and not sent:
    #     data = []
    #     landmarks = previous_hands[0]["lmList"]
    #
    #     for (_, lm) in enumerate(landmarks):
    #         new_point = normalize(lm)
    #         data.append(new_point)
    #
    #     print("Sending data...")
    #     socket.sendto(str.encode(str(data)), addr)
    #     how_long_stable = 0
    #     sent = True

    previous_hands = hands

    previous_time = current_time
    cv2.imshow("Hand", img)
    cv2.waitKey(1)
