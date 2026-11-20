namespace BookJsonManager.Models;

public class Book
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Genre { get; set; } = "";
    public int Year { get; set; }
    public int Pages { get; set; }
    public int Rating { get; set; }

    public Book Clone() => (Book)MemberwiseClone();

    public void CopyFrom(Book other)
    {
        Title = other.Title;
        Author = other.Author;
        Genre = other.Genre;
        Year = other.Year;
        Pages = other.Pages;
        Rating = other.Rating;
    }
}
