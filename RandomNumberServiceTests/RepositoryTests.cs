using Microsoft.VisualStudio.TestTools.UnitTesting;
using RandomNumberService.RandomNumberDatabase;
using System;
using System.Linq;

namespace RandomNumberServiceTests
{
    [TestClass]
    public class RepositoryTests
    {
        [TestInitialize]
        public void Setup()
        {
            RandomNumberRepository = new RandomNumberRepository();
            RandomNumberRepository.CleanDatabase();
            RandomNumberRepository.Save();
        }

        private IRandomNumberRepository RandomNumberRepository { get; set; }

        [TestMethod]
        public void TestAddRandomNumberViaRepository()
        {
            var testDateTime = DateTime.UtcNow;

            var randomNumber = new RandomNumber
            {
                GeneratedTime = testDateTime,
                Value = 99,
                ServiceCallRetryRequired = false
            };

            RandomNumberRepository.AddRandomNumber(randomNumber);
            RandomNumberRepository.Save();

            var randomNumbers = RandomNumberRepository.GetRandomNumbers();

            var actualRandomNumber = randomNumbers.SingleOrDefault(rn => rn.GeneratedTime == testDateTime);
            Assert.IsTrue(actualRandomNumber.Value == 99);
        }

        [TestMethod]
        public void TestUpdateandomNumberViaRepositoryAfterSuccessfulNotification()
        {
            var testDateTime = DateTime.UtcNow;

            var randomNumber = new RandomNumber
            {
                GeneratedTime = testDateTime,
                Value = 899,
                ServiceCallRetryRequired = true
            };

            RandomNumberRepository.AddRandomNumber(randomNumber);
            RandomNumberRepository.Save();

            var randomNumbers = RandomNumberRepository.GetRandomNumbers();
            var actualRandomNumber = randomNumbers.SingleOrDefault(rn => rn.GeneratedTime == testDateTime);
            Assert.IsTrue(actualRandomNumber.Value == 899);

            //now update this value
            actualRandomNumber.ServiceCallRetryRequired = false;
            RandomNumberRepository.UpdateRandomNumber(actualRandomNumber);
            RandomNumberRepository.Save();

            randomNumbers = RandomNumberRepository.GetRandomNumbers();
            var updatedRandomNumber = randomNumbers.SingleOrDefault(rn => rn.GeneratedTime == testDateTime);
            Assert.IsTrue(actualRandomNumber.ServiceCallRetryRequired == false);
        }

        [TestMethod]
        public void TestCleanDatabaseViaRepository()
        {
            var randomNumberOne = new RandomNumber
            {
                GeneratedTime = DateTime.UtcNow,
                Value = 1,
                ServiceCallRetryRequired = false
            };

            var randomNumberTwo = new RandomNumber
            {
                GeneratedTime = DateTime.UtcNow,
                Value = 2,
                ServiceCallRetryRequired = false
            };

            var randomNumberThree = new RandomNumber
            {
                GeneratedTime = DateTime.UtcNow,
                Value = 3,
                ServiceCallRetryRequired = false
            };

            RandomNumberRepository.AddRandomNumber(randomNumberOne);
            RandomNumberRepository.AddRandomNumber(randomNumberTwo);
            RandomNumberRepository.AddRandomNumber(randomNumberThree);
            RandomNumberRepository.Save();

            var randomNumbersExist = RandomNumberRepository.GetRandomNumbers();
            Assert.IsTrue(randomNumbersExist.Count() >= 3);

            RandomNumberRepository.CleanDatabase();
            RandomNumberRepository.Save();

            var currentRandomNumbers = RandomNumberRepository.GetRandomNumbers();
            Assert.IsTrue(!currentRandomNumbers.Any());
        }

        [TestMethod]
        public void TestGetRandomNumbersRequiringNotificationViaRepository()
        {
            RandomNumberRepository.CleanDatabase();
            RandomNumberRepository.Save();

            var randomNumberOne = new RandomNumber
            {
                GeneratedTime = DateTime.UtcNow,
                Value = 1,
                ServiceCallRetryRequired = false
            };

            var randomNumberTwo = new RandomNumber
            {
                GeneratedTime = DateTime.UtcNow,
                Value = 801,
                ServiceCallRetryRequired = true
            };

            var randomNumberThree = new RandomNumber
            {
                GeneratedTime = DateTime.UtcNow,
                Value = 3,
                ServiceCallRetryRequired = false
            };

            var randomNumberFour = new RandomNumber
            {
                GeneratedTime = DateTime.UtcNow,
                Value = 804,
                ServiceCallRetryRequired = true
            };

            RandomNumberRepository.AddRandomNumber(randomNumberOne);
            RandomNumberRepository.AddRandomNumber(randomNumberTwo);
            RandomNumberRepository.AddRandomNumber(randomNumberThree);
            RandomNumberRepository.AddRandomNumber(randomNumberFour);
            RandomNumberRepository.Save();

            var randomNumbersRequiringNotification = RandomNumberRepository.GetRandomNumbersRequiringNotification();
            Assert.IsTrue(randomNumbersRequiringNotification.Count() == 2);
        }

        [TestCleanup]
        public void Cleanup()
        {
            var disposable = (IDisposable)RandomNumberRepository;
            disposable.Dispose();
        }
    }
}
