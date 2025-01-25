using UnitTestDemo.Entities;
using UnitTestDemo.Interfaces;

namespace UnitTestDemo;

public class EmployeesRepository
{
    public static readonly int Success = 0;
    public static readonly int InvalidInput = -1;
    public static readonly int UnknownError = -999;

    public const int MaxSalary = 1000000;

    private readonly IEmployeesDAL _employeesDal;


    public EmployeesRepository(IEmployeesDAL employeesDal)
    {
        _employeesDal = employeesDal;
    }

    public int UpdateEmployee(Employee toUpdate)
    {

        if (toUpdate.AnnualSalary > MaxSalary)
        {
            return InvalidInput;
        }

        if (toUpdate.ReportsToId == toUpdate.Id)
        {
            return InvalidInput;
        }

        if (string.IsNullOrEmpty(toUpdate.Role))
        {
            return InvalidInput;
        }

        if (!_employeesDal.EmployeeExists(toUpdate.Id))
        {
            return InvalidInput;
        }

        try
        {
            _employeesDal.UpdateEmployee(toUpdate);

        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch
        {
            return UnknownError;
        }

        return Success;
    }

    public Employee GetEmployee(int employeeId)
    {
        return _employeesDal.RetrieveEmployee(employeeId);
    }

    public int FireEmployee(Employee toTerminate, DateTime terminationDate)
    {
        if (toTerminate.StartDate > terminationDate || DateTime.Now < terminationDate)
        {
            return InvalidInput;
        }
        toTerminate.EndDate = terminationDate;
        try
        {
            _employeesDal.UpdateEmployee(toTerminate);
        }
        catch
        {
            return UnknownError;
        }

        return Success;
    }
}