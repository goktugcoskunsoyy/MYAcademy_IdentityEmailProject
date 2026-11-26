namespace MYAcademy_IdentityEmailProject.Entities
{
    public class Message
    {
        public int MessageId { get; set; }
        public string Subject { get; set; }
        public string MessageDetail { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
        public DateTime SendDate { get; set; }
        public bool IsRead { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsDraft { get; set; }
        public bool IsTrashBySender { get; set; }
        public bool IsTrashByReceiver { get; set; }
        public bool IsDeletedBySender { get; set; }
        public bool IsDeletedByReceiver { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
