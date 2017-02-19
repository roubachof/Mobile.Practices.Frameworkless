using System;
using System.Threading.Tasks;

namespace SeLoger.Lab.Playground.Services
{
    public class SuperShittyHttpClient
    {
        private readonly bool _isRandomlyFailing;

        public SuperShittyHttpClient(bool randomFails)
        {
            _isRandomlyFailing = randomFails;
        }

        public async Task ShittyGetStuff()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            Random pseudoRandomGenerator = new Random();
            int generatedNumber = pseudoRandomGenerator.Next(1, 5);

            if (_isRandomlyFailing && generatedNumber == 3)
            {
                throw new UnexpectedException("Oh boy what this shit ?");
            }

            if (_isRandomlyFailing && generatedNumber == 4)
            {
                throw new CommunicationException("Oh man cannot reach these lazy servers !");
            }
        }
    }
}