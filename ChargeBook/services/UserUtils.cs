using System;
using System.Collections.Generic;
using System.Security.Claims;
using chargebook.data.user;
using chargebook.models;
using Newtonsoft.Json;

namespace ChargeBook.services {
    public interface IUserUtils {
        public bool isLoggedIn(ClaimsPrincipal principal);
        public string getEmail(ClaimsPrincipal principal);
        public string getFirstName(ClaimsPrincipal principal);
        public string getLastName(ClaimsPrincipal principal);
        public bool isAdmin(ClaimsPrincipal principal);
        public string getDefaultLocation(ClaimsPrincipal principal);
        public bool isNotificationEnabled(ClaimsPrincipal principal);
        public List<Car> getCars(ClaimsPrincipal principal);
        public long getLastChanged(ClaimsPrincipal principal);
        public string getPriorityRole(ClaimsPrincipal principal);
    }

    public class UserUtils : IUserUtils {
        private readonly IUserManager userManager;

        public UserUtils(IUserManager userManager) {
            this.userManager = userManager;
        }

        public bool isLoggedIn(ClaimsPrincipal principal) {
            return principal.HasClaim(claim => claim.Type == "email");
        }

        public List<string> getOnBehalfs(ClaimsPrincipal principal) {
            return JsonConvert.DeserializeObject<List<string>>(principal.FindFirstValue("onBehalfs"));
        }

        public string getEmail(ClaimsPrincipal principal) {
            return principal.FindFirstValue("email");
        }

        public string getFirstName(ClaimsPrincipal principal) {
            return principal.FindFirstValue("firstName");
        }

        public string getLastName(ClaimsPrincipal principal) {
            return principal.FindFirstValue("lastName");
        }

        public bool isAdmin(ClaimsPrincipal principal) {
            return Convert.ToBoolean(principal.FindFirstValue("isAdmin"));
        }

        public string getDefaultLocation(ClaimsPrincipal principal) {
            return userManager.getDefaultLocation(getEmail(principal));
        }

        public bool isNotificationEnabled(ClaimsPrincipal principal) {
            return userManager.isNotificationEnabled(getEmail(principal));
        }

        public string getSelectedOnBehalf(ClaimsPrincipal principal) {
            return principal.FindFirstValue("selectedOnBehalf");
        }

        public List<Car> getCars(ClaimsPrincipal principal) {
            return userManager.getCars(getEmail(principal));
        }

        public long getLastChanged(ClaimsPrincipal principal) {
            return Convert.ToInt64(principal.FindFirstValue("lastChanged"));
        }

        public string getPriorityRole(ClaimsPrincipal principal) {
            return principal.FindFirstValue("priorityRole");
        }
    }
}