namespace AuthService.Db.Models {
    public class Message {
        public int Id { get; set; } = 0;
        public string Body { get; set; } = "";
        public string Source { get; set; } = "";
    }
}