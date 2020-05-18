using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Pde.CodingExercise.RandomNumberGenerator;
using RandomNumberService.RandomNumberDatabase;

namespace RandomNumberService.Generator
{
    public sealed class RandomNumberGenerator : IRandomNumberGenerator, IDisposable
    {
       public RandomNumberGenerator()
        {
            _lockObject = new object();

            //a timer with an interval of 5 seconds
            _timer = new Timer(TriggerRetries, null, 5000, Timeout.Infinite);
         
            NumberGenerator = new NumberGenerator();
            NumberGenerator.NumberGenerated += NumberGenerator_NumberGenerated;

            ReadConfigurationValues();
        }

        //useful for testing and injecting a number generator
        public RandomNumberGenerator(NumberGenerator numberGenerator)
        {
            NumberGenerator = numberGenerator;
            NumberGenerator.NumberGenerated += NumberGenerator_NumberGenerated;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(RandomNumberGenerator));

        private readonly object _lockObject;
        private readonly Timer _timer;
        
        private NumberGenerator NumberGenerator { get; }

        private string NotificationEndpoint { get; set; }
       
        private int ValueNotificationThreshold { get; set; }

        private void ReadConfigurationValues()
        {
            try
            {
                NotificationEndpoint = ConfigurationManager.AppSettings["LargeRandomNumberNotificationEndpoint"];

                var parsed = int.TryParse(ConfigurationManager.AppSettings["ValueNotificationThreshhold"], out var valueNotificationThreshold);
                ValueNotificationThreshold = parsed ? valueNotificationThreshold : 800;
            }
            catch(Exception ex)
            {
                Log.Error("Failure to read configuration values", ex);
                throw;
            }
        }

        //no need to make this async since this is a simple console app and not a ui that needs to be responsive
        public void StartGeneratingRandomNumbers()
        {
            CleanDatabaseForNewRun();
            NumberGenerator.Start();
        }

        //no need to make this async since this is a simple console app and not a ui that needs to be responsive
        //dispose is not called in the stop method so that you have the flexibility to restart the service
        public void StopGeneratingRandomNumbers()
        {
            NumberGenerator.Stop();
        }

        private void CleanDatabaseForNewRun()
        {
            Log.Info("CleanDatabaseForNewRun");
            
            try
            {
                using (var numberRepository = new RandomNumberRepository())
                {
                    numberRepository.CleanDatabase();
                    numberRepository.Save();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failure to delete all random number values in database", ex);
                throw;
            }
        }

        private void NumberGenerator_NumberGenerated(object sender, NumberGeneratedEventArgs e)
        {
            Log.Info($"Number generated: {e.Number} ");
            
            lock(_lockObject)
            {
                var randonNumber = new RandomNumber
                {
                    Value = e.Number,
                    GeneratedTime = DateTime.UtcNow
                };

                randonNumber.ServiceCallRetryRequired = !CallServiceForLargeNumberNotification(randonNumber).Result;

                //instead of saving everything in one batch I am saving per item in case an error interrupts the process and we have a record of what was already generated
                using (var numberRepository = new RandomNumberRepository())
                {
                    numberRepository.AddRandomNumber(randonNumber);
                    numberRepository.Save();
                }
            }
        }

        //all the methods in the call stack are synchronous so the ui will still be synchronous, this is just asychronous because of the post method on http client.
        private async Task<bool> CallServiceForLargeNumberNotification(RandomNumber randomNumber)
        {
            if (randomNumber.Value < ValueNotificationThreshold)
            {
                return true;
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var randomNumberMessage = new RandomNumberRequest()
                    {
                        number = randomNumber.Value
                    };

                    var jsonString = JsonConvert.SerializeObject(randomNumberMessage);
                    var httpResponse = await httpClient.PostAsync(NotificationEndpoint, new StringContent(jsonString, Encoding.UTF8, "application/json"));
                    httpResponse.EnsureSuccessStatusCode();
                    return true;
                }
            }
            catch(Exception  ex)
            {
                Log.Error($"Failed to call Random Number Notification Service for value of {randomNumber.Value}", ex);
                return false;
            }
        }

        //the retry logic for failed service calls
        //this will be triggered by a timer and run on a background thread pool thread.
        private void TriggerRetries(object state)
        {
            Task.Run(RetryNotifications);
        }

        private void RetryNotifications()
        {
            using (var numberRepository = new RandomNumberRepository())
            {
                var randomNumbersRequiringNotification = numberRepository.GetRandomNumbersRequiringNotification();
                var numbersRequiringNotification = randomNumbersRequiringNotification.ToList();

                if (numbersRequiringNotification.Any())
                {
                    foreach (var randomNumber in numbersRequiringNotification)
                    {
                        var success = CallServiceForLargeNumberNotification(randomNumber).Result;

                        if (success)
                        {
                            randomNumber.ServiceCallRetryRequired = false;
                            numberRepository.UpdateRandomNumber(randomNumber);
                        }
                    }
                }

                //here we can do it in  one batch instead of saving each number as it comes in
                numberRepository.Save();
            }
        }

        public void Dispose()
        {
            //a finalizer makes no sense here since the object will be garbage collected anyhow since it is a managed resource
            NumberGenerator.NumberGenerated -= NumberGenerator_NumberGenerated;
            _timer.Dispose();
        }
    }
}
