using System;
using System.Collections.Generic;

namespace Find_My_Boef.Model
{
    public class SocialMediaPost
    {
        public string Text { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public SocialMediaPostUser CreatedBy { get; set; }
        public List<SocialMediaPostMedia> Media { get; set; }
        public string TopImageURL { get; set; }
    }
    public class SocialMediaPostUser
    {
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public string Description { get; set; }
    }
    public class SocialMediaPostMedia
    {
        public string MediaURL { get; set; }
    }
}
