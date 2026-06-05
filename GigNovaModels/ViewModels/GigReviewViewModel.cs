namespace GigNovaModels.ViewModels
{
    // Wraps a Review with the buyer's display name, bio, and username
    // so the reviews page can show "By X" on each card and open a small popup
    // with the buyer's details on click - all in one round trip from the WS.
    public class GigReviewViewModel
    {
        public string Review_id { get; set; }
        public int Review_rating { get; set; }
        public string Review_comment { get; set; }
        public string Review_creation_date { get; set; }
        public int Buyer_id { get; set; }

        public string Buyer_display_name { get; set; }
        public string Buyer_description { get; set; }
        public string Person_username { get; set; }
        public string Person_join_date { get; set; }
    }
}
