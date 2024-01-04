import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import math
import seaborn as sns
import statsmodels.api as sm
import scipy.stats as stats
import warnings

import os

# Constants
Drag_constant = 0.75
Area = 0.25
Density = 1.293
Radius = 0.1
Mass = 0.8
g = 9.82
pi = 3.141

colors = ['#66c2a5', '#fc8d62', '#8da0cb', '#e78ac3', '#a6d854', '#ffd92f']

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
    plt.figure()
    plt.hist(df[column], bins=bins)
    plt.title(str('Histogram of ' + column + '_'+ title))
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    plt.savefig(fname =('./figs/Histogram_' + column + '_' + title))
    plt.close()
    #plt.show()

def GenerateScatterPlot(df, x, y, title, xlabel, ylabel):
    # Generate a scatter plot
    plt.figure()
    plt.scatter(df[x], df[y])
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    plt.ylim(0)
    plt.xlim(0)
    plt.savefig(fname =('./figs/ScatterPlot_' + title))
    plt.close()
    #plt.show()

def GenerateBoxPlot(df, column, title, ylabel):
    # Generate a box plot
    plt.figure()
    sns.swarmplot(y=df[column], color=colors[0],size=5)
    sns.boxplot(df[column],color=colors[1],width=0.4)
    plt.title(str('Boxplot of ' + column + '_'+ title))
    plt.ylabel(ylabel)
    plt.savefig(fname =('./figs/BoxPlot_' + column + '_' + title))
    plt.close()
    #plt.show()

def GenerateQQPlot(df, column, title):
    # Generate a QQ plot
    ax1 = plt.subplot(221)
    ax1.set_title('Uniform distribution')
    sm.qqplot(df[column], line='45',dist=stats.uniform,fit=True,ax=ax1)
    ax2 = plt.subplot(222)
    ax2.set_title('Normal distribution')
    sm.qqplot(df[column], line='45',dist=stats.norm,fit=True,ax=ax2)
    ax3 = plt.subplot(223)
    ax3.set_title('Exponential distribution')
    sm.qqplot(df[column], line='45',dist=stats.expon,fit=True,ax=ax3)
    ax4 = plt.subplot(224)
    ax4.set_title('Lognormal distribution')
    sm.qqplot(df[column], line='45',dist=stats.lognorm,fit=True,ax=ax4)
    plt.tight_layout()
    plt.savefig(fname =('./figs/QQPlot_' + column + '_' + title))
    plt.close()

def GeneratePlots(df, column, title, unit):
    # Generate a histograms, scatter plot and box plot
    GenerateHistogram(df, column, title, unit)
    GenerateBoxPlot(df, column, title, unit)
    GenerateQQPlot(df, column, title)

