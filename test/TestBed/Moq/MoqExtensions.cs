using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Language.Flow;

namespace Util.Test
{
	static class MoqExtensions
	{
		public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
			params TResult[] results) where T : class
		{
			setup.Returns(new Queue<TResult>(results).Dequeue);
		}
	}
}
