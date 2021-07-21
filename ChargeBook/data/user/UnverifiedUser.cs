namespace chargebook.data.user {
    public class UnverifiedUser {
        public UserUrlToken token { get; set; }
        public bool isAdmin { get; set; }
        public string priorityRole { get; set; }
    }
}