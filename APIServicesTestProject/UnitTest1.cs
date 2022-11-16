using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using WinterProjectAPIV4.Controllers;
using WinterProjectAPIV4.Models;

namespace APIServicesTestProject;

public class UnitTest1
{
    [Fact]
    public async void Test1()
    {
        //Arrange
        bool? ExpectedResult = true;
        PaymentApidbContext DbContext = A.Fake<PaymentApidbContext>();
        UserGroupController Controller = new UserGroupController(DbContext);

        //Act
        var actionResult = await Controller.ApplicationIsOnline();

        //Assert      
        Assert.Equal(ExpectedResult, actionResult.Value);
    }
}