using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using NutriMind.Runtime.App;
using NutriMind.Runtime.App.Dto;
using UnityEngine;

namespace NutriMind.Tests.EditMode.App
{
    [TestFixture]
    public class QuizListCatalogLoaderTests
    {
        private const string Lrn = "000000000001";
        private const string Pin = "1234";

        private static string FixturePath =>
            Path.Combine(Application.dataPath, "_Project", "Nutrimind", "Resources", "DemoData", "full-demo-student-data.json");

        private LocalDemoJsonProvider NewLoggedIn()
        {
            Assert.That(File.Exists(FixturePath), Is.True, "Demo fixture missing at " + FixturePath);
            var provider = new LocalDemoJsonProvider(File.ReadAllText(FixturePath));
            var login = provider.LoginAsync(new LoginRequestDto { Lrn = Lrn, Pin = Pin }).Result;
            Assert.That(login.Success, Is.True, login.ErrorMessage);
            return provider;
        }

        [Test]
        public async Task LoadAllAsync_MergesAssignedQuizzesWithoutDuplicates()
        {
            var provider = NewLoggedIn();
            var store = new QuizAvailabilityStore();

            var result = await QuizListCatalogLoader.LoadAllAsync(provider, store);
            Assert.That(result.Success, Is.True, result.ErrorMessage);
            Assert.That(store.IsLoaded, Is.True);
            Assert.That(store.Quizzes, Is.Not.Empty);

            var ids = store.Quizzes.Select(q => q.Id).ToList();
            Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Count), "Catalog merge must dedupe by quiz id");
            Assert.That(store.Quizzes.Any(q => q.SubjectSlug == "literaquest"), Is.True);
            Assert.That(store.Quizzes.Any(q => q.SubjectSlug == "healthquest"), Is.True);
        }

        [Test]
        public async Task LoadAllAsync_SciencePreviewEmpty_DoesNotFailCatalog()
        {
            var provider = NewLoggedIn();
            var store = new QuizAvailabilityStore();

            var result = await QuizListCatalogLoader.LoadAllAsync(provider, store);
            Assert.That(result.Success, Is.True, result.ErrorMessage);

            var science = store.Quizzes.Where(q => q.SubjectSlug == "sciencequest").ToList();
            Assert.That(science, Is.Empty, "Science preview fixture should contribute zero quizzes without failing load");
        }
    }
}
