using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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