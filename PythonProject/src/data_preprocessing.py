import os
from functools import reduce

import pandas as pd

from src.constants import POOL_Q, EMOTIONS_LIST


def get_remove_dublicates_by_normalizing(df_, method_, round_digits_):
    df_normalized = df_.copy()
    if method_ == "min_max":
        df_normalized = df_.iloc[:, 0:-1].apply(lambda x: x + x.min() / (abs(x.min()) + x.max()), axis=0)
    elif method_ == "mean_std":
        df_normalized = df_.iloc[:, 0:-1].apply(lambda x: (x - x.mean()) / x.std(), axis=0)
    result = df_.iloc[df_normalized.round(round_digits_).drop_duplicates().index]
    return result


def GetWithAddMeta(source_df, meta_dict):
    keys = list(meta_dict.keys())
    values = []
    for key in keys:
        values.append(meta_dict[key])
    print(keys)
    print(values)
    print(source_df.shape[0])
    print([values]*source_df.shape[0])
    df_meta = pd.DataFrame([values]*source_df.shape[0], columns=keys)
    result = pd.concat([source_df, df_meta], axis=1)
    return result


def CreateMetaDict(answers):
    if len(answers) != len(POOL_Q):
        print("Not equal answers and questions count")
        return None

    result = {}
    for i in range(len(POOL_Q)):
        result[POOL_Q[i]] = answers[i]
    return result


def GetCombinedByPaths(first_file, second_file):
    df_1 = pd.read_csv(first_file)
    df_2 = pd.read_csv(second_file)
    return GetCombined(df_1, df_2)

NOT_NEEDED_COMBINED_COLUMNS = ["Unnamed: 0", "timeline"]

def TryRemoveSomeColumns(df_):
    for col in NOT_NEEDED_COMBINED_COLUMNS:
        if col in df_.columns:
            df_ = df_.drop(columns=[col])
    return df_

def GetCombined(df_1, df_2):
    df_1 = TryRemoveSomeColumns(df_1)
    df_2 = TryRemoveSomeColumns(df_2)

    df_merged = pd.concat([df_1, df_2], axis=0)
    print("merged: ", df_1.shape, df_2.shape, " = ", df_merged.shape)
    df_merged = df_merged.reset_index()
    df_merged = df_merged.drop(columns=["index"])
    return df_merged


def GetMeanAndStd(df_):
    df_temp = pd.concat([df_.mean(), df_.std()], axis=1)
    df_temp.columns = ['mean', 'std']
    return df_temp


files_with_q = {
    "arkady_merged_dataset.csv": [0, 1, 2, 1, 1, 1, 2, 1, 1],
    "eugene_merged_dataset.csv": [0, 2, 1, 0, 2, 0, 0, 2, 1],
    "lyubov_merged_dataset.csv": [1, 1, 0, 2, 0, 1, 2, 0, 2],
    "natalia_merged_dataset.csv": [1, 2, 1, 1, 1, 0, 1, 1, 1],
}

WITH_META = True
REMOVE_DUBLICATES = True

if __name__ == "__main__":
    # dfs = []
    # source_path = "../data/raw/ready_to_combine"
    # if WITH_META:
    #     for file, q in files_with_q.items():
    #         df_temp = pd.read_csv(source_path + '/' + file)
    #         meta_q = CreateMetaDict(q)
    #         dfs.append(GetWithAddMeta(df_temp, meta_q))
    # else:
    #     files = os.scandir(source_path + '/')
    #     for file in files:
    #         dfs.append(pd.read_csv(source_path + '/' + file.name))
    #
    # df_sum = reduce(lambda a, b: GetCombined(a, b), dfs)
    #
    # if REMOVE_DUBLICATES:
    #     df_sum = get_remove_dublicates_by_normalizing(df_sum, "mean_std", 1)
    #
    # print(GetMeanAndStd(df_sum))
    # df_sum.to_csv("../data/processed/all_four_w_meta_wo_dubl.csv")

    df = pd.read_csv("../data/processed/all_four_w_meta_wo_dubl.csv")
    print(GetMeanAndStd(df))
    df_normalized = df.iloc[:, 0:-1].apply(lambda x: (x - x.mean()) / x.std(), axis=0)
    for emotion in EMOTIONS_LIST:
        df_normalized[emotion] = df[emotion]
    print(GetMeanAndStd(df_normalized))
    df_normalized.to_csv("../data/processed/all_four_w_meta_wo_dubl_std_norm.csv")


