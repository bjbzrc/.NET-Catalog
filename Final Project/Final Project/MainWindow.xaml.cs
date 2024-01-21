using Final_Project;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;

namespace Book_Catalog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Master list of books to be worked with
        private List<Book> books;
        private string fileName = "Catalog.book";
        private bool unsavedChanges = false;

        public MainWindow()
        {
            InitializeComponent();

            // Master list of books used populate BookListView (right side of view)
            books = new List<Book>();
            LoadBooks();

            // Subscribes a new event handler to Closing, which prompts the user about unsaved changes 
            Closing += MainWindow_Closing;
        }

        // Looks for a 'Catalog.book' in the default directory, otherwise
        // loads the app with nothing in BookListView and creates a new 'Catalog.book'
        // in ...Final Project/Final Project/bin/Debug
        private void LoadBooks()
        {
            if (File.Exists(fileName))
            {
                try
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        // Create a BinaryFormatter object to deserialize the file contents into a list of Book objects
                        BinaryFormatter bf = new BinaryFormatter();
                        books = (List<Book>)bf.Deserialize(fs);
                    }
                    // Update BookListView to show the list of books and pop up a MessageBox
                    // detailing how many books were loaded
                    BookListView.ItemsSource = books;
                    int loadedBookCount = Count_Books(books);
                    MessageBox.Show($"Loaded {books.Count} books from {fileName}.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while loading books: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show($"No books found at {fileName}.");
            }
        }

        // Adds everything from the left side of the view to a new Book object
        // and adds it the the book list 'books'
        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure the title, author, and description textboxes aren't empty
                if (string.IsNullOrEmpty(TitleTextBox.Text) || 
                    string.IsNullOrEmpty(DescriptionTextBox.Text) || 
                    string.IsNullOrEmpty(AuthorTextBox.Text))
                {
                    throw new ArgumentException("Please enter a title, author, and description for the book.");
                }

                // Create new book object
                Book book = new Book
                {
                    Title = TitleTextBox.Text,
                    Author = AuthorTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Status = ((ComboBoxItem)StatusComboBox.SelectedItem)?.Content?.ToString(),
                    Rating = (int)RatingSlider.Value
                };

                // Add book to list and update ListView
                books.Add(book);
                BookListView.ItemsSource = null;
                BookListView.ItemsSource = books;

                // Clear input fields
                TitleTextBox.Clear();
                AuthorTextBox.Clear();
                DescriptionTextBox.Clear();
                StatusComboBox.SelectedIndex = 0;
                RatingSlider.Value = 1;

                // Raise a flag for unsaved changes
                unsavedChanges = true;
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Invalid input: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding book: {ex.Message}");
            }
        }

        // This event handler activates every time a book is clicked in the book list (BookListView).
        // When a book is clicked, we show a message showing all the information about the title
        private void BookListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BookListView.SelectedItem != null)
            {
                Book selectedBook = (Book)BookListView.SelectedItem;
                MessageBox.Show($"Title: {selectedBook.Title}\n\n" +
                                $"Author: {selectedBook.Author}\n\n" +
                                $"Description: {selectedBook.Description}\n\n" +
                                $"Status: {selectedBook.Status}\n\n" +
                                $"Rating: {selectedBook.Rating}");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveCatalog();
        }

        private void SaveCatalog()
        {
            try
            {
                // Overwrite Catalog.book with the new book list
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, books);
                }

                // Count the current number of books and show how many were saved
                int curBookCount = Count_Books(books);
                MessageBox.Show($"{curBookCount} books saved to {fileName}.");

                // Unflag changes so the app can exit without prompts
                unsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving books: {ex.Message}");
            }
        }

        // Allows users to delete books from the book list, but only if
        // a book is selected from BookListView
        private void DeleteBook_Click(object sender, RoutedEventArgs e)
        {
            Book selectedBook = BookListView.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Please select a book to delete.");
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete '{selectedBook.Title}'?", 
                                                       "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                books.Remove(selectedBook);
                BookListView.ItemsSource = null;
                BookListView.ItemsSource = books;

                // Flag unsaved changes on a successful book deletion
                unsavedChanges = true;
            }
        }

        // Event handler for when the user clicks on a header in the book list.
        // Uses LINQ to allow the user to toggle between sorting by book title, status,
        // or rating, and to toggle between ascending and descending order
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            // Defines the column headers and captures a string from them
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            string header = headerClicked?.Column?.Header?.ToString();

            // Sorts by the header clicked
            if (header == "Title")
            {
                // If the book list is already in ascending order, change to descending
                if (books.OrderBy(b => b.Title).ToList().SequenceEqual(books))
                {
                    books = books.OrderByDescending(b => b.Title).ToList();
                }
                // Otherwise, sort in ascending order. Same pattern for the other headers
                else
                {
                    books = books.OrderBy(b => b.Title).ToList();
                }
            }
            else if (header == "Status")
            {
                if (books.OrderBy(b => b.Status).ToList().SequenceEqual(books))
                {
                    books = books.OrderByDescending(b => b.Status).ToList();
                }
                else
                {
                    books = books.OrderBy(b => b.Status).ToList();
                }
            }
            else if (header == "Rating")
            {
                if (books.OrderBy(b => b.Rating).ToList().SequenceEqual(books))
                {
                    books = books.OrderByDescending(b => b.Rating).ToList();
                }
                else
                {
                    books = books.OrderBy(b => b.Rating).ToList();
                }
            }

            // Updates the book list to the new ordering
            BookListView.ItemsSource = null;
            BookListView.ItemsSource = books;
        }

        // Method that counts the current number of book objects in Catalog.book
        private int Count_Books(List<Book> books)
        {
            int bookCount = 0;
            foreach (Book book in books)
            {
                bookCount++;
            }

            return bookCount;
        }

        // The following two methods handle cases when there are unsaved changes
        // and the user tries to exit either from the menu bar or the window.
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (unsavedChanges)
            {
                MessageBoxResult result = MessageBox.Show("There are unsaved changes. Do you want to save the catalog before exiting?",
                                                           "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    SaveCatalog();
                }
                if (result == MessageBoxResult.No)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    return;
                }
            }

            Application.Current.Shutdown();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

            if (unsavedChanges)
            {
                MessageBoxResult result = MessageBox.Show("There are unsaved changes. Do you want to save the catalog before exiting?",
                                                           "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    SaveCatalog();
                }
                // Ensures hitting 'cancel' won't just close the program
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
