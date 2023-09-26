﻿namespace UnitTestDemo.Entities;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int ReportsToId { get; set; }
    public string Role { get; set; }
    public int AnnualSalary { get; set; }

    public Employee(int id, string firstName, string lastName, int reportsToId, string role, int annualSalary)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        ReportsToId = reportsToId;
        Role = role;
        AnnualSalary = annualSalary;
    }
}