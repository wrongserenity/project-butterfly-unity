import UnityEngine as ue
import sklearn
import joblib
import zmq
import json

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


if __name__ == '__main__':
    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind('tcp://*:5555')

    model = joblib.load(filename)

    while True:
        json_str = socket.recv_json()
        data_recv = json.loads(json_str)
        input_recv = construct_input_vector(data_recv)
        answer = model.predict(input_recv)

        answer_dict = construct_sending_dict(answer)
        socket.send_json(answer_dict)
