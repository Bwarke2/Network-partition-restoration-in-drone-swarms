import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import math

# Constants
Drag_constant = 0.75
Area = 0.25
Density = 1.293
Radius = 0.1
Mass = 0.8
g = 9.82
pi = 3.141

# Read the data from the csv files
def read_data(path):
    # Read the data from the csv files
    df = pd.read_csv(path,sep=';',header=0,decimal=',',)
    return df

def AddDistanceInMeters(df):
    # Add a column with the distance
    df['DistanceInMeters'] = df['Distance'].multiply(20)
    return df

def AddEnergy(df):
    #Add horizontal and hover energy
    df['E_horison'] = df['DistanceInMeters'] * Drag_constant * Area * Density/2
    df['E_hover'] = math.sqrt(pow((Mass * g),3) / (2*pi* Density*pow(Radius,2))) * df['Time']
    df['E_total'] = df['E_horison'] + df['E_hover']
    return df

def AddCalColums(df):
    # Add a column with the distance
    AddDistanceInMeters(df)
    AddEnergy(df)
    return df

def GetMeans(df):
    # Get the means of the data
    Time_mean = df['Time'].mean()
    Distance_mean = df['Distance'].mean()
    DistanceInMeters_mean = df['DistanceInMeters'].mean()
    E_horison_mean = df['E_horison'].mean()
    E_hover_mean = df['E_hover'].mean()
    E_total_mean = df['E_total'].mean()
    means = [{'Time_mean', 'Distance_mean', 
              'DistanceInMeters_mean', 
              'E_horison_mean', 'E_hover_mean', 
              'E_total_mean'}, 
              {Time_mean, Distance_mean, 
               DistanceInMeters_mean, 
               E_horison_mean, 
               E_hover_mean, E_total_mean}]
    means = pd.DataFrame(means)
    print(means)
    return means

def GenerateHistogram(df, column, title, xlabel, ylabel='Frequency', bins=10):
    # Generate a histogram
    plt.hist(df[column], bins=bins)
    plt.title('Histogram of ' + column)
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    plt.savefig(fname =('.\figs\Histogram_' + title))
    plt.show()


# Get the data from the csv files
raw_data = read_data('.\Assets\Results_LE\PRP1_test.txt')
print(raw_data)
print(raw_data.keys())
distance_data = AddDistanceInMeters(raw_data)
print(distance_data)
energy_data = AddEnergy(distance_data)
print(energy_data)
means = GetMeans(energy_data)
GenerateHistogram(energy_data, 'E_total', 'Histogram of E_total', 'E_total(J)', 'Frequency', 10)
GenerateHistogram(energy_data, 'Time', 'Histogram of Time', 'Time(s)', 'Frequency', 10)
