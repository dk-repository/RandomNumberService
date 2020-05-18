using System;
using System.Collections.Generic;
using System.Text;

namespace RandomNumberService.RandomNumberDatabase
{
    public interface IRandomNumberRepository
    {
        IEnumerable<RandomNumber> GetRandomNumbers();

        IEnumerable<RandomNumber> GetRandomNumbersRequiringNotification();

        void AddRandomNumber(RandomNumber randomNumber);

        void UpdateRandomNumber(RandomNumber randomNumber);

        void CleanDatabase();

        void Save();
    }
}
