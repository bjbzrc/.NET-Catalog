using System;

namespace Final_Project
{
    // Data model for the book objects used by the app
    [Serializable]
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Rating { get; set; }
    }
}
