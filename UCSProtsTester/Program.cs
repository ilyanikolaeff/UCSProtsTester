using NLog;
using OPCWrapper.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UCSProtsTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var settings = GetSettings();
            var opcClient = GetOpcDaClient(settings);

            var tester = new Tester(opcClient, settings, new DataRepository(new ExportService()));
            await tester.RunTest();
            Console.ReadKey();
        }

        static Settings GetSettings()
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(Settings));
                using (var streamReader = new StreamReader("Settings.xml"))
                {
                    return (Settings)xmlSerializer.Deserialize(streamReader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(0);
                return null;
            }
        }

        static OpcDaClient GetOpcDaClient(Settings settings)
        {
            var opcDaClient = new OpcDaClient(new OPCWrapper.ConnectionSettings(settings.ServerAddress, settings.ServerName), "SivKox");
            opcDaClient.RegisterLogger(LogManager.GetCurrentClassLogger());
            if (opcDaClient.Connect())
                return opcDaClient;
            else
            {
                Environment.Exit(0);
                return null;
            }
        }
    }
}
