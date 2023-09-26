using UnitTestDemo.Entities;
using UnitTestDemo.Interfaces;

namespace UnitTestDemo;

public class EmployeesRepository
{
    public static readonly int Success = 0;
    public static readonly int InvalidSalaryReturnCode = -1;
    public static readonly int InvalidReportsToReturnCode = -2;
    public static readonly int InvalidRoleReturnCode = -3;
    public static readonly int EmployeeNotFoundErrorCode = -4;
    public static readonly int UnknownError = -999;

    public const int MaxSalary = 1000000;
        
    private readonly IEmployeesDAL _employeesDal;


    public EmployeesRepository(IEmployeesDAL employeesDal)
    {
        _employeesDal = employeesDal;
    }

    public int UpdateEmployee(Employee toUpdate)
    {

        if(toUpdate.AnnualSalary > MaxSalary)
        {
            return InvalidSalaryReturnCode;
        }

        if(toUpdate.ReportsToId == toUpdate.Id)
        {
            return InvalidReportsToReturnCode;
        }

        if (string.IsNullOrEmpty(toUpdate.Role))
        {
            return InvalidRoleReturnCode;
        }

        if (!_employeesDal.EmployeeExists(toUpdate.Id))
        {
            return EmployeeNotFoundErrorCode;
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
}