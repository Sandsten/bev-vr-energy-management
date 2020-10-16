import pandas as pd


path_to_data = "../data-cleanup/merged-cleaned-csv/driving_data_merged_1_to_19.csv"

df = pd.read_csv(path_to_data)


# Separate into each driver first
