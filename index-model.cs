using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ContactManager.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<Contact> Contacts { get; set; } = new List<Contact>();
        
        [BindProperty]
        public Contact Contact { get; set; } = new Contact();

        public async Task OnGetAsync()
        {
            await LoadContactsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadContactsAsync();
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                
                string sql = "INSERT INTO Contacts (Name, Email, Phone) VALUES (@Name, @Email, @Phone)";
                
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", Contact.Name);
                    command.Parameters.AddWithValue("@Email", Contact.Email);
                    command.Parameters.AddWithValue("@Phone", Contact.Phone);
                    
                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            using (SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                
                string sql = "DELETE FROM Contacts WHERE Id = @Id";
                
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadContactsAsync();
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                
                string sql = "UPDATE Contacts SET Name = @Name, Email = @Email, Phone = @Phone WHERE Id = @Id";
                
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", Contact.Id);
                    command.Parameters.AddWithValue("@Name", Contact.Name);
                    command.Parameters.AddWithValue("@Email", Contact.Email);
                    command.Parameters.AddWithValue("@Phone", Contact.Phone);
                    
                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage();
        }

        private async Task LoadContactsAsync()
        {
            Contacts.Clear();
            
            using (SqlConnection connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                
                string sql = "SELECT Id, Name, Email, Phone FROM Contacts ORDER BY Name";
                
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Contacts.Add(new Contact
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                Phone = reader.GetString(3)
                            });
                        }
                    }
                }
            }
        }
    }

    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}