import os
import time

import cv2
import numpy as np
import pandas as pd
from fer import FER
from fer import Video
from matplotlib import pyplot as plt

from src.constants import EMOTIONS_LIST, FILTER_WINDOW, SUPPORTED_VIDEO_FORMATS

from moviepy.editor import VideoFileClip, vfx


def work_with_saved_file():
    df = pd.read_csv('file.csv')
    for column in df.columns[1:]:
        print(column)
        if column == 'timeline' or column == "" or column == 'box':
            continue

        plt.plot(df['timeline'], df[column], label=column)
    plt.title("all")
    plt.legend()
    plt.show()


def analyze_emotions_from_folder(source_path: str, target_path: str):
    start_time = time.time()
    files = os.scandir(source_path + '/')
    detector = FER(mtcnn=True)

    for file in files:
        if not np.any([file.name.endswith(video_format) for video_format in SUPPORTED_VIDEO_FORMATS]):
            continue

        analyzed_df = get_emotions_df_by_file_name(source_path + '/' + file.name, detector)
        file_name = str(file.name).split('.')[0]
        save_data(analyzed_df, file_name, target_path)
        save_emotion_plots(analyzed_df, target_path + '/' + file_name + "_raw")

        filtered = get_filtered_data(analyzed_df)
        save_data(filtered, file_name + "_filtered", target_path)
        save_emotion_plots(filtered, target_path + '/' + file_name + "_filtered", column_postfix='_filtered')

    elapsed_time = time.time() - start_time
    print("path ", source_path, " spent ", elapsed_time)


def get_emotions_df_by_file_name(video_filename: str, detector: FER = None):
    data_cv2 = cv2.VideoCapture(video_filename)

    return get_emotions_df_by_video(Video(video_filename), data_cv2, detector)


def save_data(df, file_name, folder):
    try:
        os.mkdir(folder)
    except FileExistsError as exc:
        print(exc)

    name_ = folder + '/' + file_name + '.csv'

    df.to_csv(name_)


def get_filtered_data(df: pd.DataFrame):
    df_copy = df.copy()
    for column in df.columns:
        if column not in EMOTIONS_LIST:
            continue

        window_wings = int((FILTER_WINDOW - 1) / 2)
        temp = []

        df_column = list(df[column].values)
        df_column_large = [df_column[0]] * window_wings + df_column + [df_column[-1]] * window_wings

        for i in range(window_wings, len(df_column_large) - window_wings):
            temp.append(get_median(df_column_large[(i - window_wings): (i + 1 + window_wings)]))

        df_copy[column] = temp
    return df_copy


def get_median(values: list):
    sorted_values = values.copy()
    sorted_values.sort()
    return sorted_values[round(len(values) / 2)]


def save_emotion_plots(df: pd.DataFrame, video_name: str, column_postfix: str = "_raw"):
    for column in df.columns:
        if column not in EMOTIONS_LIST:
            continue

        plt.plot(df['timeline'], df[column], label=column)
        plt.title(column + column_postfix)
        plt.savefig(video_name + '_' + column + '.png')
        plt.clf()


def get_emotions_df_by_video(video: Video, data_cv2, detector: FER = None):
    if not detector:
        detector = FER(mtcnn=True)

    raw_data = video.analyze(detector, display=False)

    df = video.to_pandas(raw_data)

    frames = df.shape[0]
    fps = data_cv2.get(cv2.CAP_PROP_FPS)
    timeline = np.linspace(0, (frames / fps), int(frames)).tolist()

    df['timeline'] = timeline

    return df


def ResizeVideos(source_path, min_target_size=120):
    files = os.scandir(source_path + '/')
    for file in files:
        try:
            file_path = source_path + '/' + file.name

            intermediate_clip = VideoFileClip(file_path)
            size_ = intermediate_clip.size
            ratio = min_target_size / min(size_)
            final_clip_resized = intermediate_clip.resize((int(size_[0]*ratio),
                                                           int(size_[1]*ratio)))
            print(final_clip_resized.size)
            final_clip_resized.write_videofile(source_path + "/resized/" + str(min_target_size) + file.name)
        except Exception as e:
            print("ResizeVideos(): ", e)




if __name__ == '__main__':
    # ResizeVideos('../data/source/lyubov_12_23')
    # ResizeVideos('../data/source/natalia_12_23')

    analyze_emotions_from_folder('../data/source/lyubov_12_23/resized', '../data/raw/lyubov_12_23')
    analyze_emotions_from_folder('../data/source/natalia_12_23/resized', '../data/raw/natalia_12_23')
