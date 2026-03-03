using H.Avalonia.Models.ClassMaps;
using H.Avalonia.ViewModels.Results;
using H.Avalonia.Infrastructure;
using H.Avalonia.Models;

namespace H.Avalonia.Test
{
    [TestClass]
    public class ExportDataTest
    {
        private static ExportHelpers _exportHelpers = null!;
        private static ClimateResultsViewModel _climateResultsViewModel = null!;
        private static ClimateResultsViewItemMap _climateResultsViewItemMap = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _exportHelpers = new ExportHelpers();
            _climateResultsViewModel = new ClimateResultsViewModel();
            _climateResultsViewItemMap = new ClimateResultsViewItemMap();


            var item = new ClimateViewItem()
            {
                Year = 1991,
                TotalPET = 510.10,
                TotalPPT = 505.05,
                MonthlyPPT = 123.23
            };
            _climateResultsViewModel.ClimateResultsViewItems.Add(item);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void ExportClimateData()
        {
            var directory = Directory.GetCurrentDirectory();
            var fileName = "file.csv";
            _exportHelpers.ExportPath = $"{directory}.\\{fileName}";
            _exportHelpers.ExportToCSV(_climateResultsViewModel.ClimateResultsViewItems, _climateResultsViewItemMap);
            Assert.IsTrue(Path.Exists(_exportHelpers.ExportPath));
        }
    }
}
