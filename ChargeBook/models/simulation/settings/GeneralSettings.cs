using System;
using ChargeBook.models.booking;

namespace ChargeBook.models.simulation.settings {
    public class GeneralSettings {
        public string name { get; set; }
        public TimePeriod timePeriod { get; set; }
        public int tickLength { get; set; }

        public int seed = -1;

        public int totalTicks;

        public bool isNotFullySet() {
            return (tickLength <= 0 || timePeriod == null || name == null || name == "" || seed == -1);
        }

        public GeneralSettings deepCopy() {
            return new GeneralSettings() {
                name = name,
                seed = seed,
                tickLength = tickLength,
                timePeriod = timePeriod.deepCopy(),
                totalTicks = totalTicks,
            };
            
        }
    }

}