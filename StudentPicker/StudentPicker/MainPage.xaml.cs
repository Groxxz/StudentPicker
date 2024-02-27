﻿using System.Collections.ObjectModel;

namespace StudentPicker
{
    public partial class MainPage : ContentPage
    {

        ObservableCollection<Student> students = new ObservableCollection<Student>();
        int currentId = 1;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void AddStudentButton_Clicked(object sender, EventArgs e)
        {
            var result = await DisplayPromptAsync("Dodaj ucznia", "Wprowadź imię:", "OK", "Anuluj", placeholder: "Imię");

            if (result != null)
            {
                string firstName = result;

                result = await DisplayPromptAsync("Dodaj ucznia", "Wprowadź nazwisko:", "OK", "Anuluj", placeholder: "Nazwisko");

                if (result != null)
                {
                    string lastName = result;
                    Student newStudent = new Student(currentId, firstName, lastName);
                    students.Add(newStudent);
                    var studentLabel = new Label { Text = $"{currentId}. {firstName} {lastName}" };
                    studentsStackLayout.Children.Add(studentLabel);
                    currentId++;
                }
            }
        }

        private async void CreateClassButton_Clicked(object sender, EventArgs e)
        {
            string fileName = await DisplayPromptAsync("Nazwa pliku", "Podaj nazwę pliku (bez rozszerzenia):", "OK", "Anuluj", placeholder: "Nazwa pliku");

            if (string.IsNullOrWhiteSpace(fileName))
            {
                await DisplayAlert("Błąd", "Musisz podać nazwę pliku.", "OK");
                return;
            }

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = Path.Combine(documentsPath, "Klasy");
            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, $"{fileName}.txt");

            List<string> studentsList = new List<string>();
            foreach (var student in students)
            {
                string studentInfo = $"{student.Id},{student.FirstName},{student.LastName}";
                studentsList.Add(studentInfo);
            }

            try
            {
                File.WriteAllLines(filePath, studentsList);
                await DisplayAlert("Sukces", $"Lista uczniów została zapisana do pliku: {filePath}", "OK");
                students.Clear();
                studentsStackLayout.Clear();
                currentId = 1;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Wystąpił błąd podczas zapisywania pliku: {ex.Message}", "OK");
            }
        }

        private async void LoadClassButton_Clicked(object sender, EventArgs e)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = Path.Combine(documentsPath, "Klasy");

            if (!Directory.Exists(folderPath))
            {
                await DisplayAlert("Błąd", "Folder 'Klasy' nie istnieje.", "OK");
                return;
            }

            string[] files = Directory.GetFiles(folderPath);

            string selectedFile = await DisplayActionSheet("Wybierz plik", "Anuluj", null, files.Select(Path.GetFileName).ToArray());

            if (!string.IsNullOrEmpty(selectedFile) && selectedFile != "Anuluj")
            {
                students.Clear();
                studentsStackLayout.Clear();

                string filePath = Path.Combine(folderPath, selectedFile);
                string fileContent = File.ReadAllText(filePath);

                string[] lines = fileContent.Split(Environment.NewLine);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');

                    if (parts.Length == 3)
                    {
                        if (int.TryParse(parts[0], out int id))
                        {
                            Student newStudent = new Student(id, parts[1], parts[2]);
                            students.Add(newStudent);

                            var studentLabel = new Label { Text = $"{id}. {parts[1]} {parts[2]}" };
                            studentsStackLayout.Children.Add(studentLabel);
                        }
                        else
                        {
                            await DisplayAlert("Błąd", "Nieprawidłowy format ID.", "OK");
                        }
                    }
                }
            }
        }

        private async void PickStudentButton_Clicked(object sender, EventArgs e)
        {
            if (students.Count == 0)
            {
                await DisplayAlert("Brak uczniów", "Nie ma żadnych uczniów na liście.", "OK");
                return;
            }

            Random random = new Random();
            int randomIndex = random.Next(0, students.Count);

            Student pickedStudent = students[randomIndex];

            await DisplayAlert("Wylosowany uczeń", $"Numer: {pickedStudent.Id}\nImię: {pickedStudent.FirstName}\nNazwisko: {pickedStudent.LastName}", "OK");
        }

        private async void EditStudentButton_Clicked(object sender, EventArgs e)
        {
            if (students.Count == 0)
            {
                await DisplayAlert("Brak uczniów", "Nie ma żadnych uczniów na liście.", "OK");
                return;
            }

            string[] studentNames = students.Select(student => $"{student.FirstName} {student.LastName}").ToArray();

            string selectedStudent = await DisplayActionSheet("Wybierz ucznia do edycji", "Anuluj", null, studentNames);

            if (!string.IsNullOrEmpty(selectedStudent) && selectedStudent != "Anuluj")
            {
                int selectedIndex = Array.IndexOf(studentNames, selectedStudent);
                Student selectedStudentObject = students[selectedIndex];

                string newFirstName = await DisplayPromptAsync("Edytuj imię", "Wprowadź nowe imię:", "Zapisz", "Anuluj", selectedStudentObject.FirstName);
                if(string.IsNullOrEmpty(newFirstName))
                {
                    await DisplayAlert("Błąd", "imie nie może być puste.", "OK");
                    return;
                }
                string newLastName = await DisplayPromptAsync("Edytuj nazwisko", "Wprowadź nowe nazwisko:", "Zapisz", "Anuluj", selectedStudentObject.LastName);
                if (string.IsNullOrEmpty(newLastName))
                {
                    await DisplayAlert("Błąd", "nazwisko nie może być puste.", "OK");
                    return;
                }

                if (newFirstName != selectedStudentObject.FirstName || newLastName != selectedStudentObject.LastName)
                {
                    selectedStudentObject.FirstName = newFirstName;
                    selectedStudentObject.LastName = newLastName;

                    foreach (var child in studentsStackLayout.Children)
                    {
                        if (child is Label label && label.Text.Contains(selectedStudent))
                        {
                            label.Text = $"{selectedStudentObject.Id}. {newFirstName} {newLastName}";
                            break;
                        }
                    }
                }
            }
        }
    }
}

