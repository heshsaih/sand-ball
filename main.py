import cv2
import time
from cvzone.HandTrackingModule import HandDetector

cap = cv2.VideoCapture(0)

cap.set(3, 1280)
cap.set(4, 720)

detector = HandDetector(detectionCon=.8, maxHands=1)
tolerance = 25
previous_hands = []

previous_time = time.time()
how_long_stable = 0

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
                    break
            
            if is_stable:
                how_long_stable += dt
            else:
                how_long_stable = 0
        else:
            how_long_stable = 0
    else:
        how_long_stable = 0


    if how_long_stable > 3:
        print("detected!!!")

    previous_hands = hands
    previous_time = current_time
    cv2.imshow("Hand", img)
    cv2.waitKey(1)
