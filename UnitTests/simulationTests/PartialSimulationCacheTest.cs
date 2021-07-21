using System.Collections.Generic;
using System.Reflection;
using chargebook.data.simulation;
using ChargeBook.models.booking;
using chargebook.models.infrastructure;
using ChargeBook.models.simulation;
using NUnit.Framework;

namespace UnitTests {
    public class PartialSimulationCacheTest {

        private PartialSimulationCache partialSimulationCache;
        private Dictionary<string, Simulation> simulations;
        [SetUp]
        public void SetUp() {
            partialSimulationCache = new PartialSimulationCache(new Dictionary<string, ChargeStationType>());
            var simulationsField = partialSimulationCache.GetType().GetField("simulations",
                BindingFlags.NonPublic | BindingFlags.Instance);
            simulations = simulationsField.GetValue(partialSimulationCache) as Dictionary<string, Simulation>;
        }

        [Test]
        public void createSimulationTest() {
            partialSimulationCache.createSimulation("testMail","simulation",new Dictionary<string, int>());
            partialSimulationCache.createSimulation("testMail","simulation",new Dictionary<string, int>());
            Assert.AreEqual(1,simulations.Count);
            Assert.IsTrue(simulations.ContainsKey("testMail"));
        }

        [Test]
        public void deleteSimulationTest() {
            partialSimulationCache.createSimulation("testMail","simulation",new Dictionary<string, int>());
            partialSimulationCache.deleteSimulation("testMail");
            Assert.IsNull(simulations["testMail"]);
            
        }
    }
}