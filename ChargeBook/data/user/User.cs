using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using chargebook.models;
using Newtonsoft.Json;

namespace chargebook.data.user {
    public class User {
        public string email { get; set; }
        public string password { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public List<string> onBehalfs { get; set; }
        public bool isAdmin { get; set; }
        public string priorityRole { get; set; }
        public string defaultLocation { get; set; }
        public bool notificationEnabled { get; set; }
        public string selectedOnBehalf { get; set; }
        public List<Car> ownedCars { get; set; }
        public long lastChanged { get; set; }

        public byte[] salt;

        public User() {
            ownedCars = new List<Car>();
            onBehalfs = new List<string>();
        }

        public List<Claim> getClaims() {
            return new List<Claim>() {
                new Claim("email", email),
                new Claim("firstName", firstName),
                new Claim("lastName", lastName),
                new Claim("onBehalfs", JsonConvert.SerializeObject(onBehalfs)),
                new Claim("isAdmin", isAdmin.ToString()),
                new Claim("priorityRole", priorityRole),
                new Claim("selectedOnBehalf", selectedOnBehalf),
                new Claim("lastChanged", lastChanged.ToString())
            };
        }
    }
}