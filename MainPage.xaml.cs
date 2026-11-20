using System.Text;
using System.Text.Json;
using BookJsonManager.Models;
using BookJsonManager.Services;
using CommunityToolkit.Maui.Storage;

namespace BookJsonManager;

public partial class MainPage : ContentPage
{
    private const string AllGenres = "Усі жанри";

    private readonly BookService _service = new();
    private string? _currentPath;
    private bool _loading;

    private static readonly FilePickerFileType JsonFileType = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".json" } },
            { DevicePlatform.macOS, new[] { "json" } },
            { DevicePlatform.MacCatalyst, new[] { "json" } },
            { DevicePlatform.Android, new[] { "application/json" } },
            { DevicePlatform.iOS, new[] { "public.json" } }
        });

    public MainPage()
    {
        InitializeComponent();
        RefreshGenres();
        ApplyFilter();
    }

    // ---------- Відображення та пошук ----------

    private void RefreshGenres()
    {
        string? previous = GenrePicker.SelectedItem as string;
        _loading = true;
        var genres = new List<string> { AllGenres };
        genres.AddRange(_service.GetGenres());
        GenrePicker.ItemsSource = genres;
        int index = previous is null ? 0 : genres.IndexOf(previous);
        GenrePicker.SelectedIndex = index < 0 ? 0 : index;
        _loading = false;
    }

    private void ApplyFilter()
    {
        if (_loading) return;

        string? genre = GenrePicker.SelectedIndex > 0 ? GenrePicker.SelectedItem as string : null;
        int? from = int.TryParse(YearFromEntry.Text, out int f) ? f : null;
        int? to = int.TryParse(YearToEntry.Text, out int t) ? t : null;

        var result = _service.Search(AuthorEntry.Text, genre, from, to);
        BooksView.ItemsSource = result;
        StatusLabel.Text = $"Показано: {result.Count} з {_service.Books.Count}. " +
                           $"Файл: {_currentPath ?? "не вибрано"}";
    }

    private void OnFilterChanged(object? sender, EventArgs e) => ApplyFilter();

    private void OnResetClicked(object? sender, EventArgs e)
    {
        _loading = true;
        AuthorEntry.Text = "";
        YearFromEntry.Text = "";
        YearToEntry.Text = "";
        GenrePicker.SelectedIndex = 0;
        _loading = false;
        ApplyFilter();
    }

    // ---------- Робота з файлами ----------

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Оберіть JSON-файл",
                FileTypes = JsonFileType
            });
            if (file is null) return;

            using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            _service.Deserialize(await reader.ReadToEndAsync());

            _currentPath = file.FullPath;
            RefreshGenres();
            ApplyFilter();
        }
        catch (JsonException)
        {
            await DisplayAlert("Помилка", "Файл має неправильний формат JSON.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_currentPath is null)
        {
            await SaveAsAsync();
            return;
        }

        try
        {
            await File.WriteAllTextAsync(_currentPath, _service.Serialize(), Encoding.UTF8);
            await DisplayAlert("Збереження", "Файл збережено.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

    private async void OnSaveAsClicked(object? sender, EventArgs e) => await SaveAsAsync();

    private async Task SaveAsAsync()
    {
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(_service.Serialize()));
            var result = await FileSaver.Default.SaveAsync("books.json", stream, CancellationToken.None);
            if (result.IsSuccessful)
            {
                _currentPath = result.FilePath;
                ApplyFilter();
                await DisplayAlert("Збереження", "Файл збережено.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

    // ---------- Додавання / редагування / видалення ----------

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new BookEditPage(null, book =>
        {
            _service.Add(book);
            RefreshGenres();
            ApplyFilter();
        }));
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (BooksView.SelectedItem is not Book selected)
        {
            await DisplayAlert("Редагування", "Спочатку оберіть книгу в таблиці.", "OK");
            return;
        }

        await Navigation.PushModalAsync(new BookEditPage(selected, edited =>
        {
            _service.Update(selected, edited);
            RefreshGenres();
            ApplyFilter();
        }));
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (BooksView.SelectedItem is not Book selected)
        {
            await DisplayAlert("Видалення", "Спочатку оберіть книгу в таблиці.", "OK");
            return;
        }

        bool ok = await DisplayAlert("Видалення", $"Видалити книгу «{selected.Title}»?", "Так", "Ні");
        if (!ok) return;

        _service.Remove(selected);
        RefreshGenres();
        ApplyFilter();
    }

    private async void OnAboutClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AboutPage());
    }
}
