import pandas as pd


def GetMergedCleanData(emotions_file_path, actions_file_path, eps=0.000001):
    df_emotions = pd.read_csv(emotions_file_path)
    df_emotions = df_emotions.rename(columns={'angry0': 'angry', 'disgust0': 'disgust', 'fear0': 'fear', 'happy0': 'happy', 'neutral0': 'neutral', 'sad0': 'sad','surprise0': 'surprise'})
    df_actions = pd.read_csv(actions_file_path)
    last_time = min(df_emotions["timeline"].max(), df_actions["timeline"].max()) + eps

    df_actions = df_actions[df_actions["timeline"] < last_time]
    df_emotions = df_emotions[df_emotions["timeline"] < last_time]

    df_result_merged = pd.merge_asof(df_emotions.sort_values("timeline"), df_actions.sort_values("timeline"), on="timeline", direction="nearest")
    df_result_merged = df_result_merged.drop(columns=["Unnamed: 0", "box"])
    return df_result_merged


if __name__ == "__main__":
    df = GetMergedCleanData("../data/raw/natalia_12_23/emotions_natalia_12_23_480.csv", "../data/raw/natalia_12_23/actions_natalia_12_23.csv")
    df.to_csv("../data/raw/natalia_12_23/natalia_full_12_23.csv")
