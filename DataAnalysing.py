import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import math

import os

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

def GenerateHistogram(df, column, title, xlabel, ylabel='Frequency', bins=10):
    # Generate a histogram
    plt.hist(df[column], bins=bins)
    plt.title(str('Histogram of ' + column + '_'+ title))
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    plt.savefig(fname =('./figs/Histogram_' + column + '_' + title))
    #plt.show()

def GenerateScatterPlot(df, x, y, title, xlabel):
    # Generate a scatter plot
    plt.scatter(df[x], df[y])
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel('Frequency')
    plt.savefig(fname =('./figs/ScatterPlot_' + title))
    #plt.show()

def GenerateBoxPlot(df, column, title, ylabel):
    # Generate a box plot
    
    plt.boxplot(df[column])
    plt.title(str('Boxplot of ' + column + '_'+ title))
    plt.ylabel(ylabel)
    plt.savefig(fname =('./figs/BoxPlot_' + column + '_' + title))
    #plt.show()

def GeneratePlots(df, column, title, unit):
    # Generate a histograms, scatter plot and box plot
    GenerateHistogram(df, column, title, unit)
    GenerateBoxPlot(df, column, title, unit)

def CalculateTableData(df):
    # Calculate the data for the table
    Time_mean = df['Time'].mean()
    Distance_mean = df['Distance'].mean()
    DistanceInMeters_mean = df['DistanceInMeters'].mean()
    E_horison_mean = df['E_horison'].mean()
    E_hover_mean = df['E_hover'].mean()
    E_total_mean = df['E_total'].mean()
    #print(df['NodesLost'])
    NodesLost_mean = df['NodesLost'].mean()
    P_nodeloss = 0
    for i in range(0, len(df['NodesLost'])):
        if df['NodesLost'].iloc[i] > 0:
            P_nodeloss += 1
    P_nodeloss = P_nodeloss / len(df['NodesLost'])
    data = [[Time_mean, Distance_mean, DistanceInMeters_mean, E_horison_mean, E_hover_mean, E_total_mean, NodesLost_mean, P_nodeloss]]
    data = pd.DataFrame(data)
    data.columns = ['Time_mean', 'Distance_mean', 'DistanceInMeters_mean', 'E_horison_mean', 'E_hover_mean', 'E_total_mean', 'NodesLost_mean', 'P_nodeloss']
    return data

def AddMetaData(df,path):
    # Add metadata to the dataframe
    PRP = (path.split('\\')[-1])[0:4]
    Scenario = (path.split('\\')[-2].split('\\')[0])[8]
    df['PRP'] = PRP
    df['Scenario'] = Scenario
    return df

def SaveData(df, path):
    # Save the data to a csv file
    if not os.path.isfile(path):
        df.to_csv(path, sep=';', index=False, decimal=',')
    else:
        df.to_csv(path, mode='a', index=False, header=False)



def AnalyseResultsOfPRP(path):
    # Analyse the results
    raw_data = read_data(path)
    distance_data = AddDistanceInMeters(raw_data)
    energy_data = AddEnergy(distance_data)
    calculated_data = CalculateTableData(energy_data)
    calculated_data = AddMetaData(calculated_data, path)
    data_title = str('S' + calculated_data['Scenario'][0] +  calculated_data['PRP'][0])
    print(data_title)
    GeneratePlots(energy_data, 'Time', data_title, 'Time[s]')
    GeneratePlots(energy_data, 'E_total', data_title, 'Energy[J]')
    SaveData(calculated_data, '.\CompareData.csv')
    print(calculated_data)
    return calculated_data

def AnalyseScenario(path):
    PRP1 = AnalyseResultsOfPRP(path + '\PRP1.txt')
    PRP2 = AnalyseResultsOfPRP(path + '\PRP2.txt')
    PRP3 = AnalyseResultsOfPRP(path + '\PRP3.txt')
    data = pd.concat([PRP1,PRP2])
    data = pd.concat([data,PRP3])
    print(data)


# Get the data from the csv files
path = '.\Results_No_LE\Scenario2'
AnalyseScenario(path)
