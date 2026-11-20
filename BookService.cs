using System.Text.Encodings.Web;
using System.Text.Json;
using BookJsonManager.Models;

namespace BookJsonManager.Services;

public class BookService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        // щоб українські літери не перетворювалися на \uXXXX
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public List<Book> Books { get; private set; } = new();

    // Серіалізація: список книг -> текст JSON
    public string Serialize() => JsonSerializer.Serialize(Books, Options);

    // Десеріалізація: текст JSON -> список книг
    public void Deserialize(string json)
    {
        Books = JsonSerializer.Deserialize<List<Book>>(json, Options) ?? new List<Book>();
    }

    public void Add(Book book) => Books.Add(book);

    public void Update(Book original, Book edited) => original.CopyFrom(edited);

    public void Remove(Book book) => Books.Remove(book);

    public List<string> GetGenres() =>
        Books.Select(b => b.Genre)
             .Where(g => !string.IsNullOrWhiteSpace(g))
             .Distinct()
             .OrderBy(g => g)
             .ToList();

    // Пошук за трьома критеріями (LINQ): автор, жанр, діапазон років
    public List<Book> Search(string? author, string? genre, int? yearFrom, int? yearTo)
    {
        IEnumerable<Book> query = Books;

        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(b => b.Author.Contains(author.Trim(), StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(b => b.Genre == genre);

        if (yearFrom.HasValue)
            query = query.Where(b => b.Year >= yearFrom.Value);

        if (yearTo.HasValue)
            query = query.Where(b => b.Year <= yearTo.Value);

        return query.OrderBy(b => b.Title).ToList();
    }
}
