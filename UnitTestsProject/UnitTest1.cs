using Microsoft.AspNetCore.Mvc;
using WinterProjectAPIV4.Controllers;
using WinterProjectAPIV4.Models;

namespace UnitTestsProject;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        PaymentApidbContext DbContext = new PaymentApidbContext();
        UserGroupController controller = new UserGroupController(DbContext);
        Task<ActionResult<bool>> ActualResponse = controller.ApplicationIsOnline();
        Task<ActionResult<bool>> ExpectedResponse;
        
        
    }
}