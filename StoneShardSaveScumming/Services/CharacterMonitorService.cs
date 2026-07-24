using StoneShardSaveScumming.Domain.Backups;
using StoneShardSaveScumming.Domain.Game;

namespace StoneShardSaveCheat.Services
{
    public partial class CharacterMonitorService(ILogger<CharacterMonitorService> logger) : BackgroundService
    {
        private static readonly CharacterDirectory _charDirectory = new(number: 4);
        private static readonly ExitSaveDirectory _exitSaveDirectory = new(_charDirectory);
        private static readonly GameDirectory _monitorDirectory = _exitSaveDirectory;
        private static string MonitorDirectoryName => _monitorDirectory.Id.Value;

        private readonly BackupOptions _backupOptions = new();
        private readonly BackupsStorageDirectory _backupsDirectory = new();

        private FileSystemWatcher _fsWatcher = default!;
        private SaveId _saveId = new();

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var characterDirectoryPath = _monitorDirectory.Character?.PathLocal ?? string.Empty;
            var monitorDirectoryPath = _monitorDirectory.PathLocal;

            LogStartingMonitor(monitorDirectoryPath);

            try
            {
                if (!Directory.Exists(characterDirectoryPath))
                {
                    LogCharacterFolderNotFound(characterDirectoryPath);
                    return Task.CompletedTask;
                }

                DoInitialBackup();
                StartCharacterMonitor();

                LogMonitorStartedSuccessfully(monitorDirectoryPath);
            }
            catch (Exception ex)
            {
                LogErrorStartingMonitor(ex);
                throw;
            }

            return Task.CompletedTask;

            void DoInitialBackup()
            {
                Directory.CreateDirectory(_backupsDirectory.PathLocal);

                LogPerformingInitialBackup(monitorDirectoryPath);
                BackupDirectory(_monitorDirectory);
            }

            void StartCharacterMonitor()
            {
                _fsWatcher = new FileSystemWatcher(_charDirectory.PathLocal)
                {
                    NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };

                _fsWatcher.Created += OnDirectoryCreated;
                _fsWatcher.Changed += OnDirectoryChanged;
                _fsWatcher.Deleted += OnDirectoryDeleted;
                _fsWatcher.Error += OnError;
            }
        }

        private void OnDirectoryCreated(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileName(e.FullPath) == MonitorDirectoryName)
            {
                LogTargetDirectoryCreated(e.FullPath);

                _saveId = _saveId.Next();
            }
        }

        private void OnDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileName(e.FullPath) == MonitorDirectoryName)
            {
                LogTargetDirectoryChanged(e.FullPath);

                BackupDirectory(_monitorDirectory);
            }
        }

        private void OnDirectoryDeleted(object sender, FileSystemEventArgs e)
        {
            if (Path.GetFileName(e.FullPath) == MonitorDirectoryName)
            {
                LogTargetDirectoryDeleted(e.FullPath);

                _fsWatcher.EnableRaisingEvents = false;
                RestoreLatestBackup(_monitorDirectory);
                _fsWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            LogFileSystemWatcherError(e.GetException());
        }

        private void BackupDirectory(GameDirectory directory)
        {
            var directoryPath = directory.PathLocal;

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    LogSourceDirectoryNotFound(directoryPath);
                    return;
                }

                var backupPath = _backupsDirectory.GetBackupOfSave(directory.Id, _saveId)?.PathLocal 
                    ?? _backupsDirectory.GetBackupLocalPathForSave(directory.Id, _saveId);

                LogCopyingToBackup(directoryPath, backupPath);
                CopyDirectory(directoryPath, backupPath);
                LogBackupSuccessful(directoryPath, backupPath);

                CleanupOldBackups();
            }
            catch (Exception ex)
            {
                LogErrorBackingUpDirectory(ex, directoryPath);
            }
        }

        private void RestoreLatestBackup(GameDirectory directory)
        {
            try
            {
                if (Directory.Exists(directory.PathLocal))
                {
                    LogRestoreNotRequired();
                    return;
                }

                if (!Directory.Exists(_backupsDirectory.PathLocal))
                {
                    LogBackupFolderNotFound();
                    return;
                }

                var latestBackup = _backupsDirectory.GetLatestBackupOf(directory.Id);

                if (latestBackup is null)
                {
                    LogNoBackupsFound();
                    return;
                }

                var backupPath = latestBackup.PathLocal;
                var targetPath = directory.PathLocal;

                LogRestoringLatestBackup(backupPath, targetPath);
                CopyDirectory(backupPath, targetPath);
                LogRestoreSuccessful(backupPath, targetPath);
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
                if (!Directory.Exists(_backupsDirectory.PathLocal))
                    return;

                var maxBackups = _backupOptions.MaxBackups;
                var backups = _backupsDirectory.GetAllBackupsOf(_monitorDirectory.Id)
                    .OrderByDescending(d => d.Created)
                    .ToList();

                if (maxBackups == int.MaxValue || backups.Count <= maxBackups)
                    return;

                var toDelete = backups.Skip(maxBackups).ToList();

                foreach (var backup in toDelete)
                {
                    var path = backup.PathLocal;

                    try
                    {
                        Directory.Delete(path, recursive: true);
                        LogDeletedOldBackup(path);
                    }
                    catch (Exception ex)
                    {
                        LogErrorDeletingOldBackup(ex, path);
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorCleaningupOldBackups(ex);
            }
        }

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

        [LoggerMessage(1, LogLevel.Information, "Starting character folder monitor for: {DirectoryPath}")]
        private partial void LogStartingMonitor(string directoryPath);

        [LoggerMessage(2, LogLevel.Error, "Character folder does not exist: {DirectoryPath}")]
        private partial void LogCharacterFolderNotFound(string directoryPath);

        [LoggerMessage(3, LogLevel.Information, "Performing initial backup: '{TargetDirectoryPath}'")]
        private partial void LogPerformingInitialBackup(string targetDirectoryPath);

        [LoggerMessage(4, LogLevel.Information, "Character folder monitor started successfully. Watching '{TargetDirectoryPath}' directory")]
        private partial void LogMonitorStartedSuccessfully(string targetDirectoryPath);

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

        [LoggerMessage(21, LogLevel.Information, "Successfully restored from backup: {BackupPath} to: {TargetPath}")]
        private partial void LogRestoreSuccessful(string backupPath, string targetPath);

        [LoggerMessage(22, LogLevel.Error, "Error restoring latest backup")]
        private partial void LogErrorRestoringLatestBackup(Exception ex);

        [LoggerMessage(23, LogLevel.Information, "Stopping character folder monitor")]
        private partial void LogStoppingMonitor();

        [LoggerMessage(24, LogLevel.Debug, "Save file already exists. Assuming it is the latest one. Restore not required.")]
        private partial void LogRestoreNotRequired();
    }
}
