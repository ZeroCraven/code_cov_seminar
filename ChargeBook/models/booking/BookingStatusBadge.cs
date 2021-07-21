using System.Collections.Generic;

namespace ChargeBook.models.booking {
    public class BookingStatusBadge {
        public static Dictionary<string, BookingStatusBadge> bookingStatusBadges = new Dictionary<string, BookingStatusBadge>() {
            {
                "angefordert", new BookingStatusBadge() {
                    color = "#1b6ec2", lightColor = "#59a1e8", icon = "fas fa-spinner", fontColor = "#FFFFFF"
                }
            }, {
                "abgelehnt", new BookingStatusBadge() {
                    color = "#dc3545", lightColor = "#eb8e98", icon = "fas fa-times", fontColor = "#FFFFFF"
                }
            }, {
                "abgelaufen", new BookingStatusBadge() {
                    color = "#d0d0d0", lightColor = "#ebebeb", icon = "fas fa-history", fontColor = "#000000"
                }
            }, {
                "angenommen", new BookingStatusBadge() {
                    color = "#28a745", lightColor = "#5fd87b", icon = "fas fa-check", fontColor = "#FFFFFF"
                }
            }, {
                "Ladevorgang begonnen", new BookingStatusBadge() {
                    color = "#ffc107", lightColor = "#ffda6b", icon = "fas fa-bolt", fontColor = "#000000"
                }
            }, {
                "Ladevorgang beendet", new BookingStatusBadge() {
                    color = "#d0d0d0", lightColor = "#ebebeb", icon = "fas fa-battery-full", fontColor = "#000000"
                }
            }
        };

        public string color;
        public string lightColor;
        public string icon;
        public string fontColor;
    }
}