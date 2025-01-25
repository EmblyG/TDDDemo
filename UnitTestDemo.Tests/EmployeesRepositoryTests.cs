using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NSubstitute;
using UnitTestDemo.Entities;
using UnitTestDemo.Interfaces;

namespace UnitTestDemo.Tests;

[TestFixture]
public class EmployeesRepositoryTests
{
    DateTime _startDate = new DateTime(2005, 07, 12);

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // This will happen once before any unit test is called
        // In a more complicated unit test, this might be used to set up a new database for testing purposes
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // This will happen once after every unit test has been called
        // In a more complicated unit test, this might be used to drop the database you used for testing
    }

    [SetUp]
    public void SetUp()
    {
        // This will happen once before each unit test is called
        // In a more complicated unit test, this might be used to set up data your tests all rely on before each test
    }

    [TearDown]
    public void TearDown()
    {
        // this will happen once after each unit test has been called
        // For example, you may be testing logging and you'd need to clear the logs database table between tests so you can start with a clean set of logs for each test
    }


    [Test] // Add this to each unit test method
    public void UpdateEmployee_ValidInput_CallsEmployeesDALUpdate()
    {
        // I like to write my unit tests in AAA fashion, Arrange, Act Assert

        //Arrange - This is where you set up your dependencies, and any variables you may need for testing purposes

        Employee employeeToUpdate = new Employee(1, "Ronald", "McDonald", 7, "Clown", 100000, _startDate);

        // This is a mock or substitute
        // When unit testing you want to isolate your dependencies
        // In this case we don't want our Repository tests to depend on having a REAL IEmployeesDAL object pointing to a REAL Database
        // This allows us to set up calls and responses from our dependencies as well
        // By default any call to this will do nothing with whatever you give it, and return the default value for the method's return type
        // Generally you only want to Substitute.For an interface, you can substitute a concrete class but... don't :) 
        // It's also worth noting that because we have an interface for our EmployeesDAL we can begin unit testing the EmployeesRepository without any actual implementation of the IEmployeesDAL
        IEmployeesDAL employeesDALMock = Substitute.For<IEmployeesDAL>();

        // This sets up our mock DAL to return true when we check if our employee exists
        employeesDALMock.EmployeeExists(employeeToUpdate.Id).Returns(true);

        EmployeesRepository systemUnderTest = new EmployeesRepository(employeesDALMock);

        //Act
        //This is where you actually perform the unit of work you're testing
        int result = systemUnderTest.UpdateEmployee(employeeToUpdate);

        //Assert
        //This is where you ensure the results of Act are valid

        // when you assert you ALWAYS assert the expected value followed by the actual/resulting value 
        Assert.That(result, Is.EqualTo(EmployeesRepository.Success));

        // sometimes it is useful or necessary to assert specific calls that a dependency has received 
        // one drawback of this approach is now your unit test is asserting specific implementation details as opposed to a specific result
        // This means if you change how the method you're testing works, you may have to update a lot more unit tests even if the return value doesn't change
        employeesDALMock
            .Received()
            .UpdateEmployee(employeeToUpdate); // You can even specify the specific parameters, in this case we're ensuring UpdateEmployee was called with the specific Employee object we passed in


        employeesDALMock
            .DidNotReceive() // you can also assert that a method was NOT called
            .DeleteEmployee(Arg.Any<Employee>()); // Arg.Any<Employee>() lets you assert against ANY call to this method, not just one with our specific Employee. 
        // Update probably should not call Delete on any employee. Though is this a valuable assert? Probably not.
        // You only assert that calls to your mock did/did not happen where necessary, use your best judgement
    }

    [Test]
    public void UpdateEmployee_InvalidSalary_ReturnsInvalidSalaryReturnCode()
    {
        // Arrange

        IEmployeesDAL employeesDALMock = Substitute.For<IEmployeesDAL>();
        employeesDALMock.EmployeeExists(Arg.Any<int>()).Returns(true);

        Employee employeeToUpdate =
            new Employee(1, "Ronald", "McDonald", 7, "Clown", EmployeesRepository.MaxSalary + 1, _startDate);

        EmployeesRepository systemUnderTest = new EmployeesRepository(employeesDALMock);

        // Act
        int result = systemUnderTest.UpdateEmployee(employeeToUpdate);

        // Assert
        Assert.That(result, Is.EqualTo(EmployeesRepository.InvalidInput), "Employee with invalid salary should return invalid salary return code!");

        employeesDALMock.DidNotReceive().UpdateEmployee(Arg.Any<Employee>()); // With an invalid salary nothing should have been updated
    }

    [Test]
    public void UpdateEmployee_DALThrowsArgumentException_ReturnsUnknownErrorCode()
    {
        // Arrange
        IEmployeesDAL employeesDALMock = Substitute.For<IEmployeesDAL>();
        employeesDALMock.EmployeeExists(Arg.Any<int>()).Returns(true);
        employeesDALMock
            .When(employeeDAL => employeeDAL.UpdateEmployee(Arg.Any<Employee>()))
            .Do(employeeDAL =>
                throw new ArgumentException(
                    "Something Bad happened!")); // In order to tell a mock to throw an exception on a void method you need to use the .When .Do pattern, in this case when UpdateEmployee is called with any Employee we'll throw an exception

        // it's slightly different when your mock's method has a return, then you would do something like.... 
        // employeeDal.SomeMethodThatReturns(Arg.Any<Employee>).Returns(x => { throw new Exception(); });
        // This is also how you could make your dependency return a specific value to then be used by the test


        Employee employeeToUpdate = new Employee(1, "Ronald", "McDonald", 7, "Clown", 100, _startDate);

        EmployeesRepository systemUnderTest = new EmployeesRepository(employeesDALMock);

        // Act 
        int result = int.MinValue;
        Assert.DoesNotThrow(() => { result = systemUnderTest.UpdateEmployee(employeeToUpdate); });


        // Assert
        Assert.That(result, Is.EqualTo(EmployeesRepository.UnknownError));
    }

    [Test]
    public void UpdateEmployee_EmployeeDoesNotExist_ReturnsEmployeeNotFoundErrorCode()
    {
        // Arrange
        IEmployeesDAL employeesDALMock = Substitute.For<IEmployeesDAL>();
        employeesDALMock.EmployeeExists(Arg.Any<int>()).Returns(false);
        Employee employeeToUpdate = new Employee(1, "Ronald", "McDonald", 7, "Clown", 100, _startDate);

        EmployeesRepository systemUnderTest = new EmployeesRepository(employeesDALMock);

        // Act
        int result = systemUnderTest.UpdateEmployee(employeeToUpdate);

        // Assert
        Assert.That(result, Is.EqualTo(EmployeesRepository.InvalidInput));

        // it might be worth checking that if we don't find the employee to update that we do not call update?
        employeesDALMock.DidNotReceive().UpdateEmployee(Arg.Any<Employee>());
    }

    [Test]
    public void UpdateEmployee_DALThrowsInvalidOperationException_ExceptionRethrown()
    {
        // Arrange
        IEmployeesDAL employeesDALMock = Substitute.For<IEmployeesDAL>();
        employeesDALMock.EmployeeExists(Arg.Any<int>()).Returns(true);
        employeesDALMock
            .When(employeeDAL => employeeDAL.UpdateEmployee(Arg.Any<Employee>()))
            .Do(employeeDAL =>
                throw new InvalidOperationException(
                    "Something Bad happened!")); // In order to tell a mock to throw an exception on a void method you need to use the .When .Do pattern, in this case when UpdateEmployee is called with any Employee we'll throw an exception

        // it's slightly different when your mock's method has a return, then you would do something like.... 
        // employeeDal.SomeMethodThatReturns(Arg.Any<Employee>).Returns(x => { throw new Exception(); });
        // This is also how you could make your dependency return a specific value to then be used by the test


        Employee employeeToUpdate = new Employee(1, "Ronald", "McDonald", 7, "Clown", 100, _startDate);

        EmployeesRepository systemUnderTest = new EmployeesRepository(employeesDALMock);

        // Act & Assert
        int result = int.MinValue;
        Assert.Throws<InvalidOperationException>(() => { result = systemUnderTest.UpdateEmployee(employeeToUpdate); });
    }

    [Test]
    public void FireEmployee_ValidInput_CallsEmployeesDALUpdate()
    {
        //Arrange
        Employee employeeToFire = new(1, "Ronald", "McDonald", 7, "CEO", 100000, _startDate);

        IEmployeesDAL employeesDALMock = Substitute.For<IEmployeesDAL>();

        EmployeesRepository employeeRepo = new(employeesDALMock);

        var endDate = DateTime.Now;

        //Act

        var result = employeeRepo.FireEmployee(employeeToFire, endDate);

        //Assert
        Assert.That(result, Is.EqualTo(EmployeesRepository.Success));
        employeesDALMock.Received(1).UpdateEmployee(employeeToFire);
    }

    [Test]
    //termination before startdate
    [TestCase("2004-11-07")]
    //Termination in the future 
    [TestCase("2027-11-07")]
    public void FireEmployee_InvalidInput_InvalidTerminationDate(DateTime terminationDate)
    {
        //Arrange
        Employee employeeToFire = new(1, "Ronald", "McDonald", 7, "CEO", 100000, _startDate);

        IEmployeesDAL employeesDALMock = Substitute.For<IEmployeesDAL>();

        EmployeesRepository employeeRepo = new(employeesDALMock);

        //Act

        var result = employeeRepo.FireEmployee(employeeToFire, terminationDate);

        //Assert
        Assert.That(result, Is.EqualTo(EmployeesRepository.InvalidInput));
        employeesDALMock.DidNotReceive().UpdateEmployee(employeeToFire);
    }


}