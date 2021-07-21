using System.Collections.Generic;
using System.Linq;

namespace ChargeBook.models.simulation.settings {
    public class SimulationSettings {
        public List<BookingGenerationSetting> bookingGenerationSettings { get; set; }
        public GeneralSettings generalSettings { get; set; }

        public SimulationSettings() {
            bookingGenerationSettings = new List<BookingGenerationSetting>();
        }

        public SimulationSettings deepCopy() {
            return new SimulationSettings() {
                generalSettings = generalSettings.deepCopy()
                , bookingGenerationSettings = bookingGenerationSettings.Select(x => x.deepCopy()).ToList(),
            };
        }

    }
}