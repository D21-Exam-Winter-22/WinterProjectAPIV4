using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using WinterProjectAPIV4.Controllers;
using WinterProjectAPIV4.Models;

namespace APIServicesTestProject;

public class UnitTest1
{
    private readonly PaymentApidbContext _context;
    private readonly UserGroupController _userGroupController;

    public UnitTest1()
    {
        _context = new PaymentApidbContext();
        _userGroupController = new UserGroupController(_context);
    }

    [Fact]
    public async void ApplicationIsOnlineTest()
    {
        var okResult = _userGroupController.ApplicationIsOnline().Result;

        //Assert      
        Assert.IsType<OkResult>(okResult);
    }
}