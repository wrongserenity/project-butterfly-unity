import time

import numpy as np
import pandas as pd

from src.constants import EMOTIONS_LIST
from numpy import absolute
from numpy import mean
from numpy import std
from sklearn.datasets import make_regression
from sklearn.tree import DecisionTreeRegressor
from sklearn.model_selection import cross_val_score
from sklearn.model_selection import RepeatedKFold
from sklearn.model_selection import train_test_split
from sklearn.metrics import mean_squared_error, explained_variance_score, r2_score, mean_squared_log_error
import matplotlib.pyplot as plt
from tensorflow.keras.layers import Conv1D, MaxPooling1D, Flatten, Dense
from tensorflow.keras.layers import SimpleRNN, Dense

import xgboost as xgb

from sklearn.ensemble import RandomForestRegressor

from sklearn.linear_model import LinearRegression, LogisticRegression

import tensorflow as tf
from tensorflow.keras import layers, models
from tensorflow.keras.models import Sequential
from tensorflow.python.keras.metrics import Precision, Recall
from keras.losses import categorical_crossentropy
import tensorflow_addons as tfa
from transformers import pipeline

from src.mertics_calc_and_vis import GetMetricsComaprisonWithBase, DrawAllByMetric, DrawAllModelsInOne


def get_train_test(df_):
    X = df_.copy().drop(columns=EMOTIONS_LIST)
    y = df_.copy()[EMOTIONS_LIST]

    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
    return X_train, X_test, y_train, y_test


def PrintFeatureImportance(xTrain, md):
    table = np.array([xTrain.columns, md.feature_importances_])
    features = pd.DataFrame(np.transpose(table), columns=['feature', 'importance'])
    features['importance'] = features['importance'].apply(lambda x: round(x, 3))
    features = features.sort_values(by=['importance'], ascending=False)
    print(features)


def PrintMetricsByEmotionsList(yTest, xTest, md):
    for emotion in EMOTIONS_LIST:
      print(f"mse {emotion}: {mean_squared_error(yTest[emotion], pd.DataFrame(md.predict(xTest), columns=yTest.columns)[emotion])}")
    print("")

    for emotion in EMOTIONS_LIST:
      print(f"r2 {emotion}: {r2_score(yTest[emotion], pd.DataFrame(md.predict(xTest), columns=yTest.columns)[emotion])}")
    print("")

    for emotion in EMOTIONS_LIST:
      print(f"evs {emotion}: {explained_variance_score(yTest[emotion], pd.DataFrame(md.predict(xTest), columns=yTest.columns)[emotion])}")
    print("")

    for emotion in EMOTIONS_LIST:
      print(f"log {emotion}: {mean_squared_log_error(yTest[emotion], pd.DataFrame(md.predict(xTest), columns=yTest.columns)[emotion])}")


def GetBaseLineMeanAllTrain(y_train_, y_test_):
    mean_all_train = np.stack([np.mean(y_train_, axis=0)] * y_test_.shape[0])
    mean_all_train = pd.DataFrame(mean_all_train, columns=y_test_.columns)
    return mean_all_train


def GetTrainedDecisionTree(X_train_, y_train_):
    model_dec_tree = DecisionTreeRegressor()
    result_dec_tree = model_dec_tree.fit(X_train_, y_train_)
    return result_dec_tree


def GetTrainedXgb(X_train_, y_train_):
    modelBoost = xgb.XGBRegressor(objective="reg:linear", random_state=42)
    resultBoost = modelBoost.fit(X_train_, y_train_)
    return resultBoost


def GetTrainedRandomForest(X_train_, y_train_):
    modelRandomForest = RandomForestRegressor(random_state=42)
    resultRandomForest = modelRandomForest.fit(X_train_, y_train_)
    return resultRandomForest


def GetTrainedLinearReg(X_train_, y_train_):
    modelLinearReg = LinearRegression()
    resultLinearReg = modelLinearReg.fit(X_train_, y_train_)
    return resultLinearReg


def GetTrainedCnn(X_train_, y_train_):
    model = Sequential()
    model.add(Conv1D(64, kernel_size=3, activation='relu', input_shape=(X_train.shape[1], 1)))
    model.add(MaxPooling1D(pool_size=2))
    model.add(Flatten())
    model.add(Dense(128, activation='relu'))
    model.add(Dense(7, activation='linear'))

    model.compile(optimizer='adam', loss='mean_squared_error')
    model.fit(X_train_, y_train_, epochs=100, batch_size=512, validation_split=0.2)
    return model


def GetTrainedRnn(X_train_, y_train_):
    model = Sequential()
    model.add(SimpleRNN(50, activation='relu', input_shape=(X_train.shape[1], 1)))
    model.add(Dense(7, activation='linear'))
    optimizer = tf.keras.optimizers.Adam(learning_rate=0.01)
    model.compile(optimizer=optimizer, loss='binary_crossentropy',
                     metrics=[tfa.metrics.F1Score(num_classes=7, average="macro")])
    model.fit(X_train_, y_train_, epochs=20, batch_size=512, validation_split=0.2)
    return model


