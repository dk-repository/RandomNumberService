using System;
using System.Collections.Generic;
using System.Text;

namespace RandomNumberService.Generator
{
    public interface IRandomNumberGenerator
    {
        void StartGeneratingRandomNumbers();

        void StopGeneratingRandomNumbers();
    }
}
