using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHackNelnet
{
    public class Rating
    {
        public Guid id { get; set; }
        public Guid userId { get; set; }
        public Guid productId { get; set;}
        public DateTime Timestamp { get; set; }
        public string LocationName { get; set; }
        public int RatingValue { get; set; }
        public string UserNotes { get; set; }
    }
}
