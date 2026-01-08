import cv2
import time
from cvzone.HandTrackingModule import HandDetector
import socket


def normalize(point):
    new_x = round(((point[0] - 0) / (1280)) * (7 - (-7)) + (-7), 5)
    new_y = round(((point[1] - 0) / (720)) * (4 - (-4)) + (-4), 5)
    return [new_x, new_y]

cap = cv2.VideoCapture(0)

cap.set(3, 1280)
cap.set(4, 720)

detector = HandDetector(detectionCon=.8, maxHands=1)
tolerance = 25
previous_hands = []

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

    if hands and previous_hands:
        current_landmarks = hands[0]["lmList"]
        previous_landmarks = previous_hands[0]["lmList"]

        if len(current_landmarks) == len(previous_landmarks):
            is_stable = True

            for i in range(len(current_landmarks)):
                difference_x = abs(current_landmarks[i][0] - previous_landmarks[i][0])
                difference_y = abs(current_landmarks[i][1] - previous_landmarks[i][1])

                if difference_x > tolerance or difference_y > tolerance:
                    is_stable = False
                    sent = False
                    break
            
            if is_stable:
                how_long_stable += dt
            else:
                how_long_stable = 0
        else:
            how_long_stable = 0
    else:
        how_long_stable = 0


    if how_long_stable > 3 and not sent:
        data = []
        landmarks = previous_hands[0]["lmList"]

        for (_, lm) in enumerate(landmarks):
            new_point = normalize(lm)
            data.append(new_point)
        
        print("Sending data...")
        socket.sendto(str.encode(str(data)), addr)
        how_long_stable = 0
        sent = True

    previous_hands = hands
    previous_time = current_time
    cv2.imshow("Hand", img)
    cv2.waitKey(1)