def GetTrainedNn(X_train_, y_train_):
    model_nn = models.Sequential()
    model_nn.add(layers.Dense(128, activation='relu', input_shape=(X_train.shape[1],)))
    model_nn.add(layers.Dense(64, activation='relu'))
    model_nn.add(layers.Dense(32, activation='relu'))
    model_nn.add(layers.Dense(7, activation='sigmoid'))

    optimizer = tf.keras.optimizers.Adam(learning_rate=0.01)
    model_nn.compile(optimizer=optimizer, loss='binary_crossentropy', metrics=[tfa.metrics.F1Score(num_classes=7, average="macro")])
    model_nn.fit(X_train_, y_train_, epochs=300, batch_size=512, validation_split=0.2)
    return model_nn


def GetTrainedZeroShotGptNeo(X_train_, y_train_):
    custom_zero_shot_pipeline = CustomZeroShotPipeline(model_name='EleutherAI/gpt-neo-1.3B')

    # Добавляем размеченные контексты
    custom_zero_shot_pipeline.add_context("1, 1", "2")
    custom_zero_shot_pipeline.add_context("1, 2", "3")
    custom_zero_shot_pipeline.add_context("1, 3", "4")
    custom_zero_shot_pipeline.add_context("3, 3", "6")

    # Запрос с новым текстом
    result = custom_zero_shot_pipeline.classify_with_context("3, 3", candidate_labels=["4", "3", "5", "6"])


class CustomZeroShotPipeline:
    def __init__(self, model_name):
        self.classifier = pipeline("zero-shot-classification", model=model_name)
        self.contexts = []

    def add_context(self, sequence, result):
        self.contexts.append(f"({sequence} = {result})")

    def classify_with_context(self, sequence, candidate_labels):
        context = " ".join(self.contexts)
        result_sequence = f"if {context} so what will be ({sequence} = ?)"

        result = self.classifier(result_sequence, candidate_labels)
        return result


class TimerProject:
    def __init__(self):
        self.init_time = time.time()
        self.prev_time = time.time()
        self.recorded = {}

    def lap(self, name):
        temp_time = time.time()
        elapsed = temp_time - self.prev_time
        self.prev_time = temp_time
        self.recorded[name] = elapsed
        print(name)

    def get_summary(self):
        return self.recorded


if __name__ == "__main__":
    timer = TimerProject()

    df = pd.read_csv("../data/processed/all_four_wo_meta_wo_dubl.csv")
    X_train, X_test, y_train, y_test = get_train_test(df)
    timer.lap("df inited")

    mean_all_train = GetBaseLineMeanAllTrain(y_train, y_test)
    timer.lap("mean all train")

    res_dec_tree = GetTrainedDecisionTree(X_train, y_train)
    timer.lap("decision tree")

    res_xgb = GetTrainedXgb(X_train, y_train)
    timer.lap("xgboost")

    res_r_forest = GetTrainedRandomForest(X_train, y_train)
    timer.lap("random forest")

    res_l_reg = GetTrainedLinearReg(X_train, y_train)
    timer.lap("linear regression")

    res_cnn = GetTrainedCnn(X_train, y_train)
    timer.lap("cnn")

    res_rnn = GetTrainedRnn(X_train, y_train)
    timer.lap("rnn")

    # res_nn = GetTrainedNn(X_train, y_train)
    # timer.lap("nn 256:64:32")

    met_dec_tree = GetMetricsComaprisonWithBase(y_test, mean_all_train,
                                           pd.DataFrame(res_dec_tree.predict(X_test), columns=y_test.columns))
    timer.lap("metrics dec tree")

    met_xgb = GetMetricsComaprisonWithBase(y_test, mean_all_train,
                                                pd.DataFrame(res_xgb.predict(X_test), columns=y_test.columns))
    timer.lap("metrics xgb")

    met_r_forest = GetMetricsComaprisonWithBase(y_test, mean_all_train,
                                                pd.DataFrame(res_r_forest.predict(X_test), columns=y_test.columns))
    timer.lap("metrics r forest")

    met_l_reg = GetMetricsComaprisonWithBase(y_test, mean_all_train,
                                                pd.DataFrame(res_l_reg.predict(X_test), columns=y_test.columns))
    timer.lap("metrics lin reg")

    # met_cnn = GetMetricsComaprisonWithBase(y_test, mean_all_train,
    #                                          pd.DataFrame(res_cnn.predict(X_test), columns=y_test.columns))
    # timer.lap("metrics cnn")
    #
    # met_rnn = GetMetricsComaprisonWithBase(y_test, mean_all_train,
    #                                            pd.DataFrame(res_rnn.predict(X_test), columns=y_test.columns))
    # timer.lap("metrics rnn")

    # met_nn = GetMetricsComaprisonWithBase(y_test, mean_all_train,
    #                                             pd.DataFrame(res_nn.predict(X_test), columns=y_test.columns))
    # timer.lap("metrics nn")

    DrawAllByMetric(met_dec_tree, "Decision Tree")
    DrawAllByMetric(met_xgb, "XG Boost")
    DrawAllByMetric(met_r_forest, "Random Forest")
    DrawAllByMetric(met_l_reg, "Linear Regression")
    # DrawAllByMetric(met_nn, "Neural Network (256, 64, 32)")

    DrawAllModelsInOne([met_dec_tree, met_xgb, met_r_forest,met_l_reg],
                       ["Decision Tree", "XG Boost", "Rand Forest", "Lin Reg"], "comparison_wo_meta_wo_dubl_wo_norm")
    timer.lap("visualized")

    print(timer.get_summary())
