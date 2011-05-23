using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Quartz.Impl {
	/// <summary>
	/// The interface to get type string whilch will be stored in <see cref="IJobDetail" />,
	/// where the 'Version', 'Culture', and 'PublicKeyToken' are omitted from Type.AssemblyQualifiedName.
	/// This can store generic type, including nested ones.
	/// <example>
	/// The type 'SomeNamespace.GenericJob&lt;SomeNamespace.SomePlainJob&gt;' will be converted to
	/// "SomeNamespace.GenericJob[[SomeNamespace.SomePlainJob, SomeAssembly]], SomeAssembly"
	/// </example>
	/// </summary>
	public class ShortenedAssemblyQualifiedNameResolver : ITypeNameResolver
	{
		#region public static readonly string qualifiedNameStructure = ...
		/// <summary>
		/// Text for parsing Type.AssemblyQualifiedName
		/// </summary>
		private static readonly string qualifiedNameStructure = @"
^(?'fullname'
	(?'fullNameSansGenericType'
		[^`,]+
	)
	(?'genericType'
		(?'genericQualifier' `\d+)
		\[(?'genericParameterList' .*)\]
	)?
),\s
(?'assemblyName'[^,]+)
(?:,\s(?'miscInfo'[^,]+))+
$";
		#endregion
		/// <summary>
		/// Text for parsing generic parameters in the type, if any.
		/// </summary>
		private static readonly string genericParameterList = @"^\[(?'aqn' .+? )\] (?:, \[(?'aqn' .+? )\] )*$";
		/// <summary>
		/// Text pattern of the parameter whilch is non-generic type.
		/// </summary>
		private static readonly string nonGenericParameter = @"\[(?'parameter'[^\[\]]+)\]";
		private static readonly Regex regexQualifiedNameStructure = new Regex(qualifiedNameStructure, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
		private static readonly Regex regexNonGenericParameter = new Regex(nonGenericParameter, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);
		private static readonly Regex regexGenericParameterList = new Regex(genericParameterList, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

		public string GetName(Type type)
		{
			return ParseAndShortenAssemblyQualifiedName(type.AssemblyQualifiedName);
		}
		private static string ParseAndShortenAssemblyQualifiedName(string assemblyQualifiedName)
		{
			var matchAqn = regexQualifiedNameStructure.Match(assemblyQualifiedName);

			string fullName = matchAqn.Groups["fullNameSansGenericType"].Value;
			string assemblyName = matchAqn.Groups["assemblyName"].Value;
			bool isGeneric = matchAqn.Groups["genericType"].Success;

			if (isGeneric)
			{
				string genericParameterList = matchAqn.Groups["genericParameterList"].Value;
				string shortenedGenericParameterList = GetGenericParameterList(genericParameterList);
				string genericQualifier = matchAqn.Groups["genericQualifier"].Value;
				fullName += string.Format("{0}[{1}]", genericQualifier, shortenedGenericParameterList);
			}

			return string.Format("{0}, {1}", fullName, assemblyName);
		}
		private static string GetGenericParameterList(string list)
		{
			List<string> parameters = new List<string>();
			int start, pos, depth;
			pos = 0;
			start = list.IndexOf('[', pos);
			while (start != -1)
			{
				for (pos = start + 1, depth = 1; depth != 0; pos++)
				{
					pos = list.IndexOfAny(new[] { '[', ']' }, pos + 1);
					depth += list[pos] == '[' ? 1 : -1;
				}
				pos--;
				string parameter = list.Substring(start + 1, (pos - 1) - start);
				parameters.Add(parameter);
				start = list.IndexOf('[', pos);
			}

			return parameters
				.Select(aqn => ParseAndShortenAssemblyQualifiedName(aqn))
				.Select(aqn => string.Format("[{0}]", aqn))
				.Aggregate((first, second) => string.Format("{0},{1}", first, second));
		}
	}
}
