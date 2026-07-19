namespace StoneShardSaveCheat.Services
{
    public partial class CharacterFolderMonitorService(ILogger<CharacterFolderMonitorService> logger) : BackgroundService
    {
        private FileSystemWatcher _fsWatcher = default!;

        private const string _targetDirectory = "exitsave_1";
        private const int _maxBackups = 10;

        private readonly string _charDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StoneShard",
            "characters_v1",
            "character_4"
        );
        private readonly string _localBackupFolder = "backups";

        private (Guid Prev, Guid Current) _saveId = (Guid.Empty, Guid.NewGuid());

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            LogStartingMonitor(_charDirectory);

            try
            {
                if (!Directory.Exists(_charDirectory))
                {
                    LogCharacterFolderNotFound(_charDirectory);
                    return Task.CompletedTask;
                }

                Directory.CreateDirectory(_localBackupFolder);

                LogPerformingInitialBackup(_targetDirectory);

                BackupDirectory(Path.Combine(_charDirectory, _targetDirectory));

                _fsWatcher = new FileSystemWatcher(_charDirectory)
                {
                    NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };

                _fsWatcher.Created += OnDirectoryCreated;
                _fsWatcher.Changed += OnDirectoryChanged;
                _fsWatcher.Deleted += OnDirectoryDeleted;
                _fsWatcher.Error += OnError;

                LogMonitorStartedSuccessfully(_targetDirectory);
            }
            catch (Exception ex)
            {
                LogErrorStartingMonitor(ex);
            }

            return Task.CompletedTask;
        }

        private void OnDirectoryCreated(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileName(e.FullPath) == _targetDirectory)
            {
                LogTargetDirectoryCreated(e.FullPath);

                _saveId = (_saveId.Current, Guid.NewGuid());
            }
        }

        private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileName(e.FullPath) == _targetDirectory)
            {
                LogTargetDirectoryChanged(e.FullPath);

                BackupDirectory(e.FullPath);
            }
        }

        private void OnDirectoryDeleted(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileName(e.FullPath) == _targetDirectory)
            {
                LogTargetDirectoryDeleted(e.FullPath);

                _fsWatcher.EnableRaisingEvents = false;
                RestoreLatestBackup();
                _fsWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            var exception = e.GetException();

            LogFileSystemWatcherError(exception);
        }

        private void BackupDirectory(string sourcePath)
        {
            try
            {
                if (!Directory.Exists(sourcePath))
                {
                    LogSourceDirectoryNotFound(sourcePath);
                    return;
                }

                var updatedBackupPath = GetBackupDirectories()
                    .FirstOrDefault(x => x.Contains(_saveId.Current.ToString()));

                var backupPath = Path.Combine(
                    _localBackupFolder,
                    updatedBackupPath is not null
                        ? Path.GetFileName(updatedBackupPath)
                        : $"{_targetDirectory}_{_saveId.Current}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss-fff}"
                );

                LogCopyingToBackup(_targetDirectory, backupPath);

                CopyDirectory(sourcePath, backupPath);

                LogBackupSuccessful(_targetDirectory, backupPath);

                CleanupOldBackups();
            }
            catch (Exception ex)
            {
                LogErrorBackingUpDirectory(ex, sourcePath);
            }
        }

        private void RestoreLatestBackup()
        {
            try
            {
                var targetPath = Path.Combine(_charDirectory, _targetDirectory);

                if (Directory.Exists(targetPath))
                {
                    LogRestoreNotRequired();
                    return;
                }

                if (!Directory.Exists(_localBackupFolder))
                {
                    LogBackupFolderNotFound();
                    return;
                }

                var latestBackup = GetBackupDirectories()
                    .OrderByDescending(Directory.GetCreationTime)
                    .FirstOrDefault();

                if (latestBackup is null)
                {
                    LogNoBackupsFound();
                    return;
                }

                LogRestoringLatestBackup(latestBackup, targetPath);

                CopyDirectory(latestBackup, targetPath);

                LogRestoreSuccessful(latestBackup);
            }
            catch (Exception ex)
            {
                LogErrorRestoringLatestBackup(ex);
            }
        }

        private void CleanupOldBackups()
        {
            try
            {
                if (!Directory.Exists(_localBackupFolder))
                    return;

                var backups = Directory.GetDirectories(_localBackupFolder)
                    .Where(d => Path.GetFileName(d).StartsWith(_targetDirectory))
                    .OrderByDescending(Directory.GetCreationTime)
                    .ToList();

                if (backups.Count <= _maxBackups)
                    return;

                var dirsToDelete = backups.Skip(_maxBackups).ToList();

                foreach (var dir in dirsToDelete)
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        LogDeletedOldBackup(dir);
                    }
                    catch (Exception ex)
                    {
                        LogErrorDeletingOldBackup(ex, dir);
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorCleaningupOldBackups(ex);
            }
        }

        private IEnumerable<string> GetBackupDirectories()
            => Directory.GetDirectories(_localBackupFolder)
            .Where(d => Path.GetFileName(d).StartsWith(_targetDirectory));

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

            Directory.CreateDirectory(destDir);

            foreach (var file in dir.GetFiles())
            {
                var targetFilePath = Path.Combine(destDir, file.Name);

                file.CopyTo(targetFilePath, true);
            }

            foreach (var subDir in dir.GetDirectories())
            {
                var targetSubDir = Path.Combine(destDir, subDir.Name);

                CopyDirectory(subDir.FullName, targetSubDir);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            LogStoppingMonitor();

            if (_fsWatcher != null)
            {
                _fsWatcher.EnableRaisingEvents = false;
                _fsWatcher.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }

        [LoggerMessage(1, LogLevel.Information, "Starting character folder monitor for: {FolderPath}")]
        private partial void LogStartingMonitor(string folderPath);

        [LoggerMessage(2, LogLevel.Error, "Character folder does not exist: {FolderPath}")]
        private partial void LogCharacterFolderNotFound(string folderPath);

        [LoggerMessage(3, LogLevel.Information, "Performing initial backup of '{TargetDirectory}'")]
        private partial void LogPerformingInitialBackup(string targetDirectory);

        [LoggerMessage(4, LogLevel.Information, "Character folder monitor started successfully. Watching for '{TargetDirectory}' directory")]
        private partial void LogMonitorStartedSuccessfully(string targetDirectory);

        [LoggerMessage(5, LogLevel.Error, "Error starting character folder monitor")]
        private partial void LogErrorStartingMonitor(Exception ex);

        [LoggerMessage(6, LogLevel.Debug, "Target directory created: {DirectoryPath}")]
        private partial void LogTargetDirectoryCreated(string directoryPath);

        [LoggerMessage(7, LogLevel.Debug, "Target directory changed: {DirectoryPath}")]
        private partial void LogTargetDirectoryChanged(string directoryPath);

        [LoggerMessage(8, LogLevel.Debug, "Target directory deleted: {DirectoryPath}")]
        private partial void LogTargetDirectoryDeleted(string directoryPath);

        [LoggerMessage(9, LogLevel.Error, "FileSystemWatcher error")]
        private partial void LogFileSystemWatcherError(Exception ex);

        [LoggerMessage(10, LogLevel.Warning, "Skipping backup, source directory does not exist: {DirectoryPath}")]
        private partial void LogSourceDirectoryNotFound(string directoryPath);

        [LoggerMessage(11, LogLevel.Debug, "Skipping backup of restored directory: {DirectoryPath}")]
        private partial void LogSkippingBackupOfRestoredDirectory(string directoryPath);

        [LoggerMessage(12, LogLevel.Debug, "Copying '{TargetDirectory}' to: {BackupPath}")]
        private partial void LogCopyingToBackup(string targetDirectory, string backupPath);

        [LoggerMessage(13, LogLevel.Information, "Successfully backed up '{TargetDirectory}' to: {BackupPath}")]
        private partial void LogBackupSuccessful(string targetDirectory, string backupPath);

        [LoggerMessage(14, LogLevel.Error, "Error backing up directory: {DirectoryPath}")]
        private partial void LogErrorBackingUpDirectory(Exception ex, string directoryPath);

        [LoggerMessage(15, LogLevel.Information, "Deleted old backup: {BackupPath}")]
        private partial void LogDeletedOldBackup(string backupPath);

        [LoggerMessage(16, LogLevel.Warning, "Error deleting old backup: {BackupPath}")]
        private partial void LogErrorDeletingOldBackup(Exception ex, string backupPath);

        [LoggerMessage(17, LogLevel.Error, "Error cleaning up old backups")]
        private partial void LogErrorCleaningupOldBackups(Exception ex);

        [LoggerMessage(18, LogLevel.Error, "Backup folder does not exist")]
        private partial void LogBackupFolderNotFound();

        [LoggerMessage(19, LogLevel.Warning, "No backups found to restore")]
        private partial void LogNoBackupsFound();

        [LoggerMessage(20, LogLevel.Debug, "Restoring latest backup from: {BackupPath} to: {TargetPath}")]
        private partial void LogRestoringLatestBackup(string backupPath, string targetPath);

        [LoggerMessage(21, LogLevel.Information, "Successfully restored from backup: {BackupPath}")]
        private partial void LogRestoreSuccessful(string backupPath);

        [LoggerMessage(22, LogLevel.Error, "Error restoring latest backup")]
        private partial void LogErrorRestoringLatestBackup(Exception ex);

        [LoggerMessage(23, LogLevel.Information, "Stopping character folder monitor")]
        private partial void LogStoppingMonitor();

        [LoggerMessage(24, LogLevel.Debug, "Save file already exists. Assuming it is the latest one. Restore not required.")]
        private partial void LogRestoreNotRequired();
    }
}
