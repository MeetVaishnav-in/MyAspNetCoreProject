namespace TrainTray_food_order_booking_system.Models
{

    public class EncryptionModel
    {
        public string? PlainText { get; set; }  // Input text to be encrypted
        public string? EncryptedText { get; set; } // Output encrypted text

        public string? CheckPlainText { get; set; } // Text entered for checking against encrypted text
        public string? CheckEncryptedText { get; set; } // Encrypted text entered for verification
        public string? MatchResult { get; set; }
    }

}
