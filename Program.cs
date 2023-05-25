using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
class ISBN
{
    static int rowIndex = 1;
    static void Main()
    {
        string filePath = "ISBN_Input_File.txt";
        // Create a list to store the books
        List<Book> books = new List<Book>();
        // Column names
        string[] columnNames = { "Row Number", "Data Retrieval Type", "ISBN", "Title", "Subtitle", "Author Name(s)", "Number of Pages", "Publish Date" };
        // File path to save the CSV
        string outPath = "output.csv";
        try
        {
            // Create a StreamReader instance to read the file
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;

                // Read and display lines from the file
                while ((line = reader.ReadLine()) != null)
                {
                    string[] ISBN = line.Split(',');
                    foreach (string item in ISBN)
                    {
                        CallAPI(item, books).GetAwaiter().GetResult();
                    }
                    //Increment the index once you move to the next line
                    rowIndex++;
                }
                // foreach (Book book in books)
                // {
                //     Console.WriteLine("--------------------------------------------------------------------------------------");
                //     Console.WriteLine("Row: " + book.Row);
                //     Console.WriteLine("Data type: " + book.DataType);
                //     Console.WriteLine("ISBN: " + book.ISBN);
                //     Console.WriteLine("Book title: " + book.Title);
                //     Console.WriteLine("Book Subtitle: " + book.Subtitle);

                //     foreach (string author in book.AuthorNames)
                //     {
                //         Console.WriteLine("Authors: " + author);
                //     }
                //     Console.WriteLine("Number of pages: " + book.NumberOfPages);
                //     Console.WriteLine("Publish date: " + book.PublishDate);
                // }


                // Export CSV
                ExportCsv(columnNames, books, outPath);

                Console.WriteLine("CSV file exported successfully.");
                Environment.Exit(0);
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found. Please check the file path and try again.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
    static async Task CallAPI(string Code, List<Book> books)
    {
        await API(Code, books);
    }

    static async Task API(string Code, List<Book> books)
    {
        // Check if the book already exists in the list
        if (books.Any(b => b.ISBN == Code))
        {

            // Get the existing book with the same ISBN
            Book existingBook = books.First(b => b.ISBN == Code);
            //Hardcode the dataType variable, if you return here it means the book already exists
            string dataType = "Cache";
            // Create a new book with the same information as the existing book (except for the first 2 pieces of data)
            Book newBook = new Book(rowIndex.ToString(), dataType, existingBook.ISBN, existingBook.Title, existingBook.Subtitle,
                                    existingBook.AuthorNames, existingBook.NumberOfPages, existingBook.PublishDate);

            // Add the new book to the list
            books.Add(newBook);

            return;
        }
        // Specify the ISBN you want to request
        string isbnCode = Code;

        // Construct the API URL with the specified ISBN
        string apiUrl = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbnCode}&jscmd=data&callback=mycallback";

        // Create an instance of HttpClient
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Send the GET request to the API endpoint
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Console.WriteLine(responseBody);

                // Extract the JSON data by removing the callback function wrapper
                int startIndex = responseBody.IndexOf('{');
                int endIndex = responseBody.LastIndexOf('}');
                string jsonData = responseBody.Substring(startIndex, endIndex - startIndex + 1);

                // Parse the JSON and extract the necessary data
                JsonDocument jsonDocument = JsonDocument.Parse(jsonData);
                JsonElement rootElement = jsonDocument.RootElement;


                //Hardcode the datatType variable, if youre here it means you called the API
                string dataType = "Server";
                foreach (JsonProperty bookProperty in rootElement.EnumerateObject())
                {
                    string bookKey = bookProperty.Name;
                    JsonElement bookElement = bookProperty.Value;




                    string isbn = GetIsbn(bookElement.GetProperty("identifiers"));

                    string title = string.Empty;
                    if (bookElement.TryGetProperty("title", out JsonElement titleElement) && titleElement.ValueKind == JsonValueKind.String)
                    {
                        title = titleElement.GetString();
                    }

                    string subtitle = string.Empty;
                    if (bookElement.TryGetProperty("subtitle", out JsonElement subtitleElement) && subtitleElement.ValueKind == JsonValueKind.String)
                    {
                        subtitle = subtitleElement.GetString();
                    }

                    List<string> authorNames = new List<string>();
                    if (bookElement.TryGetProperty("authors", out JsonElement authorsElement) && authorsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement authorElement in authorsElement.EnumerateArray())
                        {
                            string authorName = authorElement.GetProperty("name").GetString();
                            authorNames.Add(authorName);
                        }
                    }

                    int numberOfPages = 0;
                    if (bookElement.TryGetProperty("number_of_pages", out JsonElement numberOfPagesElement) && numberOfPagesElement.ValueKind == JsonValueKind.Number)
                    {
                        numberOfPages = numberOfPagesElement.GetInt32();
                    }

                    string publishDate = string.Empty;
                    if (bookElement.TryGetProperty("publish_date", out JsonElement publishDateElement) && publishDateElement.ValueKind == JsonValueKind.String)
                    {
                        publishDate = publishDateElement.GetString();
                    }

                    // Create a Book object
                    Book book = new Book(rowIndex.ToString(), dataType, isbn, title, subtitle, authorNames, numberOfPages, publishDate);
                    books.Add(book);
                }


            }
            catch (Exception ex)
            {
                // Handle any errors that occurred during the request
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
    public class Book
    {
        public string Row { get; set; }
        public string DataType { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public List<string> AuthorNames { get; set; }
        public int NumberOfPages { get; set; }
        public string PublishDate { get; set; }

        public Book(string rowData, string dataType, string isbn, string title, string subtitle, List<string> authorNames, int numberOfPages, string publishDate)
        {
            Row = rowData;
            DataType = dataType;
            ISBN = isbn;
            Title = title;
            Subtitle = subtitle;
            AuthorNames = authorNames;
            NumberOfPages = numberOfPages;
            PublishDate = publishDate;
        }

        public string[] ToCsvArray(string rowIndex, string dataType)
        {
            string PublishDateString = PublishDate.Replace(",", " -");

            string TitleString = Title;
            string SubtitleString = Subtitle;
            string NumberOfPagesString = NumberOfPages.ToString();


            //If any prop comes blank its changed for "N/A"
            if (Subtitle == "")
            {
                SubtitleString = "N/A";
            }
            if (Title == "")
            {
                TitleString = "N/A";
            }
            if (NumberOfPages == 0)
            {
                NumberOfPagesString = "N/A";
            }
            if (PublishDate == "")
            {
                PublishDateString = "N/A";
            }

            return new string[] { rowIndex.ToString(), dataType, ISBN, TitleString, SubtitleString, string.Join("; ", AuthorNames), NumberOfPagesString, PublishDateString };

        }
    }
    //You go first for the isbn_10 and if doesnt exist you take the isbn_13
    private static string GetIsbn(JsonElement identifiersElement)
    {
        string isbn = null;

        if (identifiersElement.TryGetProperty("isbn_10", out JsonElement isbn10Element) && isbn10Element.ValueKind == JsonValueKind.Array && isbn10Element.GetArrayLength() > 0)
        {
            isbn = isbn10Element[0].GetString();
        }
        else if (identifiersElement.TryGetProperty("isbn_13", out JsonElement isbn13Element) && isbn13Element.ValueKind == JsonValueKind.Array && isbn13Element.GetArrayLength() > 0)
        {
            isbn = isbn13Element[0].GetString();
        }
        //Use the line below if you want Excel not to format the output isbn
        // return $"'{isbn}'";
        return isbn;
    }
    static void ExportCsv(string[] columnNames, List<Book> csvData, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Write column names
            writer.WriteLine(string.Join(",", columnNames));

            // Write data
            foreach (Book book in csvData)
            {
                string[] rowData = book.ToCsvArray(book.Row, book.DataType);
                writer.WriteLine(string.Join(",", rowData));
            }
        }
    }
}