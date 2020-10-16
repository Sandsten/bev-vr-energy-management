%%
close all, clear all, clc

filenamesGuessO = {
    'data_001_GuessOMeter_20200312T131418.csv',
    'data_001_GuessOMeter_20200312T132429.csv',
    'data_003_GuessOMeter_20200319T125526.csv',
    'data_003_GuessOMeter_20200319T130843.csv',
    'data_005_GuessOMeter_20200319T164811.csv',
    'data_005_GuessOMeter_20200319T165854.csv',
    'data_007_GuessOMeter_20200322T151318.csv',
    'data_007_GuessOMeter_20200322T152353.csv',
    'data_009_GuessOMeter_20200515T134951.csv',
    'data_009_GuessOMeter_20200515T135722.csv'
    };

filenamesDifferentiated = {
    'data_002_DiffAndCOPE1_20200318T171818.csv',
    'data_002_DiffAndCOPE1_20200318T173240.csv',
    'data_004_DiffAndCOPE1_20200321T124620.csv',
    'data_004_DiffAndCOPE1_20200321T125622.csv',
    'data_006_DiffAndCOPE1_20200322T092436.csv',
    'data_006_DiffAndCOPE1_20200322T093751.csv',
    'data_008_DiffAndCOPE1_20200324T165437.csv',
    'data_008_DiffAndCOPE1_20200324T170454.csv',
    'data_010_DiffAndCOPE1_20200516T195703.csv',
    'data_010_DiffAndCOPE1_20200516T200626.csv'
    };

% diffDrivingDispVals = adaptToSameTimeSteps(filenamesDifferentiated);
%
% hold on
% for i = 1:size(diffDrivingDispVals,2)
%     plot(diffDrivingDispVals{i}(:,7), diffDrivingDispVals{i}(:,2), 'b')
% end
%
% guessOMeterDispVals = adaptToSameTimeSteps(filenamesGuessO);
%
% hold on
% for i = 1:size(guessOMeterDispVals,2)
%     plot(guessOMeterDispVals{i}(:,7), guessOMeterDispVals{i}(:,2), 'r')
% end
%
%
% ylabel("SoC [KW/h]")
% xlabel("Dist [m]")
%
% legend("Guess-o-meter", "Diff")
%
% grid on


% grid on
% plot(interpValues{1}(:,7), interpValues{1}(:,2))
% hold on
% plot(interpValues{2}(:,7), interpValues{2}(:,2))

% meanCharge = mean([newValues1(:,2) newValues2(:,2)],2)
% plot(newValues2(:,7), meanCharge, 'g')

% set1 = datatables(1)
% x = set1{1,1}{:,3} % Time
% y = set1{1,1}{:,3:end} % State of charge
% newValues1 = interp1(x,y,xq) % Interpolates all columns in y3 with the new fixed timesteps
%
% set2 = datatables(2)
% x = set2{1,1}{:,3} % Time
% y = set2{1,1}{:,3:end} % State of charge
% newValues2 = interp1(x,y,xq) % Interpolates all columns in y3 with the new fixed timesteps


% grid on


% SoC against distance
% Guess-o-meter
figure(1)
prevID = 0;
h=[];
for file = 1:numel(filenamesGuessO)
    data = readtable(filenamesGuessO{file});
    data(data{:,4} < 0, :) = [];
        
    id = data{1,1};
    
    dist = data{:,9};
    charge = data{:,4};
        hold on
    % F�rsta f�rs�ket �r str�ckad linje
    if id == prevID
        marker = plot(dist(end), charge(end), 'xr', 'markerSize', 20, 'LineWidth', 3);
        marker.Color(4) = 0.5;
        line = plot(dist, charge, 'r', 'DisplayName', 'Guess-o-meter 2nd attempt', 'LineWidth', 2);
        line.Color(4) = 0.5;
        h(end+1) = line;
        
    else
%         plot(dist(end), charge(end), 'xr', 'markerSize', 20, 'LineWidth', 3);
%         
%         line = plot(dist, charge, 'r--', 'DisplayName', 'Guess-o-meter 1st attempt', 'LineWidth', 2);
%         line.Color(4) = 0.5;
%         h(end+1) = line;
        
    end
    
    prevID = id;

end

