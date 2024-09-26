
using System;




class Program
{
    static void Main(string[] args)
    {
        var database = Database.GetInstance();
        bool exit = false;

        var notifier = new Notifier();

        var user1 = new LibraryUser();
        var user2 = new LibraryUser();

        notifier.Subscribe(user1);
        notifier.Subscribe(user2);



        while (!exit)
        {
            Console.WriteLine("1. Adauga carte");
            Console.WriteLine("2. Vizualizeaza cartile");
            Console.WriteLine("3. Sterge o carte");
            Console.WriteLine("4. Cauta o carte");
            Console.WriteLine("5. Iesire");
            Console.Write("Introdu optiunea dorita: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddBook(database);
                    notifier.Notify("O nouă carte a fost adăugată în bibliotecă!");
                    break;
                case "2":
                    database.DisplayBooks();
                    break;
                case "3":
                    RemoveBook(database);
                    break;
                case "4":
                    Console.WriteLine(" Introdu titlul cartii: ");
                    string search = Console.ReadLine();
                    var searchBook = new SearchBookCommand(search);
                    searchBook.Execute();
                    break;
                case "5":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Opțiune invalidă. Încearcă din nou.");
                    break;
            }
        }
    }

    static void AddBook(Database database)
    {
        Console.Write("Introdu titlul cărții: ");
        string? title = Console.ReadLine();
        Console.Write("Introdu autorul cărții: ");
        string? author = Console.ReadLine();

        var book = BookFactory.CreateBook(title, author);

        Console.WriteLine("Este cartea populara? (da/nu)");
        string popularInput = Console.ReadLine().ToLower();

        if (popularInput == "da")
        {
            book = new PopularBookDecorator(book);
        }

        Console.WriteLine("Este cartea recomandata? (da/nu)");
        string recommendedInput = Console.ReadLine().ToLower();

        if (recommendedInput == "da")
        {
            book = new RecommendedBookDecorator(book);
        }

        Console.WriteLine("Este cartea la promotie? (da/nu)");
        string promotionInput = Console.ReadLine().ToLower();

        if (promotionInput == "da")
        {
            book = new PromotionBookDecorator(book);
        }


        database.AddBook(book);
        Console.WriteLine("Cartea a fost adăugată cu succes.");

    }

    static void RemoveBook(Database database)
    {
        Console.Write("Introdu titlul cărții pe care dorești să o ștergi: ");
        string title = Console.ReadLine();

        var book = database.Books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        if (book != null)
        {
            database.Books.Remove(book);
            Console.WriteLine("Cartea a fost ștearsă cu succes.");
        }
        else
        {
            Console.WriteLine("Cartea nu a fost găsită.");
        }
    }
}

public class Book
{ 
    public virtual string Title { get; set; }
    public string Author { get; set; }

    public virtual string GetDescription()
    {
        return $"{Title} by {Author}";
    }
}


public class Database
{
    private static Database? _instance;
    public List<Book> Books { get; private set; }

    private Database()
    {
        Books = new List<Book>();
    }

    public static Database GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Database();
        }
        return _instance;
    }

    public void AddBook(Book book)
    {
        Books.Add(book);
    }

    public void RemoveBook(Book book)
    {
        if (Books.Contains(book))
        {
            Books.Remove(book);
            Console.WriteLine($"Cartea '{book.Title}' a fost ștearsă.");
        }
        else
        {
            Console.WriteLine($"Cartea '{book.Title}' nu a fost găsită.");
        }
    }

    public void DisplayBooks()
    {
        foreach (var book in Books)
        {
            Console.WriteLine($"{book.GetDescription()}");
        }
    }
}

public static class BookFactory
{
    public static Book CreateBook(string title, string author)
    {
        return new Book { Title = title, Author = author };
    }
}


public interface IObserver
{
    void Update(string message);
}

public class Notifier
{
    private List<IObserver> _observers = new List<IObserver>();

    public void Subscribe(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void Notify(string message)
    {
        foreach (var observer in _observers)
        {
            observer.Update(message);
        }
    }
}

public class LibraryUser : IObserver
{
    public void Update(string message)
    {
        Console.WriteLine("Notificare: " + message);
    }
}

public abstract class BookDecorator : Book
{
    protected Book _book;

    public BookDecorator(Book book)
    {
        _book = book;
    }

    public override string Title
    {
        get { return _book.Title; } // Returnează titlul original
    }
}

public class PopularBookDecorator : BookDecorator
{
    public PopularBookDecorator(Book book) : base(book) { }

    public override string GetDescription()
    {
        return $"{_book.GetDescription()} (Popular)";
    }
}

public class RecommendedBookDecorator : BookDecorator
{
    public RecommendedBookDecorator(Book book) : base(book) { }

    public override string GetDescription()
    {
         return $"{_book.GetDescription()} - [Recomandată]";
    }
}

public class PromotionBookDecorator : BookDecorator
{
    public PromotionBookDecorator(Book book) : base(book) { }

    public override string GetDescription()
    {
        return $"{_book.GetDescription()} - [Carte in promotie]";
    }
}


public interface ICommand
{
    void Execute();
    void Undo();
}

public class SearchBookCommand : ICommand
{
    private string _title;
    private Database _database;

    public SearchBookCommand(string title)
    {
        _title = title;
        _database = Database.GetInstance();
    }

    public void Execute()
    {
        var book = _database.Books.FirstOrDefault(b => b.Title.Equals(_title, StringComparison.OrdinalIgnoreCase));
        if (book != null)
        {
            Console.WriteLine($"Carte găsită: {book.Title} de {book.Author}");
        }
        else
        {
            Console.WriteLine("Carte ne găsită.");
        }
    }

    public void Undo()
    {
        // 
    }


}