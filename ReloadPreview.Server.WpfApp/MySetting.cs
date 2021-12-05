using ReloadPreview.Server.WpfApp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Setting
{
    public class MySettings
    {
        static string fileName = "Setting.json";
        public string Setting1 { get; set; }

        /// <summary>
        /// 需要保存的
        /// </summary>
        public  List<ReloadFileModel> ReloadFileModels { get; set; }

        public static void Save(MySettings setting)
        {
            string jsonString = JsonSerializer.Serialize(setting);
            File.WriteAllText(fileName, jsonString);
        }
        public static MySettings? Read()
        {
            if(!File.Exists(fileName))
                return null;
            var jsonString = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<MySettings>(jsonString);
        }
    }
}
