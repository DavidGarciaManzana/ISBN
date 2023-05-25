# ISBN
This C# console application retrieves full book information using the OpenLibrary API based on the provided input file containing ISBNs. It then outputs the retrieved information in CSV format.

## Installation
Clone the repository to your local machine:

bash
Copy code
git clone https://github.com/DavidGarciaManzana/ISBN.git
Open the project in your preferred C# IDE.

Restore the required NuGet packages:

Copy code
dotnet restore
Build the solution:

Copy code
dotnet build

## Usage

Put your input file on the same folder as the repository

Run the application.


The application will retrieve book information from the OpenLibrary API for each ISBN in the input file and generate a CSV file containing the retrieved information. The CSV file will be saved in the same directory as the input file.

## Additional Constraints
The input file must be a plain text file (.txt) with ISBN separated by commas per line.

The application requires an active internet connection to interact with the OpenLibrary API.

The OpenLibrary API has a rate limit. Please refer to their documentation for details on the rate limits and usage guidelines.

## Dependencies

.NET 7

NuGet - Package Manager for .NET

System.IO

System.Net.Http

System.Text.Json

System.Threading.Tasks

Please ensure that you have the required dependencies installed before running the application.

## Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request.
