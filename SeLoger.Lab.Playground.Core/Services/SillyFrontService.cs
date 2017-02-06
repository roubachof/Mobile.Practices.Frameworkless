// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SillyFrontService.cs" company="The Silly Company">
//   The Silly Company 2016. All rights reserved.
// </copyright>
// <summary>
//   The SillyFrontService interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Conditions;

using SeLoger.Lab.Playground.Core.Models;

namespace SeLoger.Lab.Playground.Core.Services
{
    public class UnexpectedException : Exception
    {
        public UnexpectedException(string message)
            : base(message)
        {
        }
    }

    public class CommunicationException : Exception
    {
        public CommunicationException(string message)
            : base(message)
        {
        }
    }

    public interface ISillyFrontService
    {
        Task<IReadOnlyList<SillyDudeModel>> GetAllSillyPeople();

        Task<PageResult<SillyDudeModel>> GetSillyPeoplePage(int pageNumber, int pageSize);

        Task<SillyDudeModel> GetSilly(int id);
    }

    public struct PageResult<TItem>
    {
        public readonly int TotalCount;

        public readonly IReadOnlyList<TItem> Items;

        public PageResult(int totalCount, IReadOnlyList<TItem> items)
        {
            TotalCount = totalCount;
            Items = items;
        }
    }

    public class SillyFrontService : ISillyFrontService
    {
        private readonly Dictionary<int, SillyDudeModel> _repository;

        private readonly SuperShittyHttpClient _httpClient;

        public SillyFrontService()
        {
            _httpClient = new SuperShittyHttpClient(false);

            var source = new Func<int, SillyDudeModel>[] { CreateJCVD, CreateKnightsOfNi, CreateLouisCK, CreateWillFerrell };
            var pseudoRandomGenerator = new Random();
            
            _repository = new Dictionary<int, SillyDudeModel>(200);
            for (int i = 0; i < 200; i++)
            {
                _repository.Add(i, source[pseudoRandomGenerator.Next(0, 4)](i));
            }
        }

        private SillyDudeModel CreateLouisCK(int id)
        {
            return new SillyDudeModel(
                id,
                "Louis C.K.",
                "Comedian",
                "There are people that really live by doing the right thing, but I don't know what that is, I'm really curious about that. I'm really curious about what people think they're doing when they're doing something evil, casually. I think it's really interesting, that we benefit from suffering so much, and we excuse ourselves from it.",
#if LOCAL_DATA
                "louis_ck.jpg")
#else
                "http://pixel.nymag.com/imgs/daily/vulture/2016/04/21/21-louis-ck.w529.h529.jpg");
#endif
        }

        private SillyDudeModel CreateJCVD(int id)
        {
            return new SillyDudeModel(
                id,
                "Jean-Claude",
                "Actor",
                "J’adore les cacahuètes. Tu bois une bière et tu en as marre du goût. Alors tu manges des cacahuètes. Les cacahuètes, c’est doux et salé, fort et tendre, comme une femme. Manger des cacahuètes. It’s a really strong feeling. Et après tu as de nouveau envie de boire une bière. Les cacahuètes, c’est le mouvement perpétuel à la portée de l’homme.",
#if LOCAL_DATA
                "jean_claude_van_damme.jpg")
#else
                "http://www.cultivonsnous.fr/images/stories/jean-claude-van-damme.jpg");
#endif
        }

        private SillyDudeModel CreateKnightsOfNi(int id)
        {
            return new SillyDudeModel(
                id,
                "Knights of Ni",
                "Knights",
                "Keepers of the sacred words 'Ni', 'Peng', and 'Neee-Wom'",
#if LOCAL_DATA
                "knights_of_ni.jpg")
#else
                "http://www.geekalerts.com/u/Knights-of-Ni-Plush-Hat.jpg");
#endif
        }

        private SillyDudeModel CreateWillFerrell(int id)
        {
            return new SillyDudeModel(
                id,
                "Will Ferrel",
                "Actor",
                "Hey. They laughed at Louis Armstrong when he said he was gonna go to the moon. Now he’s up there, laughing at them.",
#if LOCAL_DATA
                "will_ferrell.jpg")
#else
                "https://iwonderandwander.files.wordpress.com/2013/03/ferrell-twitter-photo.jpg?w=660");
#endif
        }

        public async Task<IReadOnlyList<SillyDudeModel>> GetAllSillyPeople()
        {
            await _httpClient.ShittyGetStuff();

            return new List<SillyDudeModel>(_repository.Values);
        }

        public async Task<PageResult<SillyDudeModel>> GetSillyPeoplePage(int pageNumber, int pageSize)
        {
            pageNumber.Requires().IsGreaterThan(0);
            pageSize.Requires().IsGreaterOrEqual(10);

            await _httpClient.ShittyGetStuff();

            return new PageResult<SillyDudeModel>(
                _repository.Count, 
                _repository.Values.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
        }

        public async Task<SillyDudeModel> GetSilly(int id)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            return _repository[id];
        }

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

                if (_isRandomlyFailing && DateTime.UtcNow.Second % 3 == 0)
                {
                    throw new UnexpectedException("Oh boy what this shit ?");
                }

                if (_isRandomlyFailing && DateTime.UtcNow.Second % 2 == 0)
                {
                    throw new CommunicationException("Oh man cannot reach these lazy servers !");
                }
            }
        }
    }
}