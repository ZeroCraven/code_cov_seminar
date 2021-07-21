using System;
using System.Collections.Generic;
using chargebook.data.infrastructure;
using chargebook.models;
using chargebook.models.infrastructure;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NUnit.Framework;

namespace UnitTests.infrastructure {
    public class InfrastructureManagerTests {
        private Dictionary<string, ChargeStationType> possibleChargeStationTypes;
        private Dictionary<string, Infrastructure> testLocations;
        private InfrastructureManager infrastructureManager;

        [SetUp]
        public void setUp() {
            possibleChargeStationTypes = new Dictionary<string, ChargeStationType>() {
                {
                    "EON1", new ChargeStationType() {
                        manufacturer = "EON",
                        modelName = "EON1",
                        maxChargePower = 1000,
                        chargePoints = {
                            new ChargePoint() {
                                connectors = new Dictionary<ConnectorType, double>() {
                                    {
                                        ConnectorType.CCS, 1000
                                    }
                                }
                            }
                        }
                    }
                },
            };

            testLocations = new Dictionary<string, Infrastructure>() {
                {
                    "Augsburg", new Infrastructure(TimeZoneInfo.Local)
                }
            };

            infrastructureManager = new InfrastructureManager(possibleChargeStationTypes, testLocations);
        }

        [Test]
        public void testPossibleChargeStationTypes() {
            Assert.AreEqual(possibleChargeStationTypes, infrastructureManager.possibleChargeStationTypes);
        }

        [Test]
        public void testGetInfrastructureByLocation() {
            Assert.AreEqual(testLocations["Augsburg"], infrastructureManager.getInfrastructureByLocation("Augsburg"));
        }

        [Test]
        public void testGetLocationNames() {
            Assert.AreEqual(new List<string>() {
                "Augsburg"
            }, infrastructureManager.getLocationNames());
        }

        [Test]
        public void testDeleteLocation() {
            infrastructureManager.deleteLocation("Augsburg");
            Assert.Throws<InfrastructurePartNotFoundException>(delegate {
                infrastructureManager.getInfrastructureByLocation("Augsburg");
            });
        }

        [Test]
        public void testCreateLocationPositive() {
            infrastructureManager.createLocation("München", TimeZoneInfo.Local);
            //should not throw exception
            Assert.DoesNotThrow(
                delegate {
                    infrastructureManager.getInfrastructureByLocation("München");
                }
            );
        }

        #region testCreateChargeGroup
        [Test]
        public void testCreateChargeGroupPositive() {
            infrastructureManager.createChargeGroup("Augsburg", "A", 10000);
            Assert.AreEqual(10000, infrastructureManager.getInfrastructureByLocation("Augsburg").chargeGroups["A"].maxChargePower);
        }

        [Test]
        public void testCreateChargeGroupWithoutExistingLocation() {
            Assert.Throws<InfrastructurePartNotFoundException>(() => infrastructureManager.createChargeGroup("existiert nicht", "A", 1000));
        }

        [Test]
        public void testCreateChargeGroupWithDuplicateChargeGroupNames() {
            infrastructureManager.createChargeGroup("Augsburg", "A", 1000);
            Assert.Throws<InfrastructurePartAlreadyExistsException>(() => infrastructureManager.createChargeGroup("Augsburg", "A", 1000));
        }
        #endregion

        [Test]
        public void testDeleteChargeGroup() {
            infrastructureManager.createChargeGroup("Augsburg", "A", 10000);
            infrastructureManager.deleteChargeGroup("Augsburg", "A");
            Assert.Throws<KeyNotFoundException>(delegate {
                var test = infrastructureManager.getInfrastructureByLocation("Augsburg").chargeGroups["A"];
            });
        }

        #region testCreateChargeStation
        [Test]
        public void testCreateChargeStationPositive() {
            infrastructureManager.createChargeGroup("Augsburg", "A", 10000);
            infrastructureManager.createChargeStation("Augsburg", "A", "1", "EON1");
            Assert.AreEqual(infrastructureManager.possibleChargeStationTypes["EON1"],
                infrastructureManager.getInfrastructureByLocation("Augsburg").chargeGroups["A"].chargeStations["1"]);
        }
        [Test]
        public void testCreateChargeStationWithNotExistingLocation() {
            Assert.Throws<InfrastructurePartNotFoundException>(() => {
                infrastructureManager.createChargeStation("existiert nicht", "A", "1","EON1");
            });
        }
        [Test]
        public void testCreateChargeStationWithNotExistingChargeGroup() {
            infrastructureManager.createChargeGroup("Augsburg", "A", 10000);
            Assert.Throws<InfrastructurePartNotFoundException>(() => {
                infrastructureManager.createChargeStation("Augsburg", "existiert nicht", "1","EON1");
            });
        }
        [Test]
        public void testCreateChargeStationWithDuplicateChargeStationName() {
            infrastructureManager.createChargeGroup("Augsburg", "A", 10000);
            infrastructureManager.createChargeStation("Augsburg", "A", "1", "EON1");
            Assert.Throws<InfrastructurePartAlreadyExistsException>(() => {
                infrastructureManager.createChargeStation("Augsburg", "A", "1", "EON1");
            });
        }
        #endregion

        [Test]
        public void testDeleteCreateChargeStation() {
            infrastructureManager.createChargeGroup("Augsburg", "A", 10000);
            infrastructureManager.createChargeStation("Augsburg", "A", "1", "EON1");
            infrastructureManager.deleteChargeStation("Augsburg", "A", "1");
            Assert.Throws<KeyNotFoundException>(delegate {
                var test = infrastructureManager.getInfrastructureByLocation("Augsburg").chargeGroups["A"].chargeStations["1"];
            });
        }

    }
}