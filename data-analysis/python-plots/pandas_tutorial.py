# -*- coding: utf-8 -*-
"""
Created on Mon Sep 21 09:59:55 2020

@author: HTC Vive
"""

from pandas import DataFrame, read_csv

import matplotlib.pyplot as plt
import pandas as pd
import sys
import matplotlib
import numpy as np

# Pandas tutorial

# names = ["ADAM", "STEVE", "POTATO"]    
# births = [11, 125, 56]

# # zip will merge these two datasets
# DataSet = list(zip(names, births)) 

# df = pd.DataFrame(data = DataSet, columns=["Names", "Births"]) 

Location = "../data-cleanup/raw-data-cleaned/data_002_DiffAndCOPE1_20200318T173240.csv"

df = pd.read_csv(Location)
print(df)
print(df.timeStamp)
print(type(df.timeStamp))

distanceWindowWidth = 20

def assignDistanceWindow(distance):
  # Get which distance pocket it would fit in.   
  distanceWindowIndex = round(distance/distanceWindowWidth)
  return distanceWindowWidth * distanceWindowIndex

# Add a new row which containts which distance group each row belongs to  
df["distance_window"] = df["distanceTraveled"].apply(assignDistanceWindow)
# Group the data by each distance group. Create a copy containing only the needed values
df_average = df[["speed", "distance_window"]].copy().groupby(df["distance_window"])
# Average the values in each distance group
df_average = df_average.mean()


# Plot
plt.style.use("seaborn")
params = {
  'font.size': 20, 
  'axes.labelsize': 18,
  'axes.titlesize':20, 
  'legend.fontsize': 18, 
  'xtick.labelsize': 15, 
  'ytick.labelsize': 15
 }
matplotlib.rcParams.update(params)

# Create the figure
fig, ax =  plt.subplots(figsize=(10,5))

ax.plot(df_average["distance_window"],df_average["speed"], color="C0", linewidth=2)


# Convert string to datetime format!
# df.timeStamp = pd.to_datetime(df.timeStamp, unit="s")
# df.set_index("timeStamp", inplace=True, drop=False)
# print(df.timeStamp)

# Resample to even time steps of 500ms
# df_new = df.resample("500ms").mean()
# print(df_new.currentStateOfCharge)

# df_soc_dist = df.columns[["currentStateOfCharge","distanceTraveled"]]
# df_soc_dist = df.loc[:,"currentStateOfCharge"]

# Extracting two specific columns
# df_soc_dist = df[["distanceTraveled","currentStateOfCharge"]]
  
# rs = pd.DataFrame(index=df.resample())

# print(df["distanceTraveled"])
# print(df["timeStamp"])


# new_range = pd.interval_range(0,8000, freq=10)
# print(new_range)
# 
# df2 = pd.DataFrame(index = new_range)
# print(df2)


  
    
    
