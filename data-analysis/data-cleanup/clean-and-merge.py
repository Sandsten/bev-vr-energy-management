import pandas as pd
import glob
from colorama import Fore, Back, Style
import os
import sys

platform = sys.platform

# Get the paths to all uncleaned csv files
if platform == "linux":
    all_files = sorted(glob.glob('../raw-data/*.csv'))
else:
    all_files = sotred(glob.glob('..\\raw-data\\*.csv'))

cleanedDataFrames = []

def cleanTheData():

    # Start with 0 as attempt number and just add 1 when saving to file to go from [0,1] -> [1,2]
    attempt_nr = 0

    print("Cleaning your data...", end=" ")

    # Add column with attempt nr
    for csv_file in all_files:
        df = pd.read_csv(csv_file)
        # Insert a new column at index 0 with the attempt number
        df.insert(0, "Attempt nr", attempt_nr + 1)

        # Remove rows where the SoC is less than zero.
        # There's a bug in the simulation when SoC goes below zero, I assume it's done and some values aren's stored anymore, such as speed.
        # Hence when the driver runs out of battery it's over, but in reality they might still be going down hill a bit etc.
        df.drop(df[df.currentStateOfCharge <= 0].index, inplace=True)
        
        # Store our cleaned data frames in an array
        cleanedDataFrames.append(df)

        # Circle around 0 -> 1 -> 0 -> 1
        # All the attempts have to be ordered and always in pairs, otherwise this won't work.
        # This is the default order if left alone
        attempt_nr = (attempt_nr + 1) % 2

    print(" OK")
    
def mergeCSVFilesToOne():
    print("Merging files...", end=" ")
    # Merge all the data into one single data frame
    df = pd.concat(cleanedDataFrames)
    print(" OK")

    print("===Cleaned and merged files===")
    [print(Fore.GREEN +  Style.BRIGHT + f + Style.RESET_ALL) for f in all_files]
    print("==================")
    print("Saving to file...", end="", flush=True)

    first_id = df.iloc[0].userID
    last_id = df.iloc[-1].userID

    merged_file_name = "driving_data_merged_" + \
        str(first_id) + "_to_" + str(last_id) + ".csv"

    # Save the merged data into a new .csv file
    if platform == "linux":
        df.to_csv('./merged-cleaned-csv/' + merged_file_name, index=False, encoding='utf-8-sig')
    else:
        df.to_csv('./merged-cleaned-csv/' + merged_file_name, index=False, encoding='utf-8-sig')
    print(" OK")
    
    if platform == "linux":
        print("Path to file: " + Fore.YELLOW + Style.BRIGHT + os.path.abspath('./merged-cleaned-csv') + Style.RESET_ALL)
    else:
        print("Path to file: " + Fore.YELLOW + Style.BRIGHT + os.path.abspath('.\\merged-cleaned-csv') + Style.RESET_ALL)
    
    print("New file: " + Fore.GREEN + Style.BRIGHT + merged_file_name + Style.RESET_ALL)
    print(Fore.CYAN + Style.BRIGHT + "Thank you for using our cleaning and merging services!" + Style.RESET_ALL)

cleanTheData()
mergeCSVFilesToOne()