def CalculateStatData(df):
    # Calculate the data for the table
    N = len(df['Time'])
    Time_mean = df['Time'].mean()
    Time_var = df['Time'].var()
    Time_max = df['Time'].max()
    Time_min = df['Time'].min()
    Time_mu_lower, Time_mu_upper = CalculateConfidenceInterval(df, 'Time', alpha=0.05)
    Distance_mean = df['Distance'].mean()
    Distance_var = df['Distance'].var()
    Distance_mu_lower, Distance_mu_upper = CalculateConfidenceInterval(df, 'Distance', alpha=0.05)
    DistanceInMeters_mean = df['DistanceInMeters'].mean()
    DistanceInMeters_var = df['DistanceInMeters'].var()
    DistanceInMeters_mu_lower, DistanceInMeters_mu_upper = CalculateConfidenceInterval(df, 'DistanceInMeters', alpha=0.05)
    E_horison_mean = df['E_horison'].mean()
    E_horison_var = df['E_horison'].var()
    E_horison_mu_lower, E_horison_mu_upper = CalculateConfidenceInterval(df, 'E_horison', alpha=0.05)
    E_hover_mean = df['E_hover'].mean()
    E_hover_var = df['E_hover'].var()
    E_hover_mu_lower, E_hover_mu_upper = CalculateConfidenceInterval(df, 'E_hover', alpha=0.05)
    E_total_mean = df['E_total'].mean()
    E_total_var = df['E_total'].var()
    E_total_mu_lower, E_total_mu_upper = CalculateConfidenceInterval(df, 'E_total', alpha=0.05)
    #print(df['NodesLost'])
    NodesLost_mean = df['NodesLost'].mean()
    NodesLost_var = df['NodesLost'].var()
    NodesLost_mu_lower, NodesLost_mu_upper = CalculateConfidenceInterval(df, 'NodesLost', alpha=0.05)

    Nodeloss_tf = pd.DataFrame()
    Nodeloss_tf['NodesLost'] = df['NodesLost'] > 0
    # Change NodesLost to 1 if node is lost and 0 if not
    Nodeloss_tf['NodesLost'] = Nodeloss_tf['NodesLost'].astype(int)
    P_nodeloss_mean = Nodeloss_tf['NodesLost'].mean()
    P_nodeloss_var = Nodeloss_tf['NodesLost'].var()
    P_nodeloss_mu_lower, P_nodeloss_mu_upper = CalculateConfidenceInterval(Nodeloss_tf, 'NodesLost', alpha=0.05)

    

    for column in df:
        CalculateConfidenceInterval(df, column)

    data = [[Time_mean, Distance_mean, DistanceInMeters_mean, E_horison_mean, E_hover_mean,  E_total_mean, NodesLost_mean, P_nodeloss_mean,N],
            [Time_var, Distance_var, DistanceInMeters_var, E_horison_var, E_hover_var, E_total_var, NodesLost_var, P_nodeloss_var,N],
            [Time_mu_upper, Distance_mu_upper,  DistanceInMeters_mu_upper, E_horison_mu_lower, E_hover_mu_upper, E_total_mu_upper, NodesLost_mu_upper, P_nodeloss_mu_upper,N],
            [Time_mu_lower, Distance_mu_lower, DistanceInMeters_mu_lower, E_horison_mu_lower, E_hover_mu_lower, E_total_mu_lower, NodesLost_mu_lower, P_nodeloss_mu_lower,N],
            [(Time_mu_upper-Time_mu_lower)/2, (Distance_mu_upper-Distance_mu_lower)/2, (DistanceInMeters_mu_upper-DistanceInMeters_mu_lower)/2,
            (E_horison_mu_upper-E_horison_mu_lower)/2, (E_hover_mu_upper-E_hover_mu_lower)/2,(E_total_mu_upper-E_total_mu_lower)/2, 
            (NodesLost_mu_upper-NodesLost_mu_lower)/2, (P_nodeloss_mu_upper-P_nodeloss_mu_lower)/2,N]]
    
    data = pd.DataFrame(data)
    data.columns = ['Time', 'Distance', 'DistanceInMeters', 'E_horison', 'E_hover' ,'E_total', 'NodesLost', 'P_nodeloss','Number of samples']
    data.index = ['Mean', 'Variance','Mu upper', 'Mu lower','Uncertainty']
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
        df.to_csv(path, sep=';', index=True, decimal='.')
    else:
        df.to_csv(path, mode='a', index=True, header=False)

def CalculateConfidenceInterval(df, column, alpha=0.05):
    # Calculate the confidence interval
    if df[column].var() == 0:
        return [df[column].mean(),df[column].mean()]
    intervals = stats.t.interval(df=len(df[column]-1), 
        loc=df[column].mean(), 
        scale=stats.sem(df[column].to_numpy()),confidence=1-alpha) 
    return intervals


def AnalyseResultsOfPRP(path,Method='No LE'):
    # Analyse the results
    raw_data = read_data(path)
    distance_data = AddDistanceInMeters(raw_data)
    energy_data = AddEnergy(distance_data)
    calculated_data = CalculateStatData(energy_data)
    calculated_data = AddMetaData(calculated_data, path)
    data_title = str(Method + '_S' + calculated_data['Scenario'].iloc[0] +  calculated_data['PRP'].iloc[0])
    #print(data_title)
    GenerateScatterPlot(energy_data, y='Time', x='DistanceInMeters', title='Time as function of distance in '+data_title, ylabel='Time[s]',xlabel='Distance[m]')
    #GeneratePlots(energy_data, 'Time', data_title, 'Time[s]')
    #GeneratePlots(energy_data, 'DistanceInMeters', data_title, 'Distance[m]')
    GeneratePlots(energy_data, 'Distance', data_title, 'Distance[UnityDistance]')
    #GeneratePlots(energy_data, 'E_total', data_title, 'Energy[J]')
    SaveData(calculated_data, '.\CompareData_' + Method + '.csv')
    #print(calculated_data)
    return calculated_data

def AnalyseScenario(path,Method='No LE'):
    PRP1 = AnalyseResultsOfPRP(path + '\PRP1.txt',Method)
    PRP2 = AnalyseResultsOfPRP(path + '\PRP2.txt',Method)
    PRP3 = AnalyseResultsOfPRP(path + '\PRP3.txt',Method)    
    data = pd.concat([PRP1,PRP2])
    data = pd.concat([data,PRP3])
    print(data)
    return data

