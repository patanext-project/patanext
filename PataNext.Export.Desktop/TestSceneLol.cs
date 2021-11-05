using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;

namespace PataNext.Export.Desktop
{
	public class TestSceneLol : TestScene
	{
		public TestSceneLol()
		{
			Add(new TestBox());
		}
	}

	public class TestBox : Box
	{
		public TestBox()
		{
			Colour = Colour4.White;
			Size   = new(80, 80);
		}
	}
}