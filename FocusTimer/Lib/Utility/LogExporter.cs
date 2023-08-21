using System.IO.Compression;

namespace FocusTimer.Lib.Utility;

internal class LogExporter
{
    public void ExportLogs()
    {
            
    }

    public string UploadLogsAndGetTicket()
    {
        ZipFile.CreateFromDirectory("logs", "logs.zip");



        return "";
    }
}