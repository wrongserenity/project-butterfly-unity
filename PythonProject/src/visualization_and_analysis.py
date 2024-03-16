import numpy as np
import pandas as pd
from matplotlib import pyplot as plt
import seaborn as sns

from src.constants import EMOTIONS_LIST, FILTER_WINDOW
from src.emotion_extraction import get_filtered_data


def vis_boxplot(df_):
    fig, axes = plt.subplots(2, 4, figsize=(20, 10), sharey=True)
    for emotion_idx in range(len(EMOTIONS_LIST)):
        sns.boxplot(y=df_[EMOTIONS_LIST[emotion_idx]], ax=axes[emotion_idx // 4, emotion_idx % 4])


def vis_heatmap_default(df_, method_="Mean"):
    plt.figure(figsize=[25, 15])

    pear = df_.corr()
    spear = df_.corr(method='spearman')
    kendall = df_.corr(method='kendall')
    mean = (pear + spear + kendall) / 3

    heatmap = mean
    if method_ == "Spearman":
        heatmap = spear
    elif method_ == "Pearson":
        heatmap = pear
    elif method_ == "Kendall":
        heatmap = kendall

    sns.heatmap(heatmap, annot=True, mask=np.triu(kendall))
    plt.title(method_)


def vis_heatmap_filtered(df_, method_):
    df_filt = get_filtered_data(df_)
    vis_heatmap_default(df_filt, method_)
