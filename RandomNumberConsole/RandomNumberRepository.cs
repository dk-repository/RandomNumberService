using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomNumberService.RandomNumberDatabase
{
    public class RandomNumberRepository : IRandomNumberRepository, IDisposable
    {
        public RandomNumberRepository() : this(new RandomNumberDbContext())
        {
        }

        private RandomNumberRepository(RandomNumberDbContext context)
        {
            Context = context;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(RandomNumberRepository));

        private RandomNumberDbContext Context { get; }

        public IEnumerable<RandomNumber> GetRandomNumbers()
        {
            try
            {
                return Context.RandomNumbers.OrderBy(rn => rn.GeneratedTime);
            }
            catch(Exception ex)
            {
                Log.Error("Failure to read Random numbers from DbContext", ex);
                throw;
            }
        }

        public IEnumerable<RandomNumber> GetRandomNumbersRequiringNotification()
        {
            try
            {
                var randomNumbers = Context.RandomNumbers.Where(rn => rn.ServiceCallRetryRequired == true).ToList<RandomNumber>();
                return randomNumbers;
            }
            catch (Exception ex)
            {
                Log.Error("Failure to read Random numbers requiring notification from DbContext", ex);
                throw;
            }
        }

        public void AddRandomNumber(RandomNumber randomNumber)
        {
            try
            {
                Context.RandomNumbers.Add(randomNumber);
            }
            catch (Exception ex)
            {
                Log.Error($"Failure to insert random number value {randomNumber.Value} in DbContext", ex);
                throw;
            }
        }

        public void UpdateRandomNumber(RandomNumber randomNumber)
        {
            try
            {
                Context.Entry(randomNumber).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                Log.Error("Failure to update random number in DbContext", ex);
                throw;
            }
        }

        public void CleanDatabase()
        {
            try
            {
                var randomNumbers = Context.RandomNumbers.OrderBy(rn => rn.RandomNumberId);

                foreach(var randomNumber in randomNumbers)
                {
                    Context.Remove(randomNumber);
                }

                Context.SaveChanges();
            }
            catch(Exception ex)
            {
                Log.Error("Failure to delete all random numbers in DbContext", ex);
                throw;
            }
        }

        public void Save()
        {
            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error($"Failure to update DbContext", ex);
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            Context.Dispose();

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }
        
        ~RandomNumberRepository()
        {
             Dispose(false);
        }
    }
}
