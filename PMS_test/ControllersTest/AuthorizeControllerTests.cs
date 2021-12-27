using System.Threading.Tasks;
using Xunit;

namespace PMS_test.ControllersTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class AuthorizeControllerTests : BaseControllerTests
    {
        public AuthorizeControllerTests() : base()
        {

        }

        [Fact]
        public async Task TestCheckAuthentucate()
        {
            var response = await _client.GetAsync("/authorize");

            Assert.True(response.IsSuccessStatusCode);
        }
    }
}