% Differentiated driving + COPE1 disp
prevID = 0;
for file = 1:numel(filenamesDifferentiated)
    data = readtable(filenamesDifferentiated{file});
    data(data{:,4} < 0, :) = [];
        
    id = data{1,1};
    
    dist = data{:,9};
    charge = data{:,4};
        hold on
    if id == prevID
        marker = plot(dist(end), charge(end), 'ob', 'markerSize', 15, 'LineWidth', 3);
        marker.Color(4) = 0.5;
        line = plot(dist, charge, 'b', 'DisplayName', 'Diff+COPE1 2nd attempt', 'LineWidth', 2);
        line.Color(4) = 0.5;
        h(end+1) = line;
        
    else
%         plot(dist(end), charge(end), 'ob', 'markerSize', 15, 'LineWidth', 3);
%         
%         line = plot(dist, charge, 'b--', 'DisplayName', 'Diff+COPE1 1st attempt', 'LineWidth', 2);
%         line.Color(4) = 0.5;
%         h(end+1) = line;       
    end
    
    prevID = id;

end

grid on

ylabel("SoC [KW/h]", "FontSize", 15)
xlabel("Dist [m]")
title("State of charge over distance traveled")

set(gca,'FontSize',24)
set(gcf, 'Position',  [100, 100, 1200, 600])

highway = xline(3000, 'LineWidth', 2, 'DisplayName', '110 km/h speed limit start');
goal = xline(8000, 'g', 'LineWidth', 2, 'DisplayName', 'Goal');
hold on

yyaxis right
data = readtable(filenamesDifferentiated{4});
dist = data{:,9};
h2 = plot(dist, data{:,end-1}, 'k--', 'LineWidth', 2, 'DisplayName', 'Road height');
ylabel("Road height [m]")
ylim([-10 20])
grid on

% legend([ h(1), h(2), h(end-1), h(end) ]);
% legend([ h(1), h(2), h(end-1), h(end) ]);
% legend([h(1), h(2), h(end-1) ,h(end) h2]);
legend([h(1), h(end) highway goal h2]);

% % Speed against distance
% figure(2)
% prevID = 0;
% for file = 1:numel(filenames)
%     data = readtable(filenames{file});
%
%     id = data{1,1};
%
%     dist = data{:,9};
%     speed = data{:,8};
%
%     % F�rsta f�rs�ket �r str�ckad linje
%     if strcmp(data{1,2},'GuessOMeter')
%         if id == prevID
%             %             plot(dist, speed, 'r')
%         else
%             plot(dist, speed, 'r--')
%         end
%     else
%         if id == prevID
%             %             plot(dist, speed, 'b')
%         else
%             plot(dist, speed, 'b--')
%         end
%     end
%
%     prevID = id;
%     hold on
% end
%
% grid on
%
% ylabel("Speed [km/h]")
% xlabel("Dist [m]")
%
% legend("Guess-o-meter", "Diff")


% % Speed against distance
% figure(3)
% prevID = 0;
% for file = 1:numel(filenames)
%     data = readtable(filenames{file});
%
%     id = data{1,1};
%
%     dist = data{:,9};
%     throttle = data{:,10};
%
%     % F�rsta f�rs�ket �r str�ckad linje
%     if strcmp(data{1,2},'GuessOMeter')
%         if id == prevID
%             %             plot(dist, throttle, 'r')
%         else
%             plot(dist, throttle, 'r--')
%         end
%     else
%         if id == prevID
%             %             plot(dist, throttle, 'b')
%         else
%             plot(dist, throttle, 'b--')
%         end
%     end
%
%     prevID = id;
%     hold on
% end
%
% grid on
%
% ylabel("Speed [km/h]")
% xlabel("Throttle [m]")
%
% legend("Guess-o-meter", "Diff")


function [interpValues, datatables] = adaptToSameTimeSteps(files)
xq = 0:0.1:1000;   % New time steps, fixed 0.1 s intervals
interpValues = {};
for file = 1:numel(files)
    datatables{file} = readtable(files{file});
    
    set = datatables(file);
    x = set{1,1}{:,3}; % Time
    y = set{1,1}{:,3:end}; % State of charge
    newValues = interp1(x,y,xq); % Interpolates all columns in y3 with the new fixed timesteps
    
    interpValues{file} = newValues;
end
end
