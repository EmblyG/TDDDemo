using UnitTestDemo.Entities;

namespace UnitTestDemo.Interfaces;

public interface IEmployeesDAL
{
    Employee CreateEmployee(Employee toCreate);
    Employee RetrieveEmployee(int employeeId);
    void UpdateEmployee(Employee toUpdate);
    void DeleteEmployee(Employee toDelete);
    bool EmployeeExists(int id);
}