import math

import pandas as pd
from matplotlib import pyplot as plt

from src.constants import METRICS_LINE_COUNT, METRICS_SIZE_FIRST, METRICS_SIZE_LAST, THRESHOLD_NEGATIVE_VALUES
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

import xgboost as xgb

from sklearn.ensemble import RandomForestRegressor

from sklearn.linear_model import LinearRegression

import tensorflow as tf
from tensorflow.keras import layers, models, metrics
from transformers import pipeline


def GetMetricsComaprisonWithBase(yTest, base_test_pred, learned_test_pred, columns_def= ['metric', 'emotion', 'best', 'learned', 'base']):
    metrics_ = pd.DataFrame(columns=columns_def)

    def GetAppended(df, metric, emotion, target, train, mean, columns_=columns_def):
      return pd.concat([metrics_, pd.DataFrame([[metric, emotion, target, train, mean]], columns=columns_)], axis=0)

    for emotion in EMOTIONS_LIST:
        metrics_ = GetAppended(metrics_, 'mse', emotion, 0.0,
                              mean_squared_error(yTest[emotion], learned_test_pred[emotion]),
                              mean_squared_error(yTest[emotion], base_test_pred[emotion])
                              )
        metrics_ = GetAppended(metrics_, 'r2', emotion, 1.0,
                              r2_score(yTest[emotion], learned_test_pred[emotion]),
                              r2_score(yTest[emotion], base_test_pred[emotion])
                              )
        # metrics_ = GetAppended(metrics_, 'evs', emotion, 1.0,
        #                       explained_variance_score(yTest[emotion], learned_test_pred[emotion]),
        #                       explained_variance_score(yTest[emotion], base_test_pred[emotion])
        #                       )
        # if learned_test_pred[emotion][learned_test_pred[emotion] < 0].abs().max() < THRESHOLD_NEGATIVE_VALUES:
        #     learned_test_pred[emotion][learned_test_pred[emotion] < 0] = 0.0
        # else:
        #     continue
        # metrics_ = GetAppended(metrics_, 'mse_log', emotion, 0.0,
        #                       mean_squared_log_error(yTest[emotion], learned_test_pred[emotion]),
        #                       mean_squared_log_error(yTest[emotion], base_test_pred[emotion])
        #                       )

    metrics_ = metrics_.sort_values(by=['metric', 'emotion']).round(6)
    return metrics_


def VisualizeMetricsComparison(metrics_, learned_label, base_label, metric_str, metric_label):
    metrics_part = metrics_.where(metrics_['metric']==metric_str).dropna()
    plt.plot(metrics_part['emotion'], metrics_part['learned'], color='blue', label=learned_label)
    plt.plot(metrics_part['emotion'], metrics_part['base'], color='red', label=base_label)
    plt.plot(metrics_part['emotion'], metrics_part['best'], color='green', label='best')

    plt.title(metric_label)
    plt.legend()
    plt.show()


def DrawAllByMetric(metrics_, label_):
    metrics_list = metrics_['metric'].unique()
    lines = math.ceil(len(metrics_list) / METRICS_LINE_COUNT)
    if lines > 1 or len(metrics_list)==METRICS_LINE_COUNT:
        fig, axes = plt.subplots(nrows=lines, ncols=METRICS_LINE_COUNT, figsize=(METRICS_SIZE_FIRST * METRICS_LINE_COUNT, METRICS_SIZE_LAST * lines))
    else:
        temp = len(metrics_list) % METRICS_LINE_COUNT
        fig, axes = plt.subplots(nrows=lines, ncols=(temp), figsize=(METRICS_SIZE_FIRST * temp, METRICS_SIZE_LAST))

    for i in range(len(metrics_list)):
        metrics_val = metrics_.where(metrics_['metric']==metrics_list[i]).dropna()
        ax = None
        if lines > 1:
            ax = axes[i % METRICS_LINE_COUNT, i // METRICS_LINE_COUNT]
        else:
            ax = axes[i % METRICS_LINE_COUNT]
        ax.plot(metrics_val['emotion'], metrics_val['learned'], color='blue', label=label_)
        ax.plot(metrics_val['emotion'], metrics_val['base'], color='red', label='base mean')
        ax.plot(metrics_val['emotion'], metrics_val['best'], color='green', label='best')
        ax.set_title(metrics_list[i])
        ax.legend()

    if lines > 1:
        for i in range(len(metrics_list) % METRICS_LINE_COUNT):
            axes[lines - 1, METRICS_LINE_COUNT - 1 - i].axis('off')

    plt.tight_layout()
    plt.savefig(label_ + '.jpg', dpi=600)
    plt.show()


def DrawAllModelsInOne(metrics_, labels_, name_):
    metrics_list = metrics_[0]['metric'].unique()

    fig, axes = plt.subplots(nrows=1, ncols=(len(metrics_list)), figsize=(METRICS_SIZE_FIRST * len(metrics_list), METRICS_SIZE_LAST))

    for i in range(len(metrics_list)):
        metrics_val_temp = metrics_[0].where(metrics_[0]['metric'] == metrics_list[i]).dropna()
        ax = axes[i % METRICS_LINE_COUNT]

        ax.plot(metrics_val_temp['emotion'], metrics_val_temp['base'], color='red', label='base mean')
        ax.plot(metrics_val_temp['emotion'], metrics_val_temp['best'], color='green', label='best')
        for j in range(len(metrics_)):
            metrics_val = metrics_[j].where(metrics_[j]['metric'] == metrics_list[i]).dropna()
            ax.plot(metrics_val['emotion'], metrics_val['learned'], label=labels_[j])
        ax.set_title(metrics_list[i])
        ax.legend()

    plt.tight_layout()
    plt.savefig(name_ + '.jpg', dpi=600)
    plt.show()