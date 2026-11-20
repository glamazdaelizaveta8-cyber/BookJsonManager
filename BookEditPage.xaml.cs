using BookJsonManager.Models;

namespace BookJsonManager;

public partial class BookEditPage : ContentPage
{
    private readonly Book _book;
    private readonly Action<Book> _onSave;

    // book == null -> додавання нової книги, інакше редагування копії
    public BookEditPage(Book? book, Action<Book> onSave)
    {
        InitializeComponent();
        _onSave = onSave;
        _book = book is null
            ? new Book { Year = DateTime.Now.Year, Pages = 100, Rating = 5 }
            : book.Clone();

        Title = book is null ? "Нова книга" : "Редагування книги";

        TitleEntry.Text = _book.Title;
        AuthorEntry.Text = _book.Author;
        GenreEntry.Text = _book.Genre;
        YearEntry.Text = _book.Year.ToString();
        PagesEntry.Text = _book.Pages.ToString();
        RatingEntry.Text = _book.Rating.ToString();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text) || string.IsNullOrWhiteSpace(AuthorEntry.Text))
        {
            ErrorLabel.Text = "Назва та автор не можуть бути порожніми.";
            return;
        }
        if (string.IsNullOrWhiteSpace(GenreEntry.Text))
        {
            ErrorLabel.Text = "Вкажіть жанр.";
            return;
        }
        if (!int.TryParse(YearEntry.Text, out int year) || year < 1000 || year > DateTime.Now.Year)
        {
            ErrorLabel.Text = $"Рік має бути числом від 1000 до {DateTime.Now.Year}.";
            return;
        }
        if (!int.TryParse(PagesEntry.Text, out int pages) || pages <= 0)
        {
            ErrorLabel.Text = "Кількість сторінок має бути додатним числом.";
            return;
        }
        if (!int.TryParse(RatingEntry.Text, out int rating) || rating < 1 || rating > 5)
        {
            ErrorLabel.Text = "Рейтинг має бути числом від 1 до 5.";
            return;
        }

        _book.Title = TitleEntry.Text.Trim();
        _book.Author = AuthorEntry.Text.Trim();
        _book.Genre = GenreEntry.Text.Trim();
        _book.Year = year;
        _book.Pages = pages;
        _book.Rating = rating;

        _onSave(_book);
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
