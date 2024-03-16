

# from src.dataset_zip import GetMergedCleanData
# from src.emotion_extraction import analyze_emotions_from_folder
#
#
# from src.model_learning import CustomZeroShotPipeline

if __name__ == '__main__':
    print('fo')
    # custom_zero_shot_pipeline = CustomZeroShotPipeline(model_name='EleutherAI/gpt-neo-1.3B')
    #
    # # Добавляем размеченные контексты
    # custom_zero_shot_pipeline.add_context("1, 1", "2")
    # custom_zero_shot_pipeline.add_context("1, 2", "3")
    # custom_zero_shot_pipeline.add_context("1, 3", "4")
    # custom_zero_shot_pipeline.add_context("3, 3", "6")
    #
    # # Запрос с новым текстом
    # result = custom_zero_shot_pipeline.classify_with_context("3, 3", candidate_labels=["4", "3", "5", "6"])
    # print(result)
    #
    # analyze_emotions_from_folder('drive/MyDrive/datasets', 'data_analyzed')
    # df_merged = GetMergedCleanData("drive/MyDrive/results/eugene_19_37_correct_emotions_timeline.csv", "eugene_19_37.csv")
    # df_merged.to_csv("drive/MyDrive/results/eugene_merged_dataset.csv")
