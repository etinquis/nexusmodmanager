using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestBed.Logging;

namespace TestBed
{
	class TestFixture
	{
		private ILogger _logger;

		protected void Log(string message)
		{
			_logger.Log(message);
		}
	}
}
