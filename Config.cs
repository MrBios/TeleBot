using grdlic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TeleBot
{
    public static class Config
    {
        private static Dictionary<string, string> config;
        private static Dictionary<string, string> secret;

        static Config()
        {
            config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("config.json"))!;

            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TeleBot.secret-config.json")!);
            secret = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd())!;
        }

        public static int api_id => Convert.ToInt32(secret["api_id"]);
        public static string api_hash => secret["api_hash"];
    }
}
