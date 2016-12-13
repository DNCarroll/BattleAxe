# Weapons
BattleAxe (DAL) and ULFBERHT javascript Framework.

//C# Example of using BattleAxe
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BattleAxe {
    public static class Examples {

        public static string ConnectionString() {
            return "ConnectionString";
        }

        public static void FirstOrDefault() {
            //default command type is Procedure so it is not required by the GetCommand Method
            //If doing inline stuff it should be supplied
            //both Procedure and Inline will derive the parameters for you

            SqlCommand commandBasedOnProcedure = "GetCustomerByID".GetCommand(ConnectionString());
            //command type provided here
            SqlCommand commandBasedOnInline = "SELECT FirstName, LastName, CustomerID FROM Customers WHERE CustomerID = @CustomerID".GetCommand(ConnectionString(), CommandType.Text);

            //Parameter expecting a value for the @CustomerID
            //BattleAxe methods  expect your object to have matching properties by Name for the Parameters - @ symbol
            Customer firstOrDefaultViaProcedure = commandBasedOnInline.FirstOrDefault(new Customer { CustomerID = 1 });
            Customer firstOrDefaultViaInlin = commandBasedOnInline.FirstOrDefault(new Customer { CustomerID = 1 });

            //extensions method also exist is you would like to run command from object
            Customer firstOrDefaultViaObject = (new Customer { CustomerID = 1 }).FirstOrDefault(commandBasedOnProcedure);
    
        }

        public static void Execute() {
            Customer customer = new Customer { CustomerID = 1, FirstName = "Nathan", LastName = "Carroll" };
            SqlCommand commandBasedOnProcedure = "UpdateCustomerInformation".GetCommand(ConnectionString());
            SqlCommand commandBasedOnInline = @"
UPDATE Customers 
SET 
    FirstName = @FirstName
    LastName = @LastName
WHERE
    CustomerID = @CustomerID".GetCommand(ConnectionString(), CommandType.Text);

            commandBasedOnProcedure.Execute(customer);
            commandBasedOnInline.Execute(customer);
            //or
            customer.Execute(commandBasedOnProcedure);

        }

        public static List<Customer> ToList() {
            Customer customer = new Customer { FirstName = "N" };
            SqlCommand commandBasedOnProcedure = "FindCustomer".GetCommand(ConnectionString());
            SqlCommand commandBasedOnInline = @"
SELECT TOP(100) 
    FirstName, 
    LastName,
    CustomerID
FROM 
    Customers
WHERE
    FirstName LIKE @FirstName".GetCommand(ConnectionString(), CommandType.Text);

            List<Customer> customersFoundViaProcedure = commandBasedOnProcedure.ToList(customer);
            List<Customer> customerFoundViaInline = commandBasedOnInline.ToList(customer);
            //or
            customersFoundViaProcedure = customer.ToList(commandBasedOnProcedure);
            return customersFoundViaProcedure;
        }

        public static void UpdateAListOfCustomers() {
            var customers = ToList();
            SqlCommand commandBasedOnProcedure = "UpdateIsActive".GetCommand(ConnectionString());
            SqlCommand commandBasedOnInline = @"
UPDATE Customers 
SET 
    IsActive = @IsActive
WHERE
    CustomerID = @CustomerID".GetCommand(ConnectionString(), CommandType.Text);

            customers.ForEach(c => c.IsActive = true);
            commandBasedOnProcedure.Update(customers);
            commandBasedOnInline.Update(customers);
            //or
            customers.Update(commandBasedOnProcedure);
        }
    }

    public class Customer {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CustomerID { get; set; }
        public bool IsActive { get; set; }
    }
}

