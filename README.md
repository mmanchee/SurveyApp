# SurveyApp

A simple multi-page survey application built with ASP.NET Core Razor Pages.

## Description

This project implements a basic web-based survey application using ASP.NET Core Razor Pages. It guides a user through several pages to collect information and preferences, utilizing session state to maintain user input across pages. The application includes client-side JavaScript for dynamic UI elements (like showing "Other" textboxes) and server-side validation and data handling. Survey responses are intended to be saved to a SQL Server database via a stored procedure.

## Technologies Used

* **ASP.NET Core:** Web framework
* **Razor Pages:** Page-based programming model for UI
* **C#:** Programming language
* **SQL Server:** Database for storing survey responses
* **Bootstrap:** Frontend styling and components
* **JavaScript:** Client-side interactivity (e.g., toggling "Other" fields, progress bar)
* **Session State:** To persist user data between pages

## Project Structure

The project is organized into the following main folders and files:

* **`SurveyApp.BLL`**: Business Logic Layer
    * `ISurveyService.cs`: Interface for the survey service.
    * `SurveyService.cs`: Implements the `ISurveyService`, containing logic for submitting survey responses and cleaning up data.
* **`SurveyApp.DAL`**: Data Access Layer
    * `ISurveyRepository.cs`: Interface for the data repository.
    * `SurveyRepository.cs`: Implements `ISurveyRepository`, handling interaction with the SQL Server database via a stored procedure (`sp_InsertSurveyResponse`).
* **`SurveyApp.Models`**: Data Models
    * `SurveyResponse.cs`: Defines the structure for storing a user's survey responses, including validation attributes and helper properties for comma-separated strings.
    * `SurveyOptions.cs`: Static class providing predefined lists for dropdowns, radio buttons, and checkboxes (Industries, Positions, Colors, Music Genres, Seasons, Likert Scale).
* **`SurveyApp.Web`**: Web Application (Razor Pages)
    * `Program.cs`: Application entry point, configuring services (Razor Pages, Session, Dependency Injection for BLL/DAL, Logging) and the HTTP request pipeline.
    * `appsettings.json`: Configuration file, including the database connection string.
    * `Pages`: Contains the Razor Pages (`.cshtml`) and their code-behind files (`.cshtml.cs`) for each step of the survey and the thank you page.
        * `Index.cshtml`, `Index.cshtml.cs`: Home page.
        * `Page1.cshtml`, `Page1.cshtml.cs`: Collects general information (Name, Age, Industry, Position).
        * `Page2.cshtml`, `Page2.cshtml.cs`: Allows the user to choose the survey length, determining subsequent pages.
        * `Page3.cshtml`, `Page3.cshtml.cs`: Collects preferences (Likes, Favorite Color, Music Genres, Season, Favorite Place).
        * `Page4.cshtml`, `Page4.cshtml.cs`: Collects opinions using a Likert scale.
        * `Page5.cshtml`, `Page5.cshtml.cs`: Collects dislikes (Worst Color, Music Genres, Season, Worst Place).
        * `ThankYou.cshtml`, `ThankYou.cshtml.cs`: Displays a summary of answers and handles final submission to the database.
        * `_ValidationScriptsPartial.cshtml`: Partial view for client-side validation scripts.
        * `Shared`: Contains shared layout (`_Layout.cshtml`).
    * `wwwroot`: Static files (CSS, JavaScript).
        * `css/site.css`: Custom CSS.
        * `js/site.js`: Custom JavaScript, including functions for the progress bar and toggling "Other" input fields.
    * `Helpers`: Helper classes.
        * `SessionExtensions.cs`: Extension methods for easily storing and retrieving objects from session state as JSON.

## Setup and Installation

1.  **Database Setup:**
    * Create a SQL Server database (e.g., `SurveyDB`).
    * Create a table to store the survey responses with columns corresponding to the properties in the `SurveyResponse` model. Ensure columns for nullable properties are nullable in the database (`INT NULL`, `NVARCHAR(MAX) NULL`, etc.). Columns for `FavoriteMusicGenresCsv` and `WorstMusicGenresCsv` should be large enough to store comma-separated strings (`NVARCHAR(MAX)`).
    * Create a stored procedure named `sp_InsertSurveyResponse` that takes parameters matching the `SurveyResponse` properties and inserts a new row into your survey responses table.
2.  **Update Connection String:**
    * Open `SurveyApp.Web/appsettings.json`.
    * Update the `SurveyDbConnection` string in the `ConnectionStrings` section to match your SQL Server instance, database name, and authentication method. Using Integrated Security (Windows Authentication) is shown, but you may need to adjust based on your environment.
3.  **Run the Application:**
    * Open the project in Visual Studio or your preferred .NET IDE.
    * Build the solution.
    * Run the `SurveyApp.Web` project.

## Features

* **Multi-Page Navigation:** Users progress through the survey page by page.
* **Session State Management:** User input is saved in the session as they navigate, allowing them to go back and forward.
* **Conditional Pages:** The survey length chosen on Page 2 determines which subsequent pages (Page 3, 4, 5) the user will see.
* **Server-Side Validation:** Data annotations on the `SurveyResponse` model are used for validation, with validation summaries and field-specific error messages displayed.
* **Client-Side "Other" Field Toggle:** JavaScript is used to show or hide "Other" specification textboxes based on dropdown or radio button selections.
* **Progress Bar:** A simple progress bar at the top of the layout indicates the user's progress through the survey based on the total number of pages determined by their length choice.
* **Database Submission:** On the Thank You page, the collected data is submitted to a SQL Server database via a dedicated repository and service layer.

## Potential Improvements

* Implement Entity Framework Core for database interactions instead of a raw stored procedure for better type safety and ORM benefits.
* Add more robust error handling and logging, especially around database operations.
* Enhance UI/UX with more detailed styling or advanced form controls.
* Implement more sophisticated validation logic (e.g., conditional validation based on "Other" field visibility).
* Consider security aspects like input sanitization (though parameterized queries help with SQL injection) and potentially more secure session management for sensitive data.
* Add unit and integration tests for BLL and DAL components.
* Implement a more dynamic page navigation system that doesn't rely on hardcoded page names in conditionals.
* Add configuration options for survey questions and options rather than hardcoding them in `SurveyOptions.cs`.