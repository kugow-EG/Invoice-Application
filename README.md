# Invoice-Application
This application follows a clean architecture with the following layers:
API: For exposing endpoints.
Interactor: For business logic and service operations.
Repository: For database operations.
Tests: For unit testing.

# Key Features
 # Invoice Management:
  Create new invoices. 
  Fetch all invoices. 
  Process payments.
  Overdue Invoice Handling:
  
  Apply late fees for overdue invoices.
  Mark invoices as "Void" or create new invoices with updated due dates and amounts.

# Technologies Used
ASP.NET Core 8.0
Entity Framework Core
AutoMapper
xUnit for unit testing
SQL Server (Database)
Docker
