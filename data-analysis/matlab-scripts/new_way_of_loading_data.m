close all, clear all, clc

files = getAllFilePaths();

guess = files(contains(files,'GuessOMeter'));
diffAndCope = files(contains(files,'DiffAndCOPE1'))