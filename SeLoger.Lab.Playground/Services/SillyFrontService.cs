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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using SeLoger.Lab.Playground.Models;

namespace SeLoger.Lab.Playground.Services
{
    public class SillyFrontService : ISillyFrontService
    {
        private readonly List<SillyDudeModel> _repository;

        private readonly SuperShittyHttpClient _httpClient;

        private int lastPage = -1;

        public SillyFrontService()
        {
            _httpClient = new SuperShittyHttpClient(true);

            var source = new Func<int, SillyDudeModel>[] { CreateJCVD, CreateKnightsOfNi, CreateLouisCK, CreateWillFerrell };
            var pseudoRandomGenerator = new Random();
            
            _repository = new List<SillyDudeModel>(200);
            for (int i = 200; i > 0; i--)
            {
                _repository.Add(source[pseudoRandomGenerator.Next(0, 4)](i));
            }
        }

        private void SlightlyChangeFirstPage()
        {
            var source = new Func<int, SillyDudeModel>[] { CreateJCVD, CreateKnightsOfNi, CreateLouisCK, CreateWillFerrell };
            var pseudoRandomGenerator = new Random();
            int id = _repository.Max(model => model.Id);

            _repository.Insert(0, source[pseudoRandomGenerator.Next(0, 4)](++id));
            _repository.Insert(0, source[pseudoRandomGenerator.Next(0, 4)](++id));

            _repository.RemoveAt(4);

            _repository.RemoveAt(6);
            _repository.Insert(6, source[pseudoRandomGenerator.Next(0, 4)](++id));
        }

        public async Task<IReadOnlyList<SillyDudeModel>> GetAllSillyPeople()
        {
            await _httpClient.ShittyGetStuff();

            return new List<SillyDudeModel>(_repository);
        }

        public async Task<PageResult<SillyDudeModel>> GetSillyPeoplePage(int pageNumber, int pageSize)
        {
            Debug.Assert(pageNumber > 0);
            Debug.Assert(pageSize >= 10);

            if (pageNumber == 1 && lastPage >= 1)
            {
                // Refresh
                SlightlyChangeFirstPage();
            }

            if (lastPage == -1)
            {
                lastPage = 0;

                // First call
                throw new UnexpectedException("Oops");
            }

            await _httpClient.ShittyGetStuff();

            lastPage = pageNumber;

            return new PageResult<SillyDudeModel>(
                _repository.Count, 
                _repository.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
        }

        public async Task<SillyDudeModel> GetSilly(int id)
        {
            await _httpClient.ShittyGetStuff();

            return _repository.Single(model => model.Id == id);
        }

        private SillyDudeModel CreateLouisCK(int id)
        {
            return new SillyDudeModel(
                id,
                "Louis C.K.",
                "Comedian",
                "There are people that really live by doing the right thing, but I don't know what that is, I'm really curious about that. I'm really curious about what people think they're doing when they're doing something evil, casually. I think it's really interesting, that we benefit from suffering so much, and we excuse ourselves from it.",
#if LOCAL_DATA
                "file:///android_asset/louis.jpg");
#else
                "http://www.brain-magazine.fr/m/posts/30019/originals/louis.jpg");
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
                "file:///android_asset/jcvd.jpg");
#else
                "http://www.vandamme.ru/photos/movies/the_order/the_order23.jpg");
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
                "file:///android_asset/knight_ni.gif");
#else
                "https://media0.giphy.com/media/ALBfFB6gP1evu/200_s.gif");
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
                "file:///android_asset/anchorman.jpg");
#else
                "http://media.salon.com/2013/12/anchorman1.jpg");
#endif
        }
    }
}