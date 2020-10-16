function files = getAllFilePaths()
    % Get info about the files inside a specific folder
    myFiles = dir('../raw-data');
    
    % Create cells with filenames and folder paths
    fileNames = {myFiles(:).name}';
    fileFolders = {myFiles(:).folder}';

    % Filter for those ending with .csv
    csvFiles = fileNames(endsWith(fileNames,'.csv'));
    csvFolders = fileFolders(endsWith(fileNames,'.csv'));

    % Merge paths with corresponding filename
    files = fullfile(csvFolders, csvFiles);
end