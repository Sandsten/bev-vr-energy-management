# -*- coding: utf-8 -*-
"""
Created on Mon Sep 21 10:48:17 2020

@author: HTC Vive
"""
import matplotlib.pyplot as plt
import matplotlib.patches as patches
import pandas as pd
import numpy as np
import matplotlib
import sys

LocationAllData = "../data-cleanup/merged-cleaned-csv/driving_data_merged_1_to_19.csv"
df = pd.read_csv(LocationAllData)

distanceWindowWidth = 20

def assignDistanceWindow(distance):
  # Get which distance pocket it would fit in.   
  distanceWindowIndex = round(distance/distanceWindowWidth)
  return distanceWindowWidth * distanceWindowIndex

# Add a new row which containts which distance group each row belongs to  
df["distanceWindow"] = df["distanceTraveled"].apply(assignDistanceWindow)

# Group the data based on attempt, which EVIS and userID
groups = df.groupby(["Attempt nr","evisID","userID", "distanceWindow"])

for name,group in groups:
  print(name)
  print(group[["distanceTraveled", "distanceWindow"]])
  break

sys.exit()

# Plot style and font sizes
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

def plotStateOfCharge():
  ax.set_xlabel("Distance [m]")
  ax.set_ylabel("State of charge [kWh]")
  ax.set_title("State of charge over distance traveled")
  
  # Plot the data
  for name,group in groups:
    # print(name[1])
    # print(group)  
    # Print the first attempt of the "guess-o-meter" users
    
    if name[0] == 2 and name[1] == "GuessOMeter":
      ax.plot(group["distanceTraveled"],group["currentStateOfCharge"], color="C0", linewidth=2)
      # ax.plot(group["timeStamp"],group["currentStateOfCharge"], color="C0", linewidth=2)
    if name[0] == 2 and name[1] == "DiffAndCOPE1":
      ax.plot(group["distanceTraveled"],group["currentStateOfCharge"], color="C1", linewidth=2)
      # ax.plot(group["timeStamp"],group["currentStateOfCharge"], color="C1", linewidth=2)
   
  # Custom legend  
  first_patch = patches.Patch(color="C0", label="guess-o-meter attempt #1")
  second_patch = patches.Patch(color="C1", label="diff+COPE1 attempt #1")
  plt.legend(handles=[first_patch, second_patch])


def plotSpeed():
  ax.set_xlabel("Distance [m]")
  ax.set_ylabel("Speed [km/h]")
  ax.set_title("Speed over distance traveled")
  
  # Plot the data
  for name,group in groups:
    if name[0] == 2 and name[1] == "GuessOMeter":
      ax.plot(group["distanceTraveled"],group["speed"], color="C0", linewidth=2)
    if name[0] == 2 and name[1] == "DiffAndCOPE1":
      ax.plot(group["distanceTraveled"],group["speed"], color="C1", linewidth=2)
   
  # Custom legend  
  first_patch = patches.Patch(color="C0", label="guess-o-meter attempt #1")
  second_patch = patches.Patch(color="C1", label="diff+COPE1 attempt #1")
  plt.legend(handles=[first_patch, second_patch])


plotSpeed()

# Enable grid. Color is grayscale when given in this format
ax.grid(b=True, color="0.75", linestyle="-")

plt.tight_layout()





