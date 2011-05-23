using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quartz {
	/// <summary>
	/// The interface to convert a type from its logical name whilch will be stored in <see cref="IJobDetail" />.
	/// </summary>
	/// <author>tQubo</author>
	public interface ITypeNameResolver
	{
		/// <summary>
		/// Get the logical name of the specified type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		string GetName(Type type);
		//TODO: Is this needed?
		///// <summary>
		///// Get the type from its logical name.
		///// </summary>
		///// <param name="name"></param>
		///// <returns></returns>
		//Type GetType(string name);
	}
}
