using WinterProjectAPIV4.Controllers;
using WinterProjectAPIV4.Models;

namespace UnitTestProject;

public class Tests
{
    private PaymentApidbContext context;
    private UserGroupController controller;
    [SetUp]
    public void Setup()
    {
        context = new PaymentApidbContext();
        controller = new UserGroupController(context);
    }

    [Test]
    public void TestIfServiceIsOnline()
    {
        bool ExpectedResult = true;
        bool ActualResult = controller.ApplicationIsOnline();
        
        Assert.AreEqual(ExpectedResult, ActualResult);
    }

    
}