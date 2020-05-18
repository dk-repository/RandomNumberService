using RandomNumberService.RandomNumberDatabase;
using RandomNumberService.Generator;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using log4net;
using log4net.Config;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace RandomNumberService.RandomNumberConsole
{
    //any exception will show up on the console and bring down the service, this is expected behavior and all errors will be logged
    public static class RandomNumberConsole
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(typeof(RandomNumberConsole));
        
        public static void Main(string[] args)
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("Log4Net.config"));

            var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

            Console.WriteLine("Press any key to start generating random numbers.");
            Console.ReadKey();

            Console.WriteLine();
            Console.WriteLine("Starting the Random Number Service.");

            using var randomNumberGenerator = new RandomNumberGenerator();
            RunRandomNumberService(randomNumberGenerator);

            var input = Console.In.ReadLine();

            randomNumberGenerator.StopGeneratingRandomNumbers();
            randomNumberGenerator.Dispose();

            Console.WriteLine("Here is a list of the generated random numbers.");

            OutputAllRandomNumbers();
        }

        private static void RunRandomNumberService(RandomNumberGenerator randomNumberGenerator)
        {
            try
            {
                randomNumberGenerator.StartGeneratingRandomNumbers();
                Console.WriteLine("Random Number Generator started press any key and then ENTER to stop the service.");
            }
            catch(Exception ex)
            {
                Console.WriteLine("An error occured.");
                Console.WriteLine();

                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                Console.WriteLine();
                OutputAllRandomNumbers();
            }
        }

        private static void OutputAllRandomNumbers()
        {
            using (var numberRepository = new RandomNumberRepository())
            {
                var allRandomNumbers = numberRepository.GetRandomNumbers().ToList();

                foreach (var randomNumber in allRandomNumbers)
                {
                    Console.WriteLine($"Value generated was {randomNumber.Value,4} " +
                                      $"and the generated time was {randomNumber.GeneratedTime.ToLongDateString()} " +
                                      $"and a service call is still required: {randomNumber.ServiceCallRetryRequired}.");
                }
            }
        }
    }
}
