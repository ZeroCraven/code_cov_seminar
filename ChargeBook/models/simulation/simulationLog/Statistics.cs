namespace ChargeBook.models.simulation.simulationLog {
    public class Statistics {
        public int countApprovedBookings { get; set; }
        public int countDeniedBookings { get; set; }
        public double peakWorkload { get; set; }
        public int peakOccupancy { get; set; }
        public long requestedEnergy { get; set; }
        public long suppliedEnergy { get; set; }
        public int countApprovedAdHocBookings { get; set; }
        public int countDeniedAdHocBookings { get; set; }

    }
}