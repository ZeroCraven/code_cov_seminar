using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace chargebook.data.user {
    public class UserUrlToken {
        public string validationToken { get; }
        public DateTime dateOfExpiry { get; }

        private static readonly int VALIDITY_PERIOD_IN_DAYS = 14;

        public UserUrlToken() {
            dateOfExpiry = DateTime.UtcNow.AddDays(VALIDITY_PERIOD_IN_DAYS);
            validationToken = generateUrlToken();
        }

        [JsonConstructor]
        public UserUrlToken(DateTime dateOfExpiry, string validationToken) {
            this.dateOfExpiry = dateOfExpiry;
            this.validationToken = validationToken;
        }

        private string generateUrlToken() {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);
            return WebEncoders.Base64UrlEncode(randomBytes);
        }

        public bool doesMatchAndIsNotExpired(string token) {
            return token == validationToken && dateOfExpiry >= DateTime.UtcNow;
        }
    }
}