using chargebook.data;
using NUnit.Framework;

namespace UnitTests {
    public class Tests {
        [Test]
        public void persistorTestWithNoRun() {
            int runCounter = 0;
            using (var persistor = new ManagerPersistor(() => runCounter++)) {
                persistor.Dispose();
            }
            Assert.AreEqual(0, runCounter);
        }
    }
}