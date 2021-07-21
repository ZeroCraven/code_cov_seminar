using System;

namespace chargebook.data.user {
    public class UserNotAuthorisedException : Exception {
        public UserNotAuthorisedException() : base() { }
    }
}