
import sklearn
import joblib
import zmq
import json

import time
import socket

filename = "random_forest_eugene_898_sec.joblib"

input_names = [
    'timeline',
    'inputDirMoveX',
    'inputDirMoveZ',
    'inputDirViewX',
    'inputDirViewZ',
    'curHP',
    'curEnergy',
    'curXiton',
    'curEnemiesCountClose',
    'curEnemiesCountDistant',
    'curBattleMusicParameterValue',
    'timeFromLastattack',
    'timeFromLastdamaged',
    'timeFromLastkill',
    'timeFromLastdeath',
    'timeFromLastenergySpending',
    'timeFromLastenergyCollecting',
    'timeFromLastxitonSpending',
    'timeFromLastxitonCharging',
    'timeFromLastshifted',
    'timeFromLastrewinded',
    'timeFromLastdepricatingWeapon',
]
emotions_list = [
    'angry',
    'disgust',
    'fear',
    'happy',
    'neutral',
    'sad',
    'surprise'
]


def construct_input_vector(data_):
    result = []
    for name in input_names:
        result.append(data_[name])
    return result


def construct_sending_dict(output_):
    result = {}
    for i in range(len(emotions_list)):
        result[emotions_list[i]] = output_[i]
    return result


class UnityConnector():
    def __init__(self, udpIP, portTX, portRX, enableRX=False, suppressWarnings=True):

        self.udpIP = udpIP
        self.udpSendPort = portTX
        self.udpRcvPort = portRX
        self.enableRX = enableRX
        self.suppressWarnings = suppressWarnings  # when true warnings are suppressed
        self.isDataReceived = False
        self.dataRX = None

        # Connect via UDP
        self.udpSock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)  # internet protocol, udp (DGRAM) socket
        self.udpSock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR,
                                1)  # allows the address/port to be reused immediately instead of it being stuck in the TIME_WAIT state waiting for late packets to arrive.
        self.udpSock.bind((udpIP, portRX))

        # Create Receiving thread if required
        if enableRX:
            import threading
            self.rxThread = threading.Thread(target=self.ReadUdpThreadFunc, daemon=True)
            self.rxThread.start()

    def __del__(self):
        self.CloseSocket()

    def CloseSocket(self):
        # Function to close socket
        self.udpSock.close()

    def SendData(self, strToSend):
        # Use this function to send string to C#
        self.udpSock.sendto(bytes(strToSend, 'utf-8'), (self.udpIP, self.udpSendPort))

    def ReceiveData(self):
        """
        Should not be called by user
        Function BLOCKS until data is returned from C#. It then attempts to convert it to string and returns on successful conversion.
        An warning/error is raised if:
            - Warning: Not connected to C# application yet. Warning can be suppressed by setting suppressWarning=True in constructor
            - Error: If data receiving procedure or conversion to string goes wrong
            - Error: If user attempts to use this without enabling RX
        :return: returns None on failure or the received string on success
        """
        if not self.enableRX:  # if RX is not enabled, raise error
            raise ValueError(
                "Attempting to receive data without enabling this setting. Ensure this is enabled from the constructor")

        data = None
        try:
            data, _ = self.udpSock.recvfrom(1024)
            data = data.decode('utf-8')
        except WindowsError as e:
            if e.winerror == 10054:  # An error occurs if you try to receive before connecting to other application
                if not self.suppressWarnings:
                    print("Are You connected to the other application? Connect to it!")
                else:
                    pass
            else:
                raise ValueError("Unexpected Error. Are you sure that the received data can be converted to a string")

        return data

    def ReadUdpThreadFunc(self):  # Should be called from thread
        """
        This function should be called from a thread [Done automatically via constructor]
                (import threading -> e.g. udpReceiveThread = threading.Thread(target=self.ReadUdpNonBlocking, daemon=True))
        This function keeps looping through the BLOCKING ReceiveData function and sets self.dataRX when data is received and sets received flag
        This function runs in the background and updates class variables to read data later

        """

        self.isDataReceived = False  # Initially nothing received

        while True:
            data = self.ReceiveData()  # Blocks (in thread) until data is returned (OR MAYBE UNTIL SOME TIMEOUT AS WELL)
            self.dataRX = data  # Populate AFTER new data is received
            self.isDataReceived = True
            # When it reaches here, data received is available

    def ReadReceivedData(self):
        """
        This is the function that should be used to read received data
        Checks if data has been received SINCE LAST CALL, if so it returns the received string and sets flag to False (to avoid re-reading received data)
        data is None if nothing has been received
        :return:
        """

        data = None

        if self.isDataReceived:  # if data has been received
            self.isDataReceived = False
            data = self.dataRX
            self.dataRX = None  # Empty receive buffer

        return data


if __name__ == '__main__':
    model = joblib.load(filename)
    # Create UDP socket to use for sending (and receiving)
    sock = UnityConnector(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)

    while True:
        data = sock.ReadReceivedData()

        if not data:
            continue

        print("Python receive: " + data)

        if not isinstance(data, str):
            print(type(data))
            continue

        if data == "stop":
            break

        data_parsed = json.loads(data)
        input_recv = construct_input_vector(data_parsed)
        answer = model.predict(input_recv)

        answer_dict = construct_sending_dict(answer)
        answer_str = json.dumps(answer_dict)

        sock.SendData(answer_str)  # Send this string to other application
        print('Sent from Python: ' + answer_str)

        time.sleep(1)



