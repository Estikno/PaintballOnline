using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class WriteFile
{
    private static string interpolaor_path = Application.persistentDataPath + "/protocolStatistics.txt"; //%userprofile%\AppData\LocalLow\<companyname>\<productname>\protocolStatistics.txt
    private static string ping_path = Application.persistentDataPath + "/rtt_shooting.txt";

    public static void WriteInterpolatorData(string[] data)
    {
        using(StreamWriter streamWriter = new StreamWriter(interpolaor_path, true))
        {
            foreach(string item in data)
            {
                streamWriter.WriteLine(item);
            }
        }
    }

    public static void WriteShootLatencyData(string[] data)
    {
        using (StreamWriter streamWriter = new StreamWriter(ping_path, true))
        {
            foreach (string item in data)
            {
                streamWriter.WriteLine(item);
            }
        }
    }
}
