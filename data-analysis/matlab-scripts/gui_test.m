close all, clear all, clc

zeta = .5;                           % Damping Ratio
wn = 2;                              % Natural Frequency
sys = tf(wn^2,[1,2*zeta*wn,wn^2]); 

f = figure;
ax = axes("Parent", f, "position", [0.13 0.39 0.77 0.54]);
h = stepplot(ax, sys);
setoptions(h, "XLim",[0,10],"YLim",[0,2]);

