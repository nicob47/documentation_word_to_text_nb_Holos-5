#region Imports

using H.CLI.Handlers;

#endregion

namespace H.CLI.Test.Handlers
{
    [TestClass]
    public class ExportedFarmsHandlerTest
    {
        #region Fields

        private ExportedFarmsHandler _handler = null!;

        #endregion

        #region Initialization

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _handler = new ExportedFarmsHandler();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        #endregion

        #region Tests

        [TestMethod]
        [Ignore]
        public void PromptUserForLocationOfExportedFarms()
        {
            _handler.PromptUserForLocationOfExportedFarms("");
        }

        #endregion
    }
}
