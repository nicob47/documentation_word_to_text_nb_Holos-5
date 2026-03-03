#region Imports

using H.Core.Providers.Feed;

#endregion

namespace H.Core.Test.Providers.Feed
{
    [TestClass]
    public class DietProviderTest
    {
        #region Fields

        private DietProvider _provider = null!;

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
            _provider = new DietProvider();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        #endregion

        #region Tests


        #endregion
    }
}
