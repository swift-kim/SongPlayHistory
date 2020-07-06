using IPA.Logging;
using Xunit.Abstractions;

namespace SongPlayHistory.UnitTest
{
    public class TestLogger : Logger
    {
        public ITestOutputHelper Output { get; set; }

        public override void Log(Level level, string message)
        {
            Output?.WriteLine(message);
        }
    }
}
