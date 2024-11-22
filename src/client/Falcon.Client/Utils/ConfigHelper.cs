using System.Xml.Linq;

namespace Falcon.Client.Utils
{
    public record Config
    {
        public string ConnectionString { set; get; }
    }

    public class ConfigHelper
    {
        public static string exampleServerIP = "https://localhost:61937/";
        public static string configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Falcon");
        public static string configFile = Path.Combine(configDirectory, "falcon.xml");

        public static Config GetConfig()
        {
            Config config = null;
            if (File.Exists(configFile))
            {
                var xml = XDocument.Load(configFile).Element("config");
                config = new Config
                {
                    ConnectionString = xml.Element("ServerIP").Value
                };
            }

            return config;
        }

        public static void CreateConfig()
        {
            var configFile = Path.Combine(configDirectory, "falcon.xml");
            if (!File.Exists(configFile))
            {
                Directory.CreateDirectory(configDirectory);
                var doc = new XDocument(
                    new XElement("config",
                        new XElement("ServerIP", exampleServerIP)
                    )
                );
                doc.Save(configFile);
            }
        }
    }
}