def GenerateBarPlot(data,X_axis, column, title, ylabel,xlabel,legend,ylim=[0,350]):
    plt.figure(0)
    for i in range(0, len(legend)):
        plt.bar(X_axis + 0.2*(i-1), data[i].loc['Mean',column], width=0.2, 
            label = legend[i], color=colors[i])
        e_bars = []
        for j in range(0, len(data[i].loc['Mu lower',column])):
            e_bars.append((data[i].loc['Mu upper',column].iloc[j]) - (data[i].loc['Mu lower',column].iloc[j]))
        plt.errorbar(X_axis + 0.2*(i-1), data[i].loc['Mean',column], 
            yerr=e_bars, fmt='none', ecolor='black', capsize=5,label='_nolegend_')
    plt.xticks(X_axis, data[0]['PRP'].loc['Mean'])
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)
    plt.legend(legend)
    plt.ylim(ylim[0],ylim[1])
    plt.savefig(fname =('./figs/'+ column + '_S' + str(data[0]['Scenario'].iloc[0])))
    plt.close()

def CompareAlgorithms(Paths, Labels,Scenario=1):
    #Analise the data
    data = list(range(0, len(Paths)))
    for i in range(0, len(Paths)):
        print(Labels[i])
        data[i] = AnalyseScenario(Paths[i],Labels[i].replace(' ','_'))

    X_axis = np.arange(len(data[0]['PRP'].loc['Mean']))  # the label locations
    print(Labels)

    # Plot the data
    GenerateBarPlot(data,X_axis, 'Time', 'Time in scenario '+ str(Scenario), 'Time [s]','PRP',Labels,ylim=[0,350])
    GenerateBarPlot(data,X_axis, 'DistanceInMeters', 'Distance in scenario '+ str(Scenario), 'Distance [m]','PRP',Labels,ylim=[0,30000])
    # Plot the data
    GenerateBarPlot(data,X_axis, 'E_total', 'Energy consumption in scenario '+ str(Scenario), 'Energy [J]','PRP',Labels,ylim=[0,30000])
    
    GenerateBarPlot(data,X_axis, 'P_nodeloss', 'Risk of losing nodes in scenario '+ str(Scenario), 'Risk of losing nodes','PRP',Labels,ylim=[0,1])
    

    return data

def Concentrate_All_data():
    All_data = pd.DataFrame()
    #Analyse scenario 0
    paths = ['.\\Results_No_LE\\Scenario0', '.\\Results_LE\\Scenario0', '.\\Results\\Scenario0']
    labels = ['No Leader Election', 'Leader election', 'Instant partition']
    S0 = CompareAlgorithms(paths,labels,Scenario=0)
    for i in range(0, len(S0)):
        S0[i]['Method']=labels[i]
        All_data = pd.concat([All_data,S0[i]])

    #Analyse scenario 1
    paths = ['.\\Results_No_LE\\Scenario1', '.\\Results_LE\\Scenario1', '.\\Results\\Scenario1']
    labels = ['No Leader Election', 'Leader election', 'Instant partition']
    S1 = CompareAlgorithms(paths,labels,Scenario=1)
    for i in range(0, len(S1)):
        S1[i]['Method']=labels[i]
        All_data = pd.concat([All_data,S1[i]])

    #Analyse scenario 2
    paths = ['.\\Results\\Scenario2']
    labels = ['Instant partition']
    S2 = CompareAlgorithms(paths,labels,Scenario=2)
    for i in range(0, len(S2)):
        S2[i]['Method']=labels[i]
        All_data = pd.concat([All_data,S2[i]])

    #Analyse scenario 3
    paths = ['.\\Results_No_LE\\Scenario3', '.\\Results_LE\\Scenario3']
    labels = ['No Leader Election', 'Leader election']
    S3 = CompareAlgorithms(paths,labels,Scenario=3)
    
    for i in range(0, len(S3)):
        S3[i]['Method']=labels[i]
        All_data = pd.concat([All_data,S3[i]])

    return All_data


data = Concentrate_All_data()
SaveData(data, '.\CompareData_All.csv')
print(data)

#Plot Energy consumption based on scenario
PRP1 = data.loc[data['PRP'] == 'PRP1']
PRP2 = data.loc[data['PRP'] == 'PRP2']
PRP3 = data.loc[data['PRP'] == 'PRP3']
PRP = [PRP1, PRP2, PRP3]
